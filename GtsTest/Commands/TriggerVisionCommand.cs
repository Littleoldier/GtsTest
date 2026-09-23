using GtsTest.Core;
using GtsTest.Modbus;
using System;
using System.Diagnostics;
using System.Threading;

namespace GtsTest.Commands
{
    /// <summary>
    /// 触发视觉拍照命令
    /// 通过 Modbus 触发视觉服务器拍照，并等待结果
    /// 
    /// ⭐ 结果码语义：
    ///   0  = OK（检测通过）
    ///   3  = NG（业务结果：未检测到目标 / 尺寸超差）→ 不抛异常，工作流继续
    ///   1  = 系统内部错误 → 抛异常
    ///   2  = 相机未连接 → 抛异常
    ///   99 = 系统未知错误 → 抛异常
    /// </summary>
    public class TriggerVisionCommand : MotionCommandBase
    {
        private readonly CommandConfig _config;
        private readonly DeviceManager _deviceManager;
        private ModbusClient? _modbusClient;

        // ============================================================
        // 视觉结果（供上层读取）
        // ============================================================
        public bool? VisionPass { get; private set; }        // true=OK, false=NG, null=未获取
        public double DiameterMm { get; private set; }
        public double X { get; private set; }
        public double Y { get; private set; }
        public int DefectCount { get; private set; }

        public TriggerVisionCommand(GtsModel model, DeviceManager deviceManager, CommandConfig config)
            : base(model)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _deviceManager = deviceManager ?? throw new ArgumentNullException(nameof(deviceManager));
            Name = $"触发视觉拍照 ({config.VisionServerIp}:{config.VisionServerPort})";
        }

