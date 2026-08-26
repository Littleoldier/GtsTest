using System.Drawing;
using System.Windows.Forms;
using GtsTest.Modbus;

namespace GtsTest.Forms
{
    partial class DebugToolboxForm
    {
        private System.ComponentModel.IContainer components = null;

        // ==================== 顶部设备选择 ====================
        private ComboBox cmbDevice;
        private Label lblDeviceName;
        private Label lblDeviceStatus;
        private Label lblProdInfo;

        // ==================== Tab 控件 ====================
        private TabControl tabMain;
        private TabPage tabAxis;
        private TabPage tabModbus;
        private TabPage tabDevice;
        private TabPage tabVision;

        // ==================== 轴控制 ====================
        private Label lblAxisValue;
        private Label lblAxisStatus;
        private Label lblAxisPos;
        private Label lblAxisVel;
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
        private Button btnAxisAlarmReset;

        // ==================== Modbus ====================
        private NumericUpDown numRegAddress;
        private NumericUpDown numCoilAddress;
        private NumericUpDown numReadAddress;
        private NumericUpDown numReadCount;
        private ComboBox cmbDataType;
        private ComboBox cmbByteOrder;
        private ComboBox cmbCoilValue;
        private TextBox txtRegValues;
        private TextBox txtReadResult;
        private Button btnWriteRegister;
        private Button btnWriteCoil;
        private Button btnReadRegister;

        // ==================== 设备控制 ====================
        private Label lblDevTitle;
        private Button btnStartDevice;
        private Button btnStopDevice;
        private Button btnDeviceConfig;
        private Button btnConnectModbus;
        private Button btnDisconnectModbus;
        private Label lblCurrentDevice;   // 显示当前设备及Modbus状态

        // ==================== 视觉触发 ====================
        private Panel visionPanel;
        private Label lblVisionTitle;
        private Label lblVisionIp;
        private TextBox txtVisionIp;
        private Label lblVisionPort;
        private NumericUpDown numVisionPort;
        private Label lblVisionTimeout;
        private NumericUpDown numVisionTimeout;
        private Button btnTriggerVision;
        private GroupBox grpVisionConfig;
        private GroupBox grpVisionStatus;
        private Label lblVisionStatus;

        // ==================== 定时器 ====================
        private System.Windows.Forms.Timer timerRefresh;

        // ==================== 布局面板 ====================
        private Panel axisPanel;
        private Panel topPanel;
        private Panel modbusPanel;
        private Panel devicePanel;

        // ==================== 标签 ====================
        private Label lblDev;
        private Label lblName;
        private Label lblStatus;
        private Label lblProd;
        private Label lblA1;
        private Label lblA2;
        private Label lblPos;
        private Label lblVel;
        private Label lblTarget;
        private Label lblJog;
        private Label lblAccLabel;
        private Label lblDecLabel;

        // ==================== Modbus 分组 ====================
        private GroupBox grpWriteReg;
        private GroupBox grpCoil;
        private GroupBox grpRead;

