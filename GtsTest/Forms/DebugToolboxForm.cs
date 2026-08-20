using GtsTest.Core;
using GtsTest.Modbus;
using GtsTest.Models;
using GtsTest.Services;
using GtsTest.Services.Alarm;
using GtsTest.Services.Authentication;
using GtsTest.Services.Data;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GtsTest.Forms
{
    public partial class DebugToolboxForm : Form
    {
        private readonly DeviceManager _deviceManager;
        private readonly GtsModel _model;
        private readonly IAlarmManager _alarmManager;
        private readonly IDataRepository _repo;
        private readonly IAuthenticationService _authService;

        private DeviceRuntime _currentDevice;
        private readonly string _currentUser;
        private readonly long _currentUserId;

        public DebugToolboxForm(
            DeviceManager deviceManager,
            GtsModel model,
            IAlarmManager alarmManager,
            IDataRepository repo,
            IAuthenticationService authService)
        {
            InitializeComponent();

            _deviceManager = deviceManager;
            _model = model;
            _alarmManager = alarmManager;
            _repo = repo;
            _authService = authService;

            // 获取当前用户（用于审计）
            var user = SessionManager.CurrentUser;
            _currentUser = user?.Username ?? "未登录";
            _currentUserId = user?.Id ?? 0;

            // 初始化 ComboBox 数据
            this.cmbDataType.Items.Clear();
            this.cmbDataType.Items.AddRange(Enum.GetNames(typeof(DataType)));
            this.cmbDataType.SelectedIndex = 0;

            this.cmbByteOrder.Items.Clear();
            this.cmbByteOrder.Items.AddRange(Enum.GetNames(typeof(ByteOrder)));
            this.cmbByteOrder.SelectedIndex = 0;

            this.cmbCoilValue.Items.Clear();
            this.cmbCoilValue.Items.Add("ON (1)");
            this.cmbCoilValue.Items.Add("OFF (0)");
            this.cmbCoilValue.SelectedIndex = 0;

            this.btnToggleMode.Text = GtsModel.UseSimulation ? "切换到真实" : "切换到模拟";

            LoadDevices();
            cmbDevice.SelectedIndexChanged += CmbDevice_SelectedIndexChanged;
            UpdateDeviceInfo();

            this.timerRefresh.Interval = 500;
            this.timerRefresh.Tick += TimerRefresh_Tick;
            this.timerRefresh.Start();

            // 绑定事件
            btnHome.Click += BtnHome_Click;
            btnMoveAbs.Click += BtnMoveAbs_Click;
            btnJogP.Click += BtnJogP_Click;
            btnJogN.Click += BtnJogN_Click;
            btnStopAxis.Click += BtnStopAxis_Click;
            btnServoOn.Click += BtnServoOn_Click;
            btnServoOff.Click += BtnServoOff_Click;
            btnAxisAlarmReset.Click += BtnAxisAlarmReset_Click;

            btnWriteRegister.Click += BtnWriteRegister_Click;
            btnWriteCoil.Click += BtnWriteCoil_Click;
            btnReadRegister.Click += BtnReadRegister_Click;

            btnStartDevice.Click += BtnStartDevice_Click;
            btnStopDevice.Click += BtnStopDevice_Click;
            btnDeviceConfig.Click += BtnDeviceConfig_Click;

            btnToggleMode.Click += BtnToggleMode_Click;
            btnHotReload.Click += BtnHotReload_Click;
            btnSaveConfig.Click += BtnSaveConfig_Click;
            btnDumpBlackBox.Click += BtnDumpBlackBox_Click;

            AppLogger.Info($"调试工具箱已打开，用户: {_currentUser}", "DebugToolbox");
        }

        private void LoadDevices()
        {
            cmbDevice.Items.Clear();
            var devices = _deviceManager.GetAllDevices();
            foreach (var d in devices)
            {
                cmbDevice.Items.Add(d.Config.Name);
            }
            if (cmbDevice.Items.Count > 0)
                cmbDevice.SelectedIndex = 0;
        }

        private void CmbDevice_SelectedIndexChanged(object sender, EventArgs e)
        {
            var name = cmbDevice.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(name)) return;
            var device = _deviceManager.GetAllDevices().FirstOrDefault(d => d.Config.Name == name);
            _currentDevice = device;
            UpdateDeviceInfo();
        }

        private void UpdateDeviceInfo()
        {
            if (_currentDevice == null)
            {
                lblDeviceName.Text = "未选择";
                lblDeviceStatus.Text = "离线";
                lblDeviceStatus.ForeColor = Color.Gray;
                lblProdInfo.Text = "0/0";
                return;
            }
            lblDeviceName.Text = _currentDevice.Config.Name;
            lblDeviceStatus.Text = _currentDevice.IsOnline ? "在线" : "离线";
            lblDeviceStatus.ForeColor = _currentDevice.IsOnline ? Color.Green : Color.Red;
            lblProdInfo.Text = $"{_currentDevice.Config.CurrentCount}/{_currentDevice.Config.TargetCount}";

            if (_currentDevice.IsOnline)
            {
                uint clk;
                int status;
                _model.GetAxisStatus(_currentDevice.Config.Axis, out status, out clk);
                double pos, vel;
                _model.GetPrfPos(_currentDevice.Config.Axis, out pos, out clk);
                _model.GetPrfVel(_currentDevice.Config.Axis, out vel, out clk);
                lblAxisValue.Text = _currentDevice.Config.Axis.ToString();
                lblAxisStatus.Text = (status & 0x200) != 0 ? "已使能" : "未使能";
                lblAxisPos.Text = pos.ToString("F2");
                lblAxisVel.Text = vel.ToString("F2");
            }
            else
            {
                lblAxisValue.Text = "--";
                lblAxisStatus.Text = "离线";
                lblAxisPos.Text = "--";
                lblAxisVel.Text = "--";
            }
        }

        private void TimerRefresh_Tick(object sender, EventArgs e)
        {
            if (this.IsDisposed) return;
            UpdateDeviceInfo();
        }

        // ========== 轴控制 ==========
        private void BtnHome_Click(object sender, EventArgs e)
        {
            if (_currentDevice == null || !_currentDevice.IsOnline)
            {
                AppLogger.Warn($"回零操作失败：设备未选择或不在线", "DebugToolbox");
                MessageBox.Show("设备未选择或不在线", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            short axis = _currentDevice.Config.Axis;
            int homePos = _currentDevice.Config.HomePosition;
            try
            {
                short result = _model.HomeAxis(axis, homePos);
                if (result != 0)
                {
                    AppLogger.Error($"回零失败: 设备={_currentDevice.Config.Name}, 轴={axis}, 错误码={result}", "DebugToolbox");
                    MessageBox.Show($"回零启动失败，错误码: {result}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                AppLogger.Info($"回零已启动: 设备={_currentDevice.Config.Name}, 轴={axis}, 目标位置={homePos}", "DebugToolbox");
                AuditService.Log(_currentUserId, _currentUser, "DebugHome", $"设备={_currentDevice.Config.Name}, 轴={axis}, 目标位置={homePos}", _repo);
                MessageBox.Show("回零已启动", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                AppLogger.Error($"回零异常: {ex.Message}", "DebugToolbox");
                MessageBox.Show($"回零异常: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnMoveAbs_Click(object sender, EventArgs e)
        {
            if (_currentDevice == null || !_currentDevice.IsOnline)
            {
                AppLogger.Warn($"定位操作失败：设备未选择或不在线", "DebugToolbox");
                MessageBox.Show("设备未选择或不在线", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!int.TryParse(txtTargetPos.Text, out int target))
            {
                AppLogger.Warn($"定位操作失败：无效目标位置 '{txtTargetPos.Text}'", "DebugToolbox");
                MessageBox.Show("请输入有效目标位置", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            short axis = _currentDevice.Config.Axis;
            double vel = 10, acc = 5;
            if (!double.TryParse(txtJogSpeed.Text, out vel)) vel = 10;
            if (!double.TryParse(txtAcc.Text, out acc)) acc = 5;
            try
            {
                short result = _model.MoveAbs(axis, target, vel, acc);
                if (result != 0)
                {
                    AppLogger.Error($"定位失败: 设备={_currentDevice.Config.Name}, 轴={axis}, 目标={target}, 错误码={result}", "DebugToolbox");
                    MessageBox.Show($"定位启动失败，错误码: {result}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                AppLogger.Info($"定位已启动: 设备={_currentDevice.Config.Name}, 轴={axis}, 目标={target}, 速度={vel}, 加速度={acc}", "DebugToolbox");
                AuditService.Log(_currentUserId, _currentUser, "DebugMoveAbs", $"设备={_currentDevice.Config.Name}, 轴={axis}, 目标={target}, 速度={vel}", _repo);
                MessageBox.Show("定位已启动", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                AppLogger.Error($"定位异常: {ex.Message}", "DebugToolbox");
                MessageBox.Show($"定位异常: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnJogP_Click(object sender, EventArgs e)
        {
            if (_currentDevice == null || !_currentDevice.IsOnline)
            {
                AppLogger.Warn($"点动+操作失败：设备未选择或不在线", "DebugToolbox");
                MessageBox.Show("设备未选择或不在线", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            short axis = _currentDevice.Config.Axis;
            double speed = 10;
            if (!double.TryParse(txtJogSpeed.Text, out speed)) speed = 10;
            try
            {
                short result = _model.StartJog(axis, speed, true);
                if (result != 0)
                {
                    AppLogger.Error($"点动+启动失败: 设备={_currentDevice.Config.Name}, 轴={axis}, 速度={speed}, 错误码={result}", "DebugToolbox");
                    MessageBox.Show($"点动启动失败，错误码: {result}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                AppLogger.Info($"点动+已启动: 设备={_currentDevice.Config.Name}, 轴={axis}, 速度={speed}", "DebugToolbox");
                AuditService.Log(_currentUserId, _currentUser, "DebugJogP", $"设备={_currentDevice.Config.Name}, 轴={axis}, 速度={speed}", _repo);
            }
            catch (Exception ex)
            {
                AppLogger.Error($"点动+异常: {ex.Message}", "DebugToolbox");
                MessageBox.Show($"点动异常: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnJogN_Click(object sender, EventArgs e)
        {
            if (_currentDevice == null || !_currentDevice.IsOnline)
            {
                AppLogger.Warn($"点动-操作失败：设备未选择或不在线", "DebugToolbox");
                MessageBox.Show("设备未选择或不在线", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            short axis = _currentDevice.Config.Axis;
            double speed = 10;
            if (!double.TryParse(txtJogSpeed.Text, out speed)) speed = 10;
            try
            {
                short result = _model.StartJog(axis, speed, false);
                if (result != 0)
                {
                    AppLogger.Error($"点动-启动失败: 设备={_currentDevice.Config.Name}, 轴={axis}, 速度={speed}, 错误码={result}", "DebugToolbox");
                    MessageBox.Show($"点动启动失败，错误码: {result}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                AppLogger.Info($"点动-已启动: 设备={_currentDevice.Config.Name}, 轴={axis}, 速度={speed}", "DebugToolbox");
                AuditService.Log(_currentUserId, _currentUser, "DebugJogN", $"设备={_currentDevice.Config.Name}, 轴={axis}, 速度={speed}", _repo);
            }
            catch (Exception ex)
            {
                AppLogger.Error($"点动-异常: {ex.Message}", "DebugToolbox");
                MessageBox.Show($"点动异常: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnStopAxis_Click(object sender, EventArgs e)
        {
            if (_currentDevice == null)
            {
                AppLogger.Warn($"停止轴操作失败：设备未选择", "DebugToolbox");
                MessageBox.Show("设备未选择", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            short axis = _currentDevice.Config.Axis;
            try
            {
                _model.GT_Stop(1 << (axis - 1), 0);
                AppLogger.Info($"轴停止: 设备={_currentDevice.Config.Name}, 轴={axis}", "DebugToolbox");
                AuditService.Log(_currentUserId, _currentUser, "DebugStopAxis", $"设备={_currentDevice.Config.Name}, 轴={axis}", _repo);
                MessageBox.Show("轴运动已停止", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                AppLogger.Error($"停止轴异常: {ex.Message}", "DebugToolbox");
                MessageBox.Show($"停止轴异常: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnServoOn_Click(object sender, EventArgs e)
        {
            if (_currentDevice == null || !_currentDevice.IsOnline)
            {
                AppLogger.Warn($"伺服使能失败：设备未选择或不在线", "DebugToolbox");
                MessageBox.Show("设备未选择或不在线", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            short axis = _currentDevice.Config.Axis;
            try
            {
                short result = _model.GT_AxisOn(axis);
                if (result != 0)
                {
                    AppLogger.Error($"伺服使能失败: 设备={_currentDevice.Config.Name}, 轴={axis}, 错误码={result}", "DebugToolbox");
                    MessageBox.Show($"使能失败，错误码: {result}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                AppLogger.Info($"伺服使能成功: 设备={_currentDevice.Config.Name}, 轴={axis}", "DebugToolbox");
                AuditService.Log(_currentUserId, _currentUser, "DebugServoOn", $"设备={_currentDevice.Config.Name}, 轴={axis}", _repo);
                MessageBox.Show("伺服已使能", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                UpdateDeviceInfo();
            }
            catch (Exception ex)
            {
                AppLogger.Error($"伺服使能异常: {ex.Message}", "DebugToolbox");
                MessageBox.Show($"使能异常: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnServoOff_Click(object sender, EventArgs e)
        {
            if (_currentDevice == null || !_currentDevice.IsOnline)
            {
                AppLogger.Warn($"伺服去使能失败：设备未选择或不在线", "DebugToolbox");
                MessageBox.Show("设备未选择或不在线", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            short axis = _currentDevice.Config.Axis;
            try
            {
                short result = _model.GT_AxisOff(axis);
                if (result != 0)
                {
                    AppLogger.Error($"伺服去使能失败: 设备={_currentDevice.Config.Name}, 轴={axis}, 错误码={result}", "DebugToolbox");
                    MessageBox.Show($"去使能失败，错误码: {result}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                AppLogger.Info($"伺服去使能成功: 设备={_currentDevice.Config.Name}, 轴={axis}", "DebugToolbox");
                AuditService.Log(_currentUserId, _currentUser, "DebugServoOff", $"设备={_currentDevice.Config.Name}, 轴={axis}", _repo);
                MessageBox.Show("伺服已去使能", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                UpdateDeviceInfo();
            }
            catch (Exception ex)
            {
                AppLogger.Error($"伺服去使能异常: {ex.Message}", "DebugToolbox");
                MessageBox.Show($"去使能异常: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAxisAlarmReset_Click(object sender, EventArgs e)
        {
            if (_currentDevice == null || !_currentDevice.IsOnline)
            {
                AppLogger.Warn($"轴报警复位失败：设备未选择或不在线", "DebugToolbox");
                MessageBox.Show("设备未选择或不在线", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            short axis = _currentDevice.Config.Axis;
            try
            {
                short result = _model.GT_ClrSts(axis, 1);
                if (result != 0)
                {
                    AppLogger.Error($"轴报警复位失败: 设备={_currentDevice.Config.Name}, 轴={axis}, 错误码={result}", "DebugToolbox");
                    MessageBox.Show($"复位报警失败，错误码: {result}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                AppLogger.Info($"轴报警复位成功: 设备={_currentDevice.Config.Name}, 轴={axis}", "DebugToolbox");
                AuditService.Log(_currentUserId, _currentUser, "DebugAxisReset", $"设备={_currentDevice.Config.Name}, 轴={axis}", _repo);
                MessageBox.Show("轴报警已复位", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                AppLogger.Error($"轴报警复位异常: {ex.Message}", "DebugToolbox");
                MessageBox.Show($"复位报警异常: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ========== Modbus ==========
        private void BtnWriteRegister_Click(object sender, EventArgs e)
        {
            if (_currentDevice == null || !_currentDevice.IsOnline)
            {
                AppLogger.Warn($"Modbus写寄存器失败：设备未选择或不在线", "DebugToolbox");
                MessageBox.Show("设备未选择或不在线", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            ushort address = (ushort)numRegAddress.Value;
            DataType dataType = (DataType)cmbDataType.SelectedIndex;
            ByteOrder byteOrder = (ByteOrder)cmbByteOrder.SelectedIndex;
            var parts = txtRegValues.Text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var values = new List<object>();
            foreach (var part in parts)
            {
                try
                {
                    object converted = dataType switch
                    {
                        DataType.Int16 => Convert.ToInt16(part),
                        DataType.UInt16 => Convert.ToUInt16(part),
                        DataType.Int32 => Convert.ToInt32(part),
                        DataType.UInt32 => Convert.ToUInt32(part),
                        DataType.Float => Convert.ToSingle(part),
                        DataType.Double => Convert.ToDouble(part),
                        _ => throw new NotSupportedException()
                    };
                    values.Add(converted);
                }
                catch
                {
                    string msg = $"值 '{part}' 无法转换为 {dataType}";
                    AppLogger.Warn($"Modbus写寄存器解析失败: {msg}", "DebugToolbox");
                    MessageBox.Show(msg, "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            if (values.Count == 0)
            {
                AppLogger.Warn("Modbus写寄存器：未提供有效值", "DebugToolbox");
                MessageBox.Show("请至少输入一个有效值", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                ushort[] raw = ModbusClient.EncodeValue(values.ToArray(), dataType, byteOrder);
                if (raw.Length == 0)
                {
                    AppLogger.Warn("Modbus写寄存器编码失败", "DebugToolbox");
                    MessageBox.Show("编码数据为空", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                bool success;
                if (raw.Length == 1)
                    success = _currentDevice.ModbusClient.WriteSingleRegister(address, raw[0]);
                else
                    success = _currentDevice.ModbusClient.WriteMultipleRegisters(address, raw);

                if (success)
                {
                    AppLogger.Info($"Modbus写寄存器成功: 设备={_currentDevice.Config.Name}, 地址={address}, 类型={dataType}, 值={string.Join(",", values)}", "DebugToolbox");
                    AuditService.Log(_currentUserId, _currentUser, "DebugModbusWriteReg", $"设备={_currentDevice.Config.Name}, 地址={address}, 类型={dataType}, 值={string.Join(",", values)}", _repo);
                    MessageBox.Show("写入成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    AppLogger.Error($"Modbus写寄存器失败: 设备={_currentDevice.Config.Name}, 地址={address}", "DebugToolbox");
                    MessageBox.Show("写入失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error($"Modbus写寄存器异常: {ex.Message}", "DebugToolbox");
                MessageBox.Show($"写入异常: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnWriteCoil_Click(object sender, EventArgs e)
        {
            if (_currentDevice == null || !_currentDevice.IsOnline)
            {
                AppLogger.Warn($"Modbus写线圈失败：设备未选择或不在线", "DebugToolbox");
                MessageBox.Show("设备未选择或不在线", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            ushort address = (ushort)numCoilAddress.Value;
            bool value = cmbCoilValue.SelectedIndex == 0;
            try
            {
                bool success = _currentDevice.ModbusClient.WriteSingleCoil(address, value);
                if (success)
                {
                    AppLogger.Info($"Modbus写线圈成功: 设备={_currentDevice.Config.Name}, 地址={address}, 值={value}", "DebugToolbox");
                    AuditService.Log(_currentUserId, _currentUser, "DebugModbusWriteCoil", $"设备={_currentDevice.Config.Name}, 地址={address}, 值={value}", _repo);
                    MessageBox.Show("线圈写入成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    AppLogger.Error($"Modbus写线圈失败: 设备={_currentDevice.Config.Name}, 地址={address}", "DebugToolbox");
                    MessageBox.Show("线圈写入失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error($"Modbus写线圈异常: {ex.Message}", "DebugToolbox");
                MessageBox.Show($"写入异常: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnReadRegister_Click(object sender, EventArgs e)
        {
            if (_currentDevice == null || !_currentDevice.IsOnline)
            {
                AppLogger.Warn($"Modbus读寄存器失败：设备未选择或不在线", "DebugToolbox");
                MessageBox.Show("设备未选择或不在线", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            ushort address = (ushort)numReadAddress.Value;
            ushort count = (ushort)numReadCount.Value;
            try
            {
                var result = _currentDevice.ModbusClient.ReadHoldingRegistersWithRaw(address, count);
                if (result == null)
                {
                    AppLogger.Error($"Modbus读寄存器失败: 设备={_currentDevice.Config.Name}, 地址={address}, 数量={count}", "DebugToolbox");
                    MessageBox.Show("读取失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                string rawStr = string.Join(", ", result.RawRegisters);
                string valStr = result.ConvertedValue?.ToString() ?? "null";
                txtReadResult.Text = $"原始: [{rawStr}]\n转换: {valStr}";
                AppLogger.Info($"Modbus读寄存器成功: 设备={_currentDevice.Config.Name}, 地址={address}, 数量={count}, 原始值=[{rawStr}]", "DebugToolbox");
                AuditService.Log(_currentUserId, _currentUser, "DebugModbusReadReg", $"设备={_currentDevice.Config.Name}, 地址={address}, 数量={count}, 原始值=[{rawStr}]", _repo);
            }
            catch (Exception ex)
            {
                AppLogger.Error($"Modbus读寄存器异常: {ex.Message}", "DebugToolbox");
                MessageBox.Show($"读取异常: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ========== 设备控制 ==========
        private void BtnStartDevice_Click(object sender, EventArgs e)
        {
            if (_currentDevice == null)
            {
                AppLogger.Warn("启动设备失败：未选择设备", "DebugToolbox");
                MessageBox.Show("未选择设备", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!_currentDevice.IsOnline)
            {
                AppLogger.Warn($"启动设备失败：设备不在线 ({_currentDevice.Config.Name})", "DebugToolbox");
                MessageBox.Show("设备不在线，无法启动", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                if (_deviceManager.StartDevice(_currentDevice.Config.DeviceId))
                {
                    AppLogger.Info($"设备启动成功: {_currentDevice.Config.Name}", "DebugToolbox");
                    AuditService.Log(_currentUserId, _currentUser, "DebugStartDevice", $"设备={_currentDevice.Config.Name}", _repo);
                    MessageBox.Show("设备启动成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    AppLogger.Error($"设备启动失败: {_currentDevice.Config.Name}", "DebugToolbox");
                    MessageBox.Show("设备启动失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error($"设备启动异常: {ex.Message}", "DebugToolbox");
                MessageBox.Show($"启动异常: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnStopDevice_Click(object sender, EventArgs e)
        {
            if (_currentDevice == null)
            {
                AppLogger.Warn("停止设备失败：未选择设备", "DebugToolbox");
                MessageBox.Show("未选择设备", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                if (_deviceManager.StopDevice(_currentDevice.Config.DeviceId))
                {
                    AppLogger.Info($"设备停止成功: {_currentDevice.Config.Name}", "DebugToolbox");
                    AuditService.Log(_currentUserId, _currentUser, "DebugStopDevice", $"设备={_currentDevice.Config.Name}", _repo);
                    MessageBox.Show("设备已停止", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    AppLogger.Error($"设备停止失败: {_currentDevice.Config.Name}", "DebugToolbox");
                    MessageBox.Show("设备停止失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error($"设备停止异常: {ex.Message}", "DebugToolbox");
                MessageBox.Show($"停止异常: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDeviceConfig_Click(object sender, EventArgs e)
        {
            if (_currentDevice == null)
            {
                AppLogger.Warn("配置设备失败：未选择设备", "DebugToolbox");
                MessageBox.Show("未选择设备", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                using (var form = new ModbusConfigForm(_currentDevice.Config.Modbus))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        _currentDevice.Config.Modbus = form.Config;
                        AppLogger.Info($"Modbus配置已更新: 设备={_currentDevice.Config.Name}", "DebugToolbox");
                        AuditService.Log(_currentUserId, _currentUser, "DebugModbusConfig", $"设备={_currentDevice.Config.Name}", _repo);
                        MessageBox.Show("配置已保存，请重新连接 Modbus", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error($"配置设备异常: {ex.Message}", "DebugToolbox");
                MessageBox.Show($"配置异常: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ========== 系统模式 ==========
        private void BtnToggleMode_Click(object sender, EventArgs e)
        {
            bool isSim = GtsModel.UseSimulation;
            try
            {
                if (!isSim)
                {
                    if (!GtsModel.CheckHardwareAvailable())
                    {
                        AppLogger.Warn("切换到真实模式失败：未检测到硬件", "DebugToolbox");
                        MessageBox.Show("未检测到硬件，无法切换到真实模式", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    short result = _model.OpenDevice(0, 0);
                    if (result != 0)
                    {
                        AppLogger.Error($"切换到真实模式失败：打开卡错误码 {result}", "DebugToolbox");
                        MessageBox.Show($"打开卡失败，错误码: {result}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        _model.CloseDevice();
                        return;
                    }
                    _model.CloseDevice();
                }
                GtsModel.UseSimulation = !isSim;
                btnToggleMode.Text = GtsModel.UseSimulation ? "切换到真实" : "切换到模拟";
                AppLogger.Info($"系统模式已切换: {(GtsModel.UseSimulation ? "模拟" : "真实")}", "DebugToolbox");
                AuditService.Log(_currentUserId, _currentUser, "DebugToggleMode", $"模式={(GtsModel.UseSimulation ? "模拟" : "真实")}", _repo);
                MessageBox.Show($"已切换到 {(GtsModel.UseSimulation ? "模拟" : "真实")} 模式", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                AppLogger.Error($"模式切换异常: {ex.Message}", "DebugToolbox");
                MessageBox.Show($"切换异常: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnHotReload_Click(object sender, EventArgs e)
        {
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "devices.json");
            if (!System.IO.File.Exists(path))
            {
                AppLogger.Warn($"热加载失败：配置文件不存在 {path}", "DebugToolbox");
                MessageBox.Show("配置文件不存在", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                string json = System.IO.File.ReadAllText(path);
                if (_deviceManager.ImportConfig(json))
                {
                    LoadDevices();
                    AppLogger.Info($"热加载配置成功: {path}", "DebugToolbox");
                    AuditService.Log(_currentUserId, _currentUser, "DebugHotReload", $"配置文件={path}", _repo);
                    MessageBox.Show("配置热加载成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    AppLogger.Error($"热加载配置失败: {path}", "DebugToolbox");
                    MessageBox.Show("配置加载失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error($"热加载异常: {ex.Message}", "DebugToolbox");
                MessageBox.Show($"加载异常: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSaveConfig_Click(object sender, EventArgs e)
        {
            try
            {
                string json = _deviceManager.ExportConfig();
                string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "devices.json");
                System.IO.File.WriteAllText(path, json);
                AppLogger.Info($"配置已保存: {path}", "DebugToolbox");
                AuditService.Log(_currentUserId, _currentUser, "DebugSaveConfig", $"配置文件={path}", _repo);
                MessageBox.Show($"配置已保存到 {path}", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                AppLogger.Error($"保存配置异常: {ex.Message}", "DebugToolbox");
                MessageBox.Show($"保存异常: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDumpBlackBox_Click(object sender, EventArgs e)
        {
            try
            {
                var file = CyclicMonitorBuffer.DumpToFile("调试工具箱手动导出");
                if (file != null)
                {
                    AppLogger.Info($"黑匣子导出成功: {file}", "DebugToolbox");
                    AuditService.Log(_currentUserId, _currentUser, "DebugDumpBlackBox", $"导出文件={file}", _repo);
                    MessageBox.Show($"黑匣子已导出到 {file}", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    AppLogger.Warn("黑匣子导出失败：缓冲区为空", "DebugToolbox");
                    MessageBox.Show("黑匣子导出失败，缓冲区为空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error($"黑匣子导出异常: {ex.Message}", "DebugToolbox");
                MessageBox.Show($"导出异常: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DebugToolboxForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            timerRefresh.Stop();
            AppLogger.Info($"调试工具箱已关闭", "DebugToolbox");
        }
    }
}