using gts;
using System;
using System.Text;
using System.IO;
using System.Text.Json;
using GtsTest.Commands;   // 引用新建的 Commands 文件夹里的所有指令


namespace GtsTest
{
    public class SimulationModeChangedEventArgs : EventArgs
    {
        public bool IsSimulationMode { get; }
        public SimulationModeChangedEventArgs(bool isSimulation) => IsSimulationMode = isSimulation;
    }
    public class GtsController
    {
        private readonly GtsModel _model;
        private readonly Form1 _view;

        private Thread? _pollingThread;
        private CancellationTokenSource? _cts;
        private readonly object _lock = new object();

        // ========== 工作流相关字段  ==========
        private CancellationTokenSource? _workflowCts;     // 用于取消正在运行的工作流
        private readonly object _workflowLock = new object(); // 防止用户狂点“启动”按钮导致线程爆炸

        public event EventHandler<SimulationModeChangedEventArgs>? SimulationModeChanged;   //切换模式事件

        public GtsController(GtsModel model, Form1 view)
        {
            _model = model;
            _view = view;

            // 订阅 View 的事件
            _view.OpenRequested += OnOpenRequested;
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
                    _view.ShowResult($"❌ 打开设备失败\n错误码: {openResult} (0x{openResult:X})" + Environment.NewLine + GetErrorMessage(openResult));
                    return; // 打开失败，直接退出，不再执行复位
                }