        // ==================== Modbus 标签 ====================
        private Label lblRegAddr;
        private Label lblRegType;
        private Label lblRegByte;
        private Label lblRegVal;
        private Label lblCoilAddr;
        private Label lblCoilVal;
        private Label lblReadAddr;
        private Label lblReadCount;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            // ================================================
            //  窗体属性
            // ================================================
            this.ClientSize = new Size(978, 347);
            this.Text = "🔧 调试工具箱";
            this.MinimumSize = new Size(800, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormClosing += DebugToolboxForm_FormClosing;

            // ================================================
            //  定时器
            // ================================================
            timerRefresh = new System.Windows.Forms.Timer(components);
            timerRefresh.Interval = 500;

            // ================================================
            //  Tab 控件
            // ================================================
            tabMain = new TabControl();
            tabMain.Dock = DockStyle.Fill;
            tabMain.Location = new Point(0, 0);
            tabMain.Size = new Size(978, 347);

            // ---- Tab: 轴控制 ----
            BuildAxisTab();

            // ---- Tab: Modbus ----
            BuildModbusTab();

            // ---- Tab: 设备控制 ----
            BuildDeviceTab();

            // ---- Tab: 视觉触发 ----
            BuildVisionTab();

            tabMain.Controls.Add(tabAxis);
            tabMain.Controls.Add(tabModbus);
            tabMain.Controls.Add(tabDevice);
            tabMain.Controls.Add(tabVision);

            this.Controls.Add(tabMain);

            // ================================================
            //  控件初始化（填充 ComboBox）
            // ================================================
            cmbDataType.Items.Clear();
            cmbDataType.Items.AddRange(Enum.GetNames(typeof(DataType)));
            cmbDataType.SelectedIndex = 0;

            cmbByteOrder.Items.Clear();
            cmbByteOrder.Items.AddRange(Enum.GetNames(typeof(ByteOrder)));
            cmbByteOrder.SelectedIndex = 0;

            cmbCoilValue.Items.Clear();
            cmbCoilValue.Items.Add("ON (1)");
            cmbCoilValue.Items.Add("OFF (0)");
            cmbCoilValue.SelectedIndex = 0;

            this.ResumeLayout(false);
        }

        // ================================================================
        //  构建各个 Tab
        // ================================================================

