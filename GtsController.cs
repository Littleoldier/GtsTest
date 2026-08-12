using gts;
using GtsTest.Commands;   // 引用新建的 Commands 文件夹里的所有指令
using GtsTest.Modbus;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Text.Json;


namespace GtsTest
{
    public class SimulationModeChangedEventArgs : EventArgs
    {
        public bool IsSimulationMode { get; }
        public SimulationModeChangedEventArgs(bool isSimulation) => IsSimulationMode = isSimulation;
    }
    public class GtsController
    {
        private ModbusConfig _modbusConfig;
        private readonly GtsModel _model;
        private readonly Form1 _view;

        private Thread? _pollingThread;
        private CancellationTokenSource? _cts;
        private readonly object _lock = new object();

        private ModbusClient _modbusClient;

        // ========== 工作流相关字段  ==========
        private CancellationTokenSource? _workflowCts;     // 用于取消正在运行的工作流
        private readonly object _workflowLock = new object(); // 防止用户狂点“启动”按钮导致线程爆炸
        public event EventHandler<SimulationModeChangedEventArgs>? SimulationModeChanged;   //切换模式事件

        public GtsController(GtsModel model, Form1 view)
        {
            _model = model;
            _view = view;
            _modbusConfig = new ModbusConfig();   // ← 关键

            AppLogger.OnLogReceived += (level, logLine, category) =>
            {
                _view.BeginInvoke(new Action(() =>
                {
                    // 根据类别分流：Modbus 和 Monitor 类放监控日志，其余放操作日志
                    if (category == "Monitor" || category == "Modbus")
                        _view.AppendMonitorLog(logLine);  // 直接显示带时间戳的整行
                    else
                        _view.ShowResult(logLine);        // 直接显示带时间戳的整行
                }));
            };

            // 订阅 View 的事件
            _view.OpenRequested += OnOpenRequested;                             //开启设备
            _view.CloseDeviceRequested += OnCloseDeviceRequested;               //关闭设备
            _view.ClearRequested += OnClearRequested;
            _view.GetStatusRequested += OnGetStatusRequested;
            _view.RunWorkflowRequested += OnRunWorkflowRequested;               // 工作流

            _view.StartMonitorRequested += (s, e) => StartMonitoring(100);      // 100ms 周期
            _view.StopMonitorRequested += (s, e) => StopMonitoring();
            _view.ToggleSimulatorRequested += (s, e) => ToggleSimulationMode(); //切换环境
            this.SimulationModeChanged += (s, e) =>
            {
                // 确保 UI 更新在主线程执行（因为事件可能在后台线程触发，但 ToggleSimulationMode 是在主线程调用的，所以这里不 Invoke 也可）
                _view.SetSimulationModeUI(e.IsSimulationMode);
            };
            _view.ToggleModbusRequested += _view_ToggleModbusRequested;
            _view.ModbusConfigChanged += (s, config) =>
            {
                _modbusConfig = config;
            };
            _modbusClient = new ModbusClient(_modbusConfig);
            _modbusClient.ConnectionStateChanged += OnModbusConnectionChanged;
            //_view.ModbusReconnectRequested += OnModbusReconnectRequested;
            //_modbusClient.Reconnect(_modbusConfig.IpAddress, _modbusConfig.Port);
            _view.WriteRegisterRequested += OnWriteRegisterRequested;
            _view.WriteCoilRequested += OnWriteCoilRequested;

            CyclicMonitorBuffer.SetMaxSize(30000); // 设置监控缓存容量
        }

        private void _view_ToggleModbusRequested(object? sender, EventArgs e)
        {
            if (_modbusClient.IsConnected)
            {
                // 已连接 → 断开
                _modbusClient.Disconnect();
                // 注意：Disconnect() 会触发 ConnectionStateChanged 事件，
                // 进而调用 UpdateModbusStatus，UI 会自动更新
            }
            else
            {
                bool success = _modbusClient.Reconnect(_modbusConfig);
                if (success)
                    AppLogger.Info($"✅ Modbus 已连接至 {_modbusConfig.IpAddress}:{_modbusConfig.Port}", "Operation");
                else
                    AppLogger.Error("❌ Modbus 连接失败", "Operation");
            }
        }

