using GtsTest.Commands;
using GtsTest.Modbus;
using GtsTest.Models;
using GtsTest.Services.Alarm;
using GtsTest.Services.Authentication;
using GtsTest.Services.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GtsTest.Core
{
    /// <summary>
    /// 设备管理器：管理多台设备的并行控制与数据采集
    /// 支持设备与工作流（配方）完全解耦，运行时动态指定工作流
    /// </summary>
    public class DeviceManager : IDisposable
    {
        // 缓存 JsonSerializerOptions
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private readonly ConcurrentDictionary<string, DeviceRuntime> _devices = new();
        private readonly GtsModel _model;
        private readonly IDataRepository? _repository;
        private bool _disposed = false;
        private readonly IAlarmManager _alarmManager;

        public event Action<string, bool>? OnDeviceOnlineChanged;
        public event Action<string, ushort[], object?>? OnDeviceDataUpdated;
        public event Action<string, string>? OnDeviceStepChanged;
        public event Action<string, int, int>? OnDeviceProductionUpdated;
        public IAlarmManager AlarmManager => _alarmManager;

        public DeviceManager(GtsModel model, IDataRepository? repository = null)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _repository = repository;
            _alarmManager = new AlarmManager();
        }

        public GtsModel Model => _model;

        // ---------- 添加/移除设备 ----------
        public bool AddDevice(DeviceConfig config)
        {
            var runtime = new DeviceRuntime
            {
                Config = config,
                ModbusClient = new ModbusClient(config.Modbus)
            };

            runtime.ConnectionStateChangedHandler = (s, e) =>
            {
                try
                {
                    runtime.IsOnline = e.IsConnected;
                    OnDeviceOnlineChanged?.Invoke(config.DeviceId, e.IsConnected);
                    if (!e.IsConnected)
                        runtime.LastError = e.ErrorMessage ?? "连接断开";
                }
                catch (Exception ex)
                {
                    AppLogger.Warn($"⚠️ ConnectionStateChanged 处理异常: {ex}", "DeviceManager");
                }
            };
            runtime.ModbusClient.ConnectionStateChanged += runtime.ConnectionStateChangedHandler;

            if (!_devices.TryAdd(config.DeviceId, runtime))
            {
                AppLogger.Warn($"⚠️ 添加设备失败（已存在）: {config.DeviceId}", "DeviceManager");
                try { runtime.ModbusClient.Dispose(); } catch { }
                try { runtime.Watchdog.Dispose(); } catch { }
                return false;
            }

            AppLogger.Info($"✅ 设备 [{config.Name}] 已添加", "DeviceManager");
            return true;
        }

        public async Task<bool> RemoveDeviceAsync(string deviceId, int stopTimeoutMs = 1000)
        {
            if (!_devices.TryRemove(deviceId, out var runtime) || runtime == null)
                return false;

            try
            {
                // 取消事件订阅
                if (runtime.ConnectionStateChangedHandler != null && runtime.ModbusClient != null)
                {
                    runtime.ModbusClient.ConnectionStateChanged -= runtime.ConnectionStateChangedHandler;
                }

                try { runtime.WorkflowCts?.Cancel(); } catch { }
                try { runtime.CurrentCommand?.Stop(); } catch { }

                var task = runtime.WorkflowTask ?? Task.CompletedTask;
                if (!task.IsCompleted)
                {
                    var finished = await Task.WhenAny(task, Task.Delay(stopTimeoutMs));
                    if (finished != task)
                        AppLogger.Warn($"⚠️ 设备 [{deviceId}] 停止超时 (>{stopTimeoutMs}ms)", "DeviceManager");
                }

                try { runtime.ModbusClient.Disconnect(); } catch { }
                try { runtime.ModbusClient.Dispose(); } catch { }
                try { runtime.Watchdog.Dispose(); } catch { }

                AppLogger.Info($"✅ 设备 [{deviceId}] 已移除", "DeviceManager");
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.Error($"❌ 移除设备 [{deviceId}] 异常: {ex}", "DeviceManager");
                return false;
            }
        }

        public bool RemoveDevice(string deviceId, int stopTimeoutMs = 1000)
            => RemoveDeviceAsync(deviceId, stopTimeoutMs).GetAwaiter().GetResult();

        // ---------- 启动/停止（支持指定工作流） ----------
        /// <summary>
        /// 启动设备，可指定工作流名称（配方名）
        /// </summary>
        public bool StartDevice(string deviceId, string? workflowName = null)
        {
            if (!_devices.TryGetValue(deviceId, out var runtime) || runtime == null) return false;

            lock (runtime)
            {
                if (runtime.IsRunning) return true;

                if (!runtime.ModbusClient.Connect())
                {
                    AppLogger.Error($"❌ 设备 [{runtime.Config.Name}] Modbus 连接失败，无法启动", "DeviceManager");
                    return false;
                }

                // 设置当前工作流名称（若传入则使用传入值，否则使用已存储的）
                if (!string.IsNullOrEmpty(workflowName))
                    runtime.CurrentWorkflowName = workflowName;
                else if (string.IsNullOrEmpty(runtime.CurrentWorkflowName))
                {
                    AppLogger.Error($"❌ 设备 [{runtime.Config.Name}] 未指定工作流，无法启动", "DeviceManager");
                    return false;
                }

                runtime.WorkflowCts = new CancellationTokenSource();
                var token = runtime.WorkflowCts.Token;
                runtime.WorkflowTask = Task.Run(() => DeviceLoopAsync(runtime, token), token);
                runtime.IsRunning = true;
                AppLogger.Info($"▶️ 设备 [{runtime.Config.Name}] 已启动，工作流: {runtime.CurrentWorkflowName}", "DeviceManager");
                return true;
            }
        }

        /// <summary>
        /// 使用已设置的工作流启动设备（兼容旧调用）
        /// </summary>
        public bool StartDevice(string deviceId) => StartDevice(deviceId, null);

        public bool StopDevice(string deviceId, int stopTimeoutMs = 1000)
        {
            if (!_devices.TryGetValue(deviceId, out var runtime) || runtime == null) return false;

            lock (runtime)
            {
                if (!runtime.IsRunning) return true;

                try { runtime.WorkflowCts?.Cancel(); } catch { }
                try { runtime.CurrentCommand?.Stop(); } catch { }

                var task = runtime.WorkflowTask ?? Task.CompletedTask;
                if (!task.IsCompleted)
                {
                    try
                    {
                        var finished = Task.WhenAny(task, Task.Delay(stopTimeoutMs)).GetAwaiter().GetResult();
                        if (finished != task)
                            AppLogger.Warn($"⚠️ 设备 [{deviceId}] 停止超时 (>{stopTimeoutMs}ms)", "DeviceManager");
                    }
                    catch { }
                }

                runtime.IsRunning = false;
                runtime.CurrentCommand = null;
                try { runtime.Watchdog.Stop(); } catch { }
                AppLogger.Info($"⏹ 设备 [{runtime.Config.Name}] 已停止", "DeviceManager");
                return true;
            }
        }

        public void StartAllDevices()
        {
            int started = 0;
            int skipped = 0;
            foreach (var kv in _devices)
            {
                var runtime = kv.Value;
                if (!runtime.IsOnline || runtime.IsRunning) continue;

                string workflow = runtime.BoundWorkflowName;
                if (string.IsNullOrEmpty(workflow))
                {
                    AppLogger.Warn($"设备 [{runtime.Config.Name}] 未绑定工作流，跳过启动", "DeviceManager");
                    skipped++;
                    continue;
                }

                // 检查工作流文件是否存在
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Workflows", workflow + ".json");
                if (!File.Exists(filePath))
                {
                    AppLogger.Warn($"设备 [{runtime.Config.Name}] 绑定的工作流文件不存在: {workflow}.json，跳过启动", "DeviceManager");
                    skipped++;
                    continue;
                }

                if (StartDevice(kv.Key, workflow))
                    started++;
            }

            if (skipped > 0)
                AppLogger.Warn($"全部启动完成：{started} 台成功，{skipped} 台跳过（未绑定或文件缺失）", "DeviceManager");
            else if (started > 0)
                AppLogger.Info($"全部启动完成：{started} 台设备已启动", "DeviceManager");
            else
                AppLogger.Warn("没有设备可启动", "DeviceManager");
        }

        /// <summary>
        /// 为设备绑定工作流（不启动设备）
        /// </summary>
        public bool SetBoundWorkflow(string deviceId, string workflowName)
        {
            if (!_devices.TryGetValue(deviceId, out var runtime) || runtime == null)
                return false;
            runtime.BoundWorkflowName = workflowName;
            AppLogger.Info($"设备 [{runtime.Config.Name}] 已绑定工作流: {workflowName}", "DeviceManager");
            return true;
        }

        /// <summary>
        /// 获取设备绑定的工作流名称
        /// </summary>
        public string? GetBoundWorkflow(string deviceId)
        {
            if (!_devices.TryGetValue(deviceId, out var runtime) || runtime == null)
                return null;
            return runtime.BoundWorkflowName;
        }

        public void StopAllDevices(int stopTimeoutMs = 1000)
        {
            var keys = _devices.Keys.ToList();
            foreach (var id in keys)
                StopDevice(id, stopTimeoutMs);
        }

        /// <summary>
        /// 为设备设置工作流（配方），仅在设备停止时有效
        /// </summary>
        public bool SetDeviceWorkflow(string deviceId, string workflowName)
        {
            if (!_devices.TryGetValue(deviceId, out var runtime) || runtime == null) return false;
            if (runtime.IsRunning)
            {
                AppLogger.Warn($"设备 [{runtime.Config.Name}] 正在运行，请先停止再切换工作流", "DeviceManager");
                return false;
            }
            runtime.CurrentWorkflowName = workflowName;
            runtime.CurrentWorkflow = null; // 清空缓存，下次启动时重新加载
            AppLogger.Info($"设备 [{runtime.Config.Name}] 已设置工作流: {workflowName}", "DeviceManager");
            return true;
        }

        /// <summary>
        /// 获取设备当前工作流名称
        /// </summary>
        public string? GetDeviceWorkflow(string deviceId)
        {
            if (!_devices.TryGetValue(deviceId, out var runtime) || runtime == null)
                return null;
            return runtime.CurrentWorkflowName;
        }

        // ---------- 获取设备 ----------
        public DeviceRuntime? GetDevice(string deviceId)
        {
            _devices.TryGetValue(deviceId, out var runtime);
            return runtime;
        }

        public List<DeviceRuntime> GetAllDevices() => _devices.Values.ToList();

        // ---------- Modbus 信号读写 ----------
        public bool WriteRegisterSignal(string targetDeviceId, int address, ushort value)
        {
            var device = GetDevice(targetDeviceId);
            if (device == null || !device.IsOnline) return false;
            try { return device.ModbusClient.WriteSingleRegister((ushort)address, value); }
            catch { return false; }
        }

        public bool WriteRegisterSignal(string targetDeviceId, int address, ushort[] values)
        {
            var device = GetDevice(targetDeviceId);
            if (device == null || !device.IsOnline) return false;
            try { return device.ModbusClient.WriteMultipleRegisters((ushort)address, values); }
            catch { return false; }
        }

        public ushort[]? ReadRegisterSignal(string targetDeviceId, int address, int count = 1)
        {
            var device = GetDevice(targetDeviceId);
            if (device == null || !device.IsOnline) return null;
            try { return device.ModbusClient.ReadHoldingRegistersWithRaw((ushort)address, (ushort)count)?.RawRegisters; }
            catch { return null; }
        }

        public bool WriteSignal(string targetDeviceId, int address, bool value)
        {
            var device = GetDevice(targetDeviceId);
            if (device == null || !device.IsOnline) return false;
            try { return device.ModbusClient.WriteSingleCoil((ushort)address, value); }
            catch { return false; }
        }

        public bool? ReadSignal(string targetDeviceId, int address)
        {
            var device = GetDevice(targetDeviceId);
            if (device == null || !device.IsOnline) return null;
            try
            {
                var result = device.ModbusClient.ReadDataByType((ushort)address, 1);
                if (result == null || result.RawRegisters.Length == 0) return null;
                return (result.RawRegisters[0] & 0x0001) != 0;
            }
            catch { return null; }
        }

        // ---------- 配置导出/导入 ----------
        public string ExportConfig()
        {
            var list = _devices.Values.Select(r => r.Config).ToList();
            return JsonSerializer.Serialize(list, _jsonOptions);
        }

        public bool ImportConfig(string json)
        {
            try
            {
                var configs = JsonSerializer.Deserialize<List<DeviceConfig>>(json);
                if (configs == null) return false;

                StopAllDevices();
                foreach (var kv in _devices.ToList())
                {
                    try { kv.Value.ModbusClient?.Disconnect(); } catch { }
                    try { kv.Value.Watchdog?.Dispose(); } catch { }
                    _devices.TryRemove(kv.Key, out _);
                }

                foreach (var config in configs)
                    AddDevice(config);
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.Error($"❌ 导入配置失败: {ex.Message}", "DeviceManager");
                return false;
            }
        }

        // ---------- 复位功能 ----------
        public enum ResetMode
        {
            HardReset,
            SoftReset,
            FullReset
        }

        public async Task<bool> ResetWorkflowAsync(string deviceId, ResetMode mode = ResetMode.SoftReset, string triggeredBy = "系统")
        {
            if (!_devices.TryGetValue(deviceId, out var runtime))
            {
                AppLogger.Warn($"复位失败：设备 [{deviceId}] 不存在", "DeviceManager");
                return false;
            }

            lock (runtime)
            {
                if (runtime.IsRunning)
                {
                    try { runtime.WorkflowCts?.Cancel(); } catch { }
                    try { runtime.CurrentCommand?.Stop(); } catch { }

                    if (runtime.WorkflowTask != null && !runtime.WorkflowTask.IsCompleted)
                    {
                        try
                        {
                            var finished = Task.WhenAny(runtime.WorkflowTask, Task.Delay(2000)).GetAwaiter().GetResult();
                            if (finished != runtime.WorkflowTask)
                                AppLogger.Warn($"设备 [{runtime.Config.Name}] 复位超时 (2s)，强制重置状态", "DeviceManager");
                        }
                        catch { }
                    }
                    runtime.IsRunning = false;
                }

                switch (mode)
                {
                    case ResetMode.HardReset:
                        runtime.Config.CurrentCount = 0;
                        runtime.CurrentStepIndex = 0;
                        runtime.IsPaused = false;
                        runtime.WorkflowContext.Clear();
                        AppLogger.Info($"设备 [{runtime.Config.Name}] 硬重置：产量归零，从头开始", "DeviceManager");
                        break;
                    case ResetMode.SoftReset:
                        runtime.IsPaused = false;
                        runtime.LastError = "";
                        AppLogger.Info($"设备 [{runtime.Config.Name}] 软重置：从步骤 {runtime.CurrentStepIndex + 1} 继续", "DeviceManager");
                        break;
                    case ResetMode.FullReset:
                        runtime.Config.CurrentCount = 0;
                        runtime.CurrentStepIndex = 0;
                        runtime.IsPaused = false;
                        runtime.CurrentWorkflow = null;
                        runtime.WorkflowContext.Clear();
                        AppLogger.Info($"设备 [{runtime.Config.Name}] 完全重置：产量归零，重新加载工作流", "DeviceManager");
                        break;
                }

                runtime.LastError = "";
                runtime.Watchdog.Feed();

                var user = SessionManager.CurrentUser;
                AuditService.Log(
                    userId: user?.Id ?? 0,
                    username: user?.Username ?? triggeredBy,
                    actionType: "WorkflowReset",
                    detail: $"设备 [{runtime.Config.Name}] {mode} 触发，当前产量 {runtime.Config.CurrentCount}，步骤索引 {runtime.CurrentStepIndex}",
                    repo: _repository
                );

                OnDeviceStepChanged?.Invoke(deviceId, "空闲");
                return true;
            }
        }

        // ================================================================
        // 🔄 核心循环（从配方库加载工作流）
        // ================================================================
        private async Task DeviceLoopAsync(DeviceRuntime runtime, CancellationToken ct)
        {
            var config = runtime.Config;
            var modbus = runtime.ModbusClient;
            var watchdog = runtime.Watchdog;

            // 从配方库加载工作流
            if (runtime.CurrentWorkflow == null && !string.IsNullOrEmpty(runtime.CurrentWorkflowName))
                runtime.CurrentWorkflow = LoadWorkflowFromFile(runtime.CurrentWorkflowName);

            var workflow = runtime.CurrentWorkflow;

            // 无工作流 -> 空闲保活
            if (workflow == null || workflow.Commands.Count == 0)
            {
                AppLogger.Warn($"设备 [{config.Name}] 未配置有效工作流，进入空闲保活模式", "DeviceManager");
                while (!ct.IsCancellationRequested && runtime.IsRunning)
                {
                    try
                    {
                        if (!modbus.IsConnected)
                        {
                            if (modbus.Reconnect(config.Modbus))
                                AppLogger.Info($"设备 [{config.Name}] 空闲重连成功", "DeviceManager");
                            else
                                await Task.Delay(2000, ct);
                        }
                        else
                        {
                            watchdog.Feed();
                            await Task.Delay(1000, ct);
                        }
                    }
                    catch (OperationCanceledException) { break; }
                    catch (Exception ex)
                    {
                        AppLogger.Error($"设备 [{config.Name}] 空闲保活异常: {ex.Message}", "DeviceManager");
                        await Task.Delay(2000, ct);
                    }
                }
                runtime.IsRunning = false;
                return;
            }

            // 连接检查
            if (!modbus.IsConnected && !modbus.Reconnect(config.Modbus))
            {
                AppLogger.Error($"设备 [{config.Name}] Modbus 初始连接失败，循环退出", "DeviceManager");
                runtime.IsRunning = false;
                return;
            }

            watchdog.Start();
            AppLogger.Info($"✅ 设备 [{config.Name}] 开始执行工作流: {workflow.Name}，目标产量 {config.TargetCount}", "DeviceManager");

            // 主循环
            while (!ct.IsCancellationRequested && config.CurrentCount < config.TargetCount)
            {
                try
                {
                    // 硬件急停检测
                    if (_model.IsEmergencyStopPressed())
                    {
                        AppLogger.Warn($"⚠️ 检测到硬件急停信号！设备 [{config.Name}] 强制暂停", "DeviceManager");
                        runtime.WorkflowCts?.Cancel();
                        runtime.IsPaused = true;
                        while (_model.IsEmergencyStopPressed() && !ct.IsCancellationRequested)
                        {
                            await Task.Delay(200, ct);
                        }
                        AppLogger.Info($"设备 [{config.Name}] 急停已复位，等待操作员恢复生产", "DeviceManager");
                        continue;
                    }

                    // 看门狗和连接检查
                    if (watchdog.IsTimeout)
                    {
                        AppLogger.Warn($"设备 [{config.Name}] 看门狗超时，尝试恢复", "DeviceManager");
                        if (!modbus.IsConnected)
                        {
                            if (modbus.Reconnect(config.Modbus))
                            {
                                watchdog.Feed();
                                AppLogger.Info($"设备 [{config.Name}] 重连成功，看门狗重置", "DeviceManager");
                            }
                            else
                            {
                                AppLogger.Error($"设备 [{config.Name}] 重连失败，循环退出", "DeviceManager");
                                break;
                            }
                        }
                        else
                        {
                            watchdog.Feed();
                            AppLogger.Warn($"设备 [{config.Name}] 看门狗已重置（连接正常）", "DeviceManager");
                        }
                    }

                    if (!modbus.IsConnected)
                    {
                        AppLogger.Warn($"设备 [{config.Name}] Modbus 连接断开，尝试重连", "DeviceManager");
                        if (modbus.Reconnect(config.Modbus))
                        {
                            watchdog.Feed();
                            AppLogger.Info($"设备 [{config.Name}] 重连成功", "DeviceManager");
                        }
                        else
                        {
                            await Task.Delay(2000, ct);
                            continue;
                        }
                    }

                    // 工作流执行（断点恢复）
                    int startIndex = runtime.CurrentStepIndex;
                    if (runtime.IsPaused)
                    {
                        AppLogger.Info($"设备 [{config.Name}] 处于暂停状态，等待恢复 (步骤 {startIndex + 1})", "DeviceManager");
                        await Task.Delay(500, ct);
                        continue;
                    }
                    else if (startIndex >= workflow.Commands.Count)
                    {
                        startIndex = 0;
                        runtime.CurrentStepIndex = 0;
                    }

                    var remainingCommands = workflow.Commands
                        .Skip(startIndex)
                        .Select(cfg => CommandFactory.Create(_model, this, cfg))
                        .ToList();

                    if (remainingCommands.Count == 0)
                    {
                        await Task.Delay(500, ct);
                        runtime.CurrentStepIndex = 0;
                        continue;
                    }

                    var sequence = new SequenceCommand(remainingCommands.ToArray());
                    runtime.CurrentCommand = sequence;

                    int currentStepIdx = startIndex;
                    sequence.OnLog += msg =>
                    {
                        if (currentStepIdx < remainingCommands.Count)
                        {
                            var cmd = remainingCommands[currentStepIdx];
                            runtime.CurrentStep = cmd.Name;
                            OnDeviceStepChanged?.Invoke(config.DeviceId, cmd.Name);
                        }
                        AppLogger.Info(msg, "Workflow");
                    };

                    AppLogger.Info($"设备 [{config.Name}] 开始执行工作流周期 (产量 {config.CurrentCount}/{config.TargetCount})", "DeviceManager");
                    sequence.Execute(ct);

                    // 执行结果处理
                    if (sequence.IsFaulted)
                    {
                        runtime.CurrentStepIndex = startIndex;
                        runtime.IsPaused = true;
                        runtime.LastError = sequence.FaultReason;
                        AppLogger.Error($"设备 [{config.Name}] 工作流执行失败，已暂停于步骤 {startIndex + 1}: {sequence.FaultReason}", "DeviceManager");
                        _alarmManager.TriggerAlarm(config.DeviceId, $"工作流暂停: {sequence.FaultReason}", AlarmSeverity.Error);
                        await Task.Delay(2000, ct);
                        continue;
                    }

                    // 成功：重置索引，产量+1
                    startIndex = 0;
                    runtime.CurrentStepIndex = 0;
                    runtime.IsPaused = false;

                    if (config.CurrentCount < config.TargetCount)
                    {
                        config.CurrentCount++;
                        OnDeviceProductionUpdated?.Invoke(config.DeviceId, config.CurrentCount, config.TargetCount);
                        ProductionService.RecordProduction(config.DeviceId, config.CurrentCount, config.TargetCount);
                        AppLogger.Info($"📈 设备 [{config.Name}] 产量 +1，当前 {config.CurrentCount}/{config.TargetCount}", "DeviceManager");
                    }

                    watchdog.Feed();
                    await Task.Delay(100, ct);
                }
                catch (OperationCanceledException)
                {
                    AppLogger.Info($"设备 [{config.Name}] 循环被取消", "DeviceManager");
                    break;
                }
                catch (Exception ex)
                {
                    runtime.LastError = ex.Message;
                    AppLogger.Error($"设备 [{config.Name}] 循环异常: {ex.Message}", "DeviceManager");
                    runtime.IsPaused = true;
                    _alarmManager.TriggerAlarm(config.DeviceId, $"循环异常: {ex.Message}", AlarmSeverity.Error);
                    await Task.Delay(2000, ct);
                }
                finally
                {
                    runtime.CurrentCommand = null;
                }
            }

            watchdog.Stop();
            runtime.IsRunning = false;
            runtime.CurrentCommand = null;
            AppLogger.Info($"⏹ 设备 [{config.Name}] 工作流循环已退出 (产量 {config.CurrentCount}/{config.TargetCount})", "DeviceManager");
        }

        // ---------- 辅助方法 ----------
        private WorkflowConfig LoadWorkflowFromFile(string workflowName)
        {
            if (string.IsNullOrEmpty(workflowName)) workflowName = "Default";
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Workflows", workflowName + ".json");
            if (!File.Exists(path))
            {
                AppLogger.Warn($"工作流文件不存在: {path}，返回空流程", "DeviceManager");
                return new WorkflowConfig { Name = "Empty", Commands = new List<CommandConfig>() };
            }
            try
            {
                string json = File.ReadAllText(path);
                var config = JsonSerializer.Deserialize<WorkflowConfig>(json);
                if (config == null)
                {
                    AppLogger.Warn($"工作流文件 {path} 解析失败", "DeviceManager");
                    return new WorkflowConfig { Name = "Invalid", Commands = new List<CommandConfig>() };
                }
                return config;
            }
            catch (Exception ex)
            {
                AppLogger.Error($"加载工作流失败: {ex.Message}", "DeviceManager");
                return new WorkflowConfig { Name = "Error", Commands = new List<CommandConfig>() };
            }
        }

        // ---------- 资源释放 ----------
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            StopAllDevices();
            foreach (var runtime in _devices.Values)
            {
                if (runtime.ConnectionStateChangedHandler != null && runtime.ModbusClient != null)
                {
                    runtime.ModbusClient.ConnectionStateChanged -= runtime.ConnectionStateChangedHandler;
                }
                try { runtime.ModbusClient?.Dispose(); } catch { }
                try { runtime.Watchdog?.Dispose(); } catch { }
            }
            _devices.Clear();
            AppLogger.Info("🗑️ DeviceManager 已释放", "DeviceManager");
        }
    }
}