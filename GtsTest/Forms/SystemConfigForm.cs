using GtsTest.Controls;
using GtsTest.Core;
using GtsTest.Forms;
using GtsTest.Models;
using GtsTest.Presenters;
using GtsTest.Services;
using GtsTest.Services.Authentication;
using GtsTest.Services.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
        private CommunicationControl commControl;        // OPC UA 控件
        private MqttControl mqttControl;                // MQTT 控件
        private MqttPresenter mqttPresenter;            // MQTT Presenter
        private Panel debugPanel;
        private DebugToolboxForm debugControl;

        // 用户管理控件
        private ListView listViewUsers;
        private Button btnAddUser;
        private Button btnEditUser;
        private Button btnDeleteUser;
        private Button btnResetPassword;
        private Label lblUserStatus;
        private Button btnToggleStatus;   // 切换状态按钮
        private CheckBox chkShowDeleted;  // 显示已删除用户

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
        // 加载子控件（OPC UA 和 MQTT 分别独立 Tab）
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

            // ---- 2. OPC UA 通信配置（独立 Tab） ----
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

            // ---- 3. MQTT 通信配置（独立 Tab） ----
            try
            {
                mqttControl = new MqttControl();
                mqttControl.Dock = DockStyle.Fill;
                tabMqtt.Controls.Add(mqttControl);

                // 创建 MQTT Presenter
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
        // 用户管理界面
        // ================================================================
        private void LoadUserManagement()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            // ---- 按钮面板 ----
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

            // ---- 显示已删除用户复选框 ----
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

            // ---- 状态标签 ----
            lblUserStatus = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 25,
                Text = "就绪",
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 9F)
            };

            // ---- 用户列表 ----
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
            listViewUsers.Columns.Add("删除标记", 80);  // 新增列

            listViewUsers.SelectedIndexChanged += (s, e) =>
            {
                bool hasSelected = listViewUsers.SelectedItems.Count > 0;
                // ★★★ 从选中项中获取 User 对象 ★★★
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

                    // ★★★ 修正：根据 IsDeleted 优先显示状态 ★★★
                    if (user.IsDeleted == 1)
                        item.SubItems.Add("已删除");
                    else
                        item.SubItems.Add(user.IsActive == 1 ? "✅ 启用" : "❌ 禁用");

                    item.SubItems.Add(user.CreatedTime);
                    item.SubItems.Add(user.FailedAttempts.ToString());

                    // 删除标记列（可保留，已单独显示）
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

            if (user.Username == "admin")
            {
                MessageBox.Show("建议通过管理员账号直接修改密码", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // 也可以允许重置，但这里给出提示
            }

            // ★★★ 使用密码重置对话框 ★★★
            using (var dialog = new ResetPasswordDialog(user.Username))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // 验证新密码不为空且长度足够
                        if (string.IsNullOrEmpty(dialog.NewPassword) || dialog.NewPassword.Length < 6)
                        {
                            MessageBox.Show("密码至少6位", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        // 使用 AuthenticationHelper 生成新密码哈希
                        string newHash = AuthenticationHelper.HashPassword(dialog.NewPassword);

                        // 更新用户密码
                        user.PasswordHash = newHash;
                        user.Salt = null;  // 清除旧 Salt

                        if (_repo.UpdateUser(user))
                        {
                            RefreshUserList();
                            MessageBox.Show($"用户 {user.Username} 的密码已重置成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            AppLogger.Info($"管理员 {_currentUser.Username} 重置了用户 {user.Username} 的密码", "SystemConfig");

                            // 审计日志
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
                tabMain.TabPages.Add(tabMqtt);      // 🆕 MQTT 对工程师可见
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
        // 系统工具事件
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

        private void BtnHotReload_Click(object sender, EventArgs e)
        {
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "devices.json");
            if (!System.IO.File.Exists(path))
            {
                MessageBox.Show("配置文件不存在", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string json = System.IO.File.ReadAllText(path);
                if (_deviceManager.ImportConfig(json))
                {
                    MessageBox.Show("配置热加载成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    AppLogger.Info($"配置热加载成功: {path}", "SystemConfig");
                }
                else
                {
                    MessageBox.Show("配置加载失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    AppLogger.Error($"配置加载失败: {path}", "SystemConfig");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载异常: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                AppLogger.Error($"加载异常: {ex.Message}", "SystemConfig");
            }
        }

        private void BtnSaveConfig_Click(object sender, EventArgs e)
        {
            try
            {
                string json = _deviceManager.ExportConfig();
                string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "devices.json");
                System.IO.File.WriteAllText(path, json);
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
            string info = $"设备总数: {devices.Count}\n";
            info += $"在线设备: {devices.Count(d => d.IsOnline)}\n";
            info += $"总产量: {devices.Sum(d => d.Config.CurrentCount)}\n";
            info += $"模拟模式: {GtsModel.UseSimulation}\n";
            info += $"时间: {DateTime.Now}\n";
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