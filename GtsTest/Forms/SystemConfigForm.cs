using GtsTest.Controls;
using GtsTest.Core;
using GtsTest.Forms;
using GtsTest.Models;
using GtsTest.Presenters;
using GtsTest.Services;
using GtsTest.Services.Authentication;
using GtsTest.Services.Data;
using GtsTest.Services.Mes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GtsTest
{
    public partial class SystemConfigForm : Form
    {
        private readonly DeviceManager _deviceManager;
        private readonly GtsModel _model;
        private readonly IDataRepository _repo;
        private readonly IAuthenticationService _authService;
        private readonly User _currentUser;

        // 子控件
        private WorkflowControl workflowControl;
        private CommunicationControl commControl;
        private MqttControl mqttControl;
        private MqttPresenter mqttPresenter;
        private Panel debugPanel;
        private DebugToolboxForm debugControl;

        // ⭐ 日志级别 UI 控件（动态创建）
        private Label? _lblCurrentLogLevel;
        private ComboBox? _cmbLogLevel;

        // 用户管理控件
        private ListView listViewUsers;
        private Button btnAddUser;
        private Button btnEditUser;
        private Button btnDeleteUser;
        private Button btnResetPassword;
        private Label lblUserStatus;
        private Button btnToggleStatus;
        private CheckBox chkShowDeleted;

        public SystemConfigForm(
            DeviceManager deviceManager,
            GtsModel model,
            IDataRepository repo,
            IAuthenticationService authService,
            User currentUser)
        {
            _deviceManager = deviceManager;
            _model = model;
            _repo = repo;
            _authService = authService;
            _currentUser = currentUser;

            InitializeComponent();

            BindEvents();
            ApplyPermissions();
            LoadControls();

            // ⭐ 动态添加日志级别面板（必须在 ApplyPermissions 之后，因为 Tab 页会被重建）
            AddLogLevelPanel();

            LoadUserManagement();

            this.Text = $"🔧 系统配置中心 - {currentUser?.FullName ?? "工程师"}";
            this.Icon = SystemIcons.Application;

            btnToggleMode.Text = GtsModel.UseSimulation ? "切换到真实" : "切换到模拟";

            AppLogger.Info($"SystemConfigForm 初始化: 用户={currentUser?.Username}, 角色={currentUser?.Role}", "SystemConfig");
        }

        private void BindEvents()
        {
            btnInit.Click += BtnInit_Click;
            btnToggleMode.Click += BtnToggleMode_Click;
            btnHotReload.Click += BtnHotReload_Click;
            btnSaveConfig.Click += BtnSaveConfig_Click;
            btnDumpBlackBox.Click += BtnDumpBlackBox_Click;
            btnClearLogs.Click += BtnClearLogs_Click;
            btnDiagnostics.Click += BtnDiagnostics_Click;
        }

        // ================================================================
        // ⭐ 动态添加"日志级别"面板
        // ================================================================
        private void AddLogLevelPanel()
        {
            try
            {
                // 1. 找到 tabSystemTools 里的主 TableLayoutPanel
                TableLayoutPanel? mainTable = null;
                foreach (Control ctrl in tabSystemTools.Controls)
                {
                    if (ctrl is Panel panel)
                    {
                        foreach (Control inner in panel.Controls)
                        {
                            if (inner is TableLayoutPanel t)
                            {
                                mainTable = t;
                                break;
                            }
                        }
                    }
                    if (mainTable != null) break;
                }

                if (mainTable == null)
                {
                    AppLogger.Warn("未找到系统工具主布局，跳过日志级别面板添加", "SystemConfig");
                    return;
                }

                // 2. 构造 GroupBox
                var grpLogLevel = new GroupBox
                {
                    Text = "📝 日志级别",
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    Padding = new Padding(10),
                    Dock = DockStyle.Fill,
                    AutoSize = true,
                    MinimumSize = new Size(400, 55)
                };

                var logLevelTable = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 3,
                    RowCount = 1,
                    AutoSize = true,
                    Padding = new Padding(5)
                };
                logLevelTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
                logLevelTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
                logLevelTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));

                // 当前级别显示
                _lblCurrentLogLevel = new Label
                {
                    Text = $"当前级别: {GtsTest.Data.LoggingConfig.GetCurrentLevelName()}",
                    Font = new Font("Segoe UI", 10F),
                    ForeColor = Color.DarkBlue,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Margin = new Padding(3)
                };

                // 级别下拉框
                _cmbLogLevel = new ComboBox
                {
                    Dock = DockStyle.Fill,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Margin = new Padding(3)
                };
                _cmbLogLevel.Items.AddRange(new object[] { "Trace", "Debug", "Info", "Warn", "Error", "Fatal" });
                _cmbLogLevel.SelectedItem = GtsTest.Data.LoggingConfig.GetCurrentLevelName();

                // 应用按钮
                var btnApplyLevel = new Button
                {
                    Text = "应用级别",
                    Dock = DockStyle.Fill,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.LightGreen,
                    Margin = new Padding(3)
                };
                btnApplyLevel.Click += (s, e) =>
                {
                    string selected = _cmbLogLevel.SelectedItem?.ToString() ?? "Info";
                    if (GtsTest.Data.LoggingConfig.SetLevel(selected))
                    {
                        if (_lblCurrentLogLevel != null)
                            _lblCurrentLogLevel.Text = $"当前级别: {selected}";
                        AppLogger.Info($"🔄 日志级别已切换为: {selected}", "SystemConfig");
                        MessageBox.Show($"日志级别已切换为: {selected}", "提示",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show($"无效的日志级别: {selected}", "错误",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                logLevelTable.Controls.Add(_lblCurrentLogLevel, 0, 0);
                logLevelTable.Controls.Add(_cmbLogLevel, 1, 0);
                logLevelTable.Controls.Add(btnApplyLevel, 2, 0);

                grpLogLevel.Controls.Add(logLevelTable);

                // 3. 追加到 mainTable 末尾
                mainTable.Controls.Add(grpLogLevel, 0, mainTable.RowCount);
                mainTable.RowCount++;

                AppLogger.Info("✅ 日志级别面板已添加", "SystemConfig");
            }
            catch (Exception ex)
            {
                AppLogger.Error($"添加日志级别面板失败: {ex.Message}", "SystemConfig");
            }
        }

        // ================================================================
        // 加载子控件
        // ================================================================
        private void LoadControls()
        {
            // ---- 1. 工作流 ----
            try
            {
                workflowControl = new WorkflowControl(_model, _deviceManager);
                workflowControl.Dock = DockStyle.Fill;
                tabWorkflow.Controls.Add(workflowControl);
                AppLogger.Info("✅ 工作流控件加载成功", "SystemConfig");
            }
            catch (Exception ex)
            {
                AppLogger.Error($"❌ 工作流控件加载失败: {ex.Message}", "SystemConfig");
                tabWorkflow.Controls.Add(new Label
                {
                    Text = $"加载失败: {ex.Message}",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = Color.Red
                });
            }

            // ---- 2. OPC UA ----
            try
            {
                commControl = new CommunicationControl();
                commControl.Dock = DockStyle.Fill;
                tabComm.Controls.Add(commControl);
                AppLogger.Info("✅ OPC UA 控件加载成功", "SystemConfig");
            }
            catch (Exception ex)
            {
                AppLogger.Error($"❌ OPC UA 控件加载失败: {ex.Message}", "SystemConfig");
                tabComm.Controls.Add(new Label
                {
                    Text = $"加载失败: {ex.Message}",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = Color.Red
                });
            }

            // ---- 3. MQTT ----
            try
            {
                mqttControl = new MqttControl();
                mqttControl.Dock = DockStyle.Fill;
                tabMqtt.Controls.Add(mqttControl);

                var mqttService = new MqttService();
                mqttPresenter = new MqttPresenter(mqttControl, mqttService);

                AppLogger.Info("✅ MQTT 控件加载成功", "SystemConfig");
            }
            catch (Exception ex)
            {
                AppLogger.Error($"❌ MQTT 控件加载失败: {ex.Message}", "SystemConfig");
                tabMqtt.Controls.Add(new Label
                {
                    Text = $"加载失败: {ex.Message}",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = Color.Red
                });
            }

            // ---- 4. 调试工具 ----
            try
            {
                debugPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(6) };
                debugControl = new DebugToolboxForm(
                    _deviceManager,
                    _model,
                    _deviceManager.AlarmManager,
                    _repo,
                    _authService);
                debugControl.TopLevel = false;
                debugControl.FormBorderStyle = FormBorderStyle.None;
                debugControl.Dock = DockStyle.Fill;
                debugControl.Visible = true;
                debugPanel.Controls.Add(debugControl);
                tabDebug.Controls.Add(debugPanel);
                AppLogger.Info("✅ 调试工具控件加载成功", "SystemConfig");
            }
            catch (Exception ex)
            {
                AppLogger.Error($"❌ 调试工具控件加载失败: {ex.Message}", "SystemConfig");
                tabDebug.Controls.Add(new Label
                {
                    Text = $"加载失败: {ex.Message}",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = Color.Red
                });
            }
        }

        // ================================================================
        // 用户管理
        // ================================================================
        private void LoadUserManagement()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            var btnPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 45,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 0, 0, 5)
            };

            btnAddUser = new Button { Text = "➕ 添加用户", Size = new Size(90, 30), FlatStyle = FlatStyle.Flat, BackColor = Color.LightGreen };
            btnEditUser = new Button { Text = "✏️ 编辑用户", Size = new Size(90, 30), FlatStyle = FlatStyle.Flat, BackColor = Color.LightYellow, Enabled = false };
            btnToggleStatus = new Button { Text = "🔄 切换状态", Size = new Size(90, 30), FlatStyle = FlatStyle.Flat, BackColor = Color.LightBlue, Enabled = false };
            btnDeleteUser = new Button { Text = "🗑️ 删除用户", Size = new Size(90, 30), FlatStyle = FlatStyle.Flat, BackColor = Color.LightCoral, Enabled = false };
            btnResetPassword = new Button { Text = "🔑 重置密码", Size = new Size(90, 30), FlatStyle = FlatStyle.Flat, BackColor = Color.LightBlue, Enabled = false };

            btnAddUser.Click += BtnAddUser_Click;
            btnEditUser.Click += BtnEditUser_Click;
            btnToggleStatus.Click += BtnToggleStatus_Click;
            btnDeleteUser.Click += BtnDeleteUser_Click;
            btnResetPassword.Click += BtnResetPassword_Click;

            btnPanel.Controls.Add(btnAddUser);
            btnPanel.Controls.Add(btnEditUser);
            btnPanel.Controls.Add(btnToggleStatus);
            btnPanel.Controls.Add(btnDeleteUser);
            btnPanel.Controls.Add(btnResetPassword);

            chkShowDeleted = new CheckBox
            {
                Text = "显示已删除用户",
                Dock = DockStyle.Right,
                AutoSize = true,
                Checked = false,
                Margin = new Padding(5, 8, 0, 0)
            };
            chkShowDeleted.CheckedChanged += (s, e) => RefreshUserList();
            btnPanel.Controls.Add(chkShowDeleted);

            lblUserStatus = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 25,
                Text = "就绪",
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 9F)
            };

            listViewUsers = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 9F)
            };
            listViewUsers.Columns.Add("用户名", 120);
            listViewUsers.Columns.Add("全名", 120);
            listViewUsers.Columns.Add("角色", 100);
            listViewUsers.Columns.Add("状态", 80);
            listViewUsers.Columns.Add("创建时间", 150);
            listViewUsers.Columns.Add("失败次数", 80);
            listViewUsers.Columns.Add("删除标记", 80);

            listViewUsers.SelectedIndexChanged += (s, e) =>
            {
                bool hasSelected = listViewUsers.SelectedItems.Count > 0;
                User? selectedUser = hasSelected ? listViewUsers.SelectedItems[0].Tag as User : null;

                btnEditUser.Enabled = hasSelected;
                btnToggleStatus.Enabled = hasSelected && selectedUser?.IsDeleted == 0;
                btnResetPassword.Enabled = hasSelected && selectedUser?.IsDeleted == 0;

                if (hasSelected)
                {
                    var user = selectedUser;
                    btnDeleteUser.Enabled = user != null && user.Username != "admin" && user.IsDeleted == 0;

                    if (user?.IsDeleted == 1)
                    {
                        btnToggleStatus.Text = "已删除，无法切换";
                        btnToggleStatus.Enabled = false;
                    }
                    else
                    {
                        btnToggleStatus.Text = (user != null && user.IsActive == 1) ? "🔴 禁用" : "🟢 启用";
                        btnToggleStatus.Enabled = true;
                    }
                }
                else
                {
                    btnDeleteUser.Enabled = false;
                    btnToggleStatus.Text = "🔄 切换状态";
                    btnToggleStatus.Enabled = false;
                }
            };
            panel.Controls.Add(listViewUsers);
            panel.Controls.Add(lblUserStatus);
            panel.Controls.Add(btnPanel);

            tabAdmin.Controls.Add(panel);
            RefreshUserList();
        }

        private void RefreshUserList()
        {
            try
            {
                listViewUsers.Items.Clear();
                bool showDeleted = chkShowDeleted?.Checked ?? false;
                var users = _repo.GetAllUsers(showDeleted);

                foreach (var user in users)
                {
                    var item = new ListViewItem(user.Username);
                    item.SubItems.Add(user.FullName);
                    item.SubItems.Add(user.Role);

                    if (user.IsDeleted == 1)
                        item.SubItems.Add("已删除");
                    else
                        item.SubItems.Add(user.IsActive == 1 ? "✅ 启用" : "❌ 禁用");

                    item.SubItems.Add(user.CreatedTime);
                    item.SubItems.Add(user.FailedAttempts.ToString());

                    if (user.IsDeleted == 1)
                    {
                        item.ForeColor = Color.Gray;
                        item.SubItems.Add("已删除");
                    }
                    else
                    {
                        item.SubItems.Add("正常");
                    }

                    item.Tag = user;
                    listViewUsers.Items.Add(item);
                }

                lblUserStatus.Text = $"共 {users.Count} 个用户{(showDeleted ? "（含已删除）" : "")}";
            }
            catch (Exception ex)
            {
                lblUserStatus.Text = $"加载用户列表失败: {ex.Message}";
                AppLogger.Error($"加载用户列表失败: {ex.Message}", "SystemConfig");
            }
        }

        private void BtnAddUser_Click(object sender, EventArgs e)
        {
            using (var dialog = new UserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    bool success = _authService.RegisterUser(
                        dialog.Username,
                        dialog.FullName,
                        dialog.Password,
                        dialog.Role);

                    if (success)
                    {
                        RefreshUserList();
                        MessageBox.Show($"用户 {dialog.Username} 创建成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        AppLogger.Info($"管理员 {_currentUser.Username} 创建了用户 {dialog.Username}", "SystemConfig");
                    }
                    else
                    {
                        MessageBox.Show($"创建用户失败，用户名可能已存在", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnEditUser_Click(object sender, EventArgs e)
        {
            if (listViewUsers.SelectedItems.Count == 0) return;
            var user = listViewUsers.SelectedItems[0].Tag as User;
            if (user == null) return;

            using (var dialog = new UserDialog(user))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    user.FullName = dialog.FullName;
                    user.Role = dialog.Role;
                    user.IsActive = dialog.IsActive ? 1 : 0;

                    if (_repo.UpdateUser(user))
                    {
                        RefreshUserList();
                        MessageBox.Show($"用户 {user.Username} 信息已更新", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        AppLogger.Info($"管理员 {_currentUser.Username} 编辑了用户 {user.Username}", "SystemConfig");
                    }
                    else
                    {
                        MessageBox.Show("更新用户信息失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnDeleteUser_Click(object sender, EventArgs e)
        {
            if (listViewUsers.SelectedItems.Count == 0) return;
            var user = listViewUsers.SelectedItems[0].Tag as User;
            if (user == null) return;

            if (user.Username == "admin")
            {
                MessageBox.Show("不能删除管理员账号", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (user.IsDeleted == 1)
            {
                MessageBox.Show("该用户已被删除", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show($"确定要删除用户 {user.Username} 吗？\n（用户数据将保留但标记为已删除）", "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                if (_repo.SoftDeleteUser(user.Id, _currentUser?.Username ?? "系统"))
                {
                    RefreshUserList();
                    MessageBox.Show($"用户 {user.Username} 已标记为删除", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    AppLogger.Info($"管理员 {_currentUser?.Username} 删除了用户 {user.Username}", "SystemConfig");
                    AuditService.Log(_currentUser?.Id ?? 0, _currentUser?.Username ?? "系统", "DeleteUser", $"逻辑删除用户 {user.Username}", _repo);
                }
                else
                {
                    MessageBox.Show("删除失败，请重试", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnResetPassword_Click(object sender, EventArgs e)
        {
            if (listViewUsers.SelectedItems.Count == 0) return;
            var user = listViewUsers.SelectedItems[0].Tag as User;
            if (user == null) return;

            using (var dialog = new ResetPasswordDialog(user.Username))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(dialog.NewPassword) || dialog.NewPassword.Length < 6)
                        {
                            MessageBox.Show("密码至少6位", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        string newHash = AuthenticationHelper.HashPassword(dialog.NewPassword);
                        user.PasswordHash = newHash;
                        user.Salt = null;

                        if (_repo.UpdateUser(user))
                        {
                            RefreshUserList();
                            MessageBox.Show($"用户 {user.Username} 的密码已重置成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            AppLogger.Info($"管理员 {_currentUser.Username} 重置了用户 {user.Username} 的密码", "SystemConfig");

                            AuditService.Log(
                                _currentUser?.Id ?? 0,
                                _currentUser?.Username ?? "系统",
                                "ResetPassword",
                                $"管理员重置了用户 {user.Username} 的密码",
                                _repo
                            );
                        }
                        else
                        {
                            MessageBox.Show("重置密码失败，请重试", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"重置密码失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        AppLogger.Error($"重置密码异常: {ex.Message}", "SystemConfig");
                    }
                }
            }
        }

        // ================================================================
        // 权限控制
        // ================================================================
        private void ApplyPermissions()
        {
            if (_currentUser == null)
            {
                AppLogger.Warn("SystemConfigForm: 当前用户为 null，仅显示基础功能", "SystemConfig");
                return;
            }

            bool isAdmin = string.Equals(_currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase);
            bool isEngineer = string.Equals(_currentUser.Role, "Engineer", StringComparison.OrdinalIgnoreCase) || isAdmin;

            AppLogger.Info($"SystemConfigForm 权限判断: IsAdmin={isAdmin}, IsEngineer={isEngineer}, Role={_currentUser.Role}", "SystemConfig");

            tabMain.TabPages.Clear();

            if (isEngineer)
            {
                tabMain.TabPages.Add(tabWorkflow);
                tabMain.TabPages.Add(tabComm);
                tabMain.TabPages.Add(tabMqtt);
                tabMain.TabPages.Add(tabDebug);
                tabMain.TabPages.Add(tabSystemTools);
            }
            if (isAdmin)
            {
                tabMain.TabPages.Add(tabAdmin);
            }

            if (tabMain.TabPages.Count == 0)
            {
                var page = new TabPage("无权限");
                page.Controls.Add(new Label
                {
                    Text = "您没有权限查看任何配置项",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = Color.Red
                });
                tabMain.TabPages.Add(page);
            }

            tabMain.SelectedIndex = 0;
        }

        private void BtnToggleStatus_Click(object sender, EventArgs e)
        {
            if (listViewUsers.SelectedItems.Count == 0) return;
            var user = listViewUsers.SelectedItems[0].Tag as User;
            if (user == null) return;

            string action = user.IsActive == 1 ? "禁用" : "启用";
            if (MessageBox.Show($"确定要{action}用户 {user.Username} 吗？", $"确认{action}", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                user.IsActive = user.IsActive == 1 ? 0 : 1;
                if (_repo.UpdateUser(user))
                {
                    RefreshUserList();
                    MessageBox.Show($"用户 {user.Username} 已{action}", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    AppLogger.Info($"管理员 {_currentUser.Username} {action}了用户 {user.Username}", "SystemConfig");
                    AuditService.Log(_currentUser?.Id ?? 0, _currentUser?.Username ?? "系统", "ToggleUserStatus", $"{action}用户 {user.Username}", _repo);
                }
                else
                {
                    MessageBox.Show("操作失败，请重试", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ================================================================
        // 系统工具
        // ================================================================
        private void BtnInit_Click(object sender, EventArgs e)
        {
            try
            {
                short result = _model.OpenDevice(0, 0);
                if (result != 0)
                {
                    MessageBox.Show($"初始化失败，错误码: {result}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                _model.CloseDevice();
                MessageBox.Show("运动控制卡初始化成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                AppLogger.Info("运动控制卡初始化成功", "SystemConfig");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化异常: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                AppLogger.Error($"初始化异常: {ex.Message}", "SystemConfig");
            }
        }

        private void BtnToggleMode_Click(object sender, EventArgs e)
        {
            bool isSim = GtsModel.UseSimulation;
            try
            {
                if (!isSim)
                {
                    if (!GtsModel.CheckHardwareAvailable())
                    {
                        MessageBox.Show("未检测到硬件，无法切换到真实模式", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    short result = _model.OpenDevice(0, 0);
                    if (result != 0)
                    {
                        MessageBox.Show($"打开卡失败，错误码: {result}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        _model.CloseDevice();
                        return;
                    }
                    _model.CloseDevice();
                }

                GtsModel.UseSimulation = !isSim;
                btnToggleMode.Text = GtsModel.UseSimulation ? "切换到真实" : "切换到模拟";
                UpdateModeStatusLabel();
                MessageBox.Show($"已切换到 {(GtsModel.UseSimulation ? "模拟" : "真实")} 模式", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                AppLogger.Info($"系统模式已切换: {(GtsModel.UseSimulation ? "模拟" : "真实")}", "SystemConfig");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"切换异常: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                AppLogger.Error($"切换异常: {ex.Message}", "SystemConfig");
            }
        }

        private void UpdateModeStatusLabel()
        {
            foreach (Control ctrl in tabSystemTools.Controls)
            {
                if (ctrl is Panel panel)
                {
                    foreach (Control inner in panel.Controls)
                    {
                        if (inner is TableLayoutPanel table)
                        {
                            foreach (Control sub in table.Controls)
                            {
                                if (sub is GroupBox gb && gb.Text.Contains("模拟模式"))
                                {
                                    foreach (Control gbCtrl in gb.Controls)
                                    {
                                        if (gbCtrl is TableLayoutPanel modeTable)
                                        {
                                            foreach (Control modeCtrl in modeTable.Controls)
                                            {
                                                if (modeCtrl is Label lbl && lbl.Text.StartsWith("当前模式:"))
                                                {
                                                    lbl.Text = $"当前模式: {(GtsModel.UseSimulation ? "模拟模式" : "真实模式")}";
                                                    return;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // ================================================================
        // ⭐ 热重载配置（设备 + 日志 + MES + 数据库检测）
        // ================================================================
        private void BtnHotReload_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(
                "确定要热重载所有配置吗？\n\n" +
                "• 设备配置 (devices.json) → 立即生效\n" +
                "• 日志配置 (appsettings.json → Logging) → 立即生效\n" +
                "• MES 配置 (mes_config.json) → 立即生效\n" +
                "• 数据库配置 (appsettings.json → Database) → 需重启程序生效",
                "热重载配置", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            btnHotReload.Enabled = false;
            btnHotReload.Text = "⏳ 重载中...";

            var sb = new StringBuilder();
            int successCount = 0;
            int failCount = 0;

            try
            {
                // 1. 设备配置
                try
                {
                    string devicesPath = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory, "devices.json");

                    if (!File.Exists(devicesPath))
                    {
                        sb.AppendLine("⚠️ 设备配置: devices.json 不存在，跳过");
                    }
                    else
                    {
                        string json = File.ReadAllText(devicesPath);
                        if (_deviceManager.ImportConfig(json))
                        {
                            int deviceCount = _deviceManager.GetAllDevices().Count;
                            sb.AppendLine($"✅ 设备配置: 已重载 {deviceCount} 台设备");
                            AppLogger.Info($"设备配置热重载成功: {devicesPath}", "SystemConfig");
                            successCount++;
                        }
                        else
                        {
                            sb.AppendLine("❌ 设备配置: 加载失败");
                            AppLogger.Error($"设备配置热重载失败: {devicesPath}", "SystemConfig");
                            failCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"❌ 设备配置: 异常 - {ex.Message}");
                    AppLogger.Error($"设备配置热重载异常: {ex.Message}", "SystemConfig");
                    failCount++;
                }

                // 2. 日志配置
                try
                {
                    var oldLevel = AppLogger.GlobalLogLevel;
                    var logCfg = GtsTest.Data.LoggingConfig.Load();

                    if (Enum.TryParse<LogLevel>(logCfg.Level, true, out var newLevel))
                    {
                        if (newLevel != oldLevel)
                        {
                            AppLogger.GlobalLogLevel = newLevel;
                            AppLogger.MaxFileSizeMB = logCfg.MaxFileSizeMB;
                            AppLogger.RetentionDays = logCfg.RetentionDays;

                            sb.AppendLine($"✅ 日志配置: 级别 {oldLevel} → {newLevel}");
                            AppLogger.Info($"日志级别动态切换: {oldLevel} → {newLevel}", "SystemConfig");
                            successCount++;

                            // ⭐ 更新 UI 显示
                            if (_lblCurrentLogLevel != null)
                                _lblCurrentLogLevel.Text = $"当前级别: {newLevel}";
                            if (_cmbLogLevel != null)
                                _cmbLogLevel.SelectedItem = newLevel.ToString();
                        }
                        else
                        {
                            sb.AppendLine($"ℹ️ 日志配置: 级别无变化 ({oldLevel})");
                        }
                    }
                    else
                    {
                        sb.AppendLine($"⚠️ 日志配置: 无效级别 '{logCfg.Level}'，跳过");
                    }
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"❌ 日志配置: 异常 - {ex.Message}");
                    AppLogger.Error($"日志配置重载异常: {ex.Message}", "SystemConfig");
                    failCount++;
                }

                // 3. MES 配置
                try
                {
                    var mesService = _deviceManager.MesService;
                    if (mesService == null)
                    {
                        sb.AppendLine("⚠️ MES 配置: MES 服务未初始化，跳过");
                    }
                    else
                    {
                        var newMesConfig = MesConfig.Load();
                        mesService.ReloadConfig(newMesConfig);

                        string mesUrl = newMesConfig.Protocol.Equals("SOAP", StringComparison.OrdinalIgnoreCase)
                            ? newMesConfig.SoapEndpoint
                            : newMesConfig.ApiUrl;

                        sb.AppendLine($"✅ MES 配置: 已重载");
                        sb.AppendLine($"     Protocol: {newMesConfig.Protocol}");
                        sb.AppendLine($"     URL: {mesUrl}");
                        sb.AppendLine($"     Enabled: {(newMesConfig.Enabled ? "是" : "否")}");

                        AppLogger.Info($"MES 配置热重载成功: Protocol={newMesConfig.Protocol}, URL={mesUrl}", "SystemConfig");
                        AuditService.Log(_currentUser?.Id ?? 0, _currentUser?.Username ?? "系统",
                            "HotReloadMES", $"重载 MES 配置: Protocol={newMesConfig.Protocol}", _repo);
                        successCount++;
                    }
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"❌ MES 配置: 异常 - {ex.Message}");
                    AppLogger.Error($"MES 配置热重载异常: {ex.Message}", "SystemConfig");
                    failCount++;
                }

                // 4. 数据库配置检测
                try
                {
                    var oldProvider = GtsTest.Data.DbContextFactory.CurrentProvider;
                    var oldConnStr = GtsTest.Data.DbContextFactory.CurrentConnectionString;

                    GtsTest.Data.DbContextFactory.ConfigureFromAppSettings();

                    var newProvider = GtsTest.Data.DbContextFactory.CurrentProvider;
                    var newConnStr = GtsTest.Data.DbContextFactory.CurrentConnectionString;

                    if (oldProvider != newProvider || oldConnStr != newConnStr)
                    {
                        sb.AppendLine($"⚠️ 数据库配置: 已检测到变更");
                        sb.AppendLine($"     Provider: {oldProvider} → {newProvider}");
                        sb.AppendLine($"     ⚠️ 需重启程序才能生效！");
                        AppLogger.Warn($"数据库配置已变更，需重启程序: {oldProvider} → {newProvider}", "SystemConfig");
                    }
                    else
                    {
                        sb.AppendLine("ℹ️ 数据库配置: 无变化");
                    }
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"⚠️ 数据库配置: 检查异常 - {ex.Message}");
                }

                sb.AppendLine();
                sb.AppendLine($"========== 汇总 ==========");
                sb.AppendLine($"成功: {successCount} 项，失败: {failCount} 项");

                string message = sb.ToString();
                var icon = failCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information;

                MessageBox.Show(message, "热重载结果", MessageBoxButtons.OK, icon);
                AppLogger.Info($"热重载完成: 成功 {successCount}, 失败 {failCount}", "SystemConfig");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"热重载失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                AppLogger.Error($"热重载整体失败: {ex.Message}", "SystemConfig");
            }
            finally
            {
                btnHotReload.Enabled = true;
                btnHotReload.Text = "🌡️ 热加载配置";
            }
        }

        private void BtnSaveConfig_Click(object sender, EventArgs e)
        {
            try
            {
                string json = _deviceManager.ExportConfig();
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "devices.json");
                File.WriteAllText(path, json);
                MessageBox.Show($"配置已保存到 {path}", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                AppLogger.Info($"配置已保存: {path}", "SystemConfig");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存异常: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                AppLogger.Error($"保存异常: {ex.Message}", "SystemConfig");
            }
        }

        private void BtnDumpBlackBox_Click(object sender, EventArgs e)
        {
            try
            {
                var file = CyclicMonitorBuffer.DumpToFile("系统配置手动导出");
                if (file != null)
                {
                    MessageBox.Show($"黑匣子已导出到 {file}", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    AppLogger.Info($"黑匣子已导出: {file}", "SystemConfig");
                }
                else
                {
                    MessageBox.Show("黑匣子导出失败，缓冲区为空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    AppLogger.Warn("黑匣子导出失败，缓冲区为空", "SystemConfig");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出异常: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                AppLogger.Error($"导出异常: {ex.Message}", "SystemConfig");
            }
        }

        private void BtnClearLogs_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定要清空操作日志和监控日志吗？（不影响日志文件）", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (this.Owner is Form1 mainForm)
                {
                    mainForm.ClearLogs();
                }
                MessageBox.Show("日志已清空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                AppLogger.Info("界面日志已清空", "SystemConfig");
            }
        }

        private void BtnDiagnostics_Click(object sender, EventArgs e)
        {
            var devices = _deviceManager.GetAllDevices();

            var mesService = _deviceManager.MesService;
            string mesInfo = mesService == null
                ? "未初始化"
                : $"{mesService.Protocol} | {(mesService.IsEnabled ? "启用" : "禁用")} | 待重传: {mesService.PendingCount}";

            string dbInfo = $"Provider={GtsTest.Data.DbContextFactory.CurrentProvider}";
            string logInfo = $"Level={AppLogger.GlobalLogLevel}";

            string info = $"=== 系统诊断 ===\n";
            info += $"设备总数: {devices.Count}\n";
            info += $"在线设备: {devices.Count(d => d.IsOnline)}\n";
            info += $"总产量: {devices.Sum(d => d.Config.CurrentCount)}\n";
            info += $"模拟模式: {GtsModel.UseSimulation}\n";
            info += $"\n=== MES 状态 ===\n";
            info += $"{mesInfo}\n";
            info += $"\n=== 数据库 ===\n";
            info += $"{dbInfo}\n";
            info += $"\n=== 日志 ===\n";
            info += $"{logInfo}\n";
            info += $"\n时间: {DateTime.Now}\n";
            info += $"用户: {_currentUser?.Username} ({_currentUser?.Role})";

            MessageBox.Show(info, "系统诊断", MessageBoxButtons.OK, MessageBoxIcon.Information);
            AppLogger.Info("执行了系统诊断", "SystemConfig");
        }

        // ================================================================
        // 生命周期
        // ================================================================
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (debugControl != null && !debugControl.IsDisposed)
            {
                debugControl.Close();
            }
            mqttPresenter?.Dispose();
            base.OnFormClosing(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
                if (debugControl != null && !debugControl.IsDisposed)
                {
                    debugControl.Dispose();
                }
                workflowControl?.Dispose();
                commControl?.Dispose();
                mqttControl?.Dispose();
                mqttPresenter?.Dispose();
                listViewUsers?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}