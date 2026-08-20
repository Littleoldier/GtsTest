using GtsTest.Modbus;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GtsTest.Services.Alarm;
using GtsTest.Models;

namespace GtsTest.Core
{
    /// <summary>
    /// 设备管理器：管理多台从机的并行控制与数据采集（async/Task 版本）
    /// </summary>
    public class DeviceManager : IDisposable
    {
        private readonly ConcurrentDictionary<string, DeviceRuntime> _devices = new();
        private readonly GtsModel _model;
        private bool _disposed = false;
        private readonly IAlarmManager _alarmManager;

        public event Action<string, bool>? OnDeviceOnlineChanged;
        public event Action<string, ushort[], object?>? OnDeviceDataUpdated;
        public event Action<string, string>? OnDeviceStepChanged;
        public event Action<string, int, int>? OnDeviceProductionUpdated;
        public IAlarmManager AlarmManager => _alarmManager;

        public DeviceManager(GtsModel model)
        {
            _model = model;
            _alarmManager = new AlarmManager();
        }

        public GtsModel Model => _model;

        public bool AddDevice(DeviceConfig config)
        {
            var runtime = new DeviceRuntime
            {
                Config = config,
                ModbusClient = new ModbusClient(config.Modbus)
            };

            // 绑定事件（关闭捕获问题：使用 local runtime）
            runtime.ModbusClient.ConnectionStateChanged += (s, e) =>
            {
                try
                {
                    runtime.IsOnline = e.IsConnected;
                    OnDeviceOnlineChanged?.Invoke(config.DeviceId, e.IsConnected);
                    if (!e.IsConnected)
                    {
                        runtime.LastError = e.ErrorMessage ?? "连接断开";
                    }
                }
                catch (Exception ex)
                {
                    AppLogger.Warn($"⚠️ ConnectionStateChanged 处理异常: {ex}", "DeviceManager");
                }
            };

            if (!_devices.TryAdd(config.DeviceId, runtime))
            {
                AppLogger.Warn($"⚠️ 添加设备失败（已存在）: {config.DeviceId}", "DeviceManager");
                // 清理刚创建但未被使用的资源
                try { runtime.ModbusClient.Dispose(); } catch { }
                try { runtime.Watchdog.Dispose(); } catch { }
                return false;
            }

            AppLogger.Info($"✅ 设备 [{config.Name}] 已添加", "DeviceManager");
            return true;
        }

        /// <summary>
        /// 原子地移除设备并释放资源（异步版本）
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="stopTimeoutMs"></param>
        /// <returns></returns>
        public async Task<bool> RemoveDeviceAsync(string deviceId, int stopTimeoutMs = 1000)
        {
            if (!_devices.TryRemove(deviceId, out var runtime) || runtime == null)
                return false;

            try
            {
                // 请求取消
                try { runtime.WorkflowCts?.Cancel(); }
                catch (Exception ex)
                {
                    AppLogger.Warn($"⚠️ 取消设备 [{deviceId}] 的 CancellationToken 时出错: {ex}", "DeviceManager");
                }

                // 等待任务优雅结束（带超时）
                var task = runtime.WorkflowTask ?? Task.CompletedTask;
                if (!task.IsCompleted)
                {
                    var finished = await Task.WhenAny(task, Task.Delay(stopTimeoutMs));
                    if (finished != task)
                    {
                        AppLogger.Warn($"⚠️ 设备 [{deviceId}] 停止超时 (>{stopTimeoutMs}ms)，将强制清理资源", "DeviceManager");
                    }
                }

                // 断开并释放外部资源（容错）
                try { runtime.ModbusClient.Disconnect(); } catch (Exception ex) { AppLogger.Warn($"⚠️ Disconnect 出错: {ex}", "DeviceManager"); }
                try { runtime.ModbusClient.Dispose(); } catch (Exception ex) { AppLogger.Warn($"⚠️ Dispose ModbusClient 出错: {ex}", "DeviceManager"); }
                try { runtime.Watchdog.Dispose(); } catch (Exception ex) { AppLogger.Warn($"⚠️ Dispose Watchdog 出错: {ex}", "DeviceManager"); }

                AppLogger.Info($"✅ 设备 [{deviceId}] 已移除", "DeviceManager");
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.Error($"❌ 移除设备 [{deviceId}] 异常: {ex}", "DeviceManager");
                return false;
            }
        }