        private void BuildAxisTab()
        {
            tabAxis = new TabPage();
            tabAxis.Text = "🎯 轴控制";
            tabAxis.Size = new Size(970, 317);

            axisPanel = new Panel();
            axisPanel.Dock = DockStyle.Fill;
            axisPanel.Padding = new Padding(10);

            // 顶部面板（设备信息）
            topPanel = new Panel();
            topPanel.Dock = DockStyle.Top;
            topPanel.Height = 36;
            topPanel.BackColor = Color.WhiteSmoke;

            lblDev = new Label();
            lblDev.Location = new Point(3, 10);
            lblDev.Size = new Size(64, 20);
            lblDev.Text = "当前设备:";
            lblDev.TextAlign = ContentAlignment.MiddleRight;

            cmbDevice = new ComboBox();
            cmbDevice.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDevice.Location = new Point(70, 8);
            cmbDevice.Size = new Size(150, 23);

            lblName = new Label();
            lblName.Location = new Point(230, 10);
            lblName.Size = new Size(45, 20);
            lblName.Text = "名称:";
            lblName.TextAlign = ContentAlignment.MiddleRight;

            lblDeviceName = new Label();
            lblDeviceName.Location = new Point(278, 10);
            lblDeviceName.Size = new Size(80, 20);
            lblDeviceName.Text = "--";
            lblDeviceName.TextAlign = ContentAlignment.MiddleLeft;

            lblStatus = new Label();
            lblStatus.Location = new Point(370, 10);
            lblStatus.Size = new Size(40, 20);
            lblStatus.Text = "状态:";
            lblStatus.TextAlign = ContentAlignment.MiddleRight;

            lblDeviceStatus = new Label();
            lblDeviceStatus.Location = new Point(412, 10);
            lblDeviceStatus.Size = new Size(60, 20);
            lblDeviceStatus.Text = "未连接";
            lblDeviceStatus.ForeColor = Color.Gray;
            lblDeviceStatus.TextAlign = ContentAlignment.MiddleLeft;

            lblProd = new Label();
            lblProd.Location = new Point(480, 10);
            lblProd.Size = new Size(40, 20);
            lblProd.Text = "产量:";
            lblProd.TextAlign = ContentAlignment.MiddleRight;

            lblProdInfo = new Label();
            lblProdInfo.Location = new Point(522, 10);
            lblProdInfo.Size = new Size(80, 20);
            lblProdInfo.Text = "0/0";
            lblProdInfo.TextAlign = ContentAlignment.MiddleLeft;

            topPanel.Controls.Add(lblDev);
            topPanel.Controls.Add(cmbDevice);
            topPanel.Controls.Add(lblName);
            topPanel.Controls.Add(lblDeviceName);
            topPanel.Controls.Add(lblStatus);
            topPanel.Controls.Add(lblDeviceStatus);
            topPanel.Controls.Add(lblProd);
            topPanel.Controls.Add(lblProdInfo);

            // 轴信息标签
            lblA1 = new Label();
            lblA1.Location = new Point(20, 45);
            lblA1.Size = new Size(50, 20);
            lblA1.Text = "轴号:";
            lblA1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            lblAxisValue = new Label();
            lblAxisValue.Location = new Point(72, 45);
            lblAxisValue.Size = new Size(60, 20);
            lblAxisValue.Text = "--";

            lblA2 = new Label();
            lblA2.Location = new Point(160, 45);
            lblA2.Size = new Size(60, 20);
            lblA2.Text = "轴状态:";
            lblA2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            lblAxisStatus = new Label();
            lblAxisStatus.Location = new Point(225, 45);
            lblAxisStatus.Size = new Size(80, 20);
            lblAxisStatus.Text = "--";

            lblPos = new Label();
            lblPos.Location = new Point(20, 70);
            lblPos.Size = new Size(60, 20);
            lblPos.Text = "当前位置:";

            lblAxisPos = new Label();
            lblAxisPos.Location = new Point(85, 70);
            lblAxisPos.Size = new Size(80, 20);
            lblAxisPos.Text = "--";

            lblVel = new Label();
            lblVel.Location = new Point(180, 70);
            lblVel.Size = new Size(60, 20);
            lblVel.Text = "当前速度:";

            lblAxisVel = new Label();
            lblAxisVel.Location = new Point(245, 70);
            lblAxisVel.Size = new Size(80, 20);
            lblAxisVel.Text = "--";

            lblTarget = new Label();
            lblTarget.Location = new Point(20, 100);
            lblTarget.Size = new Size(60, 20);
            lblTarget.Text = "目标位置:";

            txtTargetPos = new TextBox();
            txtTargetPos.Location = new Point(85, 97);
            txtTargetPos.Size = new Size(80, 23);
            txtTargetPos.Text = "10000";

            lblJog = new Label();
            lblJog.Location = new Point(180, 100);
            lblJog.Size = new Size(60, 20);
            lblJog.Text = "点动速度:";

            txtJogSpeed = new TextBox();
            txtJogSpeed.Location = new Point(245, 97);
            txtJogSpeed.Size = new Size(80, 23);
            txtJogSpeed.Text = "10";

            lblAccLabel = new Label();
            lblAccLabel.Location = new Point(20, 130);
            lblAccLabel.Size = new Size(60, 20);
            lblAccLabel.Text = "加速度:";

            txtAcc = new TextBox();
            txtAcc.Location = new Point(85, 127);
            txtAcc.Size = new Size(80, 23);
            txtAcc.Text = "5";

            lblDecLabel = new Label();
            lblDecLabel.Location = new Point(180, 130);
            lblDecLabel.Size = new Size(60, 20);
            lblDecLabel.Text = "减速度:";

            txtDec = new TextBox();
            txtDec.Location = new Point(245, 127);
            txtDec.Size = new Size(80, 23);
            txtDec.Text = "5";

            // 轴控制按钮
            btnHome = new Button();
            btnHome.Location = new Point(20, 170);
            btnHome.Size = new Size(70, 34);
            btnHome.Text = "回零";

            btnMoveAbs = new Button();
            btnMoveAbs.Location = new Point(100, 170);
            btnMoveAbs.Size = new Size(70, 34);
            btnMoveAbs.Text = "定位";

            btnJogP = new Button();
            btnJogP.Location = new Point(180, 170);
            btnJogP.Size = new Size(70, 34);
            btnJogP.Text = "点动+";

            btnJogN = new Button();
            btnJogN.Location = new Point(260, 170);
            btnJogN.Size = new Size(70, 34);
            btnJogN.Text = "点动-";

            btnStopAxis = new Button();
            btnStopAxis.Location = new Point(340, 170);
            btnStopAxis.Size = new Size(70, 34);
            btnStopAxis.Text = "停止轴";
            btnStopAxis.BackColor = Color.LightCoral;

            btnServoOn = new Button();
            btnServoOn.Location = new Point(420, 170);
            btnServoOn.Size = new Size(70, 34);
            btnServoOn.Text = "使能";
            btnServoOn.BackColor = Color.LightGreen;

            btnServoOff = new Button();
            btnServoOff.Location = new Point(500, 170);
            btnServoOff.Size = new Size(70, 34);
            btnServoOff.Text = "去使能";

            btnAxisAlarmReset = new Button();
            btnAxisAlarmReset.Location = new Point(580, 170);
            btnAxisAlarmReset.Size = new Size(70, 34);
            btnAxisAlarmReset.Text = "复位报警";
            btnAxisAlarmReset.BackColor = Color.Gold;

            axisPanel.Controls.Add(topPanel);
            axisPanel.Controls.Add(lblA1);
            axisPanel.Controls.Add(lblAxisValue);
            axisPanel.Controls.Add(lblA2);
            axisPanel.Controls.Add(lblAxisStatus);
            axisPanel.Controls.Add(lblPos);
            axisPanel.Controls.Add(lblAxisPos);
            axisPanel.Controls.Add(lblVel);
            axisPanel.Controls.Add(lblAxisVel);
            axisPanel.Controls.Add(lblTarget);
            axisPanel.Controls.Add(txtTargetPos);
            axisPanel.Controls.Add(lblJog);
            axisPanel.Controls.Add(txtJogSpeed);
            axisPanel.Controls.Add(lblAccLabel);
            axisPanel.Controls.Add(txtAcc);
            axisPanel.Controls.Add(lblDecLabel);
            axisPanel.Controls.Add(txtDec);
            axisPanel.Controls.Add(btnHome);
            axisPanel.Controls.Add(btnMoveAbs);
            axisPanel.Controls.Add(btnJogP);
            axisPanel.Controls.Add(btnJogN);
            axisPanel.Controls.Add(btnStopAxis);
            axisPanel.Controls.Add(btnServoOn);
            axisPanel.Controls.Add(btnServoOff);
            axisPanel.Controls.Add(btnAxisAlarmReset);

            tabAxis.Controls.Add(axisPanel);
        }

