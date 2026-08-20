using GtsTest.Controls;
using GtsTest.Presenters;

namespace GtsTest
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        #region Windows 窗体设计器生成的代码

        private void InitializeComponent()
        {
            this.rootLayout = new System.Windows.Forms.TableLayoutPanel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemHotReload = new System.Windows.Forms.ToolStripMenuItem();
            this.viewMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemDBManager = new System.Windows.Forms.ToolStripMenuItem();
            this.helpMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnInit = new System.Windows.Forms.ToolStripButton();
            this.btnEmergencyStop = new System.Windows.Forms.ToolStripButton();
            this.btnResetAlarm = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnRunFlow = new System.Windows.Forms.ToolStripButton();
            this.btnStopFlow = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.btnToggleSim = new System.Windows.Forms.ToolStripButton();
            this.btnToggleModbus = new System.Windows.Forms.ToolStripButton();
            this.btnConnectAll = new System.Windows.Forms.ToolStripButton();
            this.btnDisconnectAll = new System.Windows.Forms.ToolStripButton();
            this.btnSaveConfig = new System.Windows.Forms.ToolStripButton();
            this.btnDebugToolbox = new System.Windows.Forms.ToolStripButton();
            this.contentPanel = new System.Windows.Forms.TableLayoutPanel();
            this.mainContainer = new System.Windows.Forms.TableLayoutPanel();
            this.leftPanel = new System.Windows.Forms.Panel();
            this.listBoxDevices = new System.Windows.Forms.ListBox();
            this.lblDeviceList = new System.Windows.Forms.Label();
            this.flowDeviceButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.btnAddDevice = new System.Windows.Forms.Button();
            this.btnRemoveDevice = new System.Windows.Forms.Button();
            this.btnStartAll = new System.Windows.Forms.Button();
            this.btnStopAll = new System.Windows.Forms.Button();
            this.panelGlobalStats = new System.Windows.Forms.Panel();
            this.lblOnlineCount = new System.Windows.Forms.Label();
            this.lblTotalProduction = new System.Windows.Forms.Label();
            this.centerPanel = new System.Windows.Forms.Panel();
            // 新 TabControl（功能型）
            this.tabMain = new System.Windows.Forms.TabControl();
            this.tabPageOverview = new System.Windows.Forms.TabPage();
            this.tabPageAlarm = new System.Windows.Forms.TabPage();
            this.tabPageCamera = new System.Windows.Forms.TabPage();
            this.tabPageWorkflow = new System.Windows.Forms.TabPage();
            this.tabPageComm = new System.Windows.Forms.TabPage();
            this.tabPageMqtt = new System.Windows.Forms.TabPage();
            // 报警列表（移至中间Tab）
            this.listViewAlarms = new System.Windows.Forms.ListView();
            this.columnHeaderId = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderDevice = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderMessage = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderSeverity = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderTime = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderStatus = new System.Windows.Forms.ColumnHeader();
            // 用户控件（仅声明，不在设计器中实例化）
            // 实际实例在 Form1.cs 中创建并添加到对应Tab页
            this.overviewControl = null;   // 设计器中不赋值
            this.cameraControl = null;
            this.workflowControl = null;
            this.communicationControl = null;

            this.rightPanel = new System.Windows.Forms.Panel();
            this.rightTable = new System.Windows.Forms.TableLayoutPanel();
            this.grpProductionStats = new System.Windows.Forms.GroupBox();
            this.tableLayoutStats = new System.Windows.Forms.TableLayoutPanel();
            this.lblTotalProdTitle = new System.Windows.Forms.Label();
            this.lblTotalProdValue = new System.Windows.Forms.Label();
            this.lblYieldRateTitle = new System.Windows.Forms.Label();
            this.lblYieldRateValue = new System.Windows.Forms.Label();
            this.lblOnlineCountTitle = new System.Windows.Forms.Label();
            this.lblOnlineCountValue = new System.Windows.Forms.Label();
            this.lblAlarmCountTitle = new System.Windows.Forms.Label();
            this.lblAlarmCountValue = new System.Windows.Forms.Label();

            this.tabLogControl = new System.Windows.Forms.TabControl();
            this.tabPageOpLog = new System.Windows.Forms.TabPage();
            this.txtOperationLog = new System.Windows.Forms.TextBox();
            this.tabPageMonLog = new System.Windows.Forms.TabPage();
            this.txtMonitorLog = new System.Windows.Forms.TextBox();

            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblDeviceStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblServoStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblLimitStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblWatchdogStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblModbusStatusStrip = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblCurrentCmd = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblLoggedUser = new System.Windows.Forms.ToolStripStatusLabel();
            this.btnLogin = new System.Windows.Forms.ToolStripButton();

            this.rootLayout.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.contentPanel.SuspendLayout();
            this.mainContainer.SuspendLayout();
            this.leftPanel.SuspendLayout();
            this.flowDeviceButtons.SuspendLayout();
            this.panelGlobalStats.SuspendLayout();
            this.centerPanel.SuspendLayout();
            this.tabMain.SuspendLayout();
            this.tabPageAlarm.SuspendLayout();
            this.rightPanel.SuspendLayout();
            this.rightTable.SuspendLayout();
            this.grpProductionStats.SuspendLayout();
            this.tableLayoutStats.SuspendLayout();
            this.tabLogControl.SuspendLayout();
            this.tabPageOpLog.SuspendLayout();
            this.tabPageMonLog.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();

            // 
            // rootLayout
            // 
            this.rootLayout.ColumnCount = 1;
            this.rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rootLayout.Controls.Add(this.menuStrip1, 0, 0);
            this.rootLayout.Controls.Add(this.toolStrip1, 0, 1);
            this.rootLayout.Controls.Add(this.contentPanel, 0, 2);
            this.rootLayout.Controls.Add(this.statusStrip1, 0, 3);
            this.rootLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rootLayout.Location = new System.Drawing.Point(0, 0);
            this.rootLayout.Name = "rootLayout";
            this.rootLayout.RowCount = 4;
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.rootLayout.Size = new System.Drawing.Size(1561, 995);
            this.rootLayout.TabIndex = 0;

            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.fileMenu,
                this.viewMenu,
                this.toolsMenu,
                this.helpMenu
            });
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1561, 25);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";

            this.fileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.toolStripMenuItemHotReload
            });
            this.fileMenu.Name = "fileMenu";
            this.fileMenu.Size = new System.Drawing.Size(44, 21);
            this.fileMenu.Text = "文件";
            this.toolStripMenuItemHotReload.Name = "toolStripMenuItemHotReload";
            this.toolStripMenuItemHotReload.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F5)));
            this.toolStripMenuItemHotReload.Size = new System.Drawing.Size(260, 22);
            this.toolStripMenuItemHotReload.Text = "🔄 热加载配置 (Ctrl+F5)";

            this.viewMenu.Name = "viewMenu";
            this.viewMenu.Size = new System.Drawing.Size(44, 21);
            this.viewMenu.Text = "视图";

            this.toolsMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.toolStripMenuItemDBManager
            });
            this.toolsMenu.Name = "toolsMenu";
            this.toolsMenu.Size = new System.Drawing.Size(44, 21);
            this.toolsMenu.Text = "工具";
            this.toolStripMenuItemDBManager.Name = "toolStripMenuItemDBManager";
            this.toolStripMenuItemDBManager.Size = new System.Drawing.Size(156, 22);
            this.toolStripMenuItemDBManager.Text = "📊 数据库管理";

            this.helpMenu.Name = "helpMenu";
            this.helpMenu.Size = new System.Drawing.Size(44, 21);
            this.helpMenu.Text = "帮助";

            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.btnInit,
                this.btnEmergencyStop,
                this.btnResetAlarm,
                this.toolStripSeparator1,
                this.btnRunFlow,
                this.btnStopFlow,
                this.toolStripSeparator2,
                this.btnToggleSim,
                this.btnToggleModbus,
                this.btnConnectAll,
                this.btnDisconnectAll,
                this.btnSaveConfig,
                this.btnDebugToolbox
            });
            this.toolStrip1.Location = new System.Drawing.Point(0, 32);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1561, 25);
            this.toolStrip1.TabIndex = 1;

            this.btnInit.Name = "btnInit";
            this.btnInit.Size = new System.Drawing.Size(48, 22);
            this.btnInit.Text = "初始化";

            this.btnEmergencyStop.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnEmergencyStop.ForeColor = System.Drawing.Color.Red;
            this.btnEmergencyStop.Name = "btnEmergencyStop";
            this.btnEmergencyStop.Size = new System.Drawing.Size(39, 22);
            this.btnEmergencyStop.Text = "急停";

            this.btnResetAlarm.ForeColor = System.Drawing.Color.DarkOrange;
            this.btnResetAlarm.Name = "btnResetAlarm";
            this.btnResetAlarm.Size = new System.Drawing.Size(80, 22);
            this.btnResetAlarm.Text = "🔕 复位报警";

            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);

            this.btnRunFlow.Name = "btnRunFlow";
            this.btnRunFlow.Size = new System.Drawing.Size(60, 22);
            this.btnRunFlow.Text = "运行流程";

            this.btnStopFlow.Name = "btnStopFlow";
            this.btnStopFlow.Size = new System.Drawing.Size(60, 22);
            this.btnStopFlow.Text = "停止流程";

            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);

            this.btnToggleSim.Name = "btnToggleSim";
            this.btnToggleSim.Size = new System.Drawing.Size(60, 22);
            this.btnToggleSim.Text = "切换模式";

            this.btnToggleModbus.Name = "btnToggleModbus";
            this.btnToggleModbus.Size = new System.Drawing.Size(89, 22);
            this.btnToggleModbus.Text = "连接 Modbus";

            this.btnConnectAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnConnectAll.Name = "btnConnectAll";
            this.btnConnectAll.Size = new System.Drawing.Size(60, 22);
            this.btnConnectAll.Text = "全部连接";

            this.btnDisconnectAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnDisconnectAll.Name = "btnDisconnectAll";
            this.btnDisconnectAll.Size = new System.Drawing.Size(60, 22);
            this.btnDisconnectAll.Text = "全部断开";

            this.btnSaveConfig.Name = "btnSaveConfig";
            this.btnSaveConfig.Size = new System.Drawing.Size(60, 22);
            this.btnSaveConfig.Text = "保存配置";

            this.btnDebugToolbox.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnDebugToolbox.Name = "btnDebugToolbox";
            this.btnDebugToolbox.Size = new System.Drawing.Size(56, 22);
            this.btnDebugToolbox.Text = "🔧 调试";
            this.btnDebugToolbox.ToolTipText = "打开调试工具箱（轴控、Modbus、单机调试）";

            // 
            // contentPanel
            // 
            this.contentPanel.ColumnCount = 1;
            this.contentPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.contentPanel.Controls.Add(this.mainContainer, 0, 0);
            this.contentPanel.Controls.Add(this.tabLogControl, 0, 1);
            this.contentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.contentPanel.Location = new System.Drawing.Point(3, 75);
            this.contentPanel.Name = "contentPanel";
            this.contentPanel.RowCount = 2;
            this.contentPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 74.35F));
            this.contentPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25.65F));
            this.contentPanel.Size = new System.Drawing.Size(1555, 889);
            this.contentPanel.TabIndex = 2;

            // 
            // mainContainer
            // 
            this.mainContainer.ColumnCount = 3;
            this.mainContainer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 240F));
            this.mainContainer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 55F));
            this.mainContainer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
            this.mainContainer.Controls.Add(this.leftPanel, 0, 0);
            this.mainContainer.Controls.Add(this.centerPanel, 1, 0);
            this.mainContainer.Controls.Add(this.rightPanel, 2, 0);
            this.mainContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainContainer.Location = new System.Drawing.Point(3, 3);
            this.mainContainer.Name = "mainContainer";
            this.mainContainer.RowCount = 1;
            this.mainContainer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainContainer.Size = new System.Drawing.Size(1549, 654);
            this.mainContainer.TabIndex = 0;

            // 
            // leftPanel
            // 
            this.leftPanel.Controls.Add(this.listBoxDevices);
            this.leftPanel.Controls.Add(this.lblDeviceList);
            this.leftPanel.Controls.Add(this.flowDeviceButtons);
            this.leftPanel.Controls.Add(this.panelGlobalStats);
            this.leftPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.leftPanel.Location = new System.Drawing.Point(3, 3);
            this.leftPanel.Name = "leftPanel";
            this.leftPanel.Padding = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.leftPanel.Size = new System.Drawing.Size(234, 648);
            this.leftPanel.TabIndex = 0;

            this.listBoxDevices.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxDevices.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.listBoxDevices.Location = new System.Drawing.Point(5, 34);
            this.listBoxDevices.Name = "listBoxDevices";
            this.listBoxDevices.Size = new System.Drawing.Size(224, 497);
            this.listBoxDevices.TabIndex = 0;
            this.listBoxDevices.DrawItem += this.ListBoxDevices_DrawItem;

            this.lblDeviceList.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblDeviceList.Location = new System.Drawing.Point(5, 6);
            this.lblDeviceList.Name = "lblDeviceList";
            this.lblDeviceList.Size = new System.Drawing.Size(224, 28);
            this.lblDeviceList.TabIndex = 1;
            this.lblDeviceList.Text = "🖥️ 设备列表";

            this.flowDeviceButtons.Controls.Add(this.btnAddDevice);
            this.flowDeviceButtons.Controls.Add(this.btnRemoveDevice);
            this.flowDeviceButtons.Controls.Add(this.btnStartAll);
            this.flowDeviceButtons.Controls.Add(this.btnStopAll);
            this.flowDeviceButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowDeviceButtons.Location = new System.Drawing.Point(5, 531);
            this.flowDeviceButtons.Name = "flowDeviceButtons";
            this.flowDeviceButtons.Size = new System.Drawing.Size(224, 66);
            this.flowDeviceButtons.TabIndex = 2;

            this.btnAddDevice.Location = new System.Drawing.Point(3, 3);
            this.btnAddDevice.Name = "btnAddDevice";
            this.btnAddDevice.Size = new System.Drawing.Size(100, 26);
            this.btnAddDevice.TabIndex = 0;
            this.btnAddDevice.Text = "➕ 添加设备";

            this.btnRemoveDevice.Location = new System.Drawing.Point(109, 3);
            this.btnRemoveDevice.Name = "btnRemoveDevice";
            this.btnRemoveDevice.Size = new System.Drawing.Size(100, 26);
            this.btnRemoveDevice.TabIndex = 1;
            this.btnRemoveDevice.Text = "➖ 移除设备";

            this.btnStartAll.BackColor = System.Drawing.Color.LightGreen;
            this.btnStartAll.Location = new System.Drawing.Point(3, 35);
            this.btnStartAll.Name = "btnStartAll";
            this.btnStartAll.Size = new System.Drawing.Size(100, 26);
            this.btnStartAll.TabIndex = 2;
            this.btnStartAll.Text = "▶ 全部启动";
            this.btnStartAll.UseVisualStyleBackColor = false;

            this.btnStopAll.BackColor = System.Drawing.Color.LightCoral;
            this.btnStopAll.Location = new System.Drawing.Point(109, 35);
            this.btnStopAll.Name = "btnStopAll";
            this.btnStopAll.Size = new System.Drawing.Size(100, 26);
            this.btnStopAll.TabIndex = 3;
            this.btnStopAll.Text = "⏹ 全部停止";
            this.btnStopAll.UseVisualStyleBackColor = false;

            this.panelGlobalStats.Controls.Add(this.lblOnlineCount);
            this.panelGlobalStats.Controls.Add(this.lblTotalProduction);
            this.panelGlobalStats.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelGlobalStats.Location = new System.Drawing.Point(5, 597);
            this.panelGlobalStats.Name = "panelGlobalStats";
            this.panelGlobalStats.Size = new System.Drawing.Size(224, 45);
            this.panelGlobalStats.TabIndex = 3;

            this.lblOnlineCount.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblOnlineCount.Location = new System.Drawing.Point(0, 0);
            this.lblOnlineCount.Name = "lblOnlineCount";
            this.lblOnlineCount.Size = new System.Drawing.Size(100, 45);
            this.lblOnlineCount.TabIndex = 0;
            this.lblOnlineCount.Text = "在线: 0/0";

            this.lblTotalProduction.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblTotalProduction.Location = new System.Drawing.Point(124, 0);
            this.lblTotalProduction.Name = "lblTotalProduction";
            this.lblTotalProduction.Size = new System.Drawing.Size(100, 45);
            this.lblTotalProduction.TabIndex = 1;
            this.lblTotalProduction.Text = "总产量: 0";

            // 
            // centerPanel
            // 
            this.centerPanel.Controls.Add(this.tabMain);
            this.centerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.centerPanel.Location = new System.Drawing.Point(243, 3);
            this.centerPanel.Name = "centerPanel";
            this.centerPanel.Size = new System.Drawing.Size(713, 648);
            this.centerPanel.TabIndex = 1;

            // 
            // tabMain
            // 
            this.tabMain.Controls.Add(this.tabPageOverview);
            this.tabMain.Controls.Add(this.tabPageAlarm);
            this.tabMain.Controls.Add(this.tabPageCamera);
            this.tabMain.Controls.Add(this.tabPageWorkflow);
            this.tabMain.Controls.Add(this.tabPageComm);
            this.tabMain.Controls.Add(this.tabPageMqtt);
            this.tabMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabMain.Location = new System.Drawing.Point(0, 0);
            this.tabMain.Name = "tabMain";
            this.tabMain.SelectedIndex = 0;
            this.tabMain.Size = new System.Drawing.Size(713, 648);
            this.tabMain.TabIndex = 0;

            // Tab页标题
            this.tabPageOverview.Text = "📊 产线总览";
            this.tabPageAlarm.Text = "🚨 报警中心";
            this.tabPageCamera.Text = "📷 视觉检测";
            this.tabPageWorkflow.Text = "⚙️ 工作流";
            this.tabPageComm.Text = "🌐 通信中心";
            this.tabPageMqtt.Text = "📡 MQTT";

            // 报警Tab页容纳 listViewAlarms
            this.tabPageAlarm.Controls.Add(this.listViewAlarms);

            // listViewAlarms
            this.listViewAlarms.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                this.columnHeaderId,
                this.columnHeaderDevice,
                this.columnHeaderMessage,
                this.columnHeaderSeverity,
                this.columnHeaderTime,
                this.columnHeaderStatus
            });
            this.listViewAlarms.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewAlarms.FullRowSelect = true;
            this.listViewAlarms.GridLines = true;
            this.listViewAlarms.Location = new System.Drawing.Point(0, 0);
            this.listViewAlarms.Name = "listViewAlarms";
            this.listViewAlarms.Size = new System.Drawing.Size(713, 622);
            this.listViewAlarms.TabIndex = 0;
            this.listViewAlarms.UseCompatibleStateImageBehavior = false;
            this.listViewAlarms.View = System.Windows.Forms.View.Details;

            this.columnHeaderId.Text = "ID";
            this.columnHeaderId.Width = 40;
            this.columnHeaderDevice.Text = "设备";
            this.columnHeaderDevice.Width = 100;
            this.columnHeaderMessage.Text = "消息";
            this.columnHeaderMessage.Width = 400;
            this.columnHeaderSeverity.Text = "严重度";
            this.columnHeaderSeverity.Width = 80;
            this.columnHeaderTime.Text = "时间";
            this.columnHeaderTime.Width = 150;
            this.columnHeaderStatus.Text = "状态";
            this.columnHeaderStatus.Width = 80;

            // 用户控件暂不添加到Tab页（在 Form1.cs 中完成）

            // 
            // rightPanel
            // 
            this.rightPanel.Controls.Add(this.rightTable);
            this.rightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rightPanel.Location = new System.Drawing.Point(962, 3);
            this.rightPanel.Name = "rightPanel";
            this.rightPanel.Padding = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.rightPanel.Size = new System.Drawing.Size(584, 648);
            this.rightPanel.TabIndex = 2;

            // rightTable
            this.rightTable.ColumnCount = 1;
            this.rightTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rightTable.Controls.Add(this.grpProductionStats, 0, 0);
            this.rightTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rightTable.Location = new System.Drawing.Point(5, 6);
            this.rightTable.Name = "rightTable";
            this.rightTable.RowCount = 1;
            this.rightTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rightTable.Size = new System.Drawing.Size(574, 636);
            this.rightTable.TabIndex = 0;

            // grpProductionStats
            this.grpProductionStats.Controls.Add(this.tableLayoutStats);
            this.grpProductionStats.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpProductionStats.Location = new System.Drawing.Point(3, 3);
            this.grpProductionStats.Name = "grpProductionStats";
            this.grpProductionStats.Padding = new System.Windows.Forms.Padding(10);
            this.grpProductionStats.Size = new System.Drawing.Size(568, 630);
            this.grpProductionStats.TabIndex = 0;
            this.grpProductionStats.TabStop = false;
            this.grpProductionStats.Text = "📊 产线统计";

            // tableLayoutStats
            this.tableLayoutStats.ColumnCount = 2;
            this.tableLayoutStats.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutStats.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutStats.Controls.Add(this.lblTotalProdTitle, 0, 0);
            this.tableLayoutStats.Controls.Add(this.lblTotalProdValue, 1, 0);
            this.tableLayoutStats.Controls.Add(this.lblYieldRateTitle, 0, 1);
            this.tableLayoutStats.Controls.Add(this.lblYieldRateValue, 1, 1);
            this.tableLayoutStats.Controls.Add(this.lblOnlineCountTitle, 0, 2);
            this.tableLayoutStats.Controls.Add(this.lblOnlineCountValue, 1, 2);
            this.tableLayoutStats.Controls.Add(this.lblAlarmCountTitle, 0, 3);
            this.tableLayoutStats.Controls.Add(this.lblAlarmCountValue, 1, 3);
            this.tableLayoutStats.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutStats.Location = new System.Drawing.Point(10, 26);
            this.tableLayoutStats.Name = "tableLayoutStats";
            this.tableLayoutStats.RowCount = 4;
            this.tableLayoutStats.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutStats.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutStats.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutStats.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutStats.Size = new System.Drawing.Size(548, 594);
            this.tableLayoutStats.TabIndex = 0;

            // 统计标签
            this.lblTotalProdTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTotalProdTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblTotalProdTitle.ForeColor = System.Drawing.Color.DarkSlateGray;
            this.lblTotalProdTitle.Location = new System.Drawing.Point(3, 0);
            this.lblTotalProdTitle.Name = "lblTotalProdTitle";
            this.lblTotalProdTitle.Size = new System.Drawing.Size(268, 148);
            this.lblTotalProdTitle.TabIndex = 0;
            this.lblTotalProdTitle.Text = "总产量";
            this.lblTotalProdTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            this.lblTotalProdValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTotalProdValue.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblTotalProdValue.ForeColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this.lblTotalProdValue.Location = new System.Drawing.Point(277, 0);
            this.lblTotalProdValue.Name = "lblTotalProdValue";
            this.lblTotalProdValue.Size = new System.Drawing.Size(268, 148);
            this.lblTotalProdValue.TabIndex = 1;
            this.lblTotalProdValue.Text = "0";
            this.lblTotalProdValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            this.lblYieldRateTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblYieldRateTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblYieldRateTitle.ForeColor = System.Drawing.Color.DarkSlateGray;
            this.lblYieldRateTitle.Location = new System.Drawing.Point(3, 148);
            this.lblYieldRateTitle.Name = "lblYieldRateTitle";
            this.lblYieldRateTitle.Size = new System.Drawing.Size(268, 148);
            this.lblYieldRateTitle.TabIndex = 2;
            this.lblYieldRateTitle.Text = "良品率";
            this.lblYieldRateTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            this.lblYieldRateValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblYieldRateValue.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblYieldRateValue.ForeColor = System.Drawing.Color.FromArgb(0, 176, 80);
            this.lblYieldRateValue.Location = new System.Drawing.Point(277, 148);
            this.lblYieldRateValue.Name = "lblYieldRateValue";
            this.lblYieldRateValue.Size = new System.Drawing.Size(268, 148);
            this.lblYieldRateValue.TabIndex = 3;
            this.lblYieldRateValue.Text = "0%";
            this.lblYieldRateValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            this.lblOnlineCountTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblOnlineCountTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblOnlineCountTitle.ForeColor = System.Drawing.Color.DarkSlateGray;
            this.lblOnlineCountTitle.Location = new System.Drawing.Point(3, 296);
            this.lblOnlineCountTitle.Name = "lblOnlineCountTitle";
            this.lblOnlineCountTitle.Size = new System.Drawing.Size(268, 148);
            this.lblOnlineCountTitle.TabIndex = 4;
            this.lblOnlineCountTitle.Text = "在线设备";
            this.lblOnlineCountTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            this.lblOnlineCountValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblOnlineCountValue.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblOnlineCountValue.ForeColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this.lblOnlineCountValue.Location = new System.Drawing.Point(277, 296);
            this.lblOnlineCountValue.Name = "lblOnlineCountValue";
            this.lblOnlineCountValue.Size = new System.Drawing.Size(268, 148);
            this.lblOnlineCountValue.TabIndex = 5;
            this.lblOnlineCountValue.Text = "0/0";
            this.lblOnlineCountValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            this.lblAlarmCountTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblAlarmCountTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblAlarmCountTitle.ForeColor = System.Drawing.Color.DarkSlateGray;
            this.lblAlarmCountTitle.Location = new System.Drawing.Point(3, 444);
            this.lblAlarmCountTitle.Name = "lblAlarmCountTitle";
            this.lblAlarmCountTitle.Size = new System.Drawing.Size(268, 150);
            this.lblAlarmCountTitle.TabIndex = 6;
            this.lblAlarmCountTitle.Text = "未处理报警";
            this.lblAlarmCountTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            this.lblAlarmCountValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblAlarmCountValue.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblAlarmCountValue.ForeColor = System.Drawing.Color.Red;
            this.lblAlarmCountValue.Location = new System.Drawing.Point(277, 444);
            this.lblAlarmCountValue.Name = "lblAlarmCountValue";
            this.lblAlarmCountValue.Size = new System.Drawing.Size(268, 150);
            this.lblAlarmCountValue.TabIndex = 7;
            this.lblAlarmCountValue.Text = "0";
            this.lblAlarmCountValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // 
            // tabLogControl
            // 
            this.tabLogControl.Controls.Add(this.tabPageOpLog);
            this.tabLogControl.Controls.Add(this.tabPageMonLog);
            this.tabLogControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabLogControl.Location = new System.Drawing.Point(3, 663);
            this.tabLogControl.Name = "tabLogControl";
            this.tabLogControl.SelectedIndex = 0;
            this.tabLogControl.Size = new System.Drawing.Size(1549, 223);
            this.tabLogControl.TabIndex = 1;

            // tabPageOpLog
            this.tabPageOpLog.Controls.Add(this.txtOperationLog);
            this.tabPageOpLog.Location = new System.Drawing.Point(4, 26);
            this.tabPageOpLog.Name = "tabPageOpLog";
            this.tabPageOpLog.Size = new System.Drawing.Size(1541, 193);
            this.tabPageOpLog.TabIndex = 0;
            this.tabPageOpLog.Text = "📋 操作日志";

            this.txtOperationLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtOperationLog.Location = new System.Drawing.Point(0, 0);
            this.txtOperationLog.Multiline = true;
            this.txtOperationLog.Name = "txtOperationLog";
            this.txtOperationLog.ReadOnly = true;
            this.txtOperationLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtOperationLog.Size = new System.Drawing.Size(1541, 193);
            this.txtOperationLog.TabIndex = 0;

            // tabPageMonLog
            this.tabPageMonLog.Controls.Add(this.txtMonitorLog);
            this.tabPageMonLog.Location = new System.Drawing.Point(4, 26);
            this.tabPageMonLog.Name = "tabPageMonLog";
            this.tabPageMonLog.Size = new System.Drawing.Size(1541, 193);
            this.tabPageMonLog.TabIndex = 1;
            this.tabPageMonLog.Text = "📊 监控日志";

            this.txtMonitorLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMonitorLog.Location = new System.Drawing.Point(0, 0);
            this.txtMonitorLog.Multiline = true;
            this.txtMonitorLog.Name = "txtMonitorLog";
            this.txtMonitorLog.ReadOnly = true;
            this.txtMonitorLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMonitorLog.Size = new System.Drawing.Size(1541, 193);
            this.txtMonitorLog.TabIndex = 0;

            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.lblDeviceStatus,
                this.lblServoStatus,
                this.lblLimitStatus,
                this.lblWatchdogStatus,
                this.lblModbusStatusStrip,
                this.lblCurrentCmd,
                this.lblLoggedUser,
                this.btnLogin
            });
            this.statusStrip1.Location = new System.Drawing.Point(0, 972);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1561, 23);
            this.statusStrip1.TabIndex = 3;

            this.lblDeviceStatus.Name = "lblDeviceStatus";
            this.lblDeviceStatus.Size = new System.Drawing.Size(75, 18);
            this.lblDeviceStatus.Text = "设备: 未连接";

            this.lblServoStatus.Name = "lblServoStatus";
            this.lblServoStatus.Size = new System.Drawing.Size(75, 18);
            this.lblServoStatus.Text = "伺服: 未使能";

            this.lblLimitStatus.Name = "lblLimitStatus";
            this.lblLimitStatus.Size = new System.Drawing.Size(63, 18);
            this.lblLimitStatus.Text = "限位: 正常";

            this.lblWatchdogStatus.ForeColor = System.Drawing.Color.Green;
            this.lblWatchdogStatus.Name = "lblWatchdogStatus";
            this.lblWatchdogStatus.Size = new System.Drawing.Size(95, 18);
            this.lblWatchdogStatus.Text = "🐕 看门狗: 正常";

            this.lblModbusStatusStrip.Name = "lblModbusStatusStrip";
            this.lblModbusStatusStrip.Size = new System.Drawing.Size(100, 18);
            this.lblModbusStatusStrip.Text = "Modbus: 未连接";

            this.lblCurrentCmd.Name = "lblCurrentCmd";
            this.lblCurrentCmd.Size = new System.Drawing.Size(87, 18);
            this.lblCurrentCmd.Text = "当前指令: 空闲";

            this.lblLoggedUser.Name = "lblLoggedUser";
            this.lblLoggedUser.Size = new System.Drawing.Size(104, 18);
            this.lblLoggedUser.Text = "当前用户：未登录";

            this.btnLogin.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(60, 21);
            this.btnLogin.Text = "切换用户";

            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1561, 995);
            this.Controls.Add(this.rootLayout);
            this.MinimumSize = new System.Drawing.Size(1200, 901);
            this.Name = "Form1";
            this.Text = "Gts_Test - 多设备产线控制软件 (工业版)";

            this.rootLayout.ResumeLayout(false);
            this.rootLayout.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.contentPanel.ResumeLayout(false);
            this.mainContainer.ResumeLayout(false);
            this.leftPanel.ResumeLayout(false);
            this.flowDeviceButtons.ResumeLayout(false);
            this.panelGlobalStats.ResumeLayout(false);
            this.centerPanel.ResumeLayout(false);
            this.tabMain.ResumeLayout(false);
            this.tabPageAlarm.ResumeLayout(false);
            this.rightPanel.ResumeLayout(false);
            this.rightTable.ResumeLayout(false);
            this.grpProductionStats.ResumeLayout(false);
            this.tableLayoutStats.ResumeLayout(false);
            this.tabLogControl.ResumeLayout(false);
            this.tabPageOpLog.ResumeLayout(false);
            this.tabPageOpLog.PerformLayout();
            this.tabPageMonLog.ResumeLayout(false);
            this.tabPageMonLog.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        // ================================================================
        // 控件字段声明（包含新的用户控件，但不在设计器中初始化）
        // ================================================================
        private System.Windows.Forms.TableLayoutPanel rootLayout;
        private System.Windows.Forms.TableLayoutPanel contentPanel;
        private System.Windows.Forms.TableLayoutPanel mainContainer;

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileMenu, viewMenu, toolsMenu, helpMenu;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemHotReload;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDBManager;

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnInit, btnEmergencyStop, btnResetAlarm;
        private System.Windows.Forms.ToolStripButton btnRunFlow, btnStopFlow;
        private System.Windows.Forms.ToolStripButton btnToggleSim, btnToggleModbus, btnSaveConfig;
        private System.Windows.Forms.ToolStripButton btnConnectAll, btnDisconnectAll;
        private System.Windows.Forms.ToolStripButton btnDebugToolbox;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1, toolStripSeparator2;

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblDeviceStatus, lblServoStatus, lblLimitStatus;
        private System.Windows.Forms.ToolStripStatusLabel lblWatchdogStatus;
        private System.Windows.Forms.ToolStripStatusLabel lblModbusStatusStrip, lblCurrentCmd;
        private System.Windows.Forms.ToolStripStatusLabel lblLoggedUser;
        private System.Windows.Forms.ToolStripButton btnLogin;

        private System.Windows.Forms.Panel leftPanel;
        private System.Windows.Forms.Label lblDeviceList;
        private System.Windows.Forms.ListBox listBoxDevices;
        private System.Windows.Forms.FlowLayoutPanel flowDeviceButtons;
        private System.Windows.Forms.Button btnAddDevice, btnRemoveDevice;
        private System.Windows.Forms.Button btnStartAll, btnStopAll;
        private System.Windows.Forms.Panel panelGlobalStats;
        private System.Windows.Forms.Label lblOnlineCount, lblTotalProduction;

        private System.Windows.Forms.Panel centerPanel;

        // 新功能 Tab 控件
        private System.Windows.Forms.TabControl tabMain;
        private System.Windows.Forms.TabPage tabPageOverview;
        private System.Windows.Forms.TabPage tabPageAlarm;
        private System.Windows.Forms.TabPage tabPageCamera;
        private System.Windows.Forms.TabPage tabPageWorkflow;
        private System.Windows.Forms.TabPage tabPageComm;
        private System.Windows.Forms.TabPage tabPageMqtt;

        // 报警列表（已移到中心Tab）
        private System.Windows.Forms.ListView listViewAlarms;
        private System.Windows.Forms.ColumnHeader columnHeaderId;
        private System.Windows.Forms.ColumnHeader columnHeaderDevice;
        private System.Windows.Forms.ColumnHeader columnHeaderMessage;
        private System.Windows.Forms.ColumnHeader columnHeaderSeverity;
        private System.Windows.Forms.ColumnHeader columnHeaderTime;
        private System.Windows.Forms.ColumnHeader columnHeaderStatus;

        // 用户控件（仅声明，不在设计器中实例化）
        private GtsTest.Controls.OverviewControl overviewControl;
        private GtsTest.Controls.CameraControl cameraControl;
        private GtsTest.Controls.WorkflowControl workflowControl;
        private GtsTest.Controls.CommunicationControl communicationControl;
        private GtsTest.Controls.MqttControl mqttControl;

        private System.Windows.Forms.Panel rightPanel;
        private System.Windows.Forms.TableLayoutPanel rightTable;
        private System.Windows.Forms.GroupBox grpProductionStats;
        private System.Windows.Forms.TableLayoutPanel tableLayoutStats;
        private System.Windows.Forms.Label lblTotalProdTitle, lblTotalProdValue;
        private System.Windows.Forms.Label lblYieldRateTitle, lblYieldRateValue;
        private System.Windows.Forms.Label lblOnlineCountTitle, lblOnlineCountValue;
        private System.Windows.Forms.Label lblAlarmCountTitle, lblAlarmCountValue;

        private System.Windows.Forms.TabControl tabLogControl;
        private System.Windows.Forms.TabPage tabPageOpLog, tabPageMonLog;
        private System.Windows.Forms.TextBox txtOperationLog, txtMonitorLog;
    }
}