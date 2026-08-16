using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using GtsTest.Modbus;
using GtsTest.Presenters;
using GtsTest.Services;
using System.Drawing;   // 用于颜色
using GtsTest.Services; // 引用 AlarmRecord 等

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
        public event EventHandler StartDeviceClicked;
        public event EventHandler StopDeviceClicked;
        public event EventHandler DeviceConfigClicked;
        public event EventHandler ToggleSimulatorClicked;
        public event EventHandler RunWorkflowClicked;
        public event EventHandler StopWorkflowClicked;
        public event EventHandler EmergencyStopClicked;
        public event EventHandler SaveConfigClicked;
        public event EventHandler ToggleModbusClicked;
        public event EventHandler ConnectAllModbusClicked;
        public event EventHandler DisconnectAllModbusClicked;
        public event EventHandler HomeAxisClicked;
        public event EventHandler MoveAbsClicked;
        public event EventHandler JogPositiveClicked;
        public event EventHandler JogNegativeClicked;
        public event EventHandler StopAxisClicked;
        public event EventHandler ServoOnClicked;
        public event EventHandler ServoOffClicked;
        public event EventHandler AlarmResetClicked;
        public event EventHandler<WriteRegisterEventArgs> WriteRegisterRequested;
        public event EventHandler<WriteCoilEventArgs> WriteCoilRequested;
        // 事件声明（触发按钮点击时使用）
        public event EventHandler AlarmAcknowledgeClicked;
        public event EventHandler AlarmResolveClicked;

        private bool _isSelectingDevice = false;
        private GtsPresenter _presenter;
        private Dictionary<string, Chart> _deviceCharts = new();

        private IDataRepository _repo;
        private IAuthenticationService _authService;

        public Form1(IDataRepository repo, IAuthenticationService authService)
        {
            try
            {
                InitializeComponent();
                _repo = repo;
                _authService = authService;

                // 创建 Model 和 DeviceManager（在 Presenter 外部创建并注入）
                // 创建 Presenter（注入所有依赖）
                var model = new GtsModel();
                var deviceManager = new DeviceManager(model, repo);
                var logger = new AppLoggerWrapper();

                _presenter = new GtsPresenter(
                   this, model, deviceManager, logger,
                   repo,
                   deviceManager.AlarmManager,   // ← 传递报警管理器（不再传 null）
                   null, null, null,
                   authService
               );

                // ---------- 绑定 UI 事件（只触发事件，不包含业务逻辑） ----------
                this.Load += (s, e) => LoadView?.Invoke(s, e);

                // 设备列表
                btnAddDevice.Click += (s, e) => AddDeviceClicked?.Invoke(s, e);
                btnRemoveDevice.Click += (s, e) => RemoveDeviceClicked?.Invoke(s, e);
                btnStartAll.Click += (s, e) => StartAllClicked?.Invoke(s, e);
                btnStopAll.Click += (s, e) => StopAllClicked?.Invoke(s, e);
                btnStartDevice.Click += (s, e) => StartDeviceClicked?.Invoke(s, e);
                btnStopDevice.Click += (s, e) => StopDeviceClicked?.Invoke(s, e);
                btnDeviceConfig.Click += (s, e) => DeviceConfigClicked?.Invoke(s, e);

                // 工具栏
                btnToggleSim.Click += (s, e) => ToggleSimulatorClicked?.Invoke(s, e);
                btnRunFlow.Click += (s, e) => RunWorkflowClicked?.Invoke(s, e);
                btnStopFlow.Click += (s, e) => StopWorkflowClicked?.Invoke(s, e);
                btnEmergencyStop.Click += (s, e) => EmergencyStopClicked?.Invoke(s, e);
                btnSaveConfig.Click += (s, e) => SaveConfigClicked?.Invoke(s, e);
                btnToggleModbus.Click += (s, e) => ToggleModbusClicked?.Invoke(s, e);
                btnConnectAll.Click += (s, e) => ConnectAllModbusClicked?.Invoke(s, e);
                btnDisconnectAll.Click += (s, e) => DisconnectAllModbusClicked?.Invoke(s, e);

                // 轴控制
                btnHome.Click += (s, e) => HomeAxisClicked?.Invoke(s, e);
                btnMoveAbs.Click += (s, e) => MoveAbsClicked?.Invoke(s, e);
                btnJogP.Click += (s, e) => JogPositiveClicked?.Invoke(s, e);
                btnJogN.Click += (s, e) => JogNegativeClicked?.Invoke(s, e);
                btnStopAxis.Click += (s, e) => StopAxisClicked?.Invoke(s, e);
                btnServoOn.Click += (s, e) => ServoOnClicked?.Invoke(s, e);
                btnServoOff.Click += (s, e) => ServoOffClicked?.Invoke(s, e);
                btnAlarmReset.Click += (s, e) => AlarmResetClicked?.Invoke(s, e);

                // 写寄存器 / 写线圈（只触发事件，参数从UI控件读取）
                btnWriteRegister.Click += (s, e) =>
                {
                    if (WriteRegisterRequested == null) return;

                    ushort address = (ushort)numWriteAddress.Value;
                    DataType dataType = (DataType)cmbWriteDataType.SelectedIndex;
                    ByteOrder byteOrder = (ByteOrder)cmbByteOrder.SelectedIndex;

                    // 解析输入值（多个值用逗号分隔）
                    var parts = txtWriteValues.Text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
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
                            ShowMessage($"值 '{part}' 无法转换为 {dataType}", "输入错误", MessageType.Error);
                            return;
                        }
                    }

                    if (values.Count > 0)
                    {
                        WriteRegisterRequested.Invoke(this, new WriteRegisterEventArgs(address, dataType, byteOrder, values.ToArray()));
                    }
                };

                btnWriteCoil.Click += (s, e) =>
                {
                    if (WriteCoilRequested == null) return;
                    ushort address = (ushort)numCoilAddress.Value;
                    bool value = cmbCoilValue.SelectedIndex == 0; // 0为ON
                    WriteCoilRequested.Invoke(this, new WriteCoilEventArgs(address, value));
                };

                // 初始化下拉框
                cmbWriteDataType.SelectedIndex = 0;
                cmbByteOrder.SelectedIndex = 0;
                cmbCoilValue.SelectedIndex = 0;

                // 设备列表选中变化 → 通知 Presenter
                listBoxDevices.SelectedIndexChanged += (s, e) =>
                {
                    if (listBoxDevices.SelectedItem is DeviceListItem item)
                    {
                        _presenter.OnDeviceSelected(item.DeviceId);
                        // ← 新增：同步切换Tab页
                        foreach (TabPage page in tabDeviceDetails.TabPages)
                        {
                            if (page.Tag as string == item.DeviceId)
                            {
                                tabDeviceDetails.SelectedTab = page;
                                break;
                            }
                        }
                    }
                };

                // Tab 切换时同步选中
                tabDeviceDetails.SelectedIndexChanged += (s, e) =>
                {
                    if (tabDeviceDetails.SelectedTab?.Tag is string id)
                    {
                        _presenter.OnDeviceSelected(id);
                    
                        if (!_isSelectingDevice)  // ← 防止循环
                        {   
                            // 同步 ListBox
                            for (int i = 0; i < listBoxDevices.Items.Count; i++)
                            {
                                if (((DeviceListItem)listBoxDevices.Items[i]).DeviceId == id)
                                {
                                    listBoxDevices.SelectedIndex = i;
                                    break;
                                }
                            }
                        }
                    }
                };
                // 订阅用户变更事件（更新UI）
                SessionManager.OnUserChanged += OnUserChanged;

                // 初始化界面状态（未登录）
                UpdateUIByLoginState(null);

                // 按钮事件
                btnLogin.Click += BtnLogin_Click;


                listBoxDevices.DisplayMember = "Name";
            // 手动触发 LoadView（通常 Load 事件会在窗体显示时触发，这里为了立即加载也调用一次）
            // 注意：为避免重复，可在 Load 事件中只执行一次，但构造函数中直接调用会导致 Load 事件可能重复触发。
            // 为保险，使用一个标志或直接让 Load 事件处理。
            // 此处我们让 Load 事件触发即可，不在构造函数中直接调用。
            // 但为了立即显示默认设备，我们会在 Load 事件中调用，所以这里不重复调用。

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Form1 构造异常: {ex.Message}\n\n{ex.StackTrace}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw; // 重新抛出以便上层捕获
            }
        }

        // ---------- 实现 IGtsView ----------

        public void UpdateDeviceList(IEnumerable<DeviceListItem> items)
        {
            // 清空 Tab 和 ListBox
            while (tabDeviceDetails.TabPages.Count > 0)
            {
                var page = tabDeviceDetails.TabPages[0];
                tabDeviceDetails.TabPages.Remove(page);
                page.Dispose();
            }
            _deviceCharts.Clear();
            listBoxDevices.Items.Clear();

            foreach (var item in items)
            {
                listBoxDevices.Items.Add(item);

                // 创建 Tab 页
                var tab = new TabPage(item.Name) { Tag = item.DeviceId };
                var layout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 2
                };
                layout.RowStyles.Add(new RowStyle(SizeType.Percent, 70));
                layout.RowStyles.Add(new RowStyle(SizeType.Percent, 30));

                // 信息卡片
                var card = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    AutoScroll = true,
                    FlowDirection = FlowDirection.TopDown,
                    Padding = new Padding(10)
                };
                var lblPos = new Label { Text = "位置: --", AutoSize = true };
                var lblStep = new Label { Text = "步骤: 空闲", AutoSize = true };
                var lblProd = new Label { Text = "产量: 0/0", AutoSize = true };
                var lblStatus = new Label
                {
                    Text = item.IsOnline ? "状态: ● 在线" : "状态: ○ 离线",
                    AutoSize = true,
                    ForeColor = item.IsOnline ? Color.Green : Color.Red
                };
                card.Controls.AddRange(new Control[] { lblPos, lblStep, lblProd, lblStatus });
                
                // 图表
                var chart = new Chart { Dock = DockStyle.Fill };
                chart.ChartAreas.Add(new ChartArea());
                var series = new Series("数据") { ChartType = SeriesChartType.Line };
                chart.Series.Add(series);
                _deviceCharts[item.DeviceId] = chart;

                layout.Controls.Add(card, 0, 0);
                layout.Controls.Add(chart, 0, 1);
                tab.Controls.Add(layout);
                tabDeviceDetails.TabPages.Add(tab);
            }

            if (tabDeviceDetails.TabPages.Count == 0)
            {
                var defaultTab = new TabPage("请添加设备");
                tabDeviceDetails.TabPages.Add(defaultTab);
            }

            listBoxDevices.Invalidate();
        }

        public void SelectDevice(string deviceId)
        {
            if (_isSelectingDevice) return;
            _isSelectingDevice = true;

            try
            {
                for (int i = 0; i < listBoxDevices.Items.Count; i++)
                {
                    var item = (DeviceListItem)listBoxDevices.Items[i];
                    if (item.DeviceId == deviceId)
                    {
                        listBoxDevices.SelectedIndex = i;
                        break;
                    }
                }
                foreach (TabPage page in tabDeviceDetails.TabPages)
                {
                    if (page.Tag as string == deviceId)
                    {
                        tabDeviceDetails.SelectedTab = page;
                        break;
                    }
                }
            }
            finally
            {
                _isSelectingDevice = false;
            }
        }

        public string GetSelectedDeviceId()
        {
            if (listBoxDevices.SelectedItem is DeviceListItem item)
                return item.DeviceId;
            return "";
        }

        public void UpdateCurrentDevice(string deviceName)
        {
            if (string.IsNullOrEmpty(deviceName))
                lblSlaveInfo.Text = "当前设备: 未选择";
            else
                lblSlaveInfo.Text = $"当前设备: {deviceName}";
        }

        public void UpdateDeviceData(string deviceId, object data)
        {

            foreach (TabPage page in tabDeviceDetails.TabPages)
            {
                if (page.Tag as string == deviceId &&
                    page.Controls[0] is TableLayoutPanel layout &&
                    layout.Controls[0] is FlowLayoutPanel card)
                {
                    foreach (Control ctrl in card.Controls)
                    {
                        if (ctrl is Label lbl && lbl.Text.StartsWith("位置:"))
                        {
                            lbl.Text = $"位置: {data ?? "--"}";
                            break;
                        }
                    }
                }
            }

            // 更新图表
            if (_deviceCharts.TryGetValue(deviceId, out var chart))
            {
                var series = chart.Series["数据"];
                double val = 0;
                if (data != null) double.TryParse(data.ToString(), out val);
                series.Points.AddXY(DateTime.Now, val);
                if (series.Points.Count > 100) series.Points.RemoveAt(0);
                chart.Invalidate();
            }
        }

        private void RefreshUIByPermission(User user)
        {
            bool hasStart = _authService.HasPermission(user, "Device.Start");
            btnStartAll.Enabled = hasStart;
            // ... 其他按钮
        }

        public void UpdateDeviceStep(string deviceId, string step)
        {
            foreach (TabPage page in tabDeviceDetails.TabPages)
            {
                if (page.Tag as string == deviceId &&
                    page.Controls[0] is TableLayoutPanel layout &&
                    layout.Controls[0] is FlowLayoutPanel card)
                {
                    foreach (Control ctrl in card.Controls)
                    {
                        if (ctrl is Label lbl && lbl.Text.StartsWith("步骤:"))
                        {
                            lbl.Text = $"步骤: {step}";
                            break;
                        }
                    }
                }
            }
        }

        public void UpdateDeviceProduction(string deviceId, int current, int target)
        {
            foreach (TabPage page in tabDeviceDetails.TabPages)
            {
                if (page.Tag as string == deviceId &&
                    page.Controls[0] is TableLayoutPanel layout &&
                    layout.Controls[0] is FlowLayoutPanel card)
                {
                    foreach (Control ctrl in card.Controls)
                    {
                        if (ctrl is Label lbl && lbl.Text.StartsWith("产量:"))
                        {
                            lbl.Text = $"产量: {current}/{target}";
                            break;
                        }
                    }
                }
            }
        }

        public void UpdateDeviceOnlineStatus(string deviceId, bool isOnline)
        {
            foreach (TabPage page in tabDeviceDetails.TabPages)
                {
                    if (page.Tag as string == deviceId &&
                        page.Controls[0] is TableLayoutPanel layout &&
                        layout.Controls[0] is FlowLayoutPanel card)
                    {
                        foreach (Control ctrl in card.Controls)
                        {
                            if (ctrl is Label lbl && lbl.Text.StartsWith("状态:"))
                            {
                                lbl.Text = isOnline ? "状态: ● 在线" : "状态: ○ 离线";
                                lbl.ForeColor = isOnline ? Color.Green : Color.Red;
                                break;
                            }
                        }
                    }
                }

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
            if (string.IsNullOrEmpty(deviceId) || deviceId != GetSelectedDeviceId())
                return;
            lblAxisValue.Text = axis.ToString();
            lblStatusValue.Text = isOnline ? "● 在线" : "○ 离线";
            lblStatusValue.ForeColor = isOnline ? Color.Green : Color.Red;
            lblPosValue.Text = pos.ToString("F2");
            lblVelValue.Text = vel.ToString("F2");
        }

        public void UpdateGlobalStats(int onlineCount, int totalCount, int totalProduction)
        {
            lblOnlineCount.Text = $"在线: {onlineCount}/{totalCount}";
            lblTotalProduction.Text = $"总产量: {totalProduction}";
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

            // 更新 Modbus 按钮状态
            btnToggleModbus.Text = modbusConnected ? "断开 Modbus" : "连接 Modbus";
            btnToggleModbus.BackColor = modbusConnected ? Color.LightCoral : SystemColors.Control;
        }

        // 更新报警列表
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
                // 根据严重度着色
                switch (alarm.Severity)
                {
                    case AlarmSeverity.Critical: item.ForeColor = Color.Red; break;
                    case AlarmSeverity.Error: item.ForeColor = Color.DarkOrange; break;
                    case AlarmSeverity.Warning: item.ForeColor = Color.Goldenrod; break;
                    default: item.ForeColor = Color.Black; break;
                }
                listViewAlarms.Items.Add(item);
            }
        }

        // 获取当前选中的报警ID（返回字符串，实际是Id的文本）
        public string GetSelectedAlarmId()
        {
            if (listViewAlarms.SelectedItems.Count > 0)
                return listViewAlarms.SelectedItems[0].Text;
            return "";
        }

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            if (SessionManager.IsLoggedIn)
            {
                // 当前已登录，点击执行注销
                SessionManager.Logout(_repo);
                return;
            }

            // 未登录，弹出登录窗
            using (var login = new LoginForm(_authService, _repo))
            {
                if (login.ShowDialog() == DialogResult.OK)
                {
                    // 登录成功，SessionManager 会触发 OnUserChanged
                }
            }
        }

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

            // 根据权限启用/禁用操作按钮（基于用户角色）
            bool hasStart = _authService.HasPermission(user, "Device.Start");
            btnStartDevice.Enabled = hasStart;
            btnStartAll.Enabled = hasStart;
            // ... 其他按钮同理
            btnAddDevice.Enabled = _authService.HasPermission(user, "Config.Edit");
            btnDeviceConfig.Enabled = _authService.HasPermission(user, "Config.Edit");
            // 敏感操作（删除、修改参数）需要更高权限，自行判断
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

        // ---------- 兼容旧代码（临时保留，可删除） ----------
        public void ShowResult(string message) => AppendOperationLog(message);
        public void ClearResult() => ClearLogs();
        public void SetSimulationModeUI(bool isSimulation) => SetSimulationMode(isSimulation);
    }
}