        private void BuildModbusTab()
        {
            tabModbus = new TabPage();
            tabModbus.Text = "📡 Modbus";

            modbusPanel = new Panel();
            modbusPanel.Dock = DockStyle.Fill;
            modbusPanel.Padding = new Padding(10);

            // 写寄存器
            grpWriteReg = new GroupBox();
            grpWriteReg.Text = "写寄存器";
            grpWriteReg.Location = new Point(10, 10);
            grpWriteReg.Size = new Size(900, 70);

            lblRegAddr = new Label();
            lblRegAddr.Location = new Point(20, 27);
            lblRegAddr.Size = new Size(50, 20);
            lblRegAddr.Text = "地址:";
            lblRegAddr.TextAlign = ContentAlignment.MiddleRight;

            numRegAddress = new NumericUpDown();
            numRegAddress.Location = new Point(75, 24);
            numRegAddress.Maximum = 65535;
            numRegAddress.Size = new Size(80, 23);

            lblRegType = new Label();
            lblRegType.Location = new Point(170, 27);
            lblRegType.Size = new Size(40, 20);
            lblRegType.Text = "类型:";
            lblRegType.TextAlign = ContentAlignment.MiddleRight;

            cmbDataType = new ComboBox();
            cmbDataType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDataType.Location = new Point(215, 24);
            cmbDataType.Size = new Size(90, 25);

            lblRegByte = new Label();
            lblRegByte.Location = new Point(320, 27);
            lblRegByte.Size = new Size(50, 20);
            lblRegByte.Text = "字节序:";
            lblRegByte.TextAlign = ContentAlignment.MiddleRight;

            cmbByteOrder = new ComboBox();
            cmbByteOrder.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbByteOrder.Location = new Point(375, 24);
            cmbByteOrder.Size = new Size(100, 25);

            lblRegVal = new Label();
            lblRegVal.Location = new Point(490, 27);
            lblRegVal.Size = new Size(90, 20);
            lblRegVal.Text = "值(逗号分隔):";
            lblRegVal.TextAlign = ContentAlignment.MiddleRight;

            txtRegValues = new TextBox();
            txtRegValues.Location = new Point(585, 24);
            txtRegValues.Size = new Size(120, 23);
            txtRegValues.Text = "1";

            btnWriteRegister = new Button();
            btnWriteRegister.Location = new Point(730, 19);
            btnWriteRegister.Size = new Size(120, 30);
            btnWriteRegister.Text = "写入寄存器";

            grpWriteReg.Controls.Add(lblRegAddr);
            grpWriteReg.Controls.Add(numRegAddress);
            grpWriteReg.Controls.Add(lblRegType);
            grpWriteReg.Controls.Add(cmbDataType);
            grpWriteReg.Controls.Add(lblRegByte);
            grpWriteReg.Controls.Add(cmbByteOrder);
            grpWriteReg.Controls.Add(lblRegVal);
            grpWriteReg.Controls.Add(txtRegValues);
            grpWriteReg.Controls.Add(btnWriteRegister);

            // 写线圈
            grpCoil = new GroupBox();
            grpCoil.Text = "写线圈";
            grpCoil.Location = new Point(10, 85);
            grpCoil.Size = new Size(900, 65);

            lblCoilAddr = new Label();
            lblCoilAddr.Location = new Point(20, 27);
            lblCoilAddr.Size = new Size(50, 20);
            lblCoilAddr.Text = "地址:";
            lblCoilAddr.TextAlign = ContentAlignment.MiddleRight;

            numCoilAddress = new NumericUpDown();
            numCoilAddress.Location = new Point(75, 24);
            numCoilAddress.Maximum = 65535;
            numCoilAddress.Size = new Size(80, 23);

            lblCoilVal = new Label();
            lblCoilVal.Location = new Point(170, 27);
            lblCoilVal.Size = new Size(40, 20);
            lblCoilVal.Text = "值:";
            lblCoilVal.TextAlign = ContentAlignment.MiddleRight;

            cmbCoilValue = new ComboBox();
            cmbCoilValue.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCoilValue.Location = new Point(215, 24);
            cmbCoilValue.Size = new Size(90, 25);

            btnWriteCoil = new Button();
            btnWriteCoil.Location = new Point(330, 19);
            btnWriteCoil.Size = new Size(120, 30);
            btnWriteCoil.Text = "写入线圈";

            grpCoil.Controls.Add(lblCoilAddr);
            grpCoil.Controls.Add(numCoilAddress);
            grpCoil.Controls.Add(lblCoilVal);
            grpCoil.Controls.Add(cmbCoilValue);
            grpCoil.Controls.Add(btnWriteCoil);

            // 读寄存器
            grpRead = new GroupBox();
            grpRead.Text = "读寄存器";
            grpRead.Location = new Point(10, 155);
            grpRead.Size = new Size(900, 130);

            lblReadAddr = new Label();
            lblReadAddr.Location = new Point(20, 27);
            lblReadAddr.Size = new Size(50, 20);
            lblReadAddr.Text = "地址:";
            lblReadAddr.TextAlign = ContentAlignment.MiddleRight;

            numReadAddress = new NumericUpDown();
            numReadAddress.Location = new Point(75, 24);
            numReadAddress.Maximum = 65535;
            numReadAddress.Size = new Size(80, 23);

            lblReadCount = new Label();
            lblReadCount.Location = new Point(170, 27);
            lblReadCount.Size = new Size(50, 20);
            lblReadCount.Text = "数量:";
            lblReadCount.TextAlign = ContentAlignment.MiddleRight;

            numReadCount = new NumericUpDown();
            numReadCount.Location = new Point(225, 24);
            numReadCount.Maximum = 125;
            numReadCount.Minimum = 1;
            numReadCount.Value = 10;
            numReadCount.Size = new Size(80, 23);

            btnReadRegister = new Button();
            btnReadRegister.Location = new Point(330, 19);
            btnReadRegister.Size = new Size(120, 30);
            btnReadRegister.Text = "读取";

            txtReadResult = new TextBox();
            txtReadResult.Location = new Point(20, 55);
            txtReadResult.Multiline = true;
            txtReadResult.ReadOnly = true;
            txtReadResult.ScrollBars = ScrollBars.Vertical;
            txtReadResult.Size = new Size(860, 65);

            grpRead.Controls.Add(lblReadAddr);
            grpRead.Controls.Add(numReadAddress);
            grpRead.Controls.Add(lblReadCount);
            grpRead.Controls.Add(numReadCount);
            grpRead.Controls.Add(btnReadRegister);
            grpRead.Controls.Add(txtReadResult);

            modbusPanel.Controls.Add(grpWriteReg);
            modbusPanel.Controls.Add(grpCoil);
            modbusPanel.Controls.Add(grpRead);
            tabModbus.Controls.Add(modbusPanel);
        }