        // 处理“打开”请求
        private void OnOpenRequested(object? sender, EventArgs e)
        {
            try
            {
                // ========== 第一步：打开设备 ==========
                short openResult = _model.OpenDevice(0, 1);  // 注意这里改成 short
                if (openResult != 0)
                {
                    AppLogger.Error($"❌ 打开设备失败 | 错误码: {openResult} (0x{openResult:X}) | {GetErrorMessage(openResult)}","Operation");
                    return; // 打开失败，直接退出，不再执行复位
                }

                // ========== 第二步：复位/使能（仅在打开成功后执行） ==========
                short resetResult = _model.GT_Reset();
                if (resetResult == 0)
                {
                    AppLogger.Info($"✅ 初始化成功 | 打开返回值: 0 | 复位返回值: 0 (成功)","Operation");
                }
                else
                {
                    // 注意：虽然复位失败，但设备其实已经打开了，所以显示警告而非纯粹的错误
                   AppLogger.Warn($"⚠️ 设备已打开，但复位失败 | 打开返回值: 0 | 复位错误码: {resetResult} (0x{resetResult:X}) | {GetErrorMessage(resetResult)}", "Operation");
                    return;
                }

            }
            catch (Exception ex)
            {
                AppLogger.Error($"调用异常: {ex.Message}", "Operation");
            }
        }

        #region 实时监控线程
        /// <summary>
        /// 启动实时监控（供 View 的按钮调用）
        /// </summary>
        public void StartMonitoring(int intervalMs = 100)
        {
            lock (_lock)
            {
                // 防止重复启动导致线程爆炸
                if (_pollingThread != null && _pollingThread.IsAlive)
                {
                    AppLogger.Warn("⚠️ 监控线程已在运行中","Operation");
                    return;
                }

                _cts = new CancellationTokenSource();
                var token = _cts.Token;

                _pollingThread = new Thread(() => PollingLoop(intervalMs, token))
                {
                    Name = "GTS_StatusPolling_Thread",  // 调试时一眼就能认出这个线程
                    IsBackground = true // 如果主窗口意外关闭，它不会阻止进程退出
                };
                _pollingThread.Start();
                AppLogger.Info($"✅ 实时监控已启动 (周期: {intervalMs}ms)","Operation");
            }
        }

        /// <summary>
        /// 停止实时监控（供 View 的按钮和窗体关闭时调用）
        /// </summary>
        public void StopMonitoring()
        {
            lock (_lock)
            {
                if (_cts == null) return;

                _cts.Cancel(); // 通知线程优雅退出
                _pollingThread?.Join(200); // 等待最多200ms让线程自己结束
                _pollingThread = null;
                _cts = null;
                AppLogger.Info("⏹ 实时监控已停止","Operation");
            }
            // 停止监控时，顺便把正在运行的工作流也干掉
            _workflowCts?.Cancel();
        }

        /// <summary>
        /// 线程执行体：循环读取轴状态
        /// </summary>
        private void PollingLoop(int interval, CancellationToken ct)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();//计时基准：使用 Stopwatch 记录启动后的毫秒数，以计算本周期应结束的时间点。

