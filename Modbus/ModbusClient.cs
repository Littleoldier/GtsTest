using NModbus;
using NModbus.Device;
using NModbus.Serial;
using System.IO.Ports;
using System.Net.Sockets;

namespace GtsTest.Modbus
{
    public class ModbusClient : IDisposable
    {
        private readonly object _lock = new();
        private ModbusConfig _config;
        private IModbusMaster? _master;
        private TcpClient? _tcpClient;
        private SerialPort? _serialPort;
        private bool _isConnected;
        private CancellationTokenSource _cts = new();
        public event Action<string>? LogMessage;
        public class ModbusReadResult
        {
            public ushort[] RawRegisters { get; set; } = Array.Empty<ushort>();
            public object? ConvertedValue { get; set; }
        }
        public bool IsConnected => _isConnected;
        public event EventHandler<ModbusConnectionEventArgs>? ConnectionStateChanged;
        public ModbusClient(ModbusConfig config) => _config = config;
        private void Log(string msg, LogLevel level = LogLevel.Info)
        {
            // 直接调用全局日志系统（内部会触发 OnLogReceived 并写文件）
            AppLogger.Log(msg, level, "Modbus");
        }

        public bool Connect()
        {
            lock (_lock)
            {
                DisconnectInternal();
                return ConnectInternal();
            }
        }

        public bool Reconnect(ModbusConfig newConfig)
        {
            lock (_lock)
            {
                _config = newConfig;
                DisconnectInternal();
                return ConnectInternal();
            }
        }

        private bool ConnectInternal()
        {
            try
            {
                if (_config.Protocol == ModbusProtocol.Tcp)
                {
                    _tcpClient = new TcpClient
                    {
                        ReceiveTimeout = _config.TimeoutMs,
                        SendTimeout = _config.TimeoutMs
                    };
                    var ar = _tcpClient.BeginConnect(_config.IpAddress, _config.Port, null, null);
                    if (!ar.AsyncWaitHandle.WaitOne(_config.TimeoutMs))
                    {
                        _tcpClient.Close();
                        throw new TimeoutException($"TCP连接超时 ({_config.IpAddress}:{_config.Port})");
                    }
                    _tcpClient.EndConnect(ar);
                    _master = new ModbusFactory().CreateMaster(_tcpClient);
                }
                else // RTU
                {
                    _serialPort = new SerialPort(_config.PortName, _config.BaudRate,
                        _config.Parity, _config.DataBits, _config.StopBits)
                    {
                        ReadTimeout = _config.TimeoutMs,
                        WriteTimeout = _config.TimeoutMs
                    };
                    _serialPort.Open();
                    var streamResource = new SerialPortAdapter(_serialPort); // 显式适配，明确且稳定
                    _master = new ModbusFactory().CreateRtuMaster(streamResource);
                }

                _isConnected = true;
                ConnectionStateChanged?.Invoke(this, new ModbusConnectionEventArgs(true));
                return true;
            }
            catch (Exception ex)
            {
                _isConnected = false;
                string err = $"连接失败: {ex.Message}";
                Log($"❌ Modbus错误: {err}", LogLevel.Error);
                ConnectionStateChanged?.Invoke(this, new ModbusConnectionEventArgs(false, err));
                return false;
            }
        }

        private void DisconnectInternal(bool raiseEvent = true)
        {
            _cts.Cancel();
            _cts = new CancellationTokenSource();
            _tcpClient?.Close();
            _tcpClient = null;
            _serialPort?.Close();
            _serialPort = null;
            _master = null;
            _isConnected = false;

            if (raiseEvent)
                ConnectionStateChanged?.Invoke(this, new ModbusConnectionEventArgs(false, $"从机={_config.SlaveAddress} " + "Modbus连接已断开"));
        }

        /// <summary>
        /// 读取保持寄存器并返回数值（类型由 DataType 决定）
        /// </summary>
        public object? ReadHoldingRegisters(ushort startAddress, ushort count, byte slaveAddress = 1)
        {
            if (!_isConnected || _master == null) return null;

