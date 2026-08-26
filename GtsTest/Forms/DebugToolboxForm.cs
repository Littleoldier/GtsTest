using GtsTest.Commands;
using GtsTest.Core;
using GtsTest.Modbus;
using GtsTest.Models;
using GtsTest.Services.Alarm;
using GtsTest.Services.Authentication;
using GtsTest.Services.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

            var user = SessionManager.CurrentUser;
            _currentUser = user?.Username ?? "未登录";
            _currentUserId = user?.Id ?? 0;

            LoadDevices();
            cmbDevice.SelectedIndexChanged += CmbDevice_SelectedIndexChanged;
            UpdateDeviceInfo();

            timerRefresh.Tick += TimerRefresh_Tick;
            timerRefresh.Start();

            // 绑定轴控制事件
            btnHome.Click += BtnHome_Click;
            btnMoveAbs.Click += BtnMoveAbs_Click;
            btnJogP.Click += BtnJogP_Click;
            btnJogN.Click += BtnJogN_Click;
            btnStopAxis.Click += BtnStopAxis_Click;
            btnServoOn.Click += BtnServoOn_Click;
            btnServoOff.Click += BtnServoOff_Click;
            btnAxisAlarmReset.Click += BtnAxisAlarmReset_Click;

            // 绑定 Modbus 事件
            btnWriteRegister.Click += BtnWriteRegister_Click;
            btnWriteCoil.Click += BtnWriteCoil_Click;
            btnReadRegister.Click += BtnReadRegister_Click;

            // 绑定设备控制事件
            btnStartDevice.Click += BtnStartDevice_Click;
            btnStopDevice.Click += BtnStopDevice_Click;
            btnDeviceConfig.Click += BtnDeviceConfig_Click;

            // 绑定 Modbus 连接/断开事件
            btnConnectModbus.Click += BtnConnectModbus_Click;
            btnDisconnectModbus.Click += BtnDisconnectModbus_Click;

            // 绑定视觉触发事件
            btnTriggerVision.Click += BtnTriggerVision_Click;

            // 初始化视觉状态
            UpdateVisionStatus("就绪，等待触发...", Color.DimGray);

            AppLogger.Info($"调试工具箱已打开，用户: {_currentUser}", "DebugToolbox");
        }

        // ================================================================
        //  设备加载与 UI 更新
        // ================================================================

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
            // ---- 更新轴信息和状态 ----
            if (_currentDevice == null)
            {
                lblDeviceName.Text = "未选择";
                lblDeviceStatus.Text = "离线";
                lblDeviceStatus.ForeColor = Color.Gray;
                lblProdInfo.Text = "0/0";
                lblAxisValue.Text = "--";
                lblAxisStatus.Text = "--";
                lblAxisPos.Text = "--";
                lblAxisVel.Text = "--";
                // 更新设备标签
                lblCurrentDevice.Text = "当前设备: 未选择";
                // 更新 Modbus 按钮状态
                UpdateModbusButtonState();
                return;
            }

            lblDeviceName.Text = _currentDevice.Config.Name;
            // 设备状态：根据 Modbus 连接状态显示
            bool modbusConnected = _currentDevice.ModbusClient?.IsConnected ?? false;
            lblDeviceStatus.Text = modbusConnected ? "在线" : "离线";
            lblDeviceStatus.ForeColor = modbusConnected ? Color.Green : Color.Red;
            lblProdInfo.Text = $"{_currentDevice.Config.CurrentCount}/{_currentDevice.Config.TargetCount}";

            // 轴信息（仅当 Modbus 已连接时尝试读取，否则显示 --）
            if (modbusConnected)
            {
                try
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
                catch
                {
                    // 如果读取失败，显示占位
                    lblAxisValue.Text = _currentDevice.Config.Axis.ToString();
                    lblAxisStatus.Text = "--";
                    lblAxisPos.Text = "--";
                    lblAxisVel.Text = "--";
                }
            }
            else
            {
                lblAxisValue.Text = _currentDevice.Config.Axis.ToString();
                lblAxisStatus.Text = "离线";
                lblAxisPos.Text = "--";
                lblAxisVel.Text = "--";
            }

            // ---- 更新当前设备标签（含 Modbus 状态） ----
            string statusText = modbusConnected ? "Modbus已连接" : "Modbus未连接";
            lblCurrentDevice.Text = $"当前设备: {_currentDevice.Config.Name} ({statusText})";

            // ---- 更新 Modbus 按钮状态 ----
            UpdateModbusButtonState();
        }

        private void UpdateModbusButtonState()
        {
            // 只要设备存在且 ModbusClient 不为空，连接按钮就可根据当前连接状态启用
            if (_currentDevice != null && _currentDevice.ModbusClient != null)
            {
                bool connected = _currentDevice.ModbusClient.IsConnected;
                btnConnectModbus.Enabled = !connected;
                btnDisconnectModbus.Enabled = connected;
            }
            else
            {
                btnConnectModbus.Enabled = false;
                btnDisconnectModbus.Enabled = false;
            }
        }

        private void TimerRefresh_Tick(object sender, EventArgs e)
        {
            if (this.IsDisposed) return;
            UpdateDeviceInfo();
        }

        // ================================================================
        //  视觉状态更新
        // ================================================================

        private void UpdateVisionStatus(string message, Color color)
        {
            if (lblVisionStatus == null || lblVisionStatus.IsDisposed) return;
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateVisionStatus(message, color)));
                return;
            }
            lblVisionStatus.Text = message;
            lblVisionStatus.ForeColor = color;
        }

        // ================================================================
        //  轴控制
        // ================================================================

        private bool ValidateDevice()
        {
            if (_currentDevice == null)
            {
                MessageBox.Show("设备未选择", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            // 允许即使 Modbus 未连接也执行操作（某些操作可能不需要连接）
            // 但轴操作需要连接，所以我们检查连接状态
            if (!_currentDevice.ModbusClient.IsConnected)
            {
                MessageBox.Show("Modbus 未连接，请先连接设备", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void BtnHome_Click(object sender, EventArgs e)
        {
            if (!ValidateDevice()) return;
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
            if (!ValidateDevice()) return;
            if (!int.TryParse(txtTargetPos.Text, out int target))
            {
                MessageBox.Show("请输入有效目标位置", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            short axis = _currentDevice.Config.Axis;
            double vel = 10, acc = 5;
            double.TryParse(txtJogSpeed.Text, out vel);
            double.TryParse(txtAcc.Text, out acc);
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
            if (!ValidateDevice()) return;
            short axis = _currentDevice.Config.Axis;
            double speed = 10;
            double.TryParse(txtJogSpeed.Text, out speed);
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
            if (!ValidateDevice()) return;
            short axis = _currentDevice.Config.Axis;
            double speed = 10;
            double.TryParse(txtJogSpeed.Text, out speed);
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
                MessageBox.Show("设备未选择", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!_currentDevice.ModbusClient.IsConnected)
            {
                MessageBox.Show("Modbus 未连接", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            if (!ValidateDevice()) return;
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
            if (!ValidateDevice()) return;
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
            if (!ValidateDevice()) return;
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

        // ================================================================
        //  Modbus 读写
        // ================================================================

        private bool ValidateModbusDevice()
        {
            if (_currentDevice == null)
            {
                MessageBox.Show("设备未选择", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!_currentDevice.ModbusClient.IsConnected)
            {
                MessageBox.Show("Modbus 未连接，请先连接设备", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void BtnWriteRegister_Click(object sender, EventArgs e)
        {
            if (!ValidateModbusDevice()) return;
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
                    MessageBox.Show($"值 '{part}' 无法转换为 {dataType}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            if (values.Count == 0)
            {
                MessageBox.Show("请至少输入一个有效值", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                ushort[] raw = ModbusClient.EncodeValue(values.ToArray(), dataType, byteOrder);
                if (raw.Length == 0)
                {
                    MessageBox.Show("编码数据为空", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                bool success = raw.Length == 1
                    ? _currentDevice.ModbusClient.WriteSingleRegister(address, raw[0])
                    : _currentDevice.ModbusClient.WriteMultipleRegisters(address, raw);
                if (success)
                {
                    AppLogger.Info($"Modbus写寄存器成功: 设备={_currentDevice.Config.Name}, 地址={address}, 类型={dataType}, 值={string.Join(",", values)}", "DebugToolbox");
                    AuditService.Log(_currentUserId, _currentUser, "DebugModbusWriteReg", $"设备={_currentDevice.Config.Name}, 地址={address}, 类型={dataType}, 值={string.Join(",", values)}", _repo);
                    MessageBox.Show("写入成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
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
            if (!ValidateModbusDevice()) return;
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
            if (!ValidateModbusDevice()) return;
            ushort address = (ushort)numReadAddress.Value;
            ushort count = (ushort)numReadCount.Value;
            try
            {
                var result = _currentDevice.ModbusClient.ReadHoldingRegistersWithRaw(address, count);
                if (result == null)
                {
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

        // ================================================================
        //  设备控制（启动/停止/配置）
        // ================================================================

        private void BtnStartDevice_Click(object sender, EventArgs e)
        {
            if (_currentDevice == null)
            {
                MessageBox.Show("未选择设备", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // 启动设备会尝试建立 Modbus 连接，所以不需要检查连接状态
            try
            {
                if (_deviceManager.StartDevice(_currentDevice.Config.DeviceId))
                {
                    AppLogger.Info($"设备启动成功: {_currentDevice.Config.Name}", "DebugToolbox");
                    AuditService.Log(_currentUserId, _currentUser, "DebugStartDevice", $"设备={_currentDevice.Config.Name}", _repo);
                    MessageBox.Show("设备启动成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    UpdateDeviceInfo(); // 刷新状态
                }
                else
                {
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
                    UpdateDeviceInfo();
                }
                else
                {
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
                        // 更新按钮状态
                        UpdateModbusButtonState();
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error($"配置设备异常: {ex.Message}", "DebugToolbox");
                MessageBox.Show($"配置异常: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ================================================================
        //  Modbus 连接/断开
        // ================================================================

        private void BtnConnectModbus_Click(object sender, EventArgs e)
        {
            if (_currentDevice == null)
            {
                MessageBox.Show("未选择设备", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (_currentDevice.ModbusClient == null)
            {
                MessageBox.Show("Modbus 客户端未初始化", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (_currentDevice.ModbusClient.IsConnected)
            {
                MessageBox.Show("Modbus 已连接", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                if (_currentDevice.ModbusClient.Connect())
                {
                    MessageBox.Show("Modbus 连接成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    AppLogger.Info($"Modbus 连接成功: {_currentDevice.Config.Name}", "DebugToolbox");
                    UpdateDeviceInfo(); // 刷新状态
                }
                else
                {
                    MessageBox.Show("Modbus 连接失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"连接异常: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                AppLogger.Error($"Modbus 连接异常: {ex.Message}", "DebugToolbox");
            }
        }

        private void BtnDisconnectModbus_Click(object sender, EventArgs e)
        {
            if (_currentDevice == null) return;
            if (_currentDevice.ModbusClient == null) return;
            if (!_currentDevice.ModbusClient.IsConnected) return;
            try
            {
                _currentDevice.ModbusClient.Disconnect();
                MessageBox.Show("Modbus 已断开", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                AppLogger.Info($"Modbus 断开: {_currentDevice.Config.Name}", "DebugToolbox");
                UpdateDeviceInfo(); // 刷新状态
            }
            catch (Exception ex)
            {
                MessageBox.Show($"断开异常: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ================================================================
        //  视觉触发调试
        // ================================================================

        private async void BtnTriggerVision_Click(object sender, EventArgs e)
        {
            btnTriggerVision.Enabled = false;
            UpdateVisionStatus("⏳ 正在连接视觉服务器...", Color.Orange);
            try
            {
                var config = new CommandConfig
                {
                    Type = "TriggerVision",
                    VisionServerIp = txtVisionIp.Text.Trim(),
                    VisionServerPort = (int)numVisionPort.Value,
                    VisionTimeoutMs = (int)numVisionTimeout.Value,
                    TriggerCoilAddress = 100,
                    BusyCoilAddress = 101,
                    ResultCoilAddress = 102,
                    ResultCodeRegister = 1000,
                    FileNameRegisterStart = 1001
                };
                var command = new TriggerVisionCommand(_model, _deviceManager, config);
                command.OnLog += msg => AppLogger.Info(msg, "VisionTrigger");
                AppLogger.Info("🔍 手动触发视觉拍照...", "DebugToolbox");
                UpdateVisionStatus("📸 正在触发拍照...", Color.Orange);
                await Task.Run(() => command.Execute(CancellationToken.None));
                UpdateVisionStatus("✅ 视觉拍照成功！", Color.Green);
                MessageBox.Show("✅ 视觉拍照成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                AppLogger.Info("✅ 视觉拍照成功", "DebugToolbox");
            }
            catch (Exception ex)
            {
                UpdateVisionStatus($"❌ 失败: {ex.Message}", Color.Red);
                MessageBox.Show($"❌ 视觉拍照失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                AppLogger.Error($"视觉触发失败: {ex.Message}", "DebugToolbox");
            }
            finally
            {
                btnTriggerVision.Enabled = true;
            }
        }

        // ================================================================
        //  窗体关闭
        // ================================================================

        private void DebugToolboxForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            timerRefresh.Stop();
            AppLogger.Info($"调试工具箱已关闭", "DebugToolbox");
        }
    }
}