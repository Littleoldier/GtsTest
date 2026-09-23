using GtsTest.Controls;
using GtsTest.Presenters;
using GtsTest.Services.Authentication;
using GtsTest.Services.Data;
using GtsTest.Services.Mes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace GtsTest
{
    public partial class Form1 : Form, IGtsView
    {
        // ---------- 事件 ----------
        public event EventHandler LoadView;
        public event EventHandler DeviceSelected;
        public event EventHandler AddDeviceClicked;
        public event EventHandler RemoveDeviceClicked;
        public event EventHandler StartAllClicked;
        public event EventHandler StopAllClicked;
        public event EventHandler StartSelectedClicked;
        public event EventHandler StopSelectedClicked;
        public event EventHandler ResetDeviceClicked;
        public event EventHandler AlarmResetClicked;
        public event EventHandler AlarmAcknowledgeClicked;
        public event EventHandler AlarmResolveClicked;
        public event EventHandler EmergencyStopClicked;
        public event EventHandler SystemConfigClicked;
        public event EventHandler LoginClicked;
        public event EventHandler<string> WorkflowRunClicked;
        public event EventHandler WorkflowStopClicked;
        public event EventHandler<string> DeviceForWorkflowSelected;
        public event EventHandler ProductionResetClicked;
        public event EventHandler BindDeviceWorkflowClicked;

        // ---------- 私有字段 ----------
        private GtsPresenter _presenter;
        private readonly IDataRepository _repo;
        private readonly IAuthenticationService _authService;
        private readonly MesReportService? _mesService;
        private bool _isSelectingDevice = false;

        // ---------- 构造函数 ----------
        public Form1(IDataRepository repo, IAuthenticationService authService, MesReportService mesService = null)
        {
            InitializeComponent();

            _repo = repo;
            _authService = authService;
            _mesService = mesService;

            // ---- 创建核心模型和管理器 ----
            var model = new GtsTest.Core.GtsModel();
            var deviceManager = new GtsTest.Core.DeviceManager(model, repo, mesService);
            var logger = new GtsTest.Services.Logging.AppLoggerWrapper();
            var alarmManager = deviceManager.AlarmManager;

            // ---- 注入到 overviewControl ----
            overviewControl.SetDeviceManager(deviceManager, alarmManager);

            // ---- 创建 Presenter ----
            _presenter = new GtsPresenter(
                this,
                model,
                deviceManager,
                logger,
                repo,
                alarmManager,
                authService
            );

            // ---- 初始化生产执行控件的下拉列表 ----
            string workflowsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Workflows");
            if (Directory.Exists(workflowsDir))
            {
                var workflowFiles = Directory.GetFiles(workflowsDir, "*.json");
                var workflowNames = workflowFiles.Select(f => Path.GetFileNameWithoutExtension(f)).ToList();
                workflowExecutionControl.SetWorkflowList(workflowNames);
            }
            else
            {
                Directory.CreateDirectory(workflowsDir);
                workflowExecutionControl.SetWorkflowList(new List<string> { "Default" });
            }
            workflowExecutionControl.LoadWorkflowPreview();

            // ---- 订阅生产执行控件事件 ----
            workflowExecutionControl.ExecuteClicked += (s, workflowName) => WorkflowRunClicked?.Invoke(s, workflowName);
            workflowExecutionControl.StopClicked += (s, e) => WorkflowStopClicked?.Invoke(s, e);
            workflowExecutionControl.ResetClicked += (s, e) => ResetDeviceClicked?.Invoke(s, e);
            workflowExecutionControl.ProductionResetClicked += (s, e) => ProductionResetClicked?.Invoke(s, e);
            workflowExecutionControl.DeviceSelected += (s, deviceId) => DeviceForWorkflowSelected?.Invoke(s, deviceId);
            workflowExecutionControl.BindClicked += (s, e) => BindDeviceWorkflowClicked?.Invoke(s, e);

            // ---- 设置 ListBox 绘制 ----
            listBoxDevices.DrawItem += ListBoxDevices_DrawItem;

            // ---- 键盘快捷键 ----
            this.KeyPreview = true;
            this.KeyDown += (s, e) =>
            {
                if ((e.Control && e.KeyCode == Keys.E) || e.KeyCode == Keys.Escape)
                {
                    EmergencyStopClicked?.Invoke(this, EventArgs.Empty);
                    e.Handled = true;
                }
            };

            SetupToolTips();

            // ============================================================
            // ⭐ MES 相关：订阅事件 + 绑定按钮
            // ============================================================
            InitMesIntegration();
        }

        // ============================================================
        // ⭐ MES 集成初始化
        // ============================================================
        private void InitMesIntegration()
        {
            if (_mesService == null)
            {
                // 没有 MES 服务时，隐藏相关控件
                if (lblMesPendingCount != null) lblMesPendingCount.Visible = false;
                if (btnMesRetry != null) btnMesRetry.Visible = false;
                return;
            }

            // 订阅待重传数量变化事件
            _mesService.PendingCountChanged += count =>
            {
                if (InvokeRequired)
                    BeginInvoke(new Action(() => UpdateMesPendingCount(count)));
                else
                    UpdateMesPendingCount(count);
            };

            // 订阅自动重传暂停/恢复事件
            _mesService.AutoRetryPausedChanged += paused =>
            {
                if (InvokeRequired)
                    BeginInvoke(new Action(() => UpdateMesPausedState(paused)));
                else
                    UpdateMesPausedState(paused);
            };

            // 订阅上报失败事件（4xx 客户端错误需要弹窗提示）
            _mesService.ReportFailed += (failType, msg) =>
            {
                if (failType == MesFailureType.ClientError)
                {
                    if (InvokeRequired)
                        BeginInvoke(new Action(() => MessageBox.Show(msg, "MES 上报异常", MessageBoxButtons.OK, MessageBoxIcon.Warning)));
                    else
                        MessageBox.Show(msg, "MES 上报异常", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };

            // 绑定手动重传按钮
            if (btnMesRetry != null)
            {
                btnMesRetry.Click += async (s, e) =>
                {
                    if (_mesService == null || !_mesService.IsEnabled)
                    {
                        MessageBox.Show("MES 服务未启用", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (MessageBox.Show("确定要手动触发 MES 重传吗？\n（将尝试重传所有待重传数据）",
                        "确认重传", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        return;

                    btnMesRetry.Enabled = false;
                    btnMesRetry.Text = "⏳ 重传中...";
                    try
                    {
                        int successCount = await _mesService.ForceRetryAsync();
                        if (successCount < 0)
                            MessageBox.Show("已有重传任务在进行，请稍候", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        else if (successCount == 0)
                            MessageBox.Show("待重传表为空，无需重传", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        else
                            MessageBox.Show($"重传成功 {successCount} 条", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    finally
                    {
                        btnMesRetry.Enabled = true;
                        btnMesRetry.Text = "📤 重传MES";
                    }
                };
            }

            // 初始化显示
            if (_mesService.IsEnabled)
            {
                AppendOperationLog($"[{DateTime.Now:HH:mm:ss}] ✅ MES 上报服务已启用: {_mesService.ApiUrl}");
                UpdateMesPendingCount(0);
            }
            else
            {
                AppendOperationLog($"[{DateTime.Now:HH:mm:ss}] ℹ️ MES 上报服务未启用");
                if (lblMesPendingCount != null) lblMesPendingCount.Text = "MES未启用";
            }
        }

        // ⭐ 更新待重传数量标签
        private void UpdateMesPendingCount(int count)
        {
            if (lblMesPendingCount == null || lblMesPendingCount.IsDisposed) return;

            if (_mesService == null || !_mesService.IsEnabled)
            {
                lblMesPendingCount.Text = "MES未启用";
                lblMesPendingCount.ForeColor = Color.Gray;
                return;
            }

            if (_mesService.IsAutoRetryPaused)
            {
                lblMesPendingCount.Text = $"MES待重传: {count} (已暂停)";
                lblMesPendingCount.ForeColor = Color.Red;
            }
            else if (count > 0)
            {
                lblMesPendingCount.Text = $"MES待重传: {count}";
                lblMesPendingCount.ForeColor = Color.Orange;
            }
            else
            {
                lblMesPendingCount.Text = "MES待重传: 0";
                lblMesPendingCount.ForeColor = Color.LightGreen;
            }
        }

        // ⭐ 更新自动重传暂停状态
        private void UpdateMesPausedState(bool paused)
        {
            if (paused)
            {
                AppendOperationLog($"[{DateTime.Now:HH:mm:ss}] ⚠️ MES 自动重传已暂停（待重传超过阈值）");
            }
            else
            {
                AppendOperationLog($"[{DateTime.Now:HH:mm:ss}] ✅ MES 自动重传已恢复");
            }
            // 刷新标签颜色
            UpdateMesPendingCount(_mesService?.PendingCount ?? 0);
        }

        // ==================== 实现 IGtsView ====================

        public void UpdateDeviceList(IEnumerable<DeviceListItem> items)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => UpdateDeviceList(items)));
                return;
            }

            listBoxDevices.Items.Clear();
            foreach (var item in items)
            {
                listBoxDevices.Items.Add(item);
            }

            workflowExecutionControl.SetDeviceList(items);
        }

        public void SelectDevice(string deviceId)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => SelectDevice(deviceId)));
                return;
            }

            _isSelectingDevice = true;
            try
            {
                for (int i = 0; i < listBoxDevices.Items.Count; i++)
                {
                    if (listBoxDevices.Items[i] is DeviceListItem item && item.DeviceId == deviceId)
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

        public string GetSelectedDeviceId()
        {
            if (InvokeRequired)
            {
                return (string)Invoke(new Func<string>(GetSelectedDeviceId));
            }

            if (listBoxDevices.SelectedItem is DeviceListItem item)
                return item.DeviceId;
            return "";
        }

        public string GetSelectedDeviceName()
        {
            if (InvokeRequired)
            {
                return (string)Invoke(new Func<string>(GetSelectedDeviceName));
            }
            return workflowExecutionControl?.GetSelectedDeviceName() ?? "";
        }

        public string GetSelectedWorkflowName()
        {
            if (InvokeRequired)
            {
                return (string)Invoke(new Func<string>(GetSelectedWorkflowName));
            }
            return workflowExecutionControl?.GetSelectedWorkflowName() ?? "";
        }

        public void UpdateDeviceOnlineStatus(string deviceId, bool isOnline)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => UpdateDeviceOnlineStatus(deviceId, isOnline)));
                return;
            }

            for (int i = 0; i < listBoxDevices.Items.Count; i++)
            {
                if (listBoxDevices.Items[i] is DeviceListItem item && item.DeviceId == deviceId)
                {
                    item.IsOnline = isOnline;
                    listBoxDevices.Items[i] = item;
                    listBoxDevices.Invalidate();
                    break;
                }
            }
        }

        public void UpdateDeviceProduction(string deviceId, int current, int target) { }
        public void UpdateDeviceStep(string deviceId, string step) { }
        public void UpdateDeviceData(string deviceId, object data) { }
        public void UpdateGlobalStats(int onlineCount, int totalCount, int totalProduction) { }

        public void UpdateStatusBar(string deviceName, bool isOnline, bool servoOn,
            string limitStatus, bool modbusConnected, string currentStep,
            int watchdogRemainingMs, bool watchdogTimeout)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => UpdateStatusBar(deviceName, isOnline, servoOn,
                    limitStatus, modbusConnected, currentStep, watchdogRemainingMs, watchdogTimeout)));
                return;
            }

            var items = statusStrip.Items;
            if (items.Count >= 6)
            {
                items[0].Text = $"设备: {deviceName} {(isOnline ? "●在线" : "○离线")}";
                items[0].ForeColor = isOnline ? Color.Green : Color.Red;
                items[1].Text = servoOn ? "伺服: 已使能" : "伺服: 未使能";
                items[1].ForeColor = servoOn ? Color.Green : Color.Orange;
                items[2].Text = $"限位: {limitStatus}";
                items[2].ForeColor = limitStatus.Contains("限位") ? Color.Red : Color.Green;
                items[3].Text = watchdogTimeout ? "看门狗: 超时!" : $"看门狗: 正常 ({watchdogRemainingMs}ms)";
                items[3].ForeColor = watchdogTimeout ? Color.Red : Color.Green;
                items[4].Text = modbusConnected ? "Modbus: 已连接" : "Modbus: 未连接";
                items[4].ForeColor = modbusConnected ? Color.Green : Color.Red;
                items[5].Text = $"当前指令: {currentStep}";
            }
        }

        public void UpdateAlarmList(IEnumerable<AlarmRecord> alarms) { }
        public string GetSelectedAlarmId() => "";

        public void AppendOperationLog(string message)
        {
            if (txtOperationLog.InvokeRequired)
            {
                txtOperationLog.BeginInvoke(new Action(() => AppendOperationLog(message)));
                return;
            }
            txtOperationLog.AppendText(message + Environment.NewLine);
            if (txtOperationLog.Lines.Length > 500)
            {
                var lines = txtOperationLog.Lines;
                txtOperationLog.Lines = lines[100..];
            }
            txtOperationLog.ScrollToCaret();
        }

        public void AppendMonitorLog(string message)
        {
            if (txtMonitorLog.InvokeRequired)
            {
                txtMonitorLog.BeginInvoke(new Action(() => AppendMonitorLog(message)));
                return;
            }
            txtMonitorLog.AppendText(message + Environment.NewLine);
            if (txtMonitorLog.Lines.Length > 500)
            {
                var lines = txtMonitorLog.Lines;
                txtMonitorLog.Lines = lines[100..];
            }
            txtMonitorLog.ScrollToCaret();
        }

        public void ClearLogs()
        {
            if (txtOperationLog.InvokeRequired)
            {
                txtOperationLog.BeginInvoke(new Action(ClearLogs));
                return;
            }
            txtOperationLog.Clear();
            txtMonitorLog.Clear();
        }

        public void AppendExecutionLog(string message)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => AppendExecutionLog(message)));
                return;
            }
            workflowExecutionControl?.AppendLog(message);
        }

        public void SetSimulationMode(bool isSimulation) { }

        public void UpdateUIByPermissions(string role)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => UpdateUIByPermissions(role)));
                return;
            }

            bool isLoggedIn = !string.IsNullOrEmpty(role) && role != "未登录";
            bool isAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
            bool isEngineer = string.Equals(role, "Engineer", StringComparison.OrdinalIgnoreCase) || isAdmin;

            btnAddDevice.Enabled = isLoggedIn && isEngineer;
            btnRemoveDevice.Enabled = isLoggedIn && isEngineer;

            btnStartAll.Enabled = isLoggedIn;
            btnStopAll.Enabled = isLoggedIn;
            btnStartSelected.Enabled = isLoggedIn;
            btnStopSelected.Enabled = isLoggedIn;
            btnResetDevice.Enabled = isLoggedIn;

            btnSystemConfig.Visible = isLoggedIn && isEngineer;
            btnSystemConfig.Enabled = isLoggedIn && isEngineer;

            btnResetAlarm.Enabled = isLoggedIn;
            btnEmergencyStop.Enabled = true;

            // ⭐ MES 手动重传按钮需要登录后才能使用
            if (btnMesRetry != null)
                btnMesRetry.Enabled = isLoggedIn;

            if (isLoggedIn)
            {
                lblUserInfo.Text = $"👤 {role}";
                btnLogin.Text = "登出";
            }
            else
            {
                lblUserInfo.Text = "未登录";
                btnLogin.Text = "登录";
            }

            workflowExecutionControl?.SetPermissions(isLoggedIn, isEngineer);
        }

        private void SetupToolTips()
        {
            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(btnResetAlarm, "确认并解决所有设备的当前活动报警\n（报警已处理后的确认操作）");
            toolTip.SetToolTip(btnResetDevice, "重置选中设备的工作流状态\n• 软复位：从断点继续，保留产量\n• 硬复位：产量归零，从头开始");
            toolTip.SetToolTip(btnSystemConfig, "打开系统配置中心（工程师/管理员权限）");
            if (btnMesRetry != null)
                toolTip.SetToolTip(btnMesRetry, "手动触发 MES 待重传数据上报\n（4xx 错误和超出重试次数的数据会被跳过）");
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

        private void ListBoxDevices_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            e.DrawBackground();

            if (listBoxDevices.Items[e.Index] is not DeviceListItem item) return;

            Color statusColor = item.IsOnline ? Color.Green : Color.Red;

            using (var brush = new SolidBrush(e.ForeColor))
            using (var statusBrush = new SolidBrush(statusColor))
            {
                e.Graphics.FillEllipse(statusBrush, e.Bounds.X + 5, e.Bounds.Y + 5, 10, 10);
                e.Graphics.DrawString(item.Name, e.Font, brush, e.Bounds.X + 22, e.Bounds.Y + 2);
            }

            e.DrawFocusRectangle();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
                _presenter = null;
                _mesService?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}