            try
            {
                ushort[] raw = _master.ReadHoldingRegisters(_config.SlaveAddress, startAddress, count);
                return ConvertRawToType(raw, _config.DataType);
            }
            catch (Exception ex)
            {
                Log($"❌ Modbus读取失败: {ex.Message}", LogLevel.Warn);
                _isConnected = false;
                ConnectionStateChanged?.Invoke(this, new ModbusConnectionEventArgs(false, $"从机={_config.SlaveAddress} "+"读取超时，链路中断"));
                return null;
            }
        }
        public ModbusReadResult? ReadHoldingRegistersWithRaw(ushort startAddress, ushort count, byte slaveAddress = 1)
        {
            if (!_isConnected || _master == null) return null;

            try
            {
                // 读取原始寄存器
                ushort[] raw = _master.ReadHoldingRegisters(_config.SlaveAddress, startAddress, count);
                // 转换为指定类型
                object? converted = ConvertRawToType(raw, _config.DataType);
                return new ModbusReadResult
                {
                    RawRegisters = raw,
                    ConvertedValue = converted
                };
            }
            catch (Exception ex)
            {
                Log($"❌ Modbus读取转换指定类型失败:从机={_config.SlaveAddress} {ex.Message}", LogLevel.Warn);
                _isConnected = false;
                ConnectionStateChanged?.Invoke(this, new ModbusConnectionEventArgs(false, $"从机={_config.SlaveAddress} " + "读取超时，链路中断"));
                return null;
            }
        }
        public ModbusReadResult? ReadDataByType(ushort startAddress, ushort count)
        {
            if (!_isConnected || _master == null) return null;

            try
            {
                switch (_config.AddressType)
                {
                    case AddressType.HoldingRegister:
                        ushort[] hr = _master.ReadHoldingRegisters(_config.SlaveAddress, startAddress, count);
                        return new ModbusReadResult
                        {
                            RawRegisters = hr,
                            ConvertedValue = ConvertRawToType(hr, _config.DataType)
                        };

                    case AddressType.Coil:
                        bool[] coils = _master.ReadCoils(_config.SlaveAddress, startAddress, count);
                        ushort[] coilRaw = coils.Select(b => (ushort)(b ? 1 : 0)).ToArray();
                        return new ModbusReadResult
                        {
                            RawRegisters = coilRaw,
                            ConvertedValue = coils
                        };

                    case AddressType.InputRegister:
                        ushort[] ir = _master.ReadInputRegisters(_config.SlaveAddress, startAddress, count);
                        return new ModbusReadResult
                        {
                            RawRegisters = ir,
                            ConvertedValue = ConvertRawToType(ir, _config.DataType)
                        };

                    case AddressType.DiscreteInput:
                        bool[] dis = _master.ReadInputs(_config.SlaveAddress, startAddress, count);
                        ushort[] disRaw = dis.Select(b => (ushort)(b ? 1 : 0)).ToArray();
                        return new ModbusReadResult
                        {
                            RawRegisters = disRaw,
                            ConvertedValue = dis
                        };

                    default:
                        throw new NotSupportedException($"不支持的地址类型: {_config.AddressType}");
                }
            }
            catch (Exception ex)
            {
                // 统一日志和事件通知
                Log($"❌ Modbus读取失败 (类型={_config.AddressType}, 从机={_config.SlaveAddress}): {ex.Message}", LogLevel.Warn);
                _isConnected = false;
                ConnectionStateChanged?.Invoke(this, new ModbusConnectionEventArgs(false, $"从机={_config.SlaveAddress} 读取超时，链路中断"));
                return null;
            }
        }

        private object ConvertRawToType(ushort[] raw, DataType type)
        {
            if (raw == null || raw.Length == 0) return null;