        // ================================================================
        // 设备控制 Tab（重点修改）
        // ================================================================
        private void BuildDeviceTab()
        {
            tabDevice = new TabPage();
            tabDevice.Text = "⚙️ 设备控制";

            devicePanel = new Panel();
            devicePanel.Dock = DockStyle.Fill;
            devicePanel.Padding = new Padding(20);
            devicePanel.Height = 180;  // 增加高度

            // ---- 当前设备标签（显示设备名和Modbus状态） ----
            lblCurrentDevice = new Label();
            lblCurrentDevice.Location = new Point(20, 20);
            lblCurrentDevice.Size = new Size(400, 25);
            lblCurrentDevice.Text = "当前设备: 未选择";
            lblCurrentDevice.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblCurrentDevice.ForeColor = Color.DarkBlue;
            devicePanel.Controls.Add(lblCurrentDevice);

            // ---- 标题（原 lblDevTitle） ----
            lblDevTitle = new Label();
            lblDevTitle.Location = new Point(20, 55);
            lblDevTitle.Size = new Size(120, 25);
            lblDevTitle.Text = "单设备控制";
            lblDevTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            devicePanel.Controls.Add(lblDevTitle);

            // ---- 启动设备 ----
            btnStartDevice = new Button();
            btnStartDevice.Location = new Point(20, 90);
            btnStartDevice.Size = new Size(100, 40);
            btnStartDevice.Text = "▶ 启动设备";
            btnStartDevice.BackColor = Color.LightGreen;
            devicePanel.Controls.Add(btnStartDevice);

            // ---- 停止设备 ----
            btnStopDevice = new Button();
            btnStopDevice.Location = new Point(130, 90);
            btnStopDevice.Size = new Size(100, 40);
            btnStopDevice.Text = "⏹ 停止设备";
            btnStopDevice.BackColor = Color.LightCoral;
            devicePanel.Controls.Add(btnStopDevice);

            // ---- 配置 ----
            btnDeviceConfig = new Button();
            btnDeviceConfig.Location = new Point(240, 90);
            btnDeviceConfig.Size = new Size(100, 40);
            btnDeviceConfig.Text = "⚙️ 配置";
            devicePanel.Controls.Add(btnDeviceConfig);

            // ---- 连接 Modbus ----
            btnConnectModbus = new Button();
            btnConnectModbus.Location = new Point(20, 140);
            btnConnectModbus.Size = new Size(100, 40);
            btnConnectModbus.Text = "🔌 连接 Modbus";
            btnConnectModbus.BackColor = Color.LightBlue;
            btnConnectModbus.Enabled = false;
            devicePanel.Controls.Add(btnConnectModbus);

            // ---- 断开 Modbus ----
            btnDisconnectModbus = new Button();
            btnDisconnectModbus.Location = new Point(130, 140);
            btnDisconnectModbus.Size = new Size(100, 40);
            btnDisconnectModbus.Text = "🔌 断开 Modbus";
            btnDisconnectModbus.BackColor = Color.LightPink;
            btnDisconnectModbus.Enabled = false;
            devicePanel.Controls.Add(btnDisconnectModbus);

            tabDevice.Controls.Add(devicePanel);
        }