            while (!ct.IsCancellationRequested)
            {
                // 1. 计算本周期应该结束的时间点
                long nextStart = stopwatch.ElapsedMilliseconds + interval;

                // 2. 执行数据读取（仅调用 Model，不操作 UI）
                // 注意：因为要读界面的轴号，这里用 Invoke 拿一下值（为了线程安全）
                short axis = 1;
                _view.Invoke(new Action(() => { axis = _view.SelectedAxis; }));

                // 获取各项数据（调用 GtsModel）
                int status = 0;
                uint clk = 0;
                double pos = 0, vel = 0, acc = 0;
                int mode = 0;

                _model.GetAxisStatus(axis, out status, out clk);
                _model.GetPrfPos(axis, out pos, out clk);
                _model.GetPrfVel(axis, out vel, out clk);
                _model.GetPrfAcc(axis, out acc, out clk);
                _model.GetPrfMode(axis, out mode, out clk);

                string timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string dataLine = $"[{timeStamp}] 轴[{axis}] 实时 -> 位置:{pos:F2} | 速度:{vel:F2} | 状态:0x{status:X}";

                // 2. 存入循环内存缓冲区（无 I/O 操作）
                CyclicMonitorBuffer.Log(dataLine);
                // 3. 跨线程更新 UI（使用 BeginInvoke，不阻塞工作线程）
                _view.BeginInvoke(new Action(() =>
                {
                    // 这里只做显示，不写复杂逻辑
                    _view.AppendMonitorLog(dataLine);
                }));

                // 读取 Modbus 数据
                var result = _modbusClient.ReadHoldingRegistersWithRaw(_modbusConfig.StartAddress,_modbusConfig.RegisterCount);
                if (result != null)
                {
                    string hexStr = string.Join(", ", result.RawRegisters.Select(r => $"0x{r:X4}"));
                    string display = ModbusFormatter.Format(result.ConvertedValue,_modbusConfig.DisplayFormat,_modbusConfig.ByteOrder);

                    // 输出到 UI 和缓冲区
                    string hexStrLine = $"[{timeStamp}]原始寄存器: {hexStr}";
                    string rawValuesLine = $"[{timeStamp}]Modbus数据: {display}";
                    _view.BeginInvoke(new Action(() => _view.AppendMonitorLog(hexStrLine)));
                    _view.BeginInvoke(new Action(() => _view.AppendMonitorLog(rawValuesLine)));
                    CyclicMonitorBuffer.Log(hexStrLine);
                    CyclicMonitorBuffer.Log(rawValuesLine);
                }

                // 4. 精确等待到下一个周期（补偿 Sleep 误差）
                long now = stopwatch.ElapsedMilliseconds;
                int delay = (int)(nextStart - now);
                if (delay > 0)
                {
                    Thread.Sleep(delay);
                }
            }
        }
        #endregion

        private void OnWriteRegisterRequested(object? sender, WriteRegisterEventArgs e)
        {
            try
            {
                // 1. 将值编码为 ushort[]（使用 ModbusClient.EncodeValue）
                ushort[] raw = ModbusClient.EncodeValue(e.Values, e.DataType, e.ByteOrder);
                if (raw.Length == 0)
                {
                    AppLogger.Warn("写入数据为空", "Operation");
                    return;
                }

                // 2. 根据数据长度选择写入方法
                bool success;
                if (raw.Length == 1 && (e.DataType == DataType.Int16 || e.DataType == DataType.UInt16))
                {
                    // 单寄存器写入
                    success = _modbusClient.WriteSingleRegister(e.Address, raw[0]);
                }
                else
                {
                    // 多寄存器写入
                    success = _modbusClient.WriteMultipleRegisters(e.Address, raw);
                }

                if (success)
                    AppLogger.Info($"✅ 寄存器写入成功: 地址={e.Address}, 值={string.Join(",", e.Values)}", "Operation");
                else
                    AppLogger.Error($"❌ 寄存器写入失败: 地址={e.Address}", "Operation");
            }
            catch (Exception ex)
            {
                AppLogger.Error($"❌ 寄存器写入异常: {ex.Message}", "Operation");
            }
        }

        private void OnWriteCoilRequested(object? sender, WriteCoilEventArgs e)
        {
            try
            {
                bool success = _modbusClient.WriteSingleCoil(e.Address, e.Value);
                if (success)
                    AppLogger.Info($"✅ 线圈写入成功: 地址={e.Address}, 值={e.Value}", "Operation");
                else
                    AppLogger.Error($"❌ 线圈写入失败: 地址={e.Address}", "Operation");
            }
            catch (NModbus.SlaveException ex)
            {
                // 功能码 133 (0x85) 表示异常响应，异常码 1 表示不支持该功能
                if (ex.FunctionCode == 133 && ex.SlaveExceptionCode == 1)
                    AppLogger.Error($"❌ 从机不支持写线圈功能 (功能码 0x05)。如需控制数字量输出，请尝试使用“写寄存器”并操作相应位。", "Operation");
                else
                    AppLogger.Error($"❌ 线圈写入异常: 功能码={ex.FunctionCode}, 异常码={ex.SlaveExceptionCode}", "Operation");
            }
            catch (Exception ex)
            {
                AppLogger.Error($"❌ 线圈写入异常: {ex.Message}", "Operation");
            }
        }