            // 对于 Int16/UInt16，返回数组（每个寄存器一个值）
            if (type == DataType.Int16)
            {
                short[] result = new short[raw.Length];
                for (int i = 0; i < raw.Length; i++)
                    result[i] = (short)((raw[i] << 8) | (raw[i] >> 8)); // 大端转换
                return result;
            }
            if (type == DataType.UInt16)
            {
                ushort[] result = new ushort[raw.Length];
                for (int i = 0; i < raw.Length; i++)
                    result[i] = (ushort)((raw[i] << 8) | (raw[i] >> 8));
                return result;
            }

            // 多寄存器类型：需要足够的长度
            byte[] bytes = new byte[raw.Length * 2];
            for (int i = 0; i < raw.Length; i++)
            {
                bytes[i * 2] = (byte)(raw[i] >> 8);
                bytes[i * 2 + 1] = (byte)(raw[i] & 0xFF);
            }

            if (type == DataType.Int32 && raw.Length >= 2)
                return BitConverter.ToInt32(bytes, 0);
            if (type == DataType.UInt32 && raw.Length >= 2)
                return BitConverter.ToUInt32(bytes, 0);
            if (type == DataType.Float && raw.Length >= 2)
                return BitConverter.ToSingle(bytes, 0);
            if (type == DataType.Double && raw.Length >= 4)
                return BitConverter.ToDouble(bytes, 0);

            // 如果长度不足，返回 null 或抛出异常
            return null;
        }

        public void Disconnect() => DisconnectInternal(true);
        public void Dispose() { DisconnectInternal(false); _cts.Dispose(); }

        /// <summary>
        /// 写单个线圈 (功能码 0x05)
        /// </summary>
        public bool WriteSingleCoil(ushort address, bool value, byte slaveAddress = 1)
        {
            if (!_isConnected || _master == null)
            {
                Log($"❌ 写单个线圈失败：从机={_config.SlaveAddress} Modbus 未连接", LogLevel.Error);
                return false;
            }

            try
            {
                _master.WriteSingleCoil(_config.SlaveAddress, address, value);
                Log($"✅ 写单个线圈成功: 从机={_config.SlaveAddress} 地址={address}  值={value}", LogLevel.Info);
                return true;
            }
            catch (Exception ex)
            {
                Log($"❌ 写单个线圈异常: 从机={_config.SlaveAddress}  地址={address}  值={value} {ex.Message}", LogLevel.Error);
                return false;
            }

        }

        /// <summary>
        /// 写多个线圈 (功能码 0x0F)
        /// </summary>
        public bool WriteMultipleCoils(ushort address, bool[] values, byte slaveAddress = 1)
        {
            if (!_isConnected || _master == null)
            {
                Log($"❌ 写多个线圈失败： 从机={_config.SlaveAddress} Modbus 未连接", LogLevel.Error);
                return false;
            }

            try
            {
                _master.WriteMultipleCoils(_config.SlaveAddress, address, values);
                Log($"✅ 写多个线圈成功: 从机={_config.SlaveAddress}  起始地址={address} 数量={values.Length}", LogLevel.Info);
                return true;
            }
            catch (Exception ex)
            {
                Log($"❌ 写多个线圈异常: 从机={_config.SlaveAddress} {ex.Message}", LogLevel.Error);
                return false;
            }
        }

        /// <summary>
        /// 写单个保持寄存器 (功能码 0x06)
        /// </summary>
        public bool WriteSingleRegister(ushort address, ushort value, byte slaveAddress = 1)
        {
            if (!_isConnected || _master == null)
            {
                Log($"❌ 写单个寄存器失败： 从机={_config.SlaveAddress} Modbus 未连接", LogLevel.Error);
                return false;
            }

            try
            {
                _master.WriteSingleRegister(_config.SlaveAddress, address, value);
                Log($"✅ 写单个寄存器成功: 从机={_config.SlaveAddress} 地址={address} 值=0x{value:X4}", LogLevel.Info);
                return true;
            }
            catch (Exception ex)
            {
                Log($"❌ 写单个寄存器异常: 从机={_config.SlaveAddress} {ex.Message}", LogLevel.Error);
                return false;
            }
        }