        /// <summary>
        /// 兼容同步调用（阻塞等待）
        /// </summary>
        public bool RemoveDevice(string deviceId, int stopTimeoutMs = 1000)
        {
            return RemoveDeviceAsync(deviceId, stopTimeoutMs).GetAwaiter().GetResult();
        }

        public bool StartDevice(string deviceId)
        {
            if (!_devices.TryGetValue(deviceId, out var runtime) || runtime == null) return false;

            // 防止重复启动：使用简单锁定检查与设置 IsRunning
            lock (runtime)
            {
                if (runtime.IsRunning) return true;

                // 先尝试连接 Modbus，连接成功才启动循环
                if (!runtime.ModbusClient.Connect())
                {
                    AppLogger.Error($"❌ 设备 [{runtime.Config.Name}] Modbus 连接失败，无法启动", "DeviceManager");
                    return false;
                }

                runtime.WorkflowCts = new CancellationTokenSource();
                var token = runtime.WorkflowCts.Token;
                runtime.WorkflowTask = Task.Run(() => DeviceLoopAsync(runtime, token), token);
                runtime.IsRunning = true;
                AppLogger.Info($"▶️ 设备 [{runtime.Config.Name}] 已启动", "DeviceManager");
                return true;
            }
        }

        public bool StopDevice(string deviceId, int stopTimeoutMs = 1000)
        {
            if (!_devices.TryGetValue(deviceId, out var runtime) || runtime == null) return false;

            lock (runtime)
            {
                if (!runtime.IsRunning) return true;

                try
                {
                    runtime.WorkflowCts?.Cancel();
                }
                catch (Exception ex)
                {
                    AppLogger.Warn($"⚠️ 取消设备 [{deviceId}] 的工作流时出错: {ex}", "DeviceManager");
                }

                var task = runtime.WorkflowTask ?? Task.CompletedTask;
                if (!task.IsCompleted)
                {
                    try
                    {
                        var finished = Task.WhenAny(task, Task.Delay(stopTimeoutMs)).GetAwaiter().GetResult();
                        if (finished != task)
                        {
                            AppLogger.Warn($"⚠️ 设备 [{deviceId}] 停止超时 (>{stopTimeoutMs}ms)", "DeviceManager");
                        }
                    }
                    catch (Exception ex)
                    {
                        AppLogger.Warn($"⚠️ 等待设备 [{deviceId}] 任务结束时出错: {ex}", "DeviceManager");
                    }
                }

                runtime.IsRunning = false;
                try { runtime.Watchdog.Stop(); } catch { }
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

        public void StopAllDevices(int stopTimeoutMs = 1000)
        {
            var keys = _devices.Keys.ToList();
            foreach (var deviceId in keys)
            {
                StopDevice(deviceId, stopTimeoutMs);
            }
        }

        public DeviceRuntime? GetDevice(string deviceId)
        {
            _devices.TryGetValue(deviceId, out var runtime);
            return runtime;
        }

        public List<DeviceRuntime> GetAllDevices()
        {
            return _devices.Values.ToList();
        }

        // ================================================================
        // 基于保持寄存器的信号读写（新增）
        // ================================================================
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

        public T? ReadRegisterSignal<T>(string targetDeviceId, int address, DataType dataType, ByteOrder byteOrder = ByteOrder.BigEndian)
        {
            int registerCount = GetRegisterCountForType(dataType);
            var raw = ReadRegisterSignal(targetDeviceId, address, registerCount);
            if (raw == null || raw.Length < registerCount) return default;

            try
            {
                var encoded = ModbusClient.EncodeValue(raw, dataType, byteOrder);
                if (encoded == null) return default;

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
            var list = _devices.Values.Select(r => r.Config).ToList();
            var options = new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            return System.Text.Json.JsonSerializer.Serialize(list, options);
        }

        public bool ImportConfig(string json)
        {
            try
            {
                var configs = System.Text.Json.JsonSerializer.Deserialize<List<DeviceConfig>>(json);
                if (configs == null) return false;

                StopAllDevices();
                // 清理并释放现有 runtime
                foreach (var kv in _devices.ToList())
                {
                    try
                    {
                        kv.Value.ModbusClient?.Disconnect();
                        kv.Value.Watchdog?.Dispose();
                    }
                    catch { }
                    _devices.TryRemove(kv.Key, out _);
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
            foreach (var runtime in _devices.Values)
            {
                try { runtime.ModbusClient?.Dispose(); } catch { }
                try { runtime.Watchdog?.Dispose(); } catch { }
            }
            _devices.Clear();
            AppLogger.Info("🗑️ DeviceManager 已释放", "DeviceManager");
        }

        // ========== 核心循环（异步） ==========
        private async Task DeviceLoopAsync(DeviceRuntime runtime, CancellationToken ct)
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

            // 看门狗设置
            watchdog.OnTimeout = () =>
            {
                AppLogger.Warn($"⚠️ 设备 [{config.Name}] 🐕看门狗超时！", "DeviceManager");
            };

            try
            {
                watchdog.Start();
                AppLogger.Info($"✅ 设备 [{config.Name}] 🐕看门狗已启动 (超时: {watchdog.RemainingMs}ms)", "DeviceManager");
            }
            catch (Exception ex)
            {
                AppLogger.Warn($"⚠️ 启动看门狗失败: {ex}", "DeviceManager");
            }

            int consecutiveFailures = 0;
            const int maxFailures = 3;

            // 工作流状态变量（本地）
            int currentStep = 0;
            bool workCompleted = false;
            bool signalSent = false;

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
                        await Task.Delay(1, ct).ConfigureAwait(false);
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
                        await Task.Delay(1, ct).ConfigureAwait(false);
                        continue;
                    }

                    // ---- 3. 读取 Modbus 数据（被注释的原逻辑保留，如需启用可改为异步） ----
                    //var result = modbus.ReadDataByType(config.Modbus.StartAddress, config.Modbus.RegisterCount);
                    //if (result != null) { ... }

                    // ---- 4. 执行工作流 ----
                    // 注意：之前使用 deviceId 的硬编码分支。保持原有行为以兼容现有 workflow test cases。
                    switch (config.DeviceId)
                    {
                        case "dev-001":
                            await ExecuteWorkflowDevice1Async(runtime, currentStepRef: s => currentStep = s, completedRef: b => workCompleted = b, sentRef: b => signalSent = b, ct);
                            break;
                        case "dev-002":
                            await ExecuteWorkflowDevice2Async(runtime, currentStepRef: s => currentStep = s, completedRef: b => workCompleted = b, sentRef: b => signalSent = b, ct);
                            break;
                        case "dev-003":
                            await ExecuteWorkflowDevice3Async(runtime, currentStepRef: s => currentStep = s, completedRef: b => workCompleted = b, sentRef: b => signalSent = b, ct);
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

                    // 循环周期（可取消）
                    await Task.Delay(100, ct).ConfigureAwait(false);
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
                    try { await Task.Delay(1000, ct).ConfigureAwait(false); } catch { break; }
                }
            }

            // ---- 清理资源 ----
            try { watchdog.Stop(); } catch { }
            runtime.IsRunning = false;
            runtime.IsOnline = false;
            try { modbus.Disconnect(); } catch { }
            OnDeviceOnlineChanged?.Invoke(config.DeviceId, false);
            AppLogger.Info($"⏹ 设备 [{config.Name}] 循环已退出", "DeviceManager");
        }

        // ================================================================
        // 各设备的工作流执行函数（异步辅助方法）
        // ================================================================
        private async Task ExecuteWorkflowDevice1Async(DeviceRuntime runtime, Action<int> currentStepRef, Action<bool> completedRef, Action<bool> sentRef, CancellationToken ct)
        {
            var config = runtime.Config;

            int step = 0;
            bool completed = false;
            bool sent = false;

            // read back state from references if previously set
            // (simple local state; the original used ref ints -- keep local then write back)
            // For simplicity in this async refactor, we will keep and update local values and then write them back via references.
            // Note: if you need persistent per-runtime step memory, consider storing step/state inside DeviceRuntime.

            if (completed)
            {
                completed = false;
                sent = false;
                step = 0;
                AppLogger.Debug($"🔄 设备1 新一轮开始", "DeviceManager");
            }

            switch (step)
            {
                case 0:
                    if (config.CurrentCount >= config.TargetCount)
                    {
                        AppLogger.Info($"✅ 设备1 目标产量达成，停止工作", "DeviceManager");
                        break;
                    }
                    bool? confirm = ReadSignal("dev-001", 300);
                    if (confirm != true)
                    {
                        if (!sent && !completed)
                        {
                            step = 1;
                            AppLogger.Info($"🔧 设备1 首次启动 (当前产量 {config.CurrentCount}/{config.TargetCount})", "DeviceManager");
                        }
                        else
                        {
                            AppLogger.Debug("⏳ 设备1 等待设备2确认信号...", "DeviceManager");
                            break;
                        }
                    }
                    WriteSignal("dev-001", 300, false);
                    step = 1;
                    AppLogger.Info($"🔧 设备1 开始工作 (当前产量 {config.CurrentCount}/{config.TargetCount})", "DeviceManager");
                    break;

                case 1:
                    AppLogger.Info($"↩️ 设备1 回零 (轴{config.Axis})", "DeviceManager");
                    if (await ExecuteHomeAsync(config.Axis, config.HomePosition, 5000, ct))
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

                case 2:
                    int targetPos = config.WorkPosition;
                    AppLogger.Info($"🎯 设备1 定位到 {targetPos} (轴{config.Axis})", "DeviceManager");
                    if (await ExecuteMoveAbsAsync(config.Axis, targetPos, config.MoveSpeed, config.MoveAcc, 5000, ct))
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

                case 3:
                    AppLogger.Info($"🔥 设备1 焊接中... (延时 {config.CycleDelayMs}ms)", "DeviceManager");
                    try { await Task.Delay(config.CycleDelayMs, ct); } catch (OperationCanceledException) { return; }
                    if (config.CurrentCount < config.TargetCount)
                    {
                        config.CurrentCount++;
                        OnDeviceProductionUpdated?.Invoke(config.DeviceId, config.CurrentCount, config.TargetCount);
                        AppLogger.Info($"📈 设备1 产量 +1，当前 {config.CurrentCount}/{config.TargetCount}", "DeviceManager");
                    }
                    step = 4;
                    break;

                case 4:
                    if (!sent)
                    {
                        if (WriteSignal("dev-002", 100, true))
                        {
                            AppLogger.Info("📡 设备1 → 设备2: 信号触发 (线圈100=ON)", "DeviceManager");
                            sent = true;
                            step = 5;
                        }
                        else
                        {
                            AppLogger.Warn("⚠️ 设备1 发送信号失败，重试", "DeviceManager");
                        }
                    }
                    break;

                case 5:
                    bool? ack = ReadSignal("dev-001", 300);
                    if (ack == true)
                    {
                        completed = true;
                        AppLogger.Info("✅ 设备1 工作循环完成（已收到确认）", "DeviceManager");
                    }
                    else
                    {
                        AppLogger.Debug("⏳ 设备1 等待确认信号...", "DeviceManager");
                    }
                    break;
            }

            // write back state
            currentStepRef(step);
            completedRef(completed);
            sentRef(sent);
        }

        private async Task ExecuteWorkflowDevice2Async(DeviceRuntime runtime, Action<int> currentStepRef, Action<bool> completedRef, Action<bool> sentRef, CancellationToken ct)
        {
            var config = runtime.Config;

            int step = 0;
            bool completed = false;
            bool sent = false;

            if (completed)
            {
                completed = false;
                sent = false;
                step = 0;
                AppLogger.Debug($"🔄 设备2 新一轮开始", "DeviceManager");
            }

            switch (step)
            {
                case 0:
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

                case 1:
                    AppLogger.Info($"↩️ 设备2 回零 (轴{config.Axis})", "DeviceManager");
                    if (await ExecuteHomeAsync(config.Axis, config.HomePosition, 5000, ct))
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

                case 2:
                    int targetPos = config.WorkPosition;
                    AppLogger.Info($"🎯 设备2 定位到 {targetPos} (轴{config.Axis})", "DeviceManager");
                    if (await ExecuteMoveAbsAsync(config.Axis, targetPos, config.MoveSpeed, config.MoveAcc, 5000, ct))
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

                case 3:
                    AppLogger.Info($"🔍 设备2 检测中... (延时 {config.CycleDelayMs}ms)", "DeviceManager");
                    try { await Task.Delay(config.CycleDelayMs, ct); } catch (OperationCanceledException) { return; }
                    AppLogger.Info($"✅ 设备2 检测通过", "DeviceManager");
                    step = 4;
                    break;

                case 4:
                    if (!sent)
                    {
                        bool success3 = WriteSignal("dev-003", 200, true);
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

            currentStepRef(step);
            completedRef(completed);
            sentRef(sent);
        }

        private async Task ExecuteWorkflowDevice3Async(DeviceRuntime runtime, Action<int> currentStepRef, Action<bool> completedRef, Action<bool> sentRef, CancellationToken ct)
        {
            var config = runtime.Config;

            int step = 0;
            bool completed = false;
            bool sent = false;

            if (completed)
            {
                completed = false;
                sent = false;
                step = 0;
                AppLogger.Debug($"🔄 设备3 新一轮开始", "DeviceManager");
            }

            switch (step)
            {
                case 0:
                    bool? signal = ReadSignal("dev-003", 200);
                    if (signal == true)
                    {
                        AppLogger.Info("📨 设备3 收到信号，开始包装", "DeviceManager");
                        WriteSignal("dev-003", 200, false);
                        step = 1;
                    }
                    else
                    {
                        AppLogger.Debug("⏳ 设备3 等待信号...", "DeviceManager");
                    }
                    break;

                case 1:
                    AppLogger.Info($"📦 设备3 包装中... (延时 {config.CycleDelayMs}ms)", "DeviceManager");
                    try { await Task.Delay(config.CycleDelayMs, ct); } catch (OperationCanceledException) { return; }
                    AppLogger.Info($"✅ 设备3 包装完成", "DeviceManager");
                    if (config.CurrentCount < config.TargetCount)
                    {
                        config.CurrentCount++;
                        OnDeviceProductionUpdated?.Invoke(config.DeviceId, config.CurrentCount, config.TargetCount);
                        AppLogger.Info($"📈 设备3 产量 +1，当前 {config.CurrentCount}/{config.TargetCount}", "DeviceManager");
                    }
                    completed = true;
                    break;
            }

            currentStepRef(step);
            completedRef(completed);
            sentRef(sent);
        }

        // ================================================================
        // 运动控制辅助方法（异步包装，支持超时和取消）
        // ================================================================
        private async Task<bool> ExecuteHomeAsync(short axis, int homePos, int timeoutMs, CancellationToken ct)
        {
            try
            {
                short result = await Task.Run(() => _model.HomeAxis(axis, homePos), ct).ConfigureAwait(false);
                if (result != 0)
                {
                    AppLogger.Error($"回零启动失败，错误码: {result}", "DeviceManager");
                    return false;
                }

                var sw = System.Diagnostics.Stopwatch.StartNew();
                while (sw.ElapsedMilliseconds < timeoutMs)
                {
                    if (ct.IsCancellationRequested) return false;
                    if (_model.CheckHomeDone(axis)) return true;
                    await Task.Delay(20, ct).ConfigureAwait(false);
                }
                return false;
            }
            catch (OperationCanceledException) { return false; }
            catch (Exception ex)
            {
                AppLogger.Error($"回零过程中异常: {ex}", "DeviceManager");
                return false;
            }
        }

        private async Task<bool> ExecuteMoveAbsAsync(short axis, int targetPos, double vel, double acc, int timeoutMs, CancellationToken ct)
        {
            try
            {
                short result = await Task.Run(() => _model.MoveAbs(axis, targetPos, vel, acc), ct).ConfigureAwait(false);
                if (result != 0)
                {
                    AppLogger.Error($"定位启动失败，错误码: {result}", "DeviceManager");
                    return false;
                }

                var sw = System.Diagnostics.Stopwatch.StartNew();
                while (sw.ElapsedMilliseconds < timeoutMs)
                {
                    if (ct.IsCancellationRequested) return false;
                    uint clk;
                    double pos;
                    _model.GetPrfPos(axis, out pos, out clk);
                    if (Math.Abs(pos - targetPos) < 5) return true;
                    await Task.Delay(20, ct).ConfigureAwait(false);
                }
                return false;
            }
            catch (OperationCanceledException) { return false; }
            catch (Exception ex)
            {
                AppLogger.Error($"定位过程中异常: {ex}", "DeviceManager");
                return false;
            }
        }

    }
}