        private void OnModbusConnectionChanged(object? sender, ModbusConnectionEventArgs e)
        {
            // 检查 Handle 是否已创建，若未创建则放弃此次 UI 更新（后续事件会再次触发）
            if (!_view.IsHandleCreated)
            {
                // 可以选择在此处将状态暂存，或直接忽略，因为连接状态改变事件会再次触发
                return;
            }

            // 必须使用 BeginInvoke 跨线程安全更新 UI
            _view.BeginInvoke(new Action(() =>
            {
                // 调用 Form1 中刚刚写好的更新方法（更新指示灯和显示错误）
                _view.UpdateModbusStatus(e.IsConnected, _modbusConfig, e.ErrorMessage);
            }));
        }

        #region 工作流执行逻辑
        // ========== 工作流执行逻辑  ==========
        private void OnRunWorkflowRequested(object? sender, EventArgs e)
        {
            // ========= 策略 1：尝试加载 JSON 配置 =========
            string selected = _view.SelectedWorkflowName;

            if (!string.IsNullOrEmpty(selected))
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Workflows", selected + ".json");
                if (File.Exists(filePath))
                {
                    var config = LoadWorkflowFromJson(filePath);
                    if (config != null)
                    {
                        // JSON 加载成功！执行动态流程
                        ExecuteWorkflowByConfig(config);
                        return; // 执行完毕，直接返回
                    }
                    else
                    {
                        // JSON 存在但解析失败（格式错误），此时不降级，让用户去检查 JSON 格式
                        // 但为了防止卡死，我们可以选择直接 return，或者提示错误后走降级。
                        // 为了面试更稳，这里可以改成：如果解析失败，提示并继续走默认。
                        AppLogger.Warn("⚠️ JSON 解析失败，将自动切换至默认硬编码流程", "Operation");
                    }
                }
                else
                {
                    AppLogger.Warn($"⚠️ 未找到文件 {selected}.json，将自动切换至默认硬编码流程", "Operation");
                }
            }
            else
            {
                AppLogger.Warn("⚠️ 未选择工作流，将自动切换至默认硬编码流程", "Operation");
            }

