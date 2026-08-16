using GtsTest.Modbus;
using GtsTest.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GtsTest
{
    /// <summary>
    /// 设备配置（每个从机独立）
    /// </summary>
    public class DeviceConfig
    {
        public string DeviceId { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "设备1";
        public bool Enabled { get; set; } = true;
        public ModbusConfig Modbus { get; set; } = new ModbusConfig();
        public string WorkflowName { get; set; } = "";
        public short Axis { get; set; } = 1;
        public int TargetCount { get; set; } = 1000;
        public int CurrentCount { get; set; } = 0;
        public int SignalStartAddress { get; set; } = 100;
        //====================================================
        //                  工作流参数
        //====================================================
        public int WorkPosition { get; set; } = 1000;      // 工作目标位置
        public int HomePosition { get; set; } = 0;         // 回零位置
        public double MoveSpeed { get; set; } = 20.0;      // 运动速度
        public double MoveAcc { get; set; } = 10.0;        // 加速度
        public int CycleDelayMs { get; set; } = 500;       // 加工/检测延时（模拟）
    }

    /// <summary>
    /// 设备运行时状态
    /// </summary>
    public class DeviceRuntime
    {
        public DeviceConfig Config { get; set; }
        public ModbusClient ModbusClient { get; set; }
        public CancellationTokenSource WorkflowCts { get; set; }
        public Thread WorkflowThread { get; set; }
        public bool IsRunning { get; set; }
        public string CurrentStep { get; set; } = "空闲";
        public DateTime LastUpdate { get; set; }
        public object? LastModbusData { get; set; }
        public bool IsOnline { get; set; }
        public string LastError { get; set; } = "";
        public Watchdog Watchdog { get; set; } = new Watchdog(timeoutMs: 5000, checkIntervalMs: 200);
        public HashSet<int> TriggeredSignalCounts { get; set; } = new HashSet<int>();
    }

    /// <summary>
    /// 设备管理器：管理多台从机的并行控制与数据采集
    /// </summary>
    public class DeviceManager : IDisposable
    {
        private readonly Dictionary<string, DeviceRuntime> _devices = new Dictionary<string, DeviceRuntime>();
        private readonly object _lock = new object();
        private readonly GtsModel _model;
        private bool _disposed = false;
        private readonly IAlarmManager _alarmManager;

        public event Action<string, bool> OnDeviceOnlineChanged;
        public event Action<string, ushort[], object> OnDeviceDataUpdated;
        public event Action<string, string> OnDeviceStepChanged;
        public event Action<string, int, int> OnDeviceProductionUpdated;
        public IAlarmManager AlarmManager => _alarmManager;

        public DeviceManager(GtsModel model, IDataRepository repo = null)
        {
            _model = model;
            _alarmManager = new AlarmManager(repo ?? new SqliteRepository());
        }

        public GtsModel Model => _model;

        public bool AddDevice(DeviceConfig config)
        {
            lock (_lock)
            {
                if (_devices.ContainsKey(config.DeviceId))
                    return false;

                try
                {
                    var runtime = new DeviceRuntime
                    {
                        Config = config,
                        ModbusClient = new ModbusClient(config.Modbus)
                    };

                    runtime.ModbusClient.ConnectionStateChanged += (s, e) =>
                    {
                        runtime.IsOnline = e.IsConnected;
                        OnDeviceOnlineChanged?.Invoke(config.DeviceId, e.IsConnected);
                        if (!e.IsConnected)
                        {
                            runtime.LastError = e.ErrorMessage ?? "连接断开";
                        }
                    };

                    _devices[config.DeviceId] = runtime;
                    AppLogger.Info($"✅ 设备 [{config.Name}] 已添加", "DeviceManager");
                    return true;
                }
                catch (Exception ex)
                {
                    AppLogger.Error($"❌ 添加设备失败: {ex.Message}", "DeviceManager");
                    return false;
                }
            }
        }

        public bool RemoveDevice(string deviceId)
        {
            lock (_lock)
            {
                if (!_devices.ContainsKey(deviceId)) return false;
                var runtime = _devices[deviceId];
                StopDevice(deviceId);
                runtime.ModbusClient?.Disconnect();
                runtime.Watchdog?.Dispose();
                _devices.Remove(deviceId);
                AppLogger.Info($"✅ 设备 [{deviceId}] 已移除", "DeviceManager");
                return true;
            }
        }

        public bool StartDevice(string deviceId)
        {
            lock (_lock)
            {
                if (!_devices.TryGetValue(deviceId, out var runtime)) return false;
                if (runtime.IsRunning) return true;

                // 先尝试连接 Modbus，连接成功才启动循环
                if (!runtime.ModbusClient.Connect())
                {
                    AppLogger.Error($"❌ 设备 [{runtime.Config.Name}] Modbus 连接失败，无法启动", "DeviceManager");
                    return false;
                }

                runtime.WorkflowCts = new CancellationTokenSource();
                var token = runtime.WorkflowCts.Token;

                runtime.WorkflowThread = new Thread(() => DeviceLoop(runtime, token))
                {
                    Name = $"Device_{runtime.Config.Name}_Loop",
                    IsBackground = true
                };
                runtime.WorkflowThread.Start();
                runtime.IsRunning = true;
                AppLogger.Info($"▶️ 设备 [{runtime.Config.Name}] 已启动", "DeviceManager");
                return true;
            }
        }

        public bool StopDevice(string deviceId)
        {
            lock (_lock)
            {
                if (!_devices.TryGetValue(deviceId, out var runtime)) return false;
                if (!runtime.IsRunning) return true;

                runtime.WorkflowCts?.Cancel();
                runtime.WorkflowThread?.Join(500);
                runtime.IsRunning = false;
                runtime.Watchdog.Stop();
                AppLogger.Info($"⏹ 设备 [{runtime.Config.Name}] 已停止", "DeviceManager");
                return true;
            }
        }

        public void StartAllDevices()
        {
            foreach (var kv in _devices)
            {
                var runtime = kv.Value;
                if (runtime.IsOnline && !runtime.IsRunning)
                {
                    StartDevice(kv.Key);
                }
                else if (!runtime.IsOnline)
                {
                    AppLogger.Warn($"⚠️ 设备 [{runtime.Config.Name}] 未连接，跳过启动", "DeviceManager");
                }
            }
        }

        public void StopAllDevices()
        {
            foreach (var deviceId in _devices.Keys.ToList())
            {
                StopDevice(deviceId);
            }
        }

        public DeviceRuntime GetDevice(string deviceId)
        {
            _devices.TryGetValue(deviceId, out var runtime);
            return runtime;
        }

        public List<DeviceRuntime> GetAllDevices()
        {
            lock (_lock)
            {
                return _devices.Values.ToList();
            }
        }
        // ================================================================
        // 基于保持寄存器的信号读写（新增）
        // ================================================================

        /// <summary>
        /// 向目标设备的保持寄存器写入单个值 (功能码 0x06)
        /// </summary>
        public bool WriteRegisterSignal(string targetDeviceId, int address, ushort value)
        {
            var device = GetDevice(targetDeviceId);
            if (device == null || !device.IsOnline)
            {
                AppLogger.Warn($"⚠️ 设备 [{targetDeviceId}] 不在线，写入寄存器失败", "DeviceManager");
                return false;
            }

            try
            {
                return device.ModbusClient.WriteSingleRegister((ushort)address, value);
            }
            catch (Exception ex)
            {
                AppLogger.Error($"❌ 写寄存器异常: 设备={targetDeviceId}, 地址={address}, 错误={ex.Message}", "DeviceManager");
                return false;
            }
        }

        /// <summary>
        /// 向目标设备的保持寄存器写入多个值 (功能码 0x10)
        /// </summary>
        public bool WriteRegisterSignal(string targetDeviceId, int address, ushort[] values)
        {
            var device = GetDevice(targetDeviceId);
            if (device == null || !device.IsOnline)
            {
                AppLogger.Warn($"⚠️ 设备 [{targetDeviceId}] 不在线，写入寄存器失败", "DeviceManager");
                return false;
            }

            try
            {
                return device.ModbusClient.WriteMultipleRegisters((ushort)address, values);
            }
            catch (Exception ex)
            {
                AppLogger.Error($"❌ 写寄存器异常: 设备={targetDeviceId}, 地址={address}, 错误={ex.Message}", "DeviceManager");
                return false;
            }
        }

        /// <summary>
        /// 读取目标设备的保持寄存器原始值
        /// </summary>
        public ushort[]? ReadRegisterSignal(string targetDeviceId, int address, int count = 1)
        {
            var device = GetDevice(targetDeviceId);
            if (device == null || !device.IsOnline)
            {
                AppLogger.Warn($"⚠️ 设备 [{targetDeviceId}] 不在线，读取寄存器失败", "DeviceManager");
                return null;
            }

            try
            {
                var result = device.ModbusClient.ReadHoldingRegistersWithRaw((ushort)address, (ushort)count);
                return result?.RawRegisters;
            }
            catch (Exception ex)
            {
                AppLogger.Error($"❌ 读寄存器异常: 设备={targetDeviceId}, 地址={address}, 错误={ex.Message}", "DeviceManager");
                return null;
            }
        }

        /// <summary>
        /// 读取目标设备的保持寄存器并转换为指定类型
        /// </summary>
        public T? ReadRegisterSignal<T>(string targetDeviceId, int address, DataType dataType, ByteOrder byteOrder = ByteOrder.BigEndian)
        {
            int registerCount = GetRegisterCountForType(dataType);
            var raw = ReadRegisterSignal(targetDeviceId, address, registerCount);
            if (raw == null || raw.Length < registerCount) return default;

            try
            {
                // 将 ushort[] 编码为对象
                var encoded = ModbusClient.EncodeValue(raw, dataType, byteOrder);
                if (encoded == null) return default;

                // 如果是数组，取第一个元素
                if (encoded is Array arr && arr.Length > 0)
                    return (T)Convert.ChangeType(arr.GetValue(0), typeof(T));

                return (T)Convert.ChangeType(encoded, typeof(T));
            }
            catch (Exception ex)
            {
                AppLogger.Error($"❌ 转换寄存器数据失败: {ex.Message}", "DeviceManager");
                return default;
            }
        }

        private int GetRegisterCountForType(DataType type)
        {
            return type switch
            {
                DataType.Int16 or DataType.UInt16 => 1,
                DataType.Int32 or DataType.UInt32 or DataType.Float => 2,
                DataType.Double => 4,
                _ => 1
            };
        }
        public bool WriteSignal(string targetDeviceId, int address, bool value)
        {
            var device = GetDevice(targetDeviceId);
            if (device == null || !device.IsOnline) return false;
            return device.ModbusClient.WriteSingleCoil((ushort)address, value);
        }

        public bool? ReadSignal(string targetDeviceId, int address)
        {
            var device = GetDevice(targetDeviceId);
            if (device == null || !device.IsOnline) return null;
            var result = device.ModbusClient.ReadDataByType((ushort)address, 1);
            if (result == null || result.RawRegisters.Length == 0) return null;
            return (result.RawRegisters[0] & 0x0001) != 0;
        }

        public string ExportConfig()
        {
            lock (_lock)
            {
                var list = _devices.Values.Select(r => r.Config).ToList();
                var options = new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                return System.Text.Json.JsonSerializer.Serialize(list, options);
            }
        }

        public bool ImportConfig(string json)
        {
            try
            {
                var configs = System.Text.Json.JsonSerializer.Deserialize<List<DeviceConfig>>(json);
                if (configs == null) return false;

                StopAllDevices();
                lock (_lock)
                {
                    foreach (var kv in _devices.ToList())
                    {
                        kv.Value.ModbusClient?.Disconnect();
                        kv.Value.Watchdog?.Dispose();
                    }
                    _devices.Clear();
                }

                foreach (var config in configs)
                {
                    AddDevice(config);
                }
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.Error($"❌ 导入配置失败: {ex.Message}", "DeviceManager");
                return false;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            StopAllDevices();
            lock (_lock)
            {
                foreach (var runtime in _devices.Values)
                {
                    runtime.ModbusClient?.Dispose();
                    runtime.Watchdog?.Dispose();
                }
                _devices.Clear();
            }
            AppLogger.Info("🗑️ DeviceManager 已释放", "DeviceManager");
        }



        // ========== 核心循环 ==========
        private void DeviceLoop(DeviceRuntime runtime, CancellationToken ct)
        {
            var config = runtime.Config;
            var modbus = runtime.ModbusClient;
            var watchdog = runtime.Watchdog;

            if (!modbus.IsConnected)
            {
                AppLogger.Error($"❌ 设备 [{config.Name}] Modbus 未连接，循环退出", "DeviceManager");
                runtime.IsRunning = false;
                return;
            }

            // 设置看门狗超时回调
            watchdog.OnTimeout = () =>
            {
                AppLogger.Warn($"⚠️ 设备 [{config.Name}] 🐕看门狗超时！", "DeviceManager");
            };

            // 启动看门狗
            watchdog.Start();
            AppLogger.Info($"✅ 设备 [{config.Name}] 🐕看门狗已启动 (超时: {watchdog.RemainingMs}ms)", "DeviceManager");

            int consecutiveFailures = 0;
            const int maxFailures = 3;

            // ---- 工作流状态变量 ----
            int currentStep = 0;                     // 当前步骤 (0=空闲, 1=回零, 2=定位, 3=加工/检测, 4=等待信号)
            bool workCompleted = false;              // 当前循环是否完成
            bool signalSent = false;                 // 信号是否已发送

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    // ---- 1. 看门狗检查 ----
                    if (watchdog.IsTimeout)
                    {
                        AppLogger.Warn($"⚠️ 设备 [{config.Name}] 🐕看门狗已触发，尝试恢复...", "DeviceManager");
                        if (!modbus.IsConnected)
                        {
                            if (modbus.Reconnect(config.Modbus))
                            {
                                AppLogger.Info($"✅ 设备 [{config.Name}] 重连成功，重置🐕看门狗", "DeviceManager");
                                watchdog.Feed();
                            }
                            else
                            {
                                AppLogger.Error($"❌ 设备 [{config.Name}] 重连失败，停止循环", "DeviceManager");
                                break;
                            }
                        }
                        else
                        {
                            AppLogger.Warn($"⚠️ 设备 [{config.Name}] 连接正常但🐕看门狗超时，重置🐕看门狗", "DeviceManager");
                            watchdog.Feed();
                        }
                        continue;
                    }

                    // ---- 2. 连接状态检查 ----
                    if (!modbus.IsConnected)
                    {
                        AppLogger.Warn($"⚠️ 设备 [{config.Name}] Modbus 连接断开，尝试重连...", "DeviceManager");
                        if (!modbus.Reconnect(config.Modbus))
                        {
                            AppLogger.Error($"❌ 设备 [{config.Name}] 重连失败，循环退出", "DeviceManager");
                            break;
                        }
                        AppLogger.Info($"✅ 设备 [{config.Name}] 重连成功", "DeviceManager");
                        watchdog.Feed();
                        continue;
                    }

                    // ---- 3. 读取 Modbus 数据（保持连接活跃，并更新UI） ----
                    //var result = modbus.ReadDataByType(
                    //    config.Modbus.StartAddress,
                    //    config.Modbus.RegisterCount
                    //);

                    //if (result != null)
                    //{
                    //    consecutiveFailures = 0;
                    //    watchdog.Feed();
                    //    runtime.LastModbusData = result.ConvertedValue;
                    //    runtime.LastUpdate = DateTime.Now;
                    //    runtime.IsOnline = true;
                    //    OnDeviceDataUpdated?.Invoke(config.DeviceId, result.RawRegisters, result.ConvertedValue);
                    //}
                    //else
                    //{
                    //    consecutiveFailures++;
                    //    AppLogger.Warn($"⚠️ 设备 [{config.Name}] 读取失败 ({consecutiveFailures}/{maxFailures})", "DeviceManager");
                    //    if (consecutiveFailures >= maxFailures)
                    //    {
                    //        AppLogger.Error($"❌ 设备 [{config.Name}] 连续 {maxFailures} 次读取失败，循环退出", "DeviceManager");
                    //        break;
                    //    }
                    //    if (runtime.IsOnline)
                    //    {
                    //        runtime.IsOnline = false;
                    //        OnDeviceOnlineChanged?.Invoke(config.DeviceId, false);
                    //    }
                    //    Thread.Sleep(1000);
                    //    continue;
                    //}

                    // ---- 4. 执行工作流（根据设备ID执行不同流程） ----
                    switch (config.DeviceId)
                    {
                        case "dev-001":  // 设备1：焊接流程
                            ExecuteWorkflowDevice1(runtime, ref currentStep, ref workCompleted, ref signalSent, ct);
                            break;

                        case "dev-002":  // 设备2：检测流程
                            ExecuteWorkflowDevice2(runtime, ref currentStep, ref workCompleted, ref signalSent, ct);
                            break;

                        case "dev-003":  // 设备3：包装流程
                            ExecuteWorkflowDevice3(runtime, ref currentStep, ref workCompleted, ref signalSent, ct);
                            break;

                        default:
                            // 其他设备保持简单循环，只更新产量
                            if (currentStep % 10 == 0 && config.CurrentCount < config.TargetCount)
                            {
                                config.CurrentCount++;
                                OnDeviceProductionUpdated?.Invoke(config.DeviceId, config.CurrentCount, config.TargetCount);
                            }
                            break;
                    }

                    // ---- 5. 更新UI步骤 ----
                    string stepName = currentStep switch
                    {
                        0 => "空闲",
                        1 => "回零中",
                        2 => "定位中",
                        3 => "加工/检测中",
                        4 => "等待信号",
                        _ => $"步骤{currentStep}"
                    };
                    if (runtime.CurrentStep != stepName)
                    {
                        runtime.CurrentStep = stepName;
                        OnDeviceStepChanged?.Invoke(config.DeviceId, stepName);
                    }

                    Thread.Sleep(100);  // 循环周期100ms
                }
                catch (OperationCanceledException)
                {
                    AppLogger.Info($"⏹ 设备 [{config.Name}] 循环被取消", "DeviceManager");
                    break;
                }
                catch (Exception ex)
                {
                    runtime.LastError = ex.Message;
                    AppLogger.Error($"❌ 设备 [{config.Name}] 循环异常: {ex.Message}", "DeviceManager");
                    consecutiveFailures++;
                    if (consecutiveFailures >= maxFailures)
                    {
                        AppLogger.Error($"❌ 设备 [{config.Name}] 连续异常 {maxFailures} 次，循环退出", "DeviceManager");
                        break;
                    }
                    Thread.Sleep(1000);
                }
            }

            // ---- 清理资源 ----
            watchdog.Stop();
            runtime.IsRunning = false;
            runtime.IsOnline = false;
            modbus.Disconnect();
            OnDeviceOnlineChanged?.Invoke(config.DeviceId, false);
            AppLogger.Info($"⏹ 设备 [{config.Name}] 循环已退出", "DeviceManager");
        }

        // ================================================================
        // 各设备的工作流执行函数（辅助方法）
        // ================================================================

        /// <summary>
        /// 设备1 焊接工作流
        /// </summary>
        private void ExecuteWorkflowDevice1(DeviceRuntime runtime, ref int step, ref bool completed, ref bool sent, CancellationToken ct)
        {
            var config = runtime.Config;
            var model = _model;

            // 如果已经完成一个循环，重置状态（开启下一轮）
            if (completed)
            {
                completed = false;
                sent = false;
                step = 0;
                AppLogger.Debug($"🔄 设备1 新一轮开始", "DeviceManager");
            }

            switch (step)
            {
                case 0: // 空闲 → 开始工作
                    if (config.CurrentCount >= config.TargetCount)
                    {
                        AppLogger.Info($"✅ 设备1 目标产量达成，停止工作", "DeviceManager");
                        break;
                    }
                    // 检查确认信号（地址300）
                    bool? confirm = ReadSignal("dev-001", 300);
                    if (confirm != true)
                    {
                        // 没有确认信号，等待（首次启动时还没有确认，需要先发送一次信号？）
                        // 首次启动时，我们可以直接进入工作，但之后必须等待确认
                        // 这里添加一个首次启动标志：如果从未发送过信号（sent==false且completed==false），直接开始
                        if (!sent && !completed) // 首次启动
                        {
                            step = 1;
                            AppLogger.Info($"🔧 设备1 首次启动 (当前产量 {config.CurrentCount}/{config.TargetCount})", "DeviceManager");
                        }
                        else
                        {
                            AppLogger.Debug("⏳ 设备1 等待设备2确认信号...", "DeviceManager");
                            break; // 不进入下一步
                        }

                    }
                    // 收到确认信号，清除它
                    WriteSignal("dev-001", 300, false);
                    step = 1;
                    AppLogger.Info($"🔧 设备1 开始工作 (当前产量 {config.CurrentCount}/{config.TargetCount})", "DeviceManager");
                    break;
                case 1: // 回零
                    AppLogger.Info($"↩️ 设备1 回零 (轴{config.Axis})", "DeviceManager");
                    if (ExecuteHome(config.Axis, config.HomePosition, 5000))
                    {
                        step = 2;
                        AppLogger.Info($"✅ 设备1 回零完成", "DeviceManager");
                    }
                    else
                    {
                        AppLogger.Error($"❌ 设备1 回零失败，重置步骤", "DeviceManager");
                        step = 0;
                    }
                    break;

                case 2: // 定位到加工位
                    int targetPos = config.WorkPosition; // 例如 10000
                    AppLogger.Info($"🎯 设备1 定位到 {targetPos} (轴{config.Axis})", "DeviceManager");
                    if (ExecuteMoveAbs(config.Axis, targetPos, config.MoveSpeed, config.MoveAcc, 5000))
                    {
                        step = 3;
                        AppLogger.Info($"✅ 设备1 定位完成", "DeviceManager");
                    }
                    else
                    {
                        AppLogger.Error($"❌ 设备1 定位失败，重置步骤", "DeviceManager");
                        step = 0;
                    }
                    break;

                case 3: // 模拟加工（焊接）
                    AppLogger.Info($"🔥 设备1 焊接中... (延时 {config.CycleDelayMs}ms)", "DeviceManager");
                    // 模拟加工耗时（可替换为实际IO等待）
                    Thread.Sleep(config.CycleDelayMs);
                    // 加工完成，产量+1
                    if (config.CurrentCount < config.TargetCount)
                    {
                        config.CurrentCount++;
                        OnDeviceProductionUpdated?.Invoke(config.DeviceId, config.CurrentCount, config.TargetCount);
                        AppLogger.Info($"📈 设备1 产量 +1，当前 {config.CurrentCount}/{config.TargetCount}", "DeviceManager");
                    }
                    step = 4;
                    break;

                case 4: // 发送信号给设备2，并复位步骤
                    if (!sent)
                    {
                        if (WriteSignal("dev-002", 100, true))
                        {
                            AppLogger.Info("📡 设备1 → 设备2: 信号触发 (线圈100=ON)", "DeviceManager");
                            sent = true;
                            step = 5; // 进入等待确认状态
                        }
                        else
                        {
                            AppLogger.Warn("⚠️ 设备1 发送信号失败，重试", "DeviceManager");
                        }
                    }
                    break;

                case 5: // 等待设备2确认
                    bool? ack = ReadSignal("dev-001", 300);
                    if (ack == true)
                    {
                        // 收到确认，完成循环
                        //WriteSignal("dev-001", 300, false);
                        completed = true;
                        AppLogger.Info("✅ 设备1 工作循环完成（已收到确认）", "DeviceManager");
                        // 下次循环会重置 step=0
                    }
                    else
                    {
                        AppLogger.Debug("⏳ 设备1 等待确认信号...", "DeviceManager");
                    }
                    break;
            }
        }

        /// <summary>
        /// 设备2 检测工作流
        /// </summary>
        private void ExecuteWorkflowDevice2(DeviceRuntime runtime, ref int step, ref bool completed, ref bool sent, CancellationToken ct)
        {
            var config = runtime.Config;
            var model = _model;

            if (completed)
            {
                completed = false;
                sent = false;
                step = 0;
                AppLogger.Debug($"🔄 设备2 新一轮开始", "DeviceManager");
            }

            switch (step)
            {
                case 0: // 空闲，等待信号
                    bool? signal = ReadSignal("dev-002", 100);
                    if (signal == true)
                    {
                        AppLogger.Info("📨 设备2 收到信号，开始检测", "DeviceManager");
                        WriteSignal("dev-002", 100, false);
                        step = 1;
                    }
                    else
                    {
                        AppLogger.Debug("⏳ 设备2 等待信号...", "DeviceManager");
                    }
                    break;

                case 1: // 回零
                    AppLogger.Info($"↩️ 设备2 回零 (轴{config.Axis})", "DeviceManager");
                    if (ExecuteHome(config.Axis, config.HomePosition, 5000))
                    {
                        step = 2;
                        AppLogger.Info($"✅ 设备2 回零完成", "DeviceManager");
                    }
                    else
                    {
                        AppLogger.Error($"❌ 设备2 回零失败，重置步骤", "DeviceManager");
                        step = 0;
                    }
                    break;

                case 2: // 定位到检测位
                    int targetPos = config.WorkPosition; // 例如 2000
                    AppLogger.Info($"🎯 设备2 定位到 {targetPos} (轴{config.Axis})", "DeviceManager");
                    if (ExecuteMoveAbs(config.Axis, targetPos, config.MoveSpeed, config.MoveAcc, 5000))
                    {
                        step = 3;
                        AppLogger.Info($"✅ 设备2 定位完成", "DeviceManager");
                    }
                    else
                    {
                        AppLogger.Error($"❌ 设备2 定位失败，重置步骤", "DeviceManager");
                        step = 0;
                    }
                    break;

                case 3: // 模拟检测
                    AppLogger.Info($"🔍 设备2 检测中... (延时 {config.CycleDelayMs}ms)", "DeviceManager");
                    Thread.Sleep(config.CycleDelayMs);
                    // 假设检测通过
                    AppLogger.Info($"✅ 设备2 检测通过", "DeviceManager");
                    step = 4;
                    break;

                case 4: // 发送信号给设备3
                    if (!sent)
                    {
                        // 发送信号给设备3
                        bool success3 = WriteSignal("dev-003", 200, true);
                        // ⭐ 向设备1发送确认信号（写入设备1的寄存器）
                        bool success1 = WriteSignal("dev-001", 300, true);
                        if (success3 && success1)
                        {
                            AppLogger.Info("📡 设备2 → 设备3: 信号触发 (线圈200=ON)", "DeviceManager");
                            AppLogger.Info("📡 设备2 → 设备1: 确认信号 (线圈300=ON)", "DeviceManager");
                            sent = true;
                        }
                        else
                        {
                            AppLogger.Warn("⚠️ 设备2 发送信号失败，重试", "DeviceManager");
                        }
                    }
                    else
                    {
                        completed = true;
                        AppLogger.Info("✅ 设备2 工作循环完成", "DeviceManager");
                    }
                    break;
            }
        }

        /// <summary>
        /// 设备3 包装工作流
        /// </summary>
        private void ExecuteWorkflowDevice3(DeviceRuntime runtime, ref int step, ref bool completed, ref bool sent, CancellationToken ct)
        {
            var config = runtime.Config;
            var model = _model;

            if (completed)
            {
                completed = false;
                sent = false;
                step = 0;
                AppLogger.Debug($"🔄 设备3 新一轮开始", "DeviceManager");
            }

            switch (step)
            {
                case 0: // 空闲，等待信号
                    bool? signal = ReadSignal("dev-003", 200);
                    if (signal == true)
                    {
                        AppLogger.Info("📨 设备3 收到信号，开始包装", "DeviceManager");
                        // 清除信号（复位线圈）
                        WriteSignal("dev-003", 200, false);
                        step = 1;
                    }
                    else
                    {
                        AppLogger.Debug("⏳ 设备3 等待信号...", "DeviceManager");
                    }
                    break;

                case 1: // 模拟包装
                    AppLogger.Info($"📦 设备3 包装中... (延时 {config.CycleDelayMs}ms)", "DeviceManager");
                    Thread.Sleep(config.CycleDelayMs);
                    AppLogger.Info($"✅ 设备3 包装完成", "DeviceManager");
                    // 包装完成，产量+1（可选）
                    if (config.CurrentCount < config.TargetCount)
                    {
                        config.CurrentCount++;
                        OnDeviceProductionUpdated?.Invoke(config.DeviceId, config.CurrentCount, config.TargetCount);
                        AppLogger.Info($"📈 设备3 产量 +1，当前 {config.CurrentCount}/{config.TargetCount}", "DeviceManager");
                    }
                    completed = true;
                    break;
            }
        }

        // ================================================================
        // 运动控制辅助方法（带超时等待）
        // ================================================================

        /// <summary>
        /// 执行回零，并等待完成（超时 timeoutMs）
        /// </summary>
        private bool ExecuteHome(short axis, int homePos, int timeoutMs)
        {
            short result = _model.HomeAxis(axis, homePos);
            if (result != 0)
            {
                AppLogger.Error($"回零启动失败，错误码: {result}", "DeviceManager");
                return false;
            }

            // 等待回零完成
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                if (_model.CheckHomeDone(axis))
                    return true;
                Thread.Sleep(20);
            }
            return false;
        }

        /// <summary>
        /// 执行绝对定位，并等待到达目标位置（超时 timeoutMs）
        /// </summary>
        private bool ExecuteMoveAbs(short axis, int targetPos, double vel, double acc, int timeoutMs)
        {
            short result = _model.MoveAbs(axis, targetPos, vel, acc);
            if (result != 0)
            {
                AppLogger.Error($"定位启动失败，错误码: {result}", "DeviceManager");
                return false;
            }

            // 等待到达目标位置（允许±5误差）
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                uint clk;
                double pos;
                _model.GetPrfPos(axis, out pos, out clk);
                if (Math.Abs(pos - targetPos) < 5)
                    return true;
                Thread.Sleep(20);
            }
            return false;
        }

    }
}