                // ========== 第二步：复位/使能（仅在打开成功后执行） ==========
                short resetResult = _model.GT_Reset();
                if (resetResult == 0)
                {
                    _view.ShowResult($"✅ 初始化成功\n打开返回值: 0\n复位返回值: 0 (成功)");
                }
                else
                {
                    // 注意：虽然复位失败，但设备其实已经打开了，所以显示警告而非纯粹的错误
                    _view.ShowResult($"⚠️ 设备已打开，但复位失败\n打开返回值: 0\n复位错误码: {resetResult} (0x{resetResult:X})" + Environment.NewLine + GetErrorMessage(resetResult));
                    return;
                }

            }
            catch (Exception ex)
            {
                _view.ShowError($"调用异常: {ex.Message}", "Operation");
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
                    _view.ShowResult("⚠️ 监控线程已在运行中");
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
                _view.ShowResult($"✅ 实时监控已启动 (周期: {intervalMs}ms)");
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
                _view.ShowResult("⏹ 实时监控已停止");
            }
            // 停止监控时，顺便把正在运行的工作流也干掉
            _workflowCts?.Cancel();
        }

        /// <summary>
        /// 线程执行体：循环读取轴状态
        /// </summary>
        private void PollingLoop(int interval, CancellationToken ct)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            while (!ct.IsCancellationRequested)
            {
                // 1. 计算本周期应该结束的时间点
                long nextStart = stopwatch.ElapsedMilliseconds + interval;

                // 2. 执行数据读取（仅调用 Model，不操作 UI）
                // 注意：因为要读界面的轴号，这里用 Invoke 拿一下值（为了线程安全）
                short axis = 1;
                _view.Invoke(new Action(() => { axis = _view.SelectedAxis; }));

                // 获取各项数据（调用你的 GtsModel）
                int status = 0;
                uint clk = 0;
                double pos = 0, vel = 0, acc = 0;
                int mode = 0;

                _model.GetAxisStatus(axis, out status, out clk);
                _model.GetPrfPos(axis, out pos, out clk);
                _model.GetPrfVel(axis, out vel, out clk);
                _model.GetPrfAcc(axis, out acc, out clk);
                _model.GetPrfMode(axis, out mode, out clk);

                // 3. 跨线程更新 UI（使用 BeginInvoke，不阻塞工作线程）
                _view.BeginInvoke(new Action(() =>
                {
                    // 这里只做显示，不写复杂逻辑
                    _view.AppendMonitorLog($"轴[{axis}] 实时 -> 位置:{pos:F2} | 速度:{vel:F2} | 状态码:0x{status:X}");
                }));

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
                        _view.ShowResult("⚠️ JSON 解析失败，将自动切换至默认硬编码流程");
                    }
                }
                else
                {
                    _view.ShowResult($"⚠️ 未找到文件 {selected}.json，将自动切换至默认硬编码流程");
                }
            }
            else
            {
                _view.ShowResult("⚠️ 未选择工作流，将自动切换至默认硬编码流程");
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
                workflow.OnLog += msg => _view.BeginInvoke(new Action(() => _view.ShowResult(msg)));

                var thread = new Thread(() => workflow.Execute(token))
                {
                    Name = "DefaultWorkflow_Thread",
                    IsBackground = true
                };
                thread.Start();

                _view.ShowResult("🚀 已启动【默认硬编码工作流】(回零->定位10000->等待IO->定位5000->延时)");
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
            _view.ShowResult(modeStatus);
            _view.ShowResult("⚠️ 请点击【初始化】重新建立连接以生效");

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
                _view.ShowResult("❌ 无效的工作流配置");
                return;
            }

            lock (_workflowLock)
            {
                _workflowCts?.Cancel();
                _workflowCts = new CancellationTokenSource();
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
                workflow.OnLog += msg => _view.BeginInvoke(new Action(() => _view.ShowResult(msg)));

                var thread = new Thread(() => workflow.Execute(token))
                {
                    Name = $"Workflow_{config.Name}",
                    IsBackground = true
                };
                thread.Start();

                _view.ShowResult($"🚀 已启动工作流: {config.Name} (共 {commands.Count} 个步骤)");
                if (!string.IsNullOrEmpty(config.Description))
                    _view.ShowResult($"📝 描述: {config.Description}");
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
                    _view.ShowResult($"⚠️ 配置文件 {Path.GetFileName(filePath)} 为空或格式错误");
                    return null;
                }
                return config;
            }
            catch (Exception ex)
            {
                _view.ShowResult($"❌ 加载配置文件失败: {ex.Message}");
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
                    _view.ShowResult("❌ 请选择有效的轴号 (1~8)");
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
                    _view.ShowResult($"❌ 获取轴状态失败，错误码: {rt} (0x{rt:X})");
                    return;
                }

                rt = _model.GetPrfPos(AXIS, out pos, out clk);
                if (rt != 0)
                    _view.ShowResult($"⚠️ 获取规划位置失败，错误码: {rt} (0x{rt:X})");

                rt = _model.GetPrfVel(AXIS, out vel, out clk);
                if (rt != 0)
                    _view.ShowResult($"⚠️ 获取规划速度失败，错误码: {rt} (0x{rt:X})");

                rt = _model.GetPrfAcc(AXIS, out acc, out clk);
                if (rt != 0)
                    _view.ShowResult($"⚠️ 获取规划加速度失败，错误码: {rt} (0x{rt:X})");

                rt = _model.GetPrfMode(AXIS, out mode, out clk);
                if (rt != 0)
                    _view.ShowResult($"⚠️ 获取运动模式失败，错误码: {rt} (0x{rt:X})");

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
                    $"========== 轴 {AXIS} 信息 =========={Environment.NewLine}" +
                    $"【轴状态】{statusMsg}{Environment.NewLine}" +
                    $"【运动模式】{modeMsg}{Environment.NewLine}" +
                    $"【规划位置】{pos:F3} (单位){Environment.NewLine}" +
                    $"【规划速度】{vel:F3} (单位/秒){Environment.NewLine}" +
                    $"【规划加速度】{acc:F3} (单位/秒²){Environment.NewLine}";

                _view.ShowResult(message);
            }
            catch (Exception ex)
            {
                _view.ShowError($"获取轴信息异常: {ex.Message}" ,"Operation");
            }
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