            // ========= 策略 2：降级执行默认硬编码（保底方案） =========
            ExecuteDefaultWorkflow();
        }
        /// <summary>
        /// 默认硬编码工作流（作为后备方案，确保无配置时也能执行）
        /// </summary>
        private void ExecuteDefaultWorkflow()
        {
            lock (_workflowLock)
            {
                // 1. 取消旧任务
                _workflowCts?.Cancel();
                _workflowCts = new CancellationTokenSource();
                var token = _workflowCts.Token;

                // 2. 获取当前轴号
                short axis = _view.SelectedAxis;

                // 3. 装配旧的工作流（就是你原来写得特别好的那段逻辑）
                var home = new HomeCommand(_model, axis, homePos: 0);
                var move1 = new MoveAbsCommand(_model, axis, targetPos: 10000);
                var waitIO = new WaitIOCommand(_model, ioIndex: 0, expectValue: true);
                var move2 = new MoveAbsCommand(_model, axis, targetPos: 5000);
                var delay = new DelayCommand(_model, delayMs: 500);

                var workflow = new SequenceCommand(home, move1, waitIO, move2, delay);
                workflow.OnLog += msg => _view.BeginInvoke(new Action(() => AppLogger.Info(msg)));

                var thread = new Thread(() => workflow.Execute(token))
                {
                    Name = "DefaultWorkflow_Thread",
                    IsBackground = true
                };
                thread.Start();

                AppLogger.Info("🚀 已启动【默认硬编码工作流】(回零->定位10000->等待IO->定位5000->延时)", "Operation");
            }
        }
        #endregion

        #region 切换模式
        public void ToggleSimulationMode()
        {
            // 第一步：强制停止所有正在运行的后台任务（防止卡死）
            StopMonitoring();           // 停止实时监控线程
            _workflowCts?.Cancel();     // 停止正在运行的自动化流程（如果有）

            // 第二步：释放当前占用的硬件资源（关键！）
            // 不管当前是什么模式，都尝试调用 CloseDevice（模拟模式直接返回0，真实模式则释放驱动句柄）
            _model.CloseDevice();

            // 第三步：切换标志位（取反）
            GtsModel.UseSimulation = !GtsModel.UseSimulation;

            // 第四步：清空界面日志，给用户明确反馈
            _view.ClearResult();
            string modeStatus = GtsModel.UseSimulation ? "✅ 模拟器已开启 (无硬件依赖)" : "✅ 真实硬件模式已开启 (连接实际控制卡)";
            AppLogger.Info(modeStatus, "Operation");
            AppLogger.Info("⚠️ 请点击【初始化】重新建立连接以生效", "Operation");

            // 第五步：更新按钮文字（让用户知道当前状态）
            // 触发事件，通知所有View模式已改变
            SimulationModeChanged?.Invoke(this, new SimulationModeChangedEventArgs(GtsModel.UseSimulation));
        }
        #endregion

        #region 执行工作流
        /// <summary>
        /// 根据配置列表动态构建并执行工作流
        /// </summary>
        private void ExecuteWorkflowByConfig(WorkflowConfig config)
        {
            if (config == null || config.Commands == null || config.Commands.Count == 0)
            {
                AppLogger.Error("❌ 无效的工作流配置", "Operation");
                return;
            }

            lock (_workflowLock)
            {
                _workflowCts?.Cancel();//是否存在，若存在则立刻发出撤退消息，若不存在则跳过
                _workflowCts = new CancellationTokenSource();//新建一个取消发射器
                var token = _workflowCts.Token;

                var commands = new List<IMotionCommand>();
                foreach (var cmdCfg in config.Commands)
                {
                    // 如果配置里没指定轴号，则默认使用 UI 上选中的轴号
                    if (cmdCfg.Axis == 0) cmdCfg.Axis = _view.SelectedAxis;
                    var cmd = CommandFactory.Create(_model, cmdCfg);
                    commands.Add(cmd);
                }

                var workflow = new SequenceCommand(commands.ToArray());
                workflow.OnLog += msg => _view.BeginInvoke(new Action(() => AppLogger.Info(msg)));

                var thread = new Thread(() => workflow.Execute(token))
                {
                    Name = $"Workflow_{config.Name}",
                    IsBackground = true
                };
                thread.Start();

                AppLogger.Info($"🚀 已启动工作流: {config.Name} (共 {commands.Count} 个步骤)", "Operation");
                if (!string.IsNullOrEmpty(config.Description))
                    AppLogger.Info($"📝 描述: {config.Description}", "Operation");//描述步骤
            }
        }
        #endregion
        //加载配置文件
        private WorkflowConfig? LoadWorkflowFromJson(string filePath)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                var config = JsonSerializer.Deserialize<WorkflowConfig>(json);
                if (config == null || config.Commands == null || config.Commands.Count == 0)
                {
                    AppLogger.Warn($"⚠️ 配置文件 {Path.GetFileName(filePath)} 为空或格式错误", "Operation");
                    return null;
                }
                return config;
            }
            catch (Exception ex)
            {
                AppLogger.Error($"❌ 加载配置文件失败: {ex.Message}", "Operation");
                return null;
            }
        }

        // 处理“清空”请求
        private void OnClearRequested(object? sender, EventArgs e)
        {
            _view.ClearResult();
        }

        private void OnGetStatusRequested(object? sender, EventArgs e)
        {
            try
            {
                //const short AXIS = 1;   // 输入固定轴号
                short AXIS = _view.SelectedAxis;   // 从界面动态获取轴号
                if (AXIS < 1 || AXIS > 8) // 假设最多8轴
                {
                    AppLogger.Warn("❌ 请选择有效的轴号 (1~8)", "Operation");
                    return;
                }
                uint clk = 0;
                int status = 0;
                double pos = 0, vel = 0, acc = 0;
                int mode = 0;

                // 调用 Model 获取各项数据
                short rt = _model.GetAxisStatus(AXIS, out status, out clk);
                if (rt != 0)
                {
                    AppLogger.Error($"❌ 获取轴状态失败，错误码: {rt} (0x{rt:X})", "Operation");
                    return;
                }

                rt = _model.GetPrfPos(AXIS, out pos, out clk);
                if (rt != 0)
                    AppLogger.Warn($"⚠️ 获取规划位置失败，错误码: {rt} (0x{rt:X})", "Operation");

                rt = _model.GetPrfVel(AXIS, out vel, out clk);
                if (rt != 0)
                    AppLogger.Warn($"⚠️ 获取规划速度失败，错误码: {rt} (0x{rt:X})", "Operation");

                rt = _model.GetPrfAcc(AXIS, out acc, out clk);
                if (rt != 0)
                    AppLogger.Warn($"⚠️ 获取规划加速度失败，错误码: {rt} (0x{rt:X})", "Operation");

                rt = _model.GetPrfMode(AXIS, out mode, out clk);
                if (rt != 0)
                    AppLogger.Warn($"⚠️ 获取运动模式失败，错误码: {rt} (0x{rt:X})", "Operation");

                // 解析轴状态位（参照示例）
                string statusMsg = ParseAxisStatus(status);

                // 解析运动模式
                string modeMsg = mode switch
                {
                    0 => "Trap (梯形)",
                    1 => "Jog (点动)",
                    2 => "PT (位置时间)",
                    3 => "Gear (电子齿轮)",
                    4 => "Follow (跟随)",
                    5 => "Interpolation (插补)",
                    6 => "PVT",
                    _ => "未知模式"
                };

                // 组装显示信息
                string message =
                    $"========== 轴 {AXIS} 信息 ========== | " +
                    $"【轴状态】{statusMsg.Replace("\n", " ")} | " +
                    $"【运动模式】{modeMsg} | " +
                    $"【规划位置】{pos:F3} (单位) | " +
                    $"【规划速度】{vel:F3} (单位/秒) | " +
                    $"【规划加速度】{acc:F3} (单位/秒²)";
                AppLogger.Info(message, "Operation");
            }
            catch (Exception ex)
            {
                AppLogger.Error($"获取轴信息异常: {ex.Message}" ,"Operation");
            }
        }


        //关闭设备
        private void OnCloseDeviceRequested(object? sender, EventArgs e)
        {
            // 1. 停止所有后台任务（监控线程和工作流）
            StopMonitoring();
            _workflowCts?.Cancel();

            // 2. 释放硬件资源（真实模式下会调用 GT_Close）
            _model.CloseDevice();

            // 3. 清空日志并给用户反馈
            _view.ClearResult();
            AppLogger.Info("⏹ 设备已关闭，硬件资源已释放。", "Operation");

            // 4. 可选：更新 UI 状态（如按钮颜色、提示等），但当前无需额外操作
        }
        private string ParseAxisStatus(int status)
        {
            var sb = new StringBuilder();
            // 按位解析（与示例一致）
            if ((status & 0x2) != 0) sb.AppendLine("  - 伺服报警 (Alarm)");
            else sb.AppendLine("  - 伺服正常");
            if ((status & 0x10) != 0) sb.AppendLine("  - 跟随误差越限 (MError)");
            else sb.AppendLine("  - 跟随误差正常");
            if ((status & 0x20) != 0) sb.AppendLine("  - 正限位触发");
            else sb.AppendLine("  - 正限位未触发");
            if ((status & 0x40) != 0) sb.AppendLine("  - 负限位触发");
            else sb.AppendLine("  - 负限位未触发");
            if ((status & 0x80) != 0) sb.AppendLine("  - 平滑停止触发");
            else sb.AppendLine("  - 平滑停止未触发");
            if ((status & 0x100) != 0) sb.AppendLine("  - 急停触发");
            else sb.AppendLine("  - 急停未触发");
            if ((status & 0x200) != 0) sb.AppendLine("  - 伺服使能 (Servo On)");
            else sb.AppendLine("  - 伺服关闭 (Servo Off)");
            if ((status & 0x400) != 0) sb.AppendLine("  - 规划器正在运动");
            else sb.AppendLine("  - 规划器已停止");
            return sb.ToString();
        }

        private string GetErrorMessage(int errorCode)
        {
            return errorCode switch
            {
                0 => "成功",
                -1 => "参数错误",
                -2 => "函数不支持",
                -3 => "无效句柄",
                -4 => "资源未释放",
                -5 => "内存不足",
                -6 => "设备未初始化或未找到",
                _ => $"未知错误码 {errorCode}"
            };
        }
    }

}