        protected override void ExecuteCore(CancellationToken ct)
        {
            // ============================================================
            // 1. 创建 Modbus 客户端
            // ============================================================
            var modbusConfig = new ModbusConfig
            {
                Protocol = ModbusProtocol.Tcp,
                IpAddress = _config.VisionServerIp,
                Port = _config.VisionServerPort,
                SlaveAddress = 1,
                TimeoutMs = 3000
            };

            _modbusClient = new ModbusClient(modbusConfig);
            _modbusClient.LogMessage += msg => Log($"Modbus: {msg}");

            try
            {
                // ============================================================
                // 2. 连接视觉服务器
                // ============================================================
                Log($"正在连接视觉服务器 {_config.VisionServerIp}:{_config.VisionServerPort}...");
                if (!_modbusClient.Connect())
                {
                    throw new Exception($"无法连接到视觉服务器 {_config.VisionServerIp}:{_config.VisionServerPort}");
                }
                Log($"✅ 视觉服务器连接成功");

                // ============================================================
                // 3. 检查忙状态
                // ============================================================
                ct.ThrowIfCancellationRequested();

                bool isBusy = true;
                int retryCount = 0;
                while (isBusy && retryCount < 3)
                {
                    var busyResult = _modbusClient.ReadDataByType((ushort)_config.BusyCoilAddress, 1);
                    if (busyResult?.RawRegisters != null && busyResult.RawRegisters.Length > 0)
                    {
                        isBusy = busyResult.RawRegisters[0] == 1;
                        if (isBusy)
                        {
                            Log($"⚠️ 视觉服务器正忙，等待 1 秒后重试... (尝试 {retryCount + 1}/3)");
                            Thread.Sleep(1000);
                            ct.ThrowIfCancellationRequested();
                            retryCount++;
                        }
                    }
                    else
                    {
                        Log($"⚠️ 读取忙状态失败，等待 500ms 后重试...");
                        Thread.Sleep(500);
                        retryCount++;
                    }
                }

                if (isBusy)
                    throw new Exception("视觉服务器正忙，请稍后重试");

                // ============================================================
                // 4. 写入触发信号
                // ============================================================
                ct.ThrowIfCancellationRequested();
                Log($"📸 发送触发信号 (线圈 {_config.TriggerCoilAddress} = 1)");

                bool triggerSuccess = _modbusClient.WriteSingleCoil((ushort)_config.TriggerCoilAddress, true);
                if (!triggerSuccess)
                    throw new Exception("触发信号写入失败");

                // ============================================================
                // 5. 等待视觉完成
                // ============================================================
                Log($"⏳ 等待视觉服务器处理... (超时 {_config.VisionTimeoutMs}ms)");

                var sw = Stopwatch.StartNew();
                bool isBusyState = true;
                int pollCount = 0;

                while (isBusyState && sw.ElapsedMilliseconds < _config.VisionTimeoutMs)
                {
                    ct.ThrowIfCancellationRequested();

                    var status = _modbusClient.ReadDataByType((ushort)_config.BusyCoilAddress, 1);
                    if (status?.RawRegisters != null && status.RawRegisters.Length > 0)
                    {
                        isBusyState = status.RawRegisters[0] == 1;
                        pollCount++;

                        if (pollCount % 20 == 0)
                            Log($"⏳ 等待视觉完成... ({sw.ElapsedMilliseconds}ms)");
                    }
                    else
                    {
                        Log($"⚠️ 读取忙状态失败，尝试重连...");
                        _modbusClient.Disconnect();
                        Thread.Sleep(200);
                        if (!_modbusClient.Connect())
                            throw new Exception("与视觉服务器通信中断");
                    }

                    Thread.Sleep(50);
                }

                if (isBusyState)
                    throw new TimeoutException($"视觉拍照超时 ({_config.VisionTimeoutMs}ms)");

                // ============================================================
                // 6. 读取结果
                // ============================================================
                ct.ThrowIfCancellationRequested();

                var resultCoil = _modbusClient.ReadDataByType((ushort)_config.ResultCoilAddress, 1);
                var resultCode = _modbusClient.ReadHoldingRegisters((ushort)_config.ResultCodeRegister, 1);

                ushort[]? codeRegs = resultCode as ushort[];
                int code = (codeRegs?.Length > 0) ? codeRegs[0] : 999;

                // ⭐ 读取直径 / 坐标 / 缺陷数
                try
                {
                    var diaRegs = _modbusClient.ReadHoldingRegisters((ushort)1003, 1);
                    if (diaRegs is ushort[] d && d.Length > 0)
                        DiameterMm = d[0] / 100.0;

                    var xRegs = _modbusClient.ReadHoldingRegisters((ushort)1005, 1);
                    if (xRegs is ushort[] xr && xr.Length > 0)
                        X = (short)xr[0] / 100.0;

                    var yRegs = _modbusClient.ReadHoldingRegisters((ushort)1006, 1);
                    if (yRegs is ushort[] yr && yr.Length > 0)
                        Y = (short)yr[0] / 100.0;

                    var dRegs = _modbusClient.ReadHoldingRegisters((ushort)1004, 1);
                    if (dRegs is ushort[] dr && dr.Length > 0)
                        DefectCount = dr[0];
                }
                catch { }

                // ============================================================
                // 7. ⭐⭐⭐ 结果判定：区分业务 NG 与系统故障 ⭐⭐⭐
                // ============================================================
                if (code == 0)
                {
                    // ✅ OK
                    VisionPass = true;
                    Log($"✅ 视觉执行成功 (结果码: 0)");
                    Log($"📏 直径: {DiameterMm:F2} mm");
                    Log($"🔍 缺陷数: {DefectCount}");
                    Log($"📍 X 坐标: {X:F2}");
                    Log($"📍 Y 坐标: {Y:F2}");

                    SaveResultToContext(true);
                }
                else if (code == 3)
                {
                    // ⚠️ 业务 NG：未检测到目标 / 尺寸超差
                    // → 不抛异常，工作流继续
                    VisionPass = false;
                    Log($"⚠️ 视觉结果: NG（未检测到目标或尺寸超差）(结果码: 3)");

                    bool coilSuccess = resultCoil?.RawRegisters != null &&
                                       resultCoil.RawRegisters.Length > 0 &&
                                       resultCoil.RawRegisters[0] == 1;
                    Log($"   ResultCoil = {(coilSuccess ? "true" : "false")}");

                    SaveResultToContext(false);
                    // ⭐ 关键：不抛异常
                }
                else
                {
                    // ❌ 系统故障：抛异常，暂停工作流
                    string errorMsg = code switch
                    {
                        1 => "视觉系统内部错误",
                        2 => "相机未连接",
                        99 => "视觉系统未知错误",
                        _ => $"错误码 {code}"
                    };
                    throw new Exception($"视觉系统故障: {errorMsg} (code={code})");
                }

                Log($"✅ 视觉触发完成");
            }
            catch (OperationCanceledException)
            {
                Log($"⚠️ 视觉触发被取消");
                throw;
            }
            catch (Exception ex)
            {
                Log($"❌ 视觉触发失败: {ex.Message}");
                throw;
            }
            finally
            {
                if (_modbusClient != null)
                {
                    try
                    {
                        _modbusClient.Disconnect();
                        _modbusClient.Dispose();
                    }
                    catch { }
                    _modbusClient = null;
                }
                Log($"🔌 视觉服务器连接已断开");
            }
        }

        /// <summary>
        /// 把视觉结果写入设备上下文，供上层（MES 上报等）读取
        /// </summary>
        private void SaveResultToContext(bool pass)
        {
            try
            {
                foreach (var dev in _deviceManager.GetAllDevices())
                {
                    if (dev.IsRunning)
                    {
                        dev.WorkflowContext["LastVisionPass"] = pass;
                        dev.WorkflowContext["LastDiameter"] = DiameterMm;
                        dev.WorkflowContext["LastX"] = X;
                        dev.WorkflowContext["LastY"] = Y;
                        dev.WorkflowContext["LastDefect"] = DefectCount;
                        break;
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// 急停处理
        /// </summary>
        public override void Stop()
        {
            Log($"🛑 收到停止信号，立即断开视觉服务器连接");
            try
            {
                _modbusClient?.Disconnect();
                _modbusClient?.Dispose();
                _modbusClient = null;
            }
            catch { }
            base.Stop();
        }
    }
}