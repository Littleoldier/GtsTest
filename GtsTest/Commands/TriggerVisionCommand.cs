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
    /// </summary>
    public class TriggerVisionCommand : MotionCommandBase
    {
        private readonly CommandConfig _config;
        private readonly DeviceManager _deviceManager;
        private ModbusClient? _modbusClient;

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
            // 1. 创建 Modbus 客户端连接视觉服务器
            // ============================================================
            var modbusConfig = new ModbusConfig
            {
                Protocol = ModbusProtocol.Tcp,
                IpAddress = _config.VisionServerIp,
                Port = _config.VisionServerPort,
                SlaveAddress = 1,          // 视觉服务器默认从站地址为 1
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
                // 3. 检查视觉服务器是否空闲（读忙状态线圈101）
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
                {
                    throw new Exception("视觉服务器正忙，请稍后重试");
                }

                // ============================================================
                // 4. 写入触发信号（线圈100 = true）
                // ============================================================
                ct.ThrowIfCancellationRequested();
                Log($"📸 发送触发信号 (线圈 {_config.TriggerCoilAddress} = 1)");

                bool triggerSuccess = _modbusClient.WriteSingleCoil((ushort)_config.TriggerCoilAddress, true);
                if (!triggerSuccess)
                {
                    throw new Exception("触发信号写入失败");
                }

                // ============================================================
                // 5. 等待视觉完成（轮询忙状态线圈101）
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

                        if (pollCount % 20 == 0) // 约每 1 秒打印一次（50ms * 20 = 1000ms）
                        {
                            Log($"⏳ 等待视觉完成... ({sw.ElapsedMilliseconds}ms)");
                        }
                    }
                    else
                    {
                        // 通信异常，尝试重新读取
                        Log($"⚠️ 读取忙状态失败，尝试重连...");
                        _modbusClient.Disconnect();
                        Thread.Sleep(200);
                        if (!_modbusClient.Connect())
                        {
                            throw new Exception("与视觉服务器通信中断");
                        }
                    }

                    Thread.Sleep(50);
                }

                if (isBusyState)
                {
                    throw new TimeoutException($"视觉拍照超时 ({_config.VisionTimeoutMs}ms)");
                }

                // ============================================================
                // 6. 读取结果（线圈102 和 寄存器1000）
                // ============================================================
                ct.ThrowIfCancellationRequested();

                // 读取结果线圈102
                var resultCoil = _modbusClient.ReadDataByType((ushort)_config.ResultCoilAddress, 1);

                // 读取结果码寄存器1000
                var resultCode = _modbusClient.ReadHoldingRegisters((ushort)_config.ResultCodeRegister, 1);

                // 解析结果码
                ushort[]? codeRegs = resultCode as ushort[];
                int code = (codeRegs?.Length > 0) ? codeRegs[0] : 999;

                // ⭐⭐⭐ 关键修复：判断执行结果
                // 以结果码为主：码 0 表示成功，非 0 表示失败
                bool isSuccess;

                if (code == 0)
                {
                    // 结果码为 0，表示成功（即使线圈102读取异常，也以寄存器为准）
                    isSuccess = true;
                    Log($"✅ 视觉执行成功 (结果码: {code})");
                }
                else
                {
                    // 结果码非 0，检查线圈102确认
                    bool coilSuccess = resultCoil?.RawRegisters != null &&
                                       resultCoil.RawRegisters.Length > 0 &&
                                       resultCoil.RawRegisters[0] == 1;

                    isSuccess = coilSuccess;
                }

                // 如果判断为失败，抛出异常
                if (!isSuccess)
                {
                    string errorMsg = code switch
                    {
                        1 => "执行失败（未知错误）",
                        2 => "相机未连接",
                        3 => "保存失败",
                        99 => "系统错误",
                        _ => $"错误码 {code}"
                    };
                    throw new Exception($"视觉执行失败: {errorMsg}");
                }

                // ============================================================
                // 7. 读取文件名（可选）
                // ============================================================
                try
                {
                    var fileNameRegs = _modbusClient.ReadHoldingRegisters((ushort)_config.FileNameRegisterStart, 2);
                    if (fileNameRegs is ushort[] regs && regs.Length >= 2)
                    {
                        string fileName = "";
                        if (regs[0] > 0) fileName += (char)regs[0];
                        if (regs[1] > 0) fileName += (char)regs[1];
                        if (!string.IsNullOrEmpty(fileName))
                        {
                            Log($"📁 保存的文件名: {fileName}");
                        }
                    }
                }
                catch
                {
                    // 读取文件名失败不影响整体流程
                    Log($"⚠️ 读取文件名失败（非关键错误）");
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
                // 清理资源
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