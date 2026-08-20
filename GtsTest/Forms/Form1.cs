using GtsTest.Controls;
using GtsTest.Core;
using GtsTest.Forms;
using GtsTest.Modbus;
using GtsTest.Models;
using GtsTest.Presenters;
using GtsTest.Services;
using GtsTest.Services.Alarm;
using GtsTest.Services.Authentication;
using GtsTest.Services.Camera;
using GtsTest.Services.Data;
using GtsTest.Services.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace GtsTest
{
    public partial class Form1 : Form, IGtsView
    {
        // ---------- 事件（由Presenter订阅） ----------
        public event EventHandler LoadView;
        public event EventHandler AddDeviceClicked;
        public event EventHandler RemoveDeviceClicked;
        public event EventHandler StartAllClicked;
        public event EventHandler StopAllClicked;
        public event EventHandler EmergencyStopClicked;
        public event EventHandler AlarmResetClicked;
        public event EventHandler RunWorkflowClicked;
        public event EventHandler StopWorkflowClicked;
        public event EventHandler ToggleSimulatorClicked;
        public event EventHandler ToggleModbusClicked;
        public event EventHandler ConnectAllModbusClicked;
        public event EventHandler DisconnectAllModbusClicked;
        public event EventHandler SaveConfigClicked;
        public event EventHandler AlarmAcknowledgeClicked;
        public event EventHandler AlarmResolveClicked;

        // ---------- 私有字段 ----------
        private GtsPresenter _presenter;
        private readonly IDataRepository _repo;
        private readonly IAuthenticationService _authService;
        private readonly GtsModel _model;
        private readonly DeviceManager _deviceManager;
        private readonly IAlarmManager _alarmManager;
        private bool _isSelectingDevice = false;
        private Dictionary<string, Chart> _deviceCharts = new();

        // 相机 MVP 组件
        private CameraPresenter _cameraPresenter;
        private MqttPresenter _mqttPresenter;

        // ---------- 构造函数 ----------
        public Form1(IDataRepository repo, IAuthenticationService authService)
        {
            InitializeComponent();
            _repo = repo;
            _authService = authService;

            // 创建核心服务
            _model = new GtsModel();
            _deviceManager = new DeviceManager(_model);
            _alarmManager = _deviceManager.AlarmManager;
            var logger = new AppLoggerWrapper();

            // 创建主 Presenter
            _presenter = new GtsPresenter(
                this,
                _model,
                _deviceManager,
                logger,
                repo,
                _alarmManager,
                null,
                null,
                null,
                authService
            );

            // ---------- 初始化用户控件（放到对应Tab页） ----------
            // 1. 产线总览
            this.overviewControl = new OverviewControl(_deviceManager, _alarmManager);
            this.overviewControl.Dock = DockStyle.Fill;
            this.tabPageOverview.Controls.Add(this.overviewControl);

            // 2. 视觉检测（相机 MVP 装配）
            this.cameraControl = new CameraControl();
            this.cameraControl.Dock = DockStyle.Fill;
            this.tabPageCamera.Controls.Add(this.cameraControl);

            // 创建相机服务（目前为模拟，可替换为真实实现）
            var cameraService = new RealCameraService();
            // 创建相机 Presenter
            _cameraPresenter = new CameraPresenter(this.cameraControl, cameraService);
            // 释放资源（窗体关闭时）
            this.FormClosing += (s, e) => _cameraPresenter?.Dispose();

            // 3. 工作流配置
            this.workflowControl = new WorkflowControl(_model, _deviceManager);
            this.workflowControl.Dock = DockStyle.Fill;
            this.tabPageWorkflow.Controls.Add(this.workflowControl);

            // 4. 通信中心
            this.communicationControl = new CommunicationControl();
            this.communicationControl.Dock = DockStyle.Fill;
            this.tabPageComm.Controls.Add(this.communicationControl);

            // 5. MQTT
            this.mqttControl = new MqttControl();
            this.mqttControl.Dock = DockStyle.Fill;
            this.tabPageMqtt.Controls.Add(this.mqttControl);

            // ✅ 创建 MQTT Presenter（连接 View 和 Service）
            var mqttService = new MqttService();
            this._mqttPresenter = new MqttPresenter(this.mqttControl, mqttService);
            this.FormClosing += (s, e) => this._mqttPresenter?.Dispose();

            // ---------- 绑定 UI 事件 ----------
            this.Load += (s, e) => LoadView?.Invoke(s, e);

            // 设备管理
            btnAddDevice.Click += (s, e) => AddDeviceClicked?.Invoke(s, e);
            btnRemoveDevice.Click += (s, e) => RemoveDeviceClicked?.Invoke(s, e);

            // 产线控制
            btnStartAll.Click += (s, e) => StartAllClicked?.Invoke(s, e);
            btnStopAll.Click += (s, e) => StopAllClicked?.Invoke(s, e);
            btnEmergencyStop.Click += (s, e) => EmergencyStopClicked?.Invoke(s, e);
            btnResetAlarm.Click += (s, e) => AlarmResetClicked?.Invoke(s, e);

            // 流程控制
            btnRunFlow.Click += (s, e) => RunWorkflowClicked?.Invoke(s, e);
            btnStopFlow.Click += (s, e) => StopWorkflowClicked?.Invoke(s, e);

            // Modbus 连接管理
            btnToggleModbus.Click += (s, e) => ToggleModbusClicked?.Invoke(s, e);
            btnConnectAll.Click += (s, e) => ConnectAllModbusClicked?.Invoke(s, e);
            btnDisconnectAll.Click += (s, e) => DisconnectAllModbusClicked?.Invoke(s, e);
            btnSaveConfig.Click += (s, e) => SaveConfigClicked?.Invoke(s, e);

            // 模拟模式切换
            btnToggleSim.Click += (s, e) => ToggleSimulatorClicked?.Invoke(s, e);

            // 调试工具箱
            btnDebugToolbox.Click += BtnDebugToolbox_Click;

            // 登录/注销
            btnLogin.Click += BtnLogin_Click;

            // 报警确认/解决（在 ListView 中通过右键菜单或双击实现）
            // 这里我们使用双击报警项来触发确认，演示绑定
            listViewAlarms.DoubleClick += (s, e) => AlarmAcknowledgeClicked?.Invoke(s, e);
            // 也可以添加一个右键菜单项，但先简单处理

            // 设备列表选择变化
            listBoxDevices.SelectedIndexChanged += (s, e) =>
            {
                if (listBoxDevices.SelectedItem is DeviceListItem item)
                {
                    _presenter.OnDeviceSelected(item.DeviceId);
                }
            };

            // 订阅用户会话变更
            SessionManager.OnUserChanged += OnUserChanged;

            // 初始化界面状态
            UpdateUIByLoginState(null);

            // 设置 ListBox 显示属性
            listBoxDevices.DisplayMember = "Name";
        }

        // ---------- 辅助方法 ----------
        private void SyncListBoxSelection(string deviceId)
        {
            _isSelectingDevice = true;
            try
            {
                for (int i = 0; i < listBoxDevices.Items.Count; i++)
                {
                    if (((DeviceListItem)listBoxDevices.Items[i]).DeviceId == deviceId)
                    {
                        listBoxDevices.SelectedIndex = i;
                        break;
                    }
                }
            }
            finally
            {
                _isSelectingDevice = false;
            }
        }

        // ---------- 调试工具箱 ----------
        private void BtnDebugToolbox_Click(object sender, EventArgs e)
        {
            using (var toolbox = new DebugToolboxForm(
                _deviceManager,
                _model,
                _alarmManager,
                _repo,
                _authService))
            {
                toolbox.ShowDialog(this);
            }
        }

        // ---------- 登录/注销 ----------
        private void BtnLogin_Click(object sender, EventArgs e)
        {
            if (SessionManager.IsLoggedIn)
            {
                SessionManager.Logout(_repo);
                return;
            }

            using (var login = new LoginForm(_authService, _repo))
            {
                if (login.ShowDialog() == DialogResult.OK)
                {
                    // SessionManager 已触发 OnUserChanged
                }
            }
        }

        // ---------- 用户会话变更 ----------
        private void OnUserChanged(User user)
        {
            UpdateUIByLoginState(user);
        }

        private void UpdateUIByLoginState(User user)
        {
            bool isLoggedIn = user != null;
            if (isLoggedIn)
            {
                lblLoggedUser.Text = $"当前用户：{user.FullName} ({user.Role})";
                btnLogin.Text = "注销";
                btnLogin.ForeColor = System.Drawing.Color.Black;
            }
            else
            {
                lblLoggedUser.Text = "未登录";
                btnLogin.Text = "登录";
                btnLogin.ForeColor = System.Drawing.Color.Gray;
            }

            // 根据权限更新控件启用状态
            bool hasStart = _authService.HasPermission(user, "Device.Start");
            btnStartAll.Enabled = hasStart;
            bool hasEdit = _authService.HasPermission(user, "Config.Edit");
            btnAddDevice.Enabled = hasEdit;
            btnRemoveDevice.Enabled = hasEdit;
            btnSaveConfig.Enabled = hasEdit;
            bool hasAck = _authService.HasPermission(user, "Alarm.Acknowledge");
            // 报警双击确认已由 Presenter 处理，此处可启用/禁用双击行为，但未设置，暂留空
        }

        // ---------- 实现 IGtsView ----------
        public void UpdateDeviceList(IEnumerable<DeviceListItem> items)
        {
            listBoxDevices.Items.Clear();
            foreach (var item in items)
            {
                listBoxDevices.Items.Add(item);
            }
            listBoxDevices.Invalidate();
        }

        public void SelectDevice(string deviceId)
        {
            SyncListBoxSelection(deviceId);
        }

        public string GetSelectedDeviceId()
        {
            if (listBoxDevices.SelectedItem is DeviceListItem item)
                return item.DeviceId;
            return "";
        }

        public void UpdateCurrentDevice(string deviceName)
        {
            // 状态栏已有设备名称显示，由 UpdateStatusBar 负责
        }

        public void UpdateDeviceData(string deviceId, object data)
        {
            // 已无设备详情Tab，但可转发给 OverviewControl（如需要）
            // 目前仅保留空实现
        }

        public void UpdateDeviceStep(string deviceId, string step)
        {
            // 转发给 OverviewControl 可刷新卡片步骤信息
            // 但 OverviewControl 自身定时刷新，暂不处理
        }

        public void UpdateDeviceProduction(string deviceId, int current, int target)
        {
            // 同样由 OverviewControl 定时刷新
        }

        public void UpdateDeviceOnlineStatus(string deviceId, bool isOnline)
        {
            // 更新 ListBox
            for (int i = 0; i < listBoxDevices.Items.Count; i++)
            {
                var item = (DeviceListItem)listBoxDevices.Items[i];
                if (item.DeviceId == deviceId)
                {
                    item.IsOnline = isOnline;
                    listBoxDevices.Items[i] = item;
                    listBoxDevices.Invalidate();
                    break;
                }
            }
        }

        public void UpdateAxisInfo(string deviceId, short axis, bool isOnline, double pos, double vel)
        {
            // 已无轴信息显示，留空
        }

        public void UpdateGlobalStats(int onlineCount, int totalCount, int totalProduction)
        {
            lblOnlineCount.Text = $"在线: {onlineCount}/{totalCount}";
            lblTotalProduction.Text = $"总产量: {totalProduction}";
            lblTotalProdValue.Text = totalProduction.ToString();
            // 良品率暂未实现，保持为0%
        }

        public void AppendOperationLog(string message)
        {
            txtOperationLog.AppendText(message + Environment.NewLine);
        }

        public void AppendMonitorLog(string message)
        {
            txtMonitorLog.AppendText(message + Environment.NewLine);
        }

        public void ClearLogs()
        {
            txtOperationLog.Clear();
            txtMonitorLog.Clear();
        }

        public void SetSimulationMode(bool isSimulation)
        {
            btnToggleSim.Text = isSimulation ? "切换到真实" : "切换到模拟";
            btnToggleSim.BackColor = isSimulation ? Color.LightGreen : Color.LightGray;
        }

        public void UpdateStatusBar(string deviceName, bool isOnline, bool servoOn,
                                     string limitStatus, bool modbusConnected, string currentStep,
                                     int watchdogRemainingMs, bool watchdogTimeout)
        {
            if (string.IsNullOrEmpty(deviceName))
            {
                lblDeviceStatus.Text = "设备: 未选择"; lblDeviceStatus.ForeColor = Color.Gray;
                lblServoStatus.Text = "伺服: --"; lblServoStatus.ForeColor = Color.Gray;
                lblLimitStatus.Text = "限位: --"; lblLimitStatus.ForeColor = Color.Gray;
                lblModbusStatusStrip.Text = "Modbus: --"; lblModbusStatusStrip.ForeColor = Color.Gray;
                lblCurrentCmd.Text = "当前指令: 空闲";
                lblWatchdogStatus.Text = "🐕看门狗: 未启动"; lblWatchdogStatus.ForeColor = Color.Gray;
                return;
            }

            lblDeviceStatus.Text = $"设备: {deviceName} {(isOnline ? "●在线" : "○离线")}";
            lblDeviceStatus.ForeColor = isOnline ? Color.Green : Color.Red;
            lblServoStatus.Text = servoOn ? "伺服: 已使能" : "伺服: 未使能";
            lblServoStatus.ForeColor = servoOn ? Color.Green : Color.Orange;
            lblLimitStatus.Text = $"限位: {limitStatus}";
            lblLimitStatus.ForeColor = limitStatus.Contains("限位") ? Color.Red : Color.Green;
            lblModbusStatusStrip.Text = modbusConnected ? "Modbus: 已连接" : "Modbus: 未连接";
            lblModbusStatusStrip.ForeColor = modbusConnected ? Color.Green : Color.Red;
            lblCurrentCmd.Text = $"当前指令: {currentStep}";

            if (watchdogTimeout)
            {
                lblWatchdogStatus.Text = "🐕看门狗: 超时!"; lblWatchdogStatus.ForeColor = Color.Red;
            }
            else if (watchdogRemainingMs > 0)
            {
                lblWatchdogStatus.Text = $"🐕看门狗: 正常 ({watchdogRemainingMs}ms)";
                lblWatchdogStatus.ForeColor = watchdogRemainingMs > 1000 ? Color.Green : Color.Orange;
            }
            else
            {
                lblWatchdogStatus.Text = "🐕看门狗: 未启动"; lblWatchdogStatus.ForeColor = Color.Gray;
            }

            btnToggleModbus.Text = modbusConnected ? "断开 Modbus" : "连接 Modbus";
            btnToggleModbus.BackColor = modbusConnected ? Color.LightCoral : SystemColors.Control;
        }

        public void UpdateAlarmList(IEnumerable<AlarmRecord> alarms)
        {
            listViewAlarms.Items.Clear();
            foreach (var alarm in alarms)
            {
                var item = new ListViewItem(alarm.Id.ToString());
                item.SubItems.Add(alarm.DeviceId ?? "");
                item.SubItems.Add(alarm.Message);
                item.SubItems.Add(alarm.Severity.ToString());
                item.SubItems.Add(alarm.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"));
                item.SubItems.Add(alarm.IsResolved ? "已解决" : (alarm.IsAcknowledged ? "已确认" : "未确认"));
                switch (alarm.Severity)
                {
                    case AlarmSeverity.Critical: item.ForeColor = Color.Red; break;
                    case AlarmSeverity.Error: item.ForeColor = Color.DarkOrange; break;
                    case AlarmSeverity.Warning: item.ForeColor = Color.Goldenrod; break;
                    default: item.ForeColor = Color.Black; break;
                }
                listViewAlarms.Items.Add(item);
            }

            // 更新统计面板中的报警数
            int activeCount = alarms.Count(a => !a.IsResolved);
            lblAlarmCountValue.Text = activeCount.ToString();
            lblAlarmCountValue.ForeColor = activeCount > 0 ? Color.Red : Color.Green;
        }

        public string GetSelectedAlarmId()
        {
            if (listViewAlarms.SelectedItems.Count > 0)
                return listViewAlarms.SelectedItems[0].Text;
            return "";
        }

        public void ShowMessage(string text, string caption, MessageType type)
        {
            MessageBoxIcon icon = type switch
            {
                MessageType.Info => MessageBoxIcon.Information,
                MessageType.Warning => MessageBoxIcon.Warning,
                MessageType.Error => MessageBoxIcon.Error,
                _ => MessageBoxIcon.None
            };
            MessageBox.Show(text, caption, MessageBoxButtons.OK, icon);
        }

        public bool ShowConfirm(string text, string caption)
        {
            return MessageBox.Show(text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        // ---------- 自定义绘制 ----------
        private void ListBoxDevices_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            e.DrawBackground();
            var item = (DeviceListItem)listBoxDevices.Items[e.Index];
            Color statusColor = item.IsOnline ? Color.Green : Color.Red;
            e.Graphics.FillEllipse(new SolidBrush(statusColor), e.Bounds.X + 5, e.Bounds.Y + 5, 10, 10);
            using (var brush = new SolidBrush(e.ForeColor))
                e.Graphics.DrawString(item.Name, e.Font, brush, e.Bounds.X + 22, e.Bounds.Y + 2);
            e.DrawFocusRectangle();
        }

        // ---------- 兼容方法（保留，可能被外部调用） ----------
        public void ShowResult(string message) => AppendOperationLog(message);
        public void ClearResult() => ClearLogs();
        public void SetSimulationModeUI(bool isSimulation) => SetSimulationMode(isSimulation);

        // ---------- 释放资源 ----------
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // 释放相机 Presenter
                _cameraPresenter?.Dispose();
                // 释放其他控件
                overviewControl?.Dispose();
                workflowControl?.Dispose();
                communicationControl?.Dispose();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}