        // ================================================================
        // 视觉触发 Tab（保持不变）
        // ================================================================
        private void BuildVisionTab()
        {
            tabVision = new TabPage();
            tabVision.Text = "📷 视觉触发";
            tabVision.Size = new Size(970, 317);

            visionPanel = new Panel();
            visionPanel.Dock = DockStyle.Fill;
            visionPanel.Padding = new Padding(20);
            visionPanel.BackColor = Color.White;

            // ---- 标题 ----
            lblVisionTitle = new Label();
            lblVisionTitle.Location = new Point(20, 15);
            lblVisionTitle.Size = new Size(200, 28);
            lblVisionTitle.Text = "📷 视觉服务器触发调试";
            lblVisionTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblVisionTitle.ForeColor = Color.DarkSlateGray;

            // ---- 配置分组 ----
            grpVisionConfig = new GroupBox();
            grpVisionConfig.Text = "服务器配置";
            grpVisionConfig.Location = new Point(20, 50);
            grpVisionConfig.Size = new Size(450, 90);
            grpVisionConfig.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            TableLayoutPanel configLayout = new TableLayoutPanel();
            configLayout.Dock = DockStyle.Fill;
            configLayout.ColumnCount = 4;
            configLayout.RowCount = 2;
            configLayout.Padding = new Padding(8);
            configLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50));
            configLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            configLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60));
            configLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

            // IP
            configLayout.Controls.Add(new Label { Text = "IP:", TextAlign = ContentAlignment.MiddleRight }, 0, 0);
            txtVisionIp = new TextBox { Text = "127.0.0.1", Dock = DockStyle.Fill };
            configLayout.Controls.Add(txtVisionIp, 1, 0);

            // 端口
            configLayout.Controls.Add(new Label { Text = "端口:", TextAlign = ContentAlignment.MiddleRight }, 2, 0);
            numVisionPort = new NumericUpDown { Minimum = 1, Maximum = 65535, Value = 503, Dock = DockStyle.Fill };
            configLayout.Controls.Add(numVisionPort, 3, 0);

            // 超时
            configLayout.Controls.Add(new Label { Text = "超时:", TextAlign = ContentAlignment.MiddleRight }, 0, 1);
            numVisionTimeout = new NumericUpDown { Minimum = 1000, Maximum = 60000, Value = 10000, Increment = 1000, Dock = DockStyle.Fill };
            configLayout.Controls.Add(numVisionTimeout, 1, 1);

            // 触发按钮
            btnTriggerVision = new Button();
            btnTriggerVision.Text = "📸 触发拍照";
            btnTriggerVision.Dock = DockStyle.Fill;
            btnTriggerVision.BackColor = Color.DodgerBlue;
            btnTriggerVision.ForeColor = Color.White;
            btnTriggerVision.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnTriggerVision.FlatStyle = FlatStyle.Flat;
            configLayout.Controls.Add(btnTriggerVision, 2, 1);
            configLayout.SetColumnSpan(btnTriggerVision, 2);

            grpVisionConfig.Controls.Add(configLayout);

            // ---- 状态分组 ----
            grpVisionStatus = new GroupBox();
            grpVisionStatus.Text = "状态";
            grpVisionStatus.Location = new Point(20, 155);
            grpVisionStatus.Size = new Size(450, 70);
            grpVisionStatus.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            lblVisionStatus = new Label();
            lblVisionStatus.Location = new Point(15, 30);
            lblVisionStatus.Size = new Size(420, 25);
            lblVisionStatus.Text = "就绪，等待触发...";
            lblVisionStatus.Font = new Font("Segoe UI", 9F);
            lblVisionStatus.ForeColor = Color.DimGray;
            lblVisionStatus.TextAlign = ContentAlignment.MiddleLeft;

            grpVisionStatus.Controls.Add(lblVisionStatus);

            visionPanel.Controls.Add(lblVisionTitle);
            visionPanel.Controls.Add(grpVisionConfig);
            visionPanel.Controls.Add(grpVisionStatus);
            tabVision.Controls.Add(visionPanel);
        }
    }
}