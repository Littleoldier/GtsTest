using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using GtsTest.Commands;
using GtsTest.Modbus;
using GtsTest.Services;

namespace GtsTest.Presenters
{
    public class GtsPresenter
    {
        private readonly IGtsView _view;
        private readonly GtsModel _model;
        private readonly DeviceManager _deviceManager;
        private readonly ILogger _logger;
        private readonly IDataRepository _repository;
        private readonly IAlarmManager _alarmManager;
        private readonly IRecipeManager _recipeManager;
        private readonly IOpcUaClient _opcUaClient;
        private readonly IMqttPublisher _mqttPublisher;

        private CancellationTokenSource _workflowCts;
        private string _selectedDeviceId = "";
        private readonly IAuthenticationService _authService;

        public GtsPresenter(IGtsView view, GtsModel model, DeviceManager deviceManager,
                            ILogger logger, IDataRepository repository = null,
                            IAlarmManager alarmManager = null, IRecipeManager recipeManager = null,
                            IOpcUaClient opcUaClient = null, IMqttPublisher mqttPublisher = null, IAuthenticationService authService = null)
        {
            _view = view;
            _model = model;
            _deviceManager = deviceManager;
            _logger = logger;
            _repository = repository;
            _alarmManager = alarmManager;
            _recipeManager = recipeManager;
            _opcUaClient = opcUaClient;
            _mqttPublisher = mqttPublisher;
            _authService = authService;

            // ---------- 订阅视图事件 ----------
            _view.LoadView += OnLoadView;
            _view.AddDeviceClicked += OnAddDevice;
            _view.RemoveDeviceClicked += OnRemoveDevice;
            _view.StartAllClicked += OnStartAll;
            _view.StopAllClicked += OnStopAll;
            _view.StartDeviceClicked += OnStartDevice;
            _view.StopDeviceClicked += OnStopDevice;
            _view.DeviceConfigClicked += OnDeviceConfig;
            _view.ToggleSimulatorClicked += OnToggleSimulator;
            _view.RunWorkflowClicked += OnRunWorkflow;
            _view.StopWorkflowClicked += OnStopWorkflow;
            _view.EmergencyStopClicked += OnEmergencyStop;
            _view.SaveConfigClicked += OnSaveConfig;
            _view.ToggleModbusClicked += OnToggleModbus;
            _view.ConnectAllModbusClicked += OnConnectAllModbus;
            _view.DisconnectAllModbusClicked += OnDisconnectAllModbus;
            _view.HomeAxisClicked += OnHome;
            _view.MoveAbsClicked += OnMoveAbs;
            _view.JogPositiveClicked += (s, e) => OnJog(true);
            _view.JogNegativeClicked += (s, e) => OnJog(false);
            _view.StopAxisClicked += OnStopAxis;
            _view.ServoOnClicked += OnServoOn;
            _view.ServoOffClicked += OnServoOff;
            _view.AlarmResetClicked += OnAlarmReset;
            _view.WriteRegisterRequested += OnWriteRegister;
            _view.WriteCoilRequested += OnWriteCoil;

            _alarmManager.AlarmAdded += (s, a) => RunOnUI(() => _view.UpdateAlarmList(_alarmManager.GetActiveAlarms()));
            _alarmManager.AlarmAcknowledged += (s, id) => RunOnUI(() => _view.UpdateAlarmList(_alarmManager.GetActiveAlarms()));
            _alarmManager.AlarmResolved += (s, id) => RunOnUI(() => _view.UpdateAlarmList(_alarmManager.GetActiveAlarms()));
            // 处理确认和复位
            _view.AlarmAcknowledgeClicked += (s, e) =>
            {
                var id = int.Parse(_view.GetSelectedAlarmId());
                var user = SessionManager.CurrentUser;
                if (_alarmManager.AcknowledgeAlarm(id, user?.Username))
                    AuditService.Log(user.Id, user.Username, "AckAlarm", $"确认报警 {id}", _repository);
            };

            // ---------- 订阅 DeviceManager 事件 ----------
            _deviceManager.OnDeviceOnlineChanged += (id, online) =>
            {
                RunOnUI(() =>
                {
                    _view.UpdateDeviceOnlineStatus(id, online);
                    if (id == _selectedDeviceId)
                    {
                        var device = _deviceManager.GetDevice(id);
                        _view.UpdateAxisInfo(id, device?.Config.Axis ?? 0, online, 0, 0);
                    }
                    UpdateStatusBar();
                });
            };

            _deviceManager.OnDeviceDataUpdated += (id, raw, converted) =>
            {
                RunOnUI(() =>
                {
                    _view.UpdateDeviceData(id, converted);
                    if (id == _selectedDeviceId)
                    {
                        var device = _deviceManager.GetDevice(id);
                        if (device != null)
                        {
                            uint clk;
                            double pos = 0, vel = 0;
                            _model.GetPrfPos(device.Config.Axis, out pos, out clk);
                            _model.GetPrfVel(device.Config.Axis, out vel, out clk);
                            _view.UpdateAxisInfo(id, device.Config.Axis, true, pos, vel);
                        }
                    }
                    UpdateGlobalStats();
                });
            };

            _deviceManager.OnDeviceStepChanged += (id, step) =>
            {
                RunOnUI(() =>
                {
                    _view.UpdateDeviceStep(id, step);
                    if (id == _selectedDeviceId) UpdateStatusBar();
                });
            };

            _deviceManager.OnDeviceProductionUpdated += (id, current, target) =>
            {
                RunOnUI(() =>
                {
                    _view.UpdateDeviceProduction(id, current, target);
                    UpdateGlobalStats();
                });
            };

            // ---------- 订阅日志 ----------
            AppLogger.OnLogReceived += (level, logLine, category) =>
            {
                RunOnUI(() =>
                {
                    if (category == "Monitor" || category == "Modbus")
                        _view.AppendMonitorLog(logLine);
                    else
                        _view.AppendOperationLog(logLine);
                });
            };
        }