        /// <summary>
        /// 写多个保持寄存器 (功能码 0x10)
        /// </summary>
        public bool WriteMultipleRegisters(ushort address, ushort[] values, byte slaveAddress = 1)
        {
            if (!_isConnected || _master == null)
            {
                Log($"❌ 写多个寄存器失败： 从机={_config.SlaveAddress} Modbus 未连接", LogLevel.Error);
                return false;
            }

            try
            {
                _master.WriteMultipleRegisters(_config.SlaveAddress, address, values);
                Log($"✅ 写多个寄存器成功:  从机={_config.SlaveAddress} 起始地址={address} 数量={values.Length}", LogLevel.Info);
                return true;
            }
            catch (Exception ex)
            {
                Log($"❌ 写多个寄存器异常: 从机={_config.SlaveAddress} {ex.Message}", LogLevel.Error);
                return false;
            }
        }
        public static ushort[] EncodeValue(object value, DataType type, ByteOrder order)
        {
            if (value == null) return Array.Empty<ushort>();

            // 处理数组（多个值）
            if (value is Array arr)
            {
                var result = new List<ushort>();
                foreach (var item in arr)
                    result.AddRange(EncodeValue(item, type, order));
                return result.ToArray();
            }

            // 单值处理
            byte[] bytes;
            switch (type)
            {
                case DataType.Int16:
                    bytes = BitConverter.GetBytes(Convert.ToInt16(value));
                    break;
                case DataType.UInt16:
                    bytes = BitConverter.GetBytes(Convert.ToUInt16(value));
                    break;
                case DataType.Int32:
                    bytes = BitConverter.GetBytes(Convert.ToInt32(value));
                    break;
                case DataType.UInt32:
                    bytes = BitConverter.GetBytes(Convert.ToUInt32(value));
                    break;
                case DataType.Float:
                    bytes = BitConverter.GetBytes(Convert.ToSingle(value));
                    break;
                case DataType.Double:
                    bytes = BitConverter.GetBytes(Convert.ToDouble(value));
                    break;
                default:
                    throw new NotSupportedException($"不支持的数据类型: {type}");
            }

            // 根据字节序调整（默认 BitConverter 使用本机端序，通常是小端，需要按需交换）
            if (order == ByteOrder.BigEndian)
            {
                if (bytes.Length == 2) Array.Reverse(bytes);
                else if (bytes.Length == 4) { Array.Reverse(bytes, 0, 2); Array.Reverse(bytes, 2, 2); }
                else if (bytes.Length == 8) { Array.Reverse(bytes, 0, 2); Array.Reverse(bytes, 2, 2); Array.Reverse(bytes, 4, 2); Array.Reverse(bytes, 6, 2); }
            }
            // 小端则保持原样（因为 BitConverter 已是小端）

            // 将字节转换为 ushort[]
            ushort[] registers = new ushort[bytes.Length / 2];
            for (int i = 0; i < registers.Length; i++)
                registers[i] = (ushort)((bytes[i * 2] << 8) | bytes[i * 2 + 1]);
            return registers;
        }
        // ---------- 便捷方法（可选） ----------
        /// <summary>
        /// 写入单个保持寄存器（别名，与 WriteSingleRegister 相同）
        /// </summary>
        public bool WriteHoldingRegister(ushort address, ushort value, byte slaveAddress = 1)
            => WriteSingleRegister(address, value, slaveAddress);

        /// <summary>
        /// 写入多个保持寄存器（别名）
        /// </summary>
        public bool WriteHoldingRegisters(ushort address, ushort[] values, byte slaveAddress = 1)
            => WriteMultipleRegisters(address, values, slaveAddress);
    }
}