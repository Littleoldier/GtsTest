namespace GtsTest
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        private void InitializeComponent()
        {
            rootLayout = new TableLayoutPanel();
            menuStrip1 = new MenuStrip();
            fileMenu = new ToolStripMenuItem();
            toolStripMenuItemHotReload = new ToolStripMenuItem();
            viewMenu = new ToolStripMenuItem();
            toolsMenu = new ToolStripMenuItem();
            toolStripMenuItemDBManager = new ToolStripMenuItem();
            helpMenu = new ToolStripMenuItem();
            toolStrip1 = new ToolStrip();
            btnInit = new ToolStripButton();
            btnEmergencyStop = new ToolStripButton();
            btnResetAlarm = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            btnRunFlow = new ToolStripButton();
            btnStopFlow = new ToolStripButton();
            toolStripSeparator2 = new ToolStripSeparator();
            btnToggleSim = new ToolStripButton();
            btnToggleModbus = new ToolStripButton();
            btnConnectAll = new ToolStripButton();
            btnDisconnectAll = new ToolStripButton();
            btnSaveConfig = new ToolStripButton();
            btnLogin = new ToolStripButton();
            contentPanel = new TableLayoutPanel();
            mainContainer = new TableLayoutPanel();
            leftPanel = new Panel();
            listBoxDevices = new ListBox();
            lblDeviceList = new Label();
            flowDeviceButtons = new FlowLayoutPanel();
            btnAddDevice = new Button();
            btnRemoveDevice = new Button();
            btnStartAll = new Button();
            btnStopAll = new Button();
            panelGlobalStats = new Panel();
            lblOnlineCount = new Label();
            lblTotalProduction = new Label();
            centerPanel = new Panel();
            tabDeviceDetails = new TabControl();
            tabPageDefault = new TabPage();
            rightPanel = new Panel();
            rightTable = new TableLayoutPanel();
            grpAxisControl = new GroupBox();
            axisTable = new TableLayoutPanel();
            lblAxisTitle = new Label();
            lblAxisValue = new Label();
            lblStatusTitle = new Label();
            lblStatusValue = new Label();
            lblPosTitle = new Label();
            lblPosValue = new Label();
            lblVelTitle = new Label();
            lblVelValue = new Label();
            lblTargetTitle = new Label();
            txtTargetPos = new TextBox();
            lblJogSpeedTitle = new Label();
            txtJogSpeed = new TextBox();
            lblAccTitle = new Label();
            txtAcc = new TextBox();
            lblDecTitle = new Label();
            txtDec = new TextBox();
            btnPanel = new FlowLayoutPanel();
            btnHome = new Button();
            btnMoveAbs = new Button();
            btnJogP = new Button();
            btnJogN = new Button();
            btnStopAxis = new Button();
            btnServoOn = new Button();
            btnServoOff = new Button();
            btnAlarmReset = new Button();
            btnSoftLimit = new Button();
            btnSaveParams = new Button();
            grpModbusAdvanced = new GroupBox();
            modbusTable = new TableLayoutPanel();
            panelSlaveInfo = new Panel();
            lblSlaveInfo = new Label();
            grpWriteRegister = new GroupBox();
            regLayout = new TableLayoutPanel();
            lblRegAddr = new Label();
            numWriteAddress = new NumericUpDown();
            cmbWriteDataType = new ComboBox();
            lblRegByteOrder = new Label();
            cmbByteOrder = new ComboBox();
            lblRegValue = new Label();
            txtWriteValues = new TextBox();
            btnWriteRegister = new Button();
            lblRegDataType = new Label();
            grpWriteCoil = new GroupBox();
            coilLayout = new TableLayoutPanel();
            lblCoilAddr = new Label();
            numCoilAddress = new NumericUpDown();
            lblCoilValue = new Label();
            cmbCoilValue = new ComboBox();
            btnWriteCoil = new Button();
            grpDeviceControl = new GroupBox();
            flowDeviceCtrl = new FlowLayoutPanel();
            btnStartDevice = new Button();
            btnStopDevice = new Button();
            btnDeviceConfig = new Button();
            tabLogControl = new TabControl();
            tabPageOpLog = new TabPage();
            txtOperationLog = new TextBox();
            tabPageMonLog = new TabPage();
            txtMonitorLog = new TextBox();
            tabPageAlarm = new TabPage();
            listViewAlarms = new ListView();
            statusStrip1 = new StatusStrip();
            lblDeviceStatus = new ToolStripStatusLabel();
            lblServoStatus = new ToolStripStatusLabel();
            lblLimitStatus = new ToolStripStatusLabel();
            lblWatchdogStatus = new ToolStripStatusLabel();
            lblModbusStatusStrip = new ToolStripStatusLabel();
            lblCurrentCmd = new ToolStripStatusLabel();
            lblLoggedUser = new ToolStripStatusLabel();
            btnSteModbus = new Button();
            btnExportMonitor = new Button();
            lblModbusStatus = new Label();
            btnStartMonitor = new Button();
            btnStopMonitor = new Button();
            cmbWorkflow = new ComboBox();
            rootLayout.SuspendLayout();
            menuStrip1.SuspendLayout();
            toolStrip1.SuspendLayout();
            contentPanel.SuspendLayout();
            mainContainer.SuspendLayout();
            leftPanel.SuspendLayout();
            flowDeviceButtons.SuspendLayout();
            panelGlobalStats.SuspendLayout();
            centerPanel.SuspendLayout();
            tabDeviceDetails.SuspendLayout();
            rightPanel.SuspendLayout();
            rightTable.SuspendLayout();
            grpAxisControl.SuspendLayout();
            axisTable.SuspendLayout();
            btnPanel.SuspendLayout();
            grpModbusAdvanced.SuspendLayout();
            modbusTable.SuspendLayout();
            panelSlaveInfo.SuspendLayout();
            grpWriteRegister.SuspendLayout();
            regLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numWriteAddress).BeginInit();
            grpWriteCoil.SuspendLayout();
            coilLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numCoilAddress).BeginInit();
            grpDeviceControl.SuspendLayout();
            flowDeviceCtrl.SuspendLayout();
            tabLogControl.SuspendLayout();
            tabPageOpLog.SuspendLayout();
            tabPageMonLog.SuspendLayout();
            tabPageAlarm.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // rootLayout
            // 
            rootLayout.ColumnCount = 1;
            rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200F));
            rootLayout.Controls.Add(menuStrip1, 0, 0);
            rootLayout.Controls.Add(toolStrip1, 0, 1);
            rootLayout.Controls.Add(contentPanel, 0, 2);
            rootLayout.Controls.Add(statusStrip1, 0, 3);
            rootLayout.Dock = DockStyle.Fill;
            rootLayout.Location = new Point(0, 0);
            rootLayout.Name = "rootLayout";
            rootLayout.RowCount = 4;
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            rootLayout.Size = new Size(1561, 995);
            rootLayout.TabIndex = 0;
            // 
            // menuStrip1
            // 
            menuStrip1.Dock = DockStyle.Fill;
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileMenu, viewMenu, toolsMenu, helpMenu });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1561, 32);
            menuStrip1.TabIndex = 0;
            // 
            // fileMenu
            // 
            fileMenu.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItemHotReload });
            fileMenu.Name = "fileMenu";
            fileMenu.Size = new Size(44, 28);
            fileMenu.Text = "文件";
            // 
            // toolStripMenuItemHotReload
            // 
            toolStripMenuItemHotReload.Name = "toolStripMenuItemHotReload";
            toolStripMenuItemHotReload.ShortcutKeyDisplayString = "Ctrl+F5";
            toolStripMenuItemHotReload.ShortcutKeys = Keys.Control | Keys.F5;
            toolStripMenuItemHotReload.Size = new Size(260, 22);
            toolStripMenuItemHotReload.Text = "🔄 热加载配置 (Ctrl+F5)";
            // 
            // viewMenu
            // 
            viewMenu.Name = "viewMenu";
            viewMenu.Size = new Size(44, 28);
            viewMenu.Text = "视图";
            // 
            // toolsMenu
            // 
            toolsMenu.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItemDBManager });
            toolsMenu.Name = "toolsMenu";
            toolsMenu.Size = new Size(44, 28);
            toolsMenu.Text = "工具";
            // 
            // toolStripMenuItemDBManager
            // 
            toolStripMenuItemDBManager.Name = "toolStripMenuItemDBManager";
            toolStripMenuItemDBManager.Size = new Size(156, 22);
            toolStripMenuItemDBManager.Text = "📊 数据库管理";
            // 
            // helpMenu
            // 
            helpMenu.Name = "helpMenu";
            helpMenu.Size = new Size(44, 28);
            helpMenu.Text = "帮助";
            // 
            // toolStrip1
            // 
            toolStrip1.Dock = DockStyle.Fill;
            toolStrip1.Items.AddRange(new ToolStripItem[] { btnInit, btnEmergencyStop, btnResetAlarm, toolStripSeparator1, btnRunFlow, btnStopFlow, toolStripSeparator2, btnToggleSim, btnToggleModbus, btnConnectAll, btnDisconnectAll, btnSaveConfig, btnLogin });
            toolStrip1.Location = new Point(0, 32);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(1561, 40);
            toolStrip1.TabIndex = 1;
            // 
            // btnInit
            // 
            btnInit.Name = "btnInit";
            btnInit.Size = new Size(48, 37);
            btnInit.Text = "初始化";
            // 
            // btnEmergencyStop
            // 
            btnEmergencyStop.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnEmergencyStop.ForeColor = Color.Red;
            btnEmergencyStop.Name = "btnEmergencyStop";
            btnEmergencyStop.Size = new Size(39, 37);
            btnEmergencyStop.Text = "急停";
            // 
            // btnResetAlarm
            // 
            btnResetAlarm.ForeColor = Color.DarkOrange;
            btnResetAlarm.Name = "btnResetAlarm";
            btnResetAlarm.Size = new Size(80, 37);
            btnResetAlarm.Text = "🔕 复位报警";
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 40);
            // 
            // btnRunFlow
            // 
            btnRunFlow.Name = "btnRunFlow";
            btnRunFlow.Size = new Size(60, 37);
            btnRunFlow.Text = "运行流程";
            // 
            // btnStopFlow
            // 
            btnStopFlow.Name = "btnStopFlow";
            btnStopFlow.Size = new Size(60, 37);
            btnStopFlow.Text = "停止流程";
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 40);
            // 
            // btnToggleSim
            // 
            btnToggleSim.Name = "btnToggleSim";
            btnToggleSim.Size = new Size(60, 37);
            btnToggleSim.Text = "切换模式";
            // 
            // btnToggleModbus
            // 
            btnToggleModbus.Name = "btnToggleModbus";
            btnToggleModbus.Size = new Size(89, 37);
            btnToggleModbus.Text = "连接 Modbus";
            // 
            // btnConnectAll
            // 
            btnConnectAll.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnConnectAll.Name = "btnConnectAll";
            btnConnectAll.Size = new Size(60, 37);
            btnConnectAll.Text = "全部连接";
            // 
            // btnDisconnectAll
            // 
            btnDisconnectAll.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnDisconnectAll.Name = "btnDisconnectAll";
            btnDisconnectAll.Size = new Size(60, 37);
            btnDisconnectAll.Text = "全部断开";
            // 
            // btnSaveConfig
            // 
            btnSaveConfig.Name = "btnSaveConfig";
            btnSaveConfig.Size = new Size(60, 37);
            btnSaveConfig.Text = "保存配置";
            // 
            // btnLogin
            // 
            btnLogin.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(60, 37);
            btnLogin.Text = "切换用户";
            // 
            // contentPanel
            // 
            contentPanel.ColumnCount = 1;
            contentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 194F));
            contentPanel.Controls.Add(mainContainer, 0, 0);
            contentPanel.Controls.Add(tabLogControl, 0, 1);
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.Location = new Point(3, 75);
            contentPanel.Name = "contentPanel";
            contentPanel.RowCount = 2;
            contentPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 74.3532F));
            contentPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 25.6467934F));
            contentPanel.Size = new Size(1555, 889);
            contentPanel.TabIndex = 2;
            // 
            // mainContainer
            // 
            mainContainer.ColumnCount = 3;
            mainContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240F));
            mainContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            mainContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            mainContainer.Controls.Add(leftPanel, 0, 0);
            mainContainer.Controls.Add(centerPanel, 1, 0);
            mainContainer.Controls.Add(rightPanel, 2, 0);
            mainContainer.Dock = DockStyle.Fill;
            mainContainer.Location = new Point(3, 3);
            mainContainer.Name = "mainContainer";
            mainContainer.RowCount = 1;
            mainContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 23F));
            mainContainer.Size = new Size(1549, 655);
            mainContainer.TabIndex = 0;
            // 
            // leftPanel
            // 
            leftPanel.Controls.Add(listBoxDevices);
            leftPanel.Controls.Add(lblDeviceList);
            leftPanel.Controls.Add(flowDeviceButtons);
            leftPanel.Controls.Add(panelGlobalStats);
            leftPanel.Dock = DockStyle.Fill;
            leftPanel.Location = new Point(3, 3);
            leftPanel.Name = "leftPanel";
            leftPanel.Padding = new Padding(5, 6, 5, 6);
            leftPanel.Size = new Size(234, 649);
            leftPanel.TabIndex = 0;
            // 
            // listBoxDevices
            // 
            listBoxDevices.Dock = DockStyle.Fill;
            listBoxDevices.DrawMode = DrawMode.OwnerDrawVariable;
            listBoxDevices.Location = new Point(5, 34);
            listBoxDevices.Name = "listBoxDevices";
            listBoxDevices.Size = new Size(224, 498);
            listBoxDevices.TabIndex = 0;
            listBoxDevices.DrawItem += ListBoxDevices_DrawItem;
            // 
            // lblDeviceList
            // 
            lblDeviceList.Dock = DockStyle.Top;
            lblDeviceList.Location = new Point(5, 6);
            lblDeviceList.Name = "lblDeviceList";
            lblDeviceList.Size = new Size(224, 28);
            lblDeviceList.TabIndex = 1;
            lblDeviceList.Text = "🖥️ 设备列表";
            // 
            // flowDeviceButtons
            // 
            flowDeviceButtons.Controls.Add(btnAddDevice);
            flowDeviceButtons.Controls.Add(btnRemoveDevice);
            flowDeviceButtons.Controls.Add(btnStartAll);
            flowDeviceButtons.Controls.Add(btnStopAll);
            flowDeviceButtons.Dock = DockStyle.Bottom;
            flowDeviceButtons.Location = new Point(5, 532);
            flowDeviceButtons.Name = "flowDeviceButtons";
            flowDeviceButtons.Size = new Size(224, 66);
            flowDeviceButtons.TabIndex = 2;
            // 
            // btnAddDevice
            // 
            btnAddDevice.Location = new Point(3, 3);
            btnAddDevice.Name = "btnAddDevice";
            btnAddDevice.Size = new Size(100, 26);
            btnAddDevice.TabIndex = 0;
            btnAddDevice.Text = "➕ 添加设备";
            // 
            // btnRemoveDevice
            // 
            btnRemoveDevice.Location = new Point(109, 3);
            btnRemoveDevice.Name = "btnRemoveDevice";
            btnRemoveDevice.Size = new Size(100, 26);
            btnRemoveDevice.TabIndex = 1;
            btnRemoveDevice.Text = "➖ 移除设备";
            // 
            // btnStartAll
            // 
            btnStartAll.BackColor = Color.LightGreen;
            btnStartAll.Location = new Point(3, 35);
            btnStartAll.Name = "btnStartAll";
            btnStartAll.Size = new Size(100, 26);
            btnStartAll.TabIndex = 2;
            btnStartAll.Text = "▶ 全部启动";
            btnStartAll.UseVisualStyleBackColor = false;
            // 
            // btnStopAll
            // 
            btnStopAll.BackColor = Color.LightCoral;
            btnStopAll.Location = new Point(109, 35);
            btnStopAll.Name = "btnStopAll";
            btnStopAll.Size = new Size(100, 26);
            btnStopAll.TabIndex = 3;
            btnStopAll.Text = "⏹ 全部停止";
            btnStopAll.UseVisualStyleBackColor = false;
            // 
            // panelGlobalStats
            // 
            panelGlobalStats.Controls.Add(lblOnlineCount);
            panelGlobalStats.Controls.Add(lblTotalProduction);
            panelGlobalStats.Dock = DockStyle.Bottom;
            panelGlobalStats.Location = new Point(5, 598);
            panelGlobalStats.Name = "panelGlobalStats";
            panelGlobalStats.Size = new Size(224, 45);
            panelGlobalStats.TabIndex = 3;
            // 
            // lblOnlineCount
            // 
            lblOnlineCount.Dock = DockStyle.Left;
            lblOnlineCount.Location = new Point(0, 0);
            lblOnlineCount.Name = "lblOnlineCount";
            lblOnlineCount.Size = new Size(100, 45);
            lblOnlineCount.TabIndex = 0;
            lblOnlineCount.Text = "在线: 0/0";
            // 
            // lblTotalProduction
            // 
            lblTotalProduction.Dock = DockStyle.Right;
            lblTotalProduction.Location = new Point(124, 0);
            lblTotalProduction.Name = "lblTotalProduction";
            lblTotalProduction.Size = new Size(100, 45);
            lblTotalProduction.TabIndex = 1;
            lblTotalProduction.Text = "总产量: 0";
            // 
            // centerPanel
            // 
            centerPanel.Controls.Add(tabDeviceDetails);
            centerPanel.Dock = DockStyle.Fill;
            centerPanel.Location = new Point(243, 3);
            centerPanel.Name = "centerPanel";
            centerPanel.Size = new Size(713, 649);
            centerPanel.TabIndex = 1;
            // 
            // tabDeviceDetails
            // 
            tabDeviceDetails.Controls.Add(tabPageDefault);
            tabDeviceDetails.Dock = DockStyle.Fill;
            tabDeviceDetails.Location = new Point(0, 0);
            tabDeviceDetails.Name = "tabDeviceDetails";
            tabDeviceDetails.SelectedIndex = 0;
            tabDeviceDetails.Size = new Size(713, 649);
            tabDeviceDetails.TabIndex = 0;
            // 
            // tabPageDefault
            // 
            tabPageDefault.Location = new Point(4, 26);
            tabPageDefault.Name = "tabPageDefault";
            tabPageDefault.Size = new Size(705, 619);
            tabPageDefault.TabIndex = 0;
            tabPageDefault.Text = "请添加设备";
            // 
            // rightPanel
            // 
            rightPanel.Controls.Add(rightTable);
            rightPanel.Dock = DockStyle.Fill;
            rightPanel.Location = new Point(962, 3);
            rightPanel.Name = "rightPanel";
            rightPanel.Padding = new Padding(5, 6, 5, 6);
            rightPanel.Size = new Size(584, 649);
            rightPanel.TabIndex = 2;
            // 
            // rightTable
            // 
            rightTable.ColumnCount = 1;
            rightTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            rightTable.Controls.Add(grpAxisControl, 0, 0);
            rightTable.Controls.Add(grpModbusAdvanced, 0, 1);
            rightTable.Controls.Add(grpDeviceControl, 0, 2);
            rightTable.Dock = DockStyle.Fill;
            rightTable.Location = new Point(5, 6);
            rightTable.Name = "rightTable";
            rightTable.RowCount = 3;
            rightTable.RowStyles.Add(new RowStyle(SizeType.Percent, 38.6185226F));
            rightTable.RowStyles.Add(new RowStyle(SizeType.Percent, 49.92151F));
            rightTable.RowStyles.Add(new RowStyle(SizeType.Percent, 11.6169548F));
            rightTable.Size = new Size(574, 637);
            rightTable.TabIndex = 0;
            // 
            // grpAxisControl
            // 
            grpAxisControl.Controls.Add(axisTable);
            grpAxisControl.Dock = DockStyle.Fill;
            grpAxisControl.Location = new Point(3, 3);
            grpAxisControl.Name = "grpAxisControl";
            grpAxisControl.Padding = new Padding(5, 6, 5, 6);
            grpAxisControl.Size = new Size(568, 239);
            grpAxisControl.TabIndex = 0;
            grpAxisControl.TabStop = false;
            grpAxisControl.Text = "轴控制 (当前设备)";
            // 
            // axisTable
            // 
            axisTable.ColumnCount = 4;
            axisTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F));
            axisTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            axisTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 91F));
            axisTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            axisTable.Controls.Add(lblAxisTitle, 0, 0);
            axisTable.Controls.Add(lblAxisValue, 1, 0);
            axisTable.Controls.Add(lblStatusTitle, 2, 0);
            axisTable.Controls.Add(lblStatusValue, 3, 0);
            axisTable.Controls.Add(lblPosTitle, 0, 1);
            axisTable.Controls.Add(lblPosValue, 1, 1);
            axisTable.Controls.Add(lblVelTitle, 2, 1);
            axisTable.Controls.Add(lblVelValue, 3, 1);
            axisTable.Controls.Add(lblTargetTitle, 0, 2);
            axisTable.Controls.Add(txtTargetPos, 1, 2);
            axisTable.Controls.Add(lblJogSpeedTitle, 2, 2);
            axisTable.Controls.Add(txtJogSpeed, 3, 2);
            axisTable.Controls.Add(lblAccTitle, 0, 3);
            axisTable.Controls.Add(txtAcc, 1, 3);
            axisTable.Controls.Add(lblDecTitle, 2, 3);
            axisTable.Controls.Add(txtDec, 3, 3);
            axisTable.Controls.Add(btnPanel, 0, 4);
            axisTable.Dock = DockStyle.Fill;
            axisTable.Location = new Point(5, 22);
            axisTable.Name = "axisTable";
            axisTable.Padding = new Padding(3);
            axisTable.RowCount = 5;
            axisTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            axisTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 29F));
            axisTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 33F));
            axisTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 31F));
            axisTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            axisTable.Size = new Size(558, 211);
            axisTable.TabIndex = 0;
            // 
            // lblAxisTitle
            // 
            lblAxisTitle.Dock = DockStyle.Fill;
            lblAxisTitle.Font = new Font("Microsoft YaHei UI", 12F);
            lblAxisTitle.Location = new Point(6, 3);
            lblAxisTitle.Name = "lblAxisTitle";
            lblAxisTitle.Size = new Size(84, 28);
            lblAxisTitle.TabIndex = 0;
            lblAxisTitle.Text = "当 前 轴 :";
            // 
            // lblAxisValue
            // 
            lblAxisValue.Dock = DockStyle.Fill;
            lblAxisValue.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblAxisValue.Location = new Point(96, 3);
            lblAxisValue.Name = "lblAxisValue";
            lblAxisValue.Size = new Size(142, 28);
            lblAxisValue.TabIndex = 1;
            lblAxisValue.Text = "1";
            lblAxisValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblStatusTitle
            // 
            lblStatusTitle.Dock = DockStyle.Fill;
            lblStatusTitle.Font = new Font("Microsoft YaHei UI", 12F);
            lblStatusTitle.Location = new Point(244, 3);
            lblStatusTitle.Name = "lblStatusTitle";
            lblStatusTitle.Size = new Size(85, 28);
            lblStatusTitle.TabIndex = 2;
            lblStatusTitle.Text = "轴 状 态 :";
            // 
            // lblStatusValue
            // 
            lblStatusValue.Dock = DockStyle.Fill;
            lblStatusValue.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblStatusValue.ForeColor = Color.Green;
            lblStatusValue.Location = new Point(335, 3);
            lblStatusValue.Name = "lblStatusValue";
            lblStatusValue.Size = new Size(217, 28);
            lblStatusValue.TabIndex = 3;
            lblStatusValue.Text = "● 就绪";
            lblStatusValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblPosTitle
            // 
            lblPosTitle.Dock = DockStyle.Fill;
            lblPosTitle.Font = new Font("Microsoft YaHei UI", 12F);
            lblPosTitle.Location = new Point(6, 31);
            lblPosTitle.Name = "lblPosTitle";
            lblPosTitle.Size = new Size(84, 29);
            lblPosTitle.TabIndex = 4;
            lblPosTitle.Text = "当前位置:";
            // 
            // lblPosValue
            // 
            lblPosValue.Dock = DockStyle.Fill;
            lblPosValue.Location = new Point(96, 31);
            lblPosValue.Name = "lblPosValue";
            lblPosValue.Size = new Size(142, 29);
            lblPosValue.TabIndex = 5;
            lblPosValue.Text = "0.00";
            lblPosValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblVelTitle
            // 
            lblVelTitle.Dock = DockStyle.Fill;
            lblVelTitle.Font = new Font("Microsoft YaHei UI", 12F);
            lblVelTitle.Location = new Point(244, 31);
            lblVelTitle.Name = "lblVelTitle";
            lblVelTitle.Size = new Size(85, 29);
            lblVelTitle.TabIndex = 6;
            lblVelTitle.Text = "当前速度:";
            // 
            // lblVelValue
            // 
            lblVelValue.Dock = DockStyle.Fill;
            lblVelValue.Location = new Point(335, 31);
            lblVelValue.Name = "lblVelValue";
            lblVelValue.Size = new Size(217, 29);
            lblVelValue.TabIndex = 7;
            lblVelValue.Text = "0.00";
            lblVelValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblTargetTitle
            // 
            lblTargetTitle.Dock = DockStyle.Fill;
            lblTargetTitle.Font = new Font("Microsoft YaHei UI", 12F);
            lblTargetTitle.Location = new Point(6, 60);
            lblTargetTitle.Name = "lblTargetTitle";
            lblTargetTitle.Size = new Size(84, 33);
            lblTargetTitle.TabIndex = 8;
            lblTargetTitle.Text = "目标位置:";
            // 
            // txtTargetPos
            // 
            txtTargetPos.Dock = DockStyle.Fill;
            txtTargetPos.Location = new Point(96, 63);
            txtTargetPos.Name = "txtTargetPos";
            txtTargetPos.Size = new Size(142, 23);
            txtTargetPos.TabIndex = 9;
            txtTargetPos.Text = "10000";
            // 
            // lblJogSpeedTitle
            // 
            lblJogSpeedTitle.Dock = DockStyle.Fill;
            lblJogSpeedTitle.Font = new Font("Microsoft YaHei UI", 12F);
            lblJogSpeedTitle.Location = new Point(244, 60);
            lblJogSpeedTitle.Name = "lblJogSpeedTitle";
            lblJogSpeedTitle.Size = new Size(85, 33);
            lblJogSpeedTitle.TabIndex = 10;
            lblJogSpeedTitle.Text = "点动速度:";
            // 
            // txtJogSpeed
            // 
            txtJogSpeed.Dock = DockStyle.Fill;
            txtJogSpeed.Location = new Point(335, 63);
            txtJogSpeed.Name = "txtJogSpeed";
            txtJogSpeed.Size = new Size(217, 23);
            txtJogSpeed.TabIndex = 11;
            txtJogSpeed.Text = "10";
            // 
            // lblAccTitle
            // 
            lblAccTitle.Dock = DockStyle.Fill;
            lblAccTitle.Font = new Font("Microsoft YaHei UI", 12F);
            lblAccTitle.Location = new Point(6, 93);
            lblAccTitle.Name = "lblAccTitle";
            lblAccTitle.Size = new Size(84, 31);
            lblAccTitle.TabIndex = 12;
            lblAccTitle.Text = "加 速 度 :";
            // 
            // txtAcc
            // 
            txtAcc.Dock = DockStyle.Fill;
            txtAcc.Location = new Point(96, 96);
            txtAcc.Name = "txtAcc";
            txtAcc.Size = new Size(142, 23);
            txtAcc.TabIndex = 13;
            txtAcc.Text = "5";
            // 
            // lblDecTitle
            // 
            lblDecTitle.Dock = DockStyle.Fill;
            lblDecTitle.Font = new Font("Microsoft YaHei UI", 12F);
            lblDecTitle.Location = new Point(244, 93);
            lblDecTitle.Name = "lblDecTitle";
            lblDecTitle.Size = new Size(85, 31);
            lblDecTitle.TabIndex = 14;
            lblDecTitle.Text = "减 速 度 :";
            // 
            // txtDec
            // 
            txtDec.Dock = DockStyle.Fill;
            txtDec.Location = new Point(335, 96);
            txtDec.Name = "txtDec";
            txtDec.Size = new Size(217, 23);
            txtDec.TabIndex = 15;
            txtDec.Text = "5";
            // 
            // btnPanel
            // 
            axisTable.SetColumnSpan(btnPanel, 4);
            btnPanel.Controls.Add(btnHome);
            btnPanel.Controls.Add(btnMoveAbs);
            btnPanel.Controls.Add(btnJogP);
            btnPanel.Controls.Add(btnJogN);
            btnPanel.Controls.Add(btnStopAxis);
            btnPanel.Controls.Add(btnServoOn);
            btnPanel.Controls.Add(btnServoOff);
            btnPanel.Controls.Add(btnAlarmReset);
            btnPanel.Controls.Add(btnSoftLimit);
            btnPanel.Controls.Add(btnSaveParams);
            btnPanel.Dock = DockStyle.Fill;
            btnPanel.Location = new Point(6, 127);
            btnPanel.Name = "btnPanel";
            btnPanel.Size = new Size(546, 78);
            btnPanel.TabIndex = 16;
            // 
            // btnHome
            // 
            btnHome.Location = new Point(2, 2);
            btnHome.Margin = new Padding(2);
            btnHome.Name = "btnHome";
            btnHome.Size = new Size(70, 34);
            btnHome.TabIndex = 0;
            btnHome.Text = "回零";
            // 
            // btnMoveAbs
            // 
            btnMoveAbs.Location = new Point(76, 2);
            btnMoveAbs.Margin = new Padding(2);
            btnMoveAbs.Name = "btnMoveAbs";
            btnMoveAbs.Size = new Size(70, 34);
            btnMoveAbs.TabIndex = 1;
            btnMoveAbs.Text = "定位";
            // 
            // btnJogP
            // 
            btnJogP.Location = new Point(150, 2);
            btnJogP.Margin = new Padding(2);
            btnJogP.Name = "btnJogP";
            btnJogP.Size = new Size(70, 34);
            btnJogP.TabIndex = 2;
            btnJogP.Text = "点动+";
            // 
            // btnJogN
            // 
            btnJogN.Location = new Point(224, 2);
            btnJogN.Margin = new Padding(2);
            btnJogN.Name = "btnJogN";
            btnJogN.Size = new Size(70, 34);
            btnJogN.TabIndex = 3;
            btnJogN.Text = "点动-";
            // 
            // btnStopAxis
            // 
            btnStopAxis.BackColor = Color.LightCoral;
            btnStopAxis.Location = new Point(298, 2);
            btnStopAxis.Margin = new Padding(2);
            btnStopAxis.Name = "btnStopAxis";
            btnStopAxis.Size = new Size(70, 34);
            btnStopAxis.TabIndex = 4;
            btnStopAxis.Text = "停止轴";
            btnStopAxis.UseVisualStyleBackColor = false;
            // 
            // btnServoOn
            // 
            btnServoOn.BackColor = Color.LightGreen;
            btnServoOn.Location = new Point(372, 2);
            btnServoOn.Margin = new Padding(2);
            btnServoOn.Name = "btnServoOn";
            btnServoOn.Size = new Size(70, 34);
            btnServoOn.TabIndex = 5;
            btnServoOn.Text = "使能";
            btnServoOn.UseVisualStyleBackColor = false;
            // 
            // btnServoOff
            // 
            btnServoOff.BackColor = Color.LightGray;
            btnServoOff.Location = new Point(446, 2);
            btnServoOff.Margin = new Padding(2);
            btnServoOff.Name = "btnServoOff";
            btnServoOff.Size = new Size(70, 34);
            btnServoOff.TabIndex = 6;
            btnServoOff.Text = "去使能";
            btnServoOff.UseVisualStyleBackColor = false;
            // 
            // btnAlarmReset
            // 
            btnAlarmReset.BackColor = Color.Gold;
            btnAlarmReset.Location = new Point(2, 40);
            btnAlarmReset.Margin = new Padding(2);
            btnAlarmReset.Name = "btnAlarmReset";
            btnAlarmReset.Size = new Size(70, 34);
            btnAlarmReset.TabIndex = 7;
            btnAlarmReset.Text = "复位报警";
            btnAlarmReset.UseVisualStyleBackColor = false;
            // 
            // btnSoftLimit
            // 
            btnSoftLimit.Location = new Point(76, 40);
            btnSoftLimit.Margin = new Padding(2);
            btnSoftLimit.Name = "btnSoftLimit";
            btnSoftLimit.Size = new Size(70, 34);
            btnSoftLimit.TabIndex = 8;
            btnSoftLimit.Text = "软限位";
            // 
            // btnSaveParams
            // 
            btnSaveParams.Location = new Point(150, 40);
            btnSaveParams.Margin = new Padding(2);
            btnSaveParams.Name = "btnSaveParams";
            btnSaveParams.Size = new Size(70, 34);
            btnSaveParams.TabIndex = 9;
            btnSaveParams.Text = "保存参数";
            // 
            // grpModbusAdvanced
            // 
            grpModbusAdvanced.Controls.Add(modbusTable);
            grpModbusAdvanced.Dock = DockStyle.Fill;
            grpModbusAdvanced.Location = new Point(3, 248);
            grpModbusAdvanced.Name = "grpModbusAdvanced";
            grpModbusAdvanced.Size = new Size(568, 311);
            grpModbusAdvanced.TabIndex = 1;
            grpModbusAdvanced.TabStop = false;
            grpModbusAdvanced.Text = "Modbus 读写 (当前设备)";
            // 
            // modbusTable
            // 
            modbusTable.ColumnCount = 1;
            modbusTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            modbusTable.Controls.Add(panelSlaveInfo, 0, 0);
            modbusTable.Controls.Add(grpWriteRegister, 0, 1);
            modbusTable.Controls.Add(grpWriteCoil, 0, 2);
            modbusTable.Dock = DockStyle.Fill;
            modbusTable.Location = new Point(3, 19);
            modbusTable.Name = "modbusTable";
            modbusTable.RowCount = 3;
            modbusTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            modbusTable.RowStyles.Add(new RowStyle(SizeType.Percent, 56.69643F));
            modbusTable.RowStyles.Add(new RowStyle(SizeType.Percent, 43.30357F));
            modbusTable.Size = new Size(562, 289);
            modbusTable.TabIndex = 0;
            // 
            // panelSlaveInfo
            // 
            panelSlaveInfo.BackColor = Color.FromArgb(240, 240, 240);
            panelSlaveInfo.Controls.Add(lblSlaveInfo);
            panelSlaveInfo.Dock = DockStyle.Fill;
            panelSlaveInfo.Location = new Point(3, 3);
            panelSlaveInfo.Name = "panelSlaveInfo";
            panelSlaveInfo.Size = new Size(556, 28);
            panelSlaveInfo.TabIndex = 0;
            // 
            // lblSlaveInfo
            // 
            lblSlaveInfo.Dock = DockStyle.Fill;
            lblSlaveInfo.Location = new Point(0, 0);
            lblSlaveInfo.Name = "lblSlaveInfo";
            lblSlaveInfo.Size = new Size(556, 28);
            lblSlaveInfo.TabIndex = 0;
            lblSlaveInfo.Text = "当前设备: 未选择";
            lblSlaveInfo.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // grpWriteRegister
            // 
            grpWriteRegister.Controls.Add(regLayout);
            grpWriteRegister.Dock = DockStyle.Fill;
            grpWriteRegister.Location = new Point(3, 37);
            grpWriteRegister.Name = "grpWriteRegister";
            grpWriteRegister.Size = new Size(556, 138);
            grpWriteRegister.TabIndex = 1;
            grpWriteRegister.TabStop = false;
            grpWriteRegister.Text = "写保持寄存器";
            // 
            // regLayout
            // 
            regLayout.ColumnCount = 4;
            regLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 94F));
            regLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 187F));
            regLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 59F));
            regLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 27F));
            regLayout.Controls.Add(lblRegAddr, 0, 0);
            regLayout.Controls.Add(numWriteAddress, 1, 0);
            regLayout.Controls.Add(cmbWriteDataType, 3, 0);
            regLayout.Controls.Add(lblRegByteOrder, 0, 1);
            regLayout.Controls.Add(cmbByteOrder, 1, 1);
            regLayout.Controls.Add(lblRegValue, 2, 1);
            regLayout.Controls.Add(txtWriteValues, 3, 1);
            regLayout.Controls.Add(btnWriteRegister, 3, 2);
            regLayout.Controls.Add(lblRegDataType, 2, 0);
            regLayout.Dock = DockStyle.Fill;
            regLayout.Location = new Point(3, 19);
            regLayout.Name = "regLayout";
            regLayout.RowCount = 3;
            regLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 39.6551743F));
            regLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 28.4482765F));
            regLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 31.034483F));
            regLayout.Size = new Size(550, 116);
            regLayout.TabIndex = 0;
            // 
            // lblRegAddr
            // 
            lblRegAddr.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            lblRegAddr.Location = new Point(3, 3);
            lblRegAddr.Margin = new Padding(3, 3, 3, 0);
            lblRegAddr.Name = "lblRegAddr";
            lblRegAddr.Size = new Size(74, 24);
            lblRegAddr.TabIndex = 0;
            lblRegAddr.Text = "地址:";
            // 
            // numWriteAddress
            // 
            numWriteAddress.Location = new Point(97, 3);
            numWriteAddress.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numWriteAddress.Name = "numWriteAddress";
            numWriteAddress.Size = new Size(162, 23);
            numWriteAddress.TabIndex = 1;
            // 
            // cmbWriteDataType
            // 
            cmbWriteDataType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbWriteDataType.Items.AddRange(new object[] { "Int16", "UInt16", "Int32", "UInt32", "Float", "Double" });
            cmbWriteDataType.Location = new Point(343, 3);
            cmbWriteDataType.Name = "cmbWriteDataType";
            cmbWriteDataType.Size = new Size(121, 25);
            cmbWriteDataType.TabIndex = 3;
            // 
            // lblRegByteOrder
            // 
            lblRegByteOrder.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            lblRegByteOrder.Location = new Point(3, 49);
            lblRegByteOrder.Margin = new Padding(3, 3, 3, 0);
            lblRegByteOrder.Name = "lblRegByteOrder";
            lblRegByteOrder.Size = new Size(74, 22);
            lblRegByteOrder.TabIndex = 4;
            lblRegByteOrder.Text = "字节序:";
            // 
            // cmbByteOrder
            // 
            cmbByteOrder.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbByteOrder.Items.AddRange(new object[] { "Big Endian", "Little Endian" });
            cmbByteOrder.Location = new Point(97, 49);
            cmbByteOrder.Name = "cmbByteOrder";
            cmbByteOrder.Size = new Size(162, 25);
            cmbByteOrder.TabIndex = 5;
            // 
            // lblRegValue
            // 
            lblRegValue.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            lblRegValue.Location = new Point(284, 49);
            lblRegValue.Margin = new Padding(3, 3, 3, 0);
            lblRegValue.Name = "lblRegValue";
            lblRegValue.Size = new Size(53, 22);
            lblRegValue.TabIndex = 6;
            lblRegValue.Text = "值:";
            // 
            // txtWriteValues
            // 
            txtWriteValues.Location = new Point(343, 49);
            txtWriteValues.Name = "txtWriteValues";
            txtWriteValues.Size = new Size(121, 23);
            txtWriteValues.TabIndex = 7;
            txtWriteValues.Text = "1";
            // 
            // btnWriteRegister
            // 
            btnWriteRegister.Location = new Point(343, 82);
            btnWriteRegister.Name = "btnWriteRegister";
            btnWriteRegister.Size = new Size(121, 31);
            btnWriteRegister.TabIndex = 8;
            btnWriteRegister.Text = "写入";
            // 
            // lblRegDataType
            // 
            lblRegDataType.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            lblRegDataType.Location = new Point(284, 3);
            lblRegDataType.Margin = new Padding(3, 3, 3, 0);
            lblRegDataType.Name = "lblRegDataType";
            lblRegDataType.Size = new Size(53, 24);
            lblRegDataType.TabIndex = 2;
            lblRegDataType.Text = "类型:";
            // 
            // grpWriteCoil
            // 
            grpWriteCoil.Controls.Add(coilLayout);
            grpWriteCoil.Dock = DockStyle.Fill;
            grpWriteCoil.Location = new Point(3, 181);
            grpWriteCoil.Name = "grpWriteCoil";
            grpWriteCoil.Size = new Size(556, 105);
            grpWriteCoil.TabIndex = 2;
            grpWriteCoil.TabStop = false;
            grpWriteCoil.Text = "写线圈";
            // 
            // coilLayout
            // 
            coilLayout.ColumnCount = 4;
            coilLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95F));
            coilLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 193F));
            coilLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 52F));
            coilLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 37F));
            coilLayout.Controls.Add(lblCoilAddr, 0, 0);
            coilLayout.Controls.Add(numCoilAddress, 1, 0);
            coilLayout.Controls.Add(lblCoilValue, 2, 0);
            coilLayout.Controls.Add(cmbCoilValue, 3, 0);
            coilLayout.Controls.Add(btnWriteCoil, 3, 1);
            coilLayout.Dock = DockStyle.Fill;
            coilLayout.Location = new Point(3, 19);
            coilLayout.Name = "coilLayout";
            coilLayout.RowCount = 2;
            coilLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            coilLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 8F));
            coilLayout.Size = new Size(550, 83);
            coilLayout.TabIndex = 0;
            // 
            // lblCoilAddr
            // 
            lblCoilAddr.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            lblCoilAddr.Location = new Point(3, 3);
            lblCoilAddr.Margin = new Padding(3, 3, 3, 0);
            lblCoilAddr.Name = "lblCoilAddr";
            lblCoilAddr.Size = new Size(67, 23);
            lblCoilAddr.TabIndex = 0;
            lblCoilAddr.Text = "地址:";
            // 
            // numCoilAddress
            // 
            numCoilAddress.Location = new Point(98, 3);
            numCoilAddress.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numCoilAddress.Name = "numCoilAddress";
            numCoilAddress.Size = new Size(161, 23);
            numCoilAddress.TabIndex = 1;
            // 
            // lblCoilValue
            // 
            lblCoilValue.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            lblCoilValue.Location = new Point(291, 3);
            lblCoilValue.Margin = new Padding(3, 3, 3, 0);
            lblCoilValue.Name = "lblCoilValue";
            lblCoilValue.Size = new Size(46, 23);
            lblCoilValue.TabIndex = 2;
            lblCoilValue.Text = "值:";
            // 
            // cmbCoilValue
            // 
            cmbCoilValue.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCoilValue.Items.AddRange(new object[] { "ON (1)", "OFF (0)" });
            cmbCoilValue.Location = new Point(343, 3);
            cmbCoilValue.Name = "cmbCoilValue";
            cmbCoilValue.Size = new Size(122, 25);
            cmbCoilValue.TabIndex = 3;
            // 
            // btnWriteCoil
            // 
            btnWriteCoil.Location = new Point(343, 47);
            btnWriteCoil.Name = "btnWriteCoil";
            btnWriteCoil.Size = new Size(122, 31);
            btnWriteCoil.TabIndex = 4;
            btnWriteCoil.Text = "写入";
            // 
            // grpDeviceControl
            // 
            grpDeviceControl.Controls.Add(flowDeviceCtrl);
            grpDeviceControl.Dock = DockStyle.Fill;
            grpDeviceControl.Location = new Point(3, 565);
            grpDeviceControl.Name = "grpDeviceControl";
            grpDeviceControl.Size = new Size(568, 69);
            grpDeviceControl.TabIndex = 2;
            grpDeviceControl.TabStop = false;
            grpDeviceControl.Text = "设备控制";
            // 
            // flowDeviceCtrl
            // 
            flowDeviceCtrl.Controls.Add(btnStartDevice);
            flowDeviceCtrl.Controls.Add(btnStopDevice);
            flowDeviceCtrl.Controls.Add(btnDeviceConfig);
            flowDeviceCtrl.Dock = DockStyle.Fill;
            flowDeviceCtrl.Location = new Point(3, 19);
            flowDeviceCtrl.Name = "flowDeviceCtrl";
            flowDeviceCtrl.Size = new Size(562, 47);
            flowDeviceCtrl.TabIndex = 0;
            // 
            // btnStartDevice
            // 
            btnStartDevice.BackColor = Color.LightGreen;
            btnStartDevice.Location = new Point(3, 3);
            btnStartDevice.Name = "btnStartDevice";
            btnStartDevice.Size = new Size(80, 26);
            btnStartDevice.TabIndex = 0;
            btnStartDevice.Text = "▶ 启动";
            btnStartDevice.UseVisualStyleBackColor = false;
            // 
            // btnStopDevice
            // 
            btnStopDevice.BackColor = Color.LightCoral;
            btnStopDevice.Location = new Point(89, 3);
            btnStopDevice.Name = "btnStopDevice";
            btnStopDevice.Size = new Size(80, 26);
            btnStopDevice.TabIndex = 1;
            btnStopDevice.Text = "⏹ 停止";
            btnStopDevice.UseVisualStyleBackColor = false;
            // 
            // btnDeviceConfig
            // 
            btnDeviceConfig.Location = new Point(175, 3);
            btnDeviceConfig.Name = "btnDeviceConfig";
            btnDeviceConfig.Size = new Size(80, 26);
            btnDeviceConfig.TabIndex = 2;
            btnDeviceConfig.Text = "⚙️ 配置";
            // 
            // tabLogControl
            // 
            tabLogControl.Controls.Add(tabPageOpLog);
            tabLogControl.Controls.Add(tabPageMonLog);
            tabLogControl.Controls.Add(tabPageAlarm);
            tabLogControl.Dock = DockStyle.Fill;
            tabLogControl.Location = new Point(3, 664);
            tabLogControl.Name = "tabLogControl";
            tabLogControl.SelectedIndex = 0;
            tabLogControl.Size = new Size(1549, 222);
            tabLogControl.TabIndex = 1;
            // 
            // tabPageOpLog
            // 
            tabPageOpLog.Controls.Add(txtOperationLog);
            tabPageOpLog.Location = new Point(4, 26);
            tabPageOpLog.Name = "tabPageOpLog";
            tabPageOpLog.Size = new Size(1541, 192);
            tabPageOpLog.TabIndex = 0;
            tabPageOpLog.Text = "📋 操作日志";
            // 
            // txtOperationLog
            // 
            txtOperationLog.Dock = DockStyle.Fill;
            txtOperationLog.Location = new Point(0, 0);
            txtOperationLog.Multiline = true;
            txtOperationLog.Name = "txtOperationLog";
            txtOperationLog.ReadOnly = true;
            txtOperationLog.ScrollBars = ScrollBars.Vertical;
            txtOperationLog.Size = new Size(1541, 192);
            txtOperationLog.TabIndex = 0;
            // 
            // tabPageMonLog
            // 
            tabPageMonLog.Controls.Add(txtMonitorLog);
            tabPageMonLog.Location = new Point(4, 26);
            tabPageMonLog.Name = "tabPageMonLog";
            tabPageMonLog.Size = new Size(1541, 192);
            tabPageMonLog.TabIndex = 1;
            tabPageMonLog.Text = "📊 监控日志";
            // 
            // txtMonitorLog
            // 
            txtMonitorLog.Dock = DockStyle.Fill;
            txtMonitorLog.Location = new Point(0, 0);
            txtMonitorLog.Multiline = true;
            txtMonitorLog.Name = "txtMonitorLog";
            txtMonitorLog.ReadOnly = true;
            txtMonitorLog.ScrollBars = ScrollBars.Vertical;
            txtMonitorLog.Size = new Size(1541, 192);
            txtMonitorLog.TabIndex = 0;
            // 
            // tabPageAlarm
            // 
            tabPageAlarm.Controls.Add(listViewAlarms);
            tabPageAlarm.Location = new Point(4, 26);
            tabPageAlarm.Name = "tabPageAlarm";
            tabPageAlarm.Size = new Size(1541, 192);
            tabPageAlarm.TabIndex = 2;
            tabPageAlarm.Text = "🚨 实时报警";
            // 
            // listViewAlarms
            // 
            listViewAlarms.Dock = DockStyle.Fill;
            listViewAlarms.FullRowSelect = true;
            listViewAlarms.GridLines = true;
            listViewAlarms.Location = new Point(0, 0);
            listViewAlarms.Name = "listViewAlarms";
            listViewAlarms.Size = new Size(1541, 192);
            listViewAlarms.TabIndex = 0;
            listViewAlarms.UseCompatibleStateImageBehavior = false;
            listViewAlarms.View = View.Details;
            // 
            // statusStrip1
            // 
            statusStrip1.Dock = DockStyle.Fill;
            statusStrip1.Items.AddRange(new ToolStripItem[] { lblDeviceStatus, lblServoStatus, lblLimitStatus, lblWatchdogStatus, lblModbusStatusStrip, lblCurrentCmd, lblLoggedUser });
            statusStrip1.Location = new Point(0, 967);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(1561, 28);
            statusStrip1.TabIndex = 3;
            // 
            // lblDeviceStatus
            // 
            lblDeviceStatus.Name = "lblDeviceStatus";
            lblDeviceStatus.Size = new Size(75, 23);
            lblDeviceStatus.Text = "设备: 未连接";
            // 
            // lblServoStatus
            // 
            lblServoStatus.Name = "lblServoStatus";
            lblServoStatus.Size = new Size(75, 23);
            lblServoStatus.Text = "伺服: 未使能";
            // 
            // lblLimitStatus
            // 
            lblLimitStatus.Name = "lblLimitStatus";
            lblLimitStatus.Size = new Size(63, 23);
            lblLimitStatus.Text = "限位: 正常";
            // 
            // lblWatchdogStatus
            // 
            lblWatchdogStatus.ForeColor = Color.Green;
            lblWatchdogStatus.Name = "lblWatchdogStatus";
            lblWatchdogStatus.Size = new Size(95, 23);
            lblWatchdogStatus.Text = "🐕 看门狗: 正常";
            // 
            // lblModbusStatusStrip
            // 
            lblModbusStatusStrip.Name = "lblModbusStatusStrip";
            lblModbusStatusStrip.Size = new Size(100, 23);
            lblModbusStatusStrip.Text = "Modbus: 未连接";
            // 
            // lblCurrentCmd
            // 
            lblCurrentCmd.Name = "lblCurrentCmd";
            lblCurrentCmd.Size = new Size(87, 23);
            lblCurrentCmd.Text = "当前指令: 空闲";
            // 
            // lblLoggedUser
            // 
            lblLoggedUser.Name = "lblLoggedUser";
            lblLoggedUser.Size = new Size(104, 23);
            lblLoggedUser.Text = "当前用户：未登录";
            // 
            // btnSteModbus
            // 
            btnSteModbus.Location = new Point(0, 0);
            btnSteModbus.Name = "btnSteModbus";
            btnSteModbus.Size = new Size(75, 23);
            btnSteModbus.TabIndex = 0;
            btnSteModbus.Visible = false;
            // 
            // btnExportMonitor
            // 
            btnExportMonitor.Location = new Point(0, 0);
            btnExportMonitor.Name = "btnExportMonitor";
            btnExportMonitor.Size = new Size(75, 23);
            btnExportMonitor.TabIndex = 0;
            btnExportMonitor.Visible = false;
            // 
            // lblModbusStatus
            // 
            lblModbusStatus.Location = new Point(0, 0);
            lblModbusStatus.Name = "lblModbusStatus";
            lblModbusStatus.Size = new Size(100, 23);
            lblModbusStatus.TabIndex = 0;
            lblModbusStatus.Visible = false;
            // 
            // btnStartMonitor
            // 
            btnStartMonitor.Location = new Point(0, 0);
            btnStartMonitor.Name = "btnStartMonitor";
            btnStartMonitor.Size = new Size(75, 23);
            btnStartMonitor.TabIndex = 0;
            btnStartMonitor.Visible = false;
            // 
            // btnStopMonitor
            // 
            btnStopMonitor.Location = new Point(0, 0);
            btnStopMonitor.Name = "btnStopMonitor";
            btnStopMonitor.Size = new Size(75, 23);
            btnStopMonitor.TabIndex = 0;
            btnStopMonitor.Visible = false;
            // 
            // cmbWorkflow
            // 
            cmbWorkflow.Location = new Point(0, 0);
            cmbWorkflow.Name = "cmbWorkflow";
            cmbWorkflow.Size = new Size(121, 25);
            cmbWorkflow.TabIndex = 0;
            cmbWorkflow.Visible = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1561, 995);
            Controls.Add(rootLayout);
            MinimumSize = new Size(1200, 901);
            Name = "Form1";
            Text = "Gts_Test - 多设备产线控制软件 (工业版)";
            rootLayout.ResumeLayout(false);
            rootLayout.PerformLayout();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            contentPanel.ResumeLayout(false);
            mainContainer.ResumeLayout(false);
            leftPanel.ResumeLayout(false);
            flowDeviceButtons.ResumeLayout(false);
            panelGlobalStats.ResumeLayout(false);
            centerPanel.ResumeLayout(false);
            tabDeviceDetails.ResumeLayout(false);
            rightPanel.ResumeLayout(false);
            rightTable.ResumeLayout(false);
            grpAxisControl.ResumeLayout(false);
            axisTable.ResumeLayout(false);
            axisTable.PerformLayout();
            btnPanel.ResumeLayout(false);
            grpModbusAdvanced.ResumeLayout(false);
            modbusTable.ResumeLayout(false);
            panelSlaveInfo.ResumeLayout(false);
            grpWriteRegister.ResumeLayout(false);
            regLayout.ResumeLayout(false);
            regLayout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numWriteAddress).EndInit();
            grpWriteCoil.ResumeLayout(false);
            coilLayout.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)numCoilAddress).EndInit();
            grpDeviceControl.ResumeLayout(false);
            flowDeviceCtrl.ResumeLayout(false);
            tabLogControl.ResumeLayout(false);
            tabPageOpLog.ResumeLayout(false);
            tabPageOpLog.PerformLayout();
            tabPageMonLog.ResumeLayout(false);
            tabPageMonLog.PerformLayout();
            tabPageAlarm.ResumeLayout(false);
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        // ================================================================
        // 控件字段声明
        // ================================================================
        private TableLayoutPanel rootLayout;
        private TableLayoutPanel contentPanel;
        private TableLayoutPanel mainContainer;

        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileMenu, viewMenu, toolsMenu, helpMenu;
        private ToolStripMenuItem toolStripMenuItemHotReload;
        private ToolStripMenuItem toolStripMenuItemDBManager;

        private ToolStrip toolStrip1;
        private ToolStripButton btnInit, btnEmergencyStop, btnResetAlarm;
        private ToolStripButton btnRunFlow, btnStopFlow;
        private ToolStripButton btnToggleSim, btnToggleModbus, btnSaveConfig;
        private ToolStripSeparator toolStripSeparator1, toolStripSeparator2;

        private StatusStrip statusStrip1;
        private ToolStripStatusLabel lblDeviceStatus, lblServoStatus, lblLimitStatus;
        private ToolStripStatusLabel lblWatchdogStatus;
        private ToolStripStatusLabel lblModbusStatusStrip, lblCurrentCmd;

        private Panel leftPanel;
        private Label lblDeviceList;
        private ListBox listBoxDevices;
        private FlowLayoutPanel flowDeviceButtons;
        private Button btnAddDevice, btnRemoveDevice;
        private Button btnStartAll, btnStopAll;
        private Panel panelGlobalStats;
        private Label lblOnlineCount, lblTotalProduction;

        private Panel centerPanel;
        private TabControl tabDeviceDetails;
        private TabPage tabPageDefault;

        private Panel rightPanel;
        private TableLayoutPanel rightTable;

        private ToolStripStatusLabel lblLoggedUser;
        private ToolStripButton btnLogin;

        // 轴控制 (新)
        private GroupBox grpAxisControl;
        private Label lblAxisValue;
        private Label lblStatusValue;
        private Label lblPosValue;
        private Label lblVelValue;
        private TextBox txtTargetPos;
        private TextBox txtJogSpeed;
        private TextBox txtAcc;
        private TextBox txtDec;
        private Button btnHome;
        private Button btnMoveAbs;
        private Button btnJogP;
        private Button btnJogN;
        private Button btnStopAxis;
        private Button btnServoOn;
        private Button btnServoOff;
        private Button btnAlarmReset;
        private Button btnSoftLimit;
        private Button btnSaveParams;

        // Modbus 读写
        private GroupBox grpModbusAdvanced;
        private TableLayoutPanel modbusTable;
        private Panel panelSlaveInfo;
        private Label lblSlaveInfo;
        private GroupBox grpWriteRegister, grpWriteCoil;
        private TableLayoutPanel regLayout, coilLayout;
        private Label lblRegAddr, lblRegDataType, lblRegByteOrder, lblRegValue;
        private NumericUpDown numWriteAddress;
        private ComboBox cmbWriteDataType, cmbByteOrder;
        private TextBox txtWriteValues;
        private Button btnWriteRegister;
        private Label lblCoilAddr, lblCoilValue;
        private NumericUpDown numCoilAddress;
        private ComboBox cmbCoilValue;
        private Button btnWriteCoil;

        private GroupBox grpDeviceControl;
        private FlowLayoutPanel flowDeviceCtrl;
        private Button btnStartDevice, btnStopDevice, btnDeviceConfig;
        private ToolStripButton btnConnectAll;
        private ToolStripButton btnDisconnectAll;

        private TabControl tabLogControl;
        private TabPage tabPageOpLog, tabPageMonLog, tabPageAlarm;
        private TextBox txtOperationLog, txtMonitorLog;
        private ListView listViewAlarms;

        // 隐藏旧控件
        private Button btnSteModbus, btnExportMonitor;
        private Label lblModbusStatus;
        private Button btnStartMonitor, btnStopMonitor;
        private ComboBox cmbWorkflow;

        private TableLayoutPanel axisTable;
        private Label lblAxisTitle;
        private Label lblStatusTitle;
        private Label lblPosTitle;
        private Label lblVelTitle;
        private Label lblTargetTitle;
        private Label lblJogSpeedTitle;
        private Label lblAccTitle;
        private Label lblDecTitle;
        private FlowLayoutPanel btnPanel;
    }
}