        // ================================================================
        // UI 线程调度辅助方法
        // ================================================================
        private void RunOnUI(Action action)
        {
            if (_view is Control control && control.InvokeRequired)
            {
                control.BeginInvoke(action);
            }
            else
            {
                action();
            }
        }

        // ================================================================
        // 视图加载
        // ================================================================
        private void OnLoadView(object sender, EventArgs e)
        {
            // OnLoadView 本身在 UI 线程触发，但内部调用可能涉及后台操作
            LoadDefaultDevices();
            RunOnUI(() =>
            {
                _view.SetSimulationMode(GtsModel.UseSimulation);
                UpdateDeviceList();
                UpdateGlobalStats();
                UpdateStatusBar();
            });
        }

        private void LoadDefaultDevices()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "devices.json");
            bool hasDevices = false;

            if (File.Exists(path))
            {
                try
                {
                    string json = File.ReadAllText(path);
                    if (_deviceManager.ImportConfig(json))
                    {
                        hasDevices = _deviceManager.GetAllDevices().Count > 0;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warn($"加载 devices.json 失败: {ex.Message}，将使用默认设备", "Init");
                }
            }

            if (!hasDevices)
            {
                var configs = new[]
                {
                    new DeviceConfig { DeviceId = "dev-001", Name = "设备1 - 焊接", Modbus = new ModbusConfig { IpAddress = "192.168.1.10", Port = 502, StartAddress = 0, RegisterCount = 10 }, Axis = 1, TargetCount = 100 },
                    new DeviceConfig { DeviceId = "dev-002", Name = "设备2 - 检测", Modbus = new ModbusConfig { IpAddress = "192.168.1.11", Port = 502, StartAddress = 100, RegisterCount = 5 }, Axis = 2, TargetCount = 100 },
                    new DeviceConfig { DeviceId = "dev-003", Name = "设备3 - 包装", Modbus = new ModbusConfig { IpAddress = "192.168.1.12", Port = 502, StartAddress = 200, RegisterCount = 8 }, Axis = 3, TargetCount = 100 }
                };
                foreach (var cfg in configs)
                    _deviceManager.AddDevice(cfg);

                _logger.Info("已加载默认设备（3台）", "Init");
            }
        }

        // ================================================================
        // 更新UI辅助（所有方法都通过 RunOnUI 调用）
        // ================================================================
        private void UpdateDeviceList()
        {
            // 此方法可能被后台事件调用，但调用方已经用 RunOnUI 包装
            var devices = _deviceManager.GetAllDevices();
            var items = devices.Select(d => new DeviceListItem
            {
                DeviceId = d.Config.DeviceId,
                Name = d.Config.Name,
                IsOnline = d.IsOnline
            });
            _view.UpdateDeviceList(items);

            // 保持当前选中的设备，只有当前选中无效时才切换到第一个
            string targetId = _selectedDeviceId;
            if (string.IsNullOrEmpty(targetId) || !devices.Any(d => d.Config.DeviceId == targetId))
            {
                targetId = devices.Count > 0 ? devices[0].Config.DeviceId : "";
            }

            if (!string.IsNullOrEmpty(targetId))
            {
                _view.SelectDevice(targetId);
                var device = _deviceManager.GetDevice(targetId);
                _view.UpdateCurrentDevice(device?.Config.Name ?? "");
            }
            else
            {
                _view.UpdateCurrentDevice("");
            }
        }

        private void UpdateGlobalStats()
        {
            var devices = _deviceManager.GetAllDevices();
            int online = devices.Count(d => d.IsOnline);
            int total = devices.Sum(d => d.Config.CurrentCount);
            _view.UpdateGlobalStats(online, devices.Count, total);
        }

        private void UpdateStatusBar()
        {
            var device = _deviceManager.GetDevice(_selectedDeviceId);
            if (device == null)
            {
                _view.UpdateStatusBar("", false, false, "正常", false, "空闲", 0, false);
                return;
            }

            bool isOnline = device.IsOnline;
            bool servoOn = false;
            string limitStatus = "正常";

            if (isOnline)
            {
                uint clk;
                int status = 0;
                _model.GetAxisStatus(device.Config.Axis, out status, out clk);
                servoOn = (status & 0x200) != 0;
                if ((status & 0x01) != 0) limitStatus = "正向限位";
                else if ((status & 0x02) != 0) limitStatus = "负向限位";
            }

            bool modbusConnected = device.ModbusClient?.IsConnected ?? false;
            int remaining = device.Watchdog.RemainingMs;
            bool timeout = device.Watchdog.IsTimeout;

            _view.UpdateStatusBar(
                device.Config.Name,
                isOnline,
                servoOn,
                limitStatus,
                modbusConnected,
                device.CurrentStep,
                remaining,
                timeout
            );
        }

        // ================================================================
        // 设备管理
        // ================================================================
        private void OnAddDevice(object sender, EventArgs e)
        {
            using (var form = new DeviceConfigForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    _deviceManager.AddDevice(form.Config);
                    RunOnUI(() =>
                    {
                        UpdateDeviceList();
                        UpdateGlobalStats();
                    });
                }
            }
        }

        private void OnRemoveDevice(object sender, EventArgs e)
        {
            string id = _view.GetSelectedDeviceId();
            if (string.IsNullOrEmpty(id)) return;
            var device = _deviceManager.GetDevice(id);
            if (device == null) return;

            if (_view.ShowConfirm($"确定移除设备 {device.Config.Name} 吗？", "确认删除"))
            {
                _deviceManager.RemoveDevice(id);
                RunOnUI(() =>
                {
                    UpdateDeviceList();
                    UpdateGlobalStats();
                    _view.UpdateAxisInfo("", 0, false, 0, 0);
                    UpdateStatusBar();
                });
            }
        }

        private void OnStartAll(object sender, EventArgs e)
        {
            var devices = _deviceManager.GetAllDevices();
            int offline = devices.Count(d => !d.IsOnline);
            if (offline > 0)
            {
                if (!_view.ShowConfirm($"有 {offline} 台设备未连接，是否跳过并启动已连接的？", "提示"))
                    return;
            }
            _deviceManager.StartAllDevices();
        }

        private void OnStopAll(object sender, EventArgs e)
        {
            _deviceManager.StopAllDevices();
            RunOnUI(UpdateStatusBar);
        }

        private void OnStartDevice(object sender, EventArgs e)
        {
            if (!CheckPermission("Device.Start")) return;
            string id = _view.GetSelectedDeviceId();
            if (string.IsNullOrEmpty(id)) return;
            var device = _deviceManager.GetDevice(id);
            if (device == null) return;
            if (!device.IsOnline)
            {
                _view.ShowMessage("设备未连接 Modbus，请先连接", "提示", MessageType.Warning);
                return;
            }
            _deviceManager.StartDevice(id);

            var user = SessionManager.CurrentUser;
            AuditService.Log(user.Id, user.Username, "StartDevice", $"启动设备 {id}", _repository); // ✅ 改为 id
        }

        private void OnStopDevice(object sender, EventArgs e)
        {
            string id = _view.GetSelectedDeviceId();
            if (!string.IsNullOrEmpty(id))
            {
                _deviceManager.StopDevice(id);
                RunOnUI(UpdateStatusBar);
            }
        }

        private void OnDeviceConfig(object sender, EventArgs e)
        {
            string id = _view.GetSelectedDeviceId();
            if (string.IsNullOrEmpty(id)) return;
            var device = _deviceManager.GetDevice(id);
            if (device == null) return;

            using (var form = new ModbusConfigForm(device.Config.Modbus))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    device.Config.Modbus = form.Config;
                    _logger.Info($"设备 {device.Config.Name} Modbus 配置已更新", "UI");
                    _view.ShowMessage("配置已保存，请重新连接 Modbus 生效", "提示", MessageType.Info);
                    RunOnUI(UpdateStatusBar);
                }
            }
        }

        // ================================================================
        // Modbus 连接控制
        // ================================================================
        private void OnToggleModbus(object sender, EventArgs e)
        {
            string id = _view.GetSelectedDeviceId();
            if (string.IsNullOrEmpty(id))
            {
                _view.ShowMessage("请先选择一个设备", "提示", MessageType.Warning);
                return;
            }

            var device = _deviceManager.GetDevice(id);
            if (device == null) return;

            if (device.IsOnline)
            {
                device.ModbusClient.Disconnect();
                _logger.Info($"设备 {device.Config.Name} Modbus 已断开", "UI");
            }
            else
            {
                bool success = device.ModbusClient.Connect();
                if (success)
                    _logger.Info($"设备 {device.Config.Name} Modbus 连接成功", "UI");
                else
                    _view.ShowMessage($"设备 {device.Config.Name} 连接失败", "错误", MessageType.Error);
            }

            RunOnUI(() =>
            {
                UpdateStatusBar();
                UpdateDeviceList();
            });
        }

        private void OnConnectAllModbus(object sender, EventArgs e)
        {
            var devices = _deviceManager.GetAllDevices();
            if (devices.Count == 0)
            {
                _view.ShowMessage("没有设备", "提示", MessageType.Info);
                return;
            }

            int success = 0, already = 0, fail = 0;
            var failedNames = new List<string>();
            foreach (var dev in devices)
            {
                if (dev.IsOnline) { already++; continue; }
                if (dev.ModbusClient.Connect()) success++;
                else { fail++; failedNames.Add(dev.Config.Name); }
            }

            string msg = $"连接完成：成功 {success}，已连接 {already}，失败 {fail}";
            if (fail > 0) msg += $"\n失败设备: {string.Join(", ", failedNames)}";
            _view.ShowMessage(msg, "批量连接", fail > 0 ? MessageType.Warning : MessageType.Info);

            RunOnUI(() =>
            {
                UpdateDeviceList();
                UpdateStatusBar();
            });
        }

        private void OnDisconnectAllModbus(object sender, EventArgs e)
        {
            var devices = _deviceManager.GetAllDevices();
            if (devices.Count == 0)
            {
                _view.ShowMessage("没有设备", "提示", MessageType.Info);
                return;
            }

            int success = 0, already = 0, fail = 0;
            var failedNames = new List<string>();
            foreach (var dev in devices)
            {
                if (!dev.IsOnline) { already++; continue; }
                try { dev.ModbusClient.Disconnect(); success++; }
                catch { fail++; failedNames.Add(dev.Config.Name); }
            }

            string msg = $"断开完成：成功 {success}，已断开 {already}，失败 {fail}";
            if (fail > 0) msg += $"\n失败设备: {string.Join(", ", failedNames)}";
            _view.ShowMessage(msg, "批量断开", fail > 0 ? MessageType.Warning : MessageType.Info);

            RunOnUI(() =>
            {
                UpdateDeviceList();
                UpdateStatusBar();
            });
        }

        // ================================================================
        // 轴控制
        // ================================================================
        private void OnHome(object sender, EventArgs e)
        {
            string id = _view.GetSelectedDeviceId();
            if (string.IsNullOrEmpty(id)) return;
            var device = _deviceManager.GetDevice(id);
            if (device == null || !device.IsOnline) return;
            _logger.Info($"轴 {device.Config.Axis} 回零已触发", "Axis");
            RunOnUI(UpdateStatusBar);
        }

        private void OnMoveAbs(object sender, EventArgs e)
        {
            string id = _view.GetSelectedDeviceId();
            if (string.IsNullOrEmpty(id)) return;
            var device = _deviceManager.GetDevice(id);
            if (device == null || !device.IsOnline) return;
            _logger.Info($"轴 {device.Config.Axis} 绝对定位已触发", "Axis");
        }

        private void OnJog(bool positive)
        {
            string id = _view.GetSelectedDeviceId();
            if (string.IsNullOrEmpty(id)) return;
            var device = _deviceManager.GetDevice(id);
            if (device == null || !device.IsOnline) return;
            _logger.Info($"轴 {device.Config.Axis} 点动 {(positive ? "正向" : "负向")} 已触发", "Axis");
        }

        private void OnStopAxis(object sender, EventArgs e)
        {
            string id = _view.GetSelectedDeviceId();
            if (string.IsNullOrEmpty(id)) return;
            var device = _deviceManager.GetDevice(id);
            if (device == null) return;
            _model.GT_Stop(1 << (device.Config.Axis - 1), 0);
            _logger.Info($"轴 {device.Config.Axis} 停止", "Axis");
            RunOnUI(UpdateStatusBar);
        }

        private void OnServoOn(object sender, EventArgs e)
        {
            string id = _view.GetSelectedDeviceId();
            if (string.IsNullOrEmpty(id)) return;
            var device = _deviceManager.GetDevice(id);
            if (device == null) return;
            _model.GT_AxisOn(device.Config.Axis);
            _logger.Info($"轴 {device.Config.Axis} 伺服使能", "Axis");
            RunOnUI(UpdateStatusBar);
        }

        private void OnServoOff(object sender, EventArgs e)
        {
            string id = _view.GetSelectedDeviceId();
            if (string.IsNullOrEmpty(id)) return;
            var device = _deviceManager.GetDevice(id);
            if (device == null) return;
            _model.GT_AxisOff(device.Config.Axis);
            _logger.Info($"轴 {device.Config.Axis} 伺服去使能", "Axis");
            RunOnUI(UpdateStatusBar);
        }

        private void OnAlarmReset(object sender, EventArgs e)
        {
            string id = _view.GetSelectedDeviceId();
            if (string.IsNullOrEmpty(id)) return;
            var device = _deviceManager.GetDevice(id);
            if (device == null) return;
            _model.GT_ClrSts(device.Config.Axis, 1);
            _logger.Info($"轴 {device.Config.Axis} 报警复位", "Axis");
            RunOnUI(UpdateStatusBar);
        }

        // ================================================================
        // 写寄存器 / 写线圈（核心业务逻辑）
        // ================================================================
        private void OnWriteRegister(object sender, WriteRegisterEventArgs e)
        {
            string id = _view.GetSelectedDeviceId();
            if (string.IsNullOrEmpty(id))
            {
                _view.ShowMessage("请先选择一个设备", "错误", MessageType.Error);
                return;
            }

            var device = _deviceManager.GetDevice(id);
            if (device == null || !device.IsOnline)
            {
                _view.ShowMessage("设备未在线", "错误", MessageType.Error);
                return;
            }

            ushort[] raw = ModbusClient.EncodeValue(e.Values, e.DataType, e.ByteOrder);
            if (raw.Length == 0)
            {
                _view.ShowMessage("编码数据为空", "错误", MessageType.Error);
                return;
            }

            bool success;
            if (raw.Length == 1)
                success = device.ModbusClient.WriteSingleRegister(e.Address, raw[0]);
            else
                success = device.ModbusClient.WriteMultipleRegisters(e.Address, raw);

            _view.ShowMessage(success ? "写入成功" : "写入失败", success ? "提示" : "错误",
                              success ? MessageType.Info : MessageType.Error);

            if (success)
                _logger.Info($"寄存器写入成功: 设备={device.Config.Name}, 地址={e.Address}, 长度={raw.Length}", "Modbus");
            else
                _logger.Error($"寄存器写入失败: 设备={device.Config.Name}, 地址={e.Address}", "Modbus");
        }

        private void OnWriteCoil(object sender, WriteCoilEventArgs e)
        {
            string id = _view.GetSelectedDeviceId();
            if (string.IsNullOrEmpty(id))
            {
                _view.ShowMessage("请先选择一个设备", "错误", MessageType.Error);
                return;
            }

            var device = _deviceManager.GetDevice(id);
            if (device == null || !device.IsOnline)
            {
                _view.ShowMessage("设备未在线", "错误", MessageType.Error);
                return;
            }

            bool success = device.ModbusClient.WriteSingleCoil(e.Address, e.Value);
            _view.ShowMessage(success ? "线圈写入成功" : "线圈写入失败", success ? "提示" : "错误",
                              success ? MessageType.Info : MessageType.Error);

            if (success)
                _logger.Info($"线圈写入成功: 设备={device.Config.Name}, 地址={e.Address}, 值={e.Value}", "Modbus");
            else
                _logger.Error($"线圈写入失败: 设备={device.Config.Name}, 地址={e.Address}", "Modbus");
        }

        // ================================================================
        // 其他全局功能
        // ================================================================
        private void OnToggleSimulator(object sender, EventArgs e)
        {
            bool isCurrentlySimulation = GtsModel.UseSimulation;
            bool willSwitchToReal = isCurrentlySimulation; // 如果当前是模拟，则要切换到真实

            // 只有切换到真实模式时才检测硬件
            if (willSwitchToReal)
            {
                if (!GtsModel.CheckHardwareAvailable())
                {
                    _view.ShowMessage(
                        "未检测到固高运动控制卡或驱动。\n请确认：\n1. 已安装 gts.dll 驱动\n2. 运动控制卡已正确连接",
                        "切换失败",
                        MessageType.Warning);
                    return;
                }

                // 进一步验证：尝试打开卡
                short openResult = _model.OpenDevice(0, 0);
                if (openResult != 0)
                {
                    _view.ShowMessage(
                        $"运动控制卡打开失败，错误码: {openResult}\n请检查硬件连接和电源。",
                        "切换失败",
                        MessageType.Error);
                    _model.CloseDevice();
                    return;
                }
                _model.CloseDevice();
                _logger.Info("硬件检测通过", "Operation");
            }

            // 执行切换
            _deviceManager.StopAllDevices();
            _model.CloseDevice();
            GtsModel.UseSimulation = !isCurrentlySimulation; // 取反切换
            _view.ClearLogs();
            _logger.Info(GtsModel.UseSimulation ? "模拟模式已开启" : "真实硬件模式已开启", "Operation");

            // 如果切换到真实模式，正式打开卡
            if (!GtsModel.UseSimulation)
            {
                short result = _model.OpenDevice(0, 0);
                if (result != 0)
                {
                    _view.ShowMessage($"打开卡失败，错误码: {result}，已退回模拟模式", "错误", MessageType.Error);
                    GtsModel.UseSimulation = true;
                    _logger.Warn("真实模式打开卡失败，退回模拟模式", "Operation");
                }
                else
                {
                    _logger.Info("运动控制卡已成功打开", "Operation");
                }
            }
            _view.SetSimulationMode(GtsModel.UseSimulation);
        }

        private void OnRunWorkflow(object sender, EventArgs e)
        {
            var devices = _deviceManager.GetAllDevices();
            if (devices.Count == 0)
            {
                _view.ShowMessage("没有设备可用", "提示", MessageType.Warning);
                return;
            }

            string selectedId = _view.GetSelectedDeviceId();
            var target = _deviceManager.GetDevice(selectedId) ?? devices[0];

            string workflowName = "Default";
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Workflows", workflowName + ".json");
            if (!File.Exists(filePath))
            {
                _view.ShowMessage($"工作流文件 {filePath} 不存在", "错误", MessageType.Error);
                return;
            }

            var config = LoadWorkflowFromJson(filePath);
            if (config == null) return;

            foreach (var cmdCfg in config.Commands)
                if (cmdCfg.Axis == 0) cmdCfg.Axis = target.Config.Axis;

            _workflowCts?.Cancel();
            _workflowCts = new CancellationTokenSource();
            var token = _workflowCts.Token;

            var commands = config.Commands.Select(cfg => CommandFactory.Create(_model, cfg)).ToList();
            var workflow = new SequenceCommand(commands.ToArray());
            workflow.OnLog += msg => _logger.Info(msg, "Workflow");

            var thread = new Thread(() => workflow.Execute(token)) { IsBackground = true };
            thread.Start();
            _logger.Info($"启动工作流: {config.Name} 设备: {target.Config.Name}", "Operation");
        }

        private void OnStopWorkflow(object sender, EventArgs e)
        {
            _workflowCts?.Cancel();
            _logger.Info("工作流已停止", "Operation");
        }

        private void OnEmergencyStop(object sender, EventArgs e)
        {
            _deviceManager.StopAllDevices();
            _model.GT_Stop(0xFF, 0);
            _logger.Warn("全局急停触发！", "Operation");
            _view.ShowMessage("全局急停已触发，所有运动停止", "急停", MessageType.Warning);
        }

        #region 保存配置
        private void OnSaveConfig(object sender, EventArgs e)
        {
            string json = _deviceManager.ExportConfig();
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "devices.json");
            File.WriteAllText(path, json);
            _logger.Info($"配置已保存至 {path}", "Operation");
            _view.ShowMessage($"配置已保存到 {path}", "提示", MessageType.Info);
        }
        #endregion

        // ================================================================
        // 设备选择（由视图调用）
        // ================================================================

        #region 设备选择
        public void OnDeviceSelected(string deviceId)
        {
            _selectedDeviceId = deviceId;
            RunOnUI(() =>
            {
                UpdateStatusBar();
                var device = _deviceManager.GetDevice(deviceId);
                _view.UpdateCurrentDevice(device?.Config.Name ?? "");   // ← 新增
                if (device != null)
                {
                    uint clk;
                    double pos = 0, vel = 0;
                    _model.GetPrfPos(device.Config.Axis, out pos, out clk);
                    _model.GetPrfVel(device.Config.Axis, out vel, out clk);
                    _view.UpdateAxisInfo(deviceId, device.Config.Axis, device.IsOnline, pos, vel);
                }
            });
        }
        #endregion

        // ================================================================
        // 辅助方法
        // ================================================================
        #region 加载工作流
        private WorkflowConfig LoadWorkflowFromJson(string path)
        {
            try
            {
                string json = File.ReadAllText(path);
                return System.Text.Json.JsonSerializer.Deserialize<WorkflowConfig>(json);
            }
            catch
            {
                return null;
            }
        }
        #endregion

        private bool CheckPermission(string permissionCode)
        {
            var user = SessionManager.CurrentUser;
            if (user == null)
            {
                _view.ShowMessage("请先登录！", "未登录", MessageType.Warning);
                return false;
            }
            if (!_authService.HasPermission(user, permissionCode))
            {
                _view.ShowMessage($"用户 {user.Username} 没有执行此操作的权限！", "权限不足", MessageType.Warning);
                return false;
            }
            return true;
        }
    }
}