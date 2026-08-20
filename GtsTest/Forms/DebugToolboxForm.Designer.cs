namespace GtsTest.Forms
{
    partial class DebugToolboxForm
    {
        private System.ComponentModel.IContainer components = null;

        // 顶部设备选择
        private ComboBox cmbDevice;
        private Label lblDeviceName, lblDeviceStatus, lblProdInfo;

        // Tab 控件
        private TabControl tabMain;
        private TabPage tabAxis;
        private TabPage tabModbus;
        private TabPage tabDevice;
        private TabPage tabSystem;

        // 轴控制
        private Label lblAxisValue, lblAxisStatus, lblAxisPos, lblAxisVel;
        private TextBox txtTargetPos, txtJogSpeed, txtAcc, txtDec;
        private Button btnHome, btnMoveAbs, btnJogP, btnJogN, btnStopAxis, btnServoOn, btnServoOff, btnAxisAlarmReset;

        // Modbus
        private NumericUpDown numRegAddress, numCoilAddress, numReadAddress, numReadCount;
        private ComboBox cmbDataType, cmbByteOrder, cmbCoilValue;
        private TextBox txtRegValues, txtReadResult;
        private Button btnWriteRegister, btnWriteCoil, btnReadRegister;

        // 设备控制
        private Button btnStartDevice, btnStopDevice, btnDeviceConfig;

        // 系统模式
        private Button btnToggleMode, btnHotReload, btnSaveConfig, btnDumpBlackBox;

        private System.Windows.Forms.Timer timerRefresh;   // 明确使用 Forms.Timer

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            cmbDevice = new ComboBox();
            lblDeviceName = new Label();
            lblDeviceStatus = new Label();
            lblProdInfo = new Label();
            tabMain = new TabControl();
            tabAxis = new TabPage();
            axisPanel = new Panel();
            topPanel = new Panel();
            lblDev = new Label();
            lblName = new Label();
            lblStatus = new Label();
            lblProd = new Label();
            lblA1 = new Label();
            lblAxisValue = new Label();
            lblA2 = new Label();
            lblAxisStatus = new Label();
            lblPos = new Label();
            lblAxisPos = new Label();
            lblVel = new Label();
            lblAxisVel = new Label();
            lblTarget = new Label();
            txtTargetPos = new TextBox();
            lblJog = new Label();
            txtJogSpeed = new TextBox();
            lblAcc = new Label();
            txtAcc = new TextBox();
            lblDec = new Label();
            txtDec = new TextBox();
            btnHome = new Button();
            btnMoveAbs = new Button();
            btnJogP = new Button();
            btnJogN = new Button();
            btnStopAxis = new Button();
            btnServoOn = new Button();
            btnServoOff = new Button();
            btnAxisAlarmReset = new Button();
            tabModbus = new TabPage();
            modbusPanel = new Panel();
            grpWriteReg = new GroupBox();
            lblRegAddr = new Label();
            numRegAddress = new NumericUpDown();
            lblRegType = new Label();
            cmbDataType = new ComboBox();
            lblRegByte = new Label();
            cmbByteOrder = new ComboBox();
            lblRegVal = new Label();
            txtRegValues = new TextBox();
            btnWriteRegister = new Button();
            grpCoil = new GroupBox();
            lblCoilAddr = new Label();
            numCoilAddress = new NumericUpDown();
            lblCoilVal = new Label();
            cmbCoilValue = new ComboBox();
            btnWriteCoil = new Button();
            grpRead = new GroupBox();
            lblReadAddr = new Label();
            numReadAddress = new NumericUpDown();
            lblReadCount = new Label();
            numReadCount = new NumericUpDown();
            btnReadRegister = new Button();
            txtReadResult = new TextBox();
            tabDevice = new TabPage();
            devicePanel = new Panel();
            lblDevTitle = new Label();
            btnStartDevice = new Button();
            btnStopDevice = new Button();
            btnDeviceConfig = new Button();
            tabSystem = new TabPage();
            sysPanel = new Panel();
            lblSysTitle = new Label();
            btnToggleMode = new Button();
            btnHotReload = new Button();
            btnSaveConfig = new Button();
            btnDumpBlackBox = new Button();
            timerRefresh = new System.Windows.Forms.Timer(components);
            tabMain.SuspendLayout();
            tabAxis.SuspendLayout();
            axisPanel.SuspendLayout();
            topPanel.SuspendLayout();
            tabModbus.SuspendLayout();
            modbusPanel.SuspendLayout();
            grpWriteReg.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numRegAddress).BeginInit();
            grpCoil.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numCoilAddress).BeginInit();
            grpRead.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numReadAddress).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numReadCount).BeginInit();
            tabDevice.SuspendLayout();
            devicePanel.SuspendLayout();
            tabSystem.SuspendLayout();
            sysPanel.SuspendLayout();
            SuspendLayout();
            // 
            // cmbDevice
            // 
            cmbDevice.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDevice.Location = new Point(88, 8);
            cmbDevice.Name = "cmbDevice";
            cmbDevice.Size = new Size(150, 25);
            cmbDevice.TabIndex = 1;
            // 
            // lblDeviceName
            // 
            lblDeviceName.Location = new Point(300, 12);
            lblDeviceName.Name = "lblDeviceName";
            lblDeviceName.Size = new Size(80, 20);
            lblDeviceName.TabIndex = 3;
            lblDeviceName.Text = "--";
            lblDeviceName.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblDeviceStatus
            // 
            lblDeviceStatus.ForeColor = Color.Gray;
            lblDeviceStatus.Location = new Point(435, 12);
            lblDeviceStatus.Name = "lblDeviceStatus";
            lblDeviceStatus.Size = new Size(60, 20);
            lblDeviceStatus.TabIndex = 5;
            lblDeviceStatus.Text = "未连接";
            lblDeviceStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblProdInfo
            // 
            lblProdInfo.Location = new Point(555, 12);
            lblProdInfo.Name = "lblProdInfo";
            lblProdInfo.Size = new Size(60, 20);
            lblProdInfo.TabIndex = 7;
            lblProdInfo.Text = "0/0";
            lblProdInfo.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // tabMain
            // 
            tabMain.Controls.Add(tabAxis);
            tabMain.Controls.Add(tabModbus);
            tabMain.Controls.Add(tabDevice);
            tabMain.Controls.Add(tabSystem);
            tabMain.Dock = DockStyle.Fill;
            tabMain.Location = new Point(0, 0);
            tabMain.Name = "tabMain";
            tabMain.SelectedIndex = 0;
            tabMain.Size = new Size(978, 347);
            tabMain.TabIndex = 1;
            // 
            // tabAxis
            // 
            tabAxis.Controls.Add(axisPanel);
            tabAxis.Location = new Point(4, 26);
            tabAxis.Name = "tabAxis";
            tabAxis.Size = new Size(970, 350);
            tabAxis.TabIndex = 0;
            tabAxis.Text = "🎯 轴控制";
            // 
            // axisPanel
            // 
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
            axisPanel.Controls.Add(lblAcc);
            axisPanel.Controls.Add(txtAcc);
            axisPanel.Controls.Add(lblDec);
            axisPanel.Controls.Add(txtDec);
            axisPanel.Controls.Add(btnHome);
            axisPanel.Controls.Add(btnMoveAbs);
            axisPanel.Controls.Add(btnJogP);
            axisPanel.Controls.Add(btnJogN);
            axisPanel.Controls.Add(btnStopAxis);
            axisPanel.Controls.Add(btnServoOn);
            axisPanel.Controls.Add(btnServoOff);
            axisPanel.Controls.Add(btnAxisAlarmReset);
            axisPanel.Dock = DockStyle.Fill;
            axisPanel.Location = new Point(0, 0);
            axisPanel.Name = "axisPanel";
            axisPanel.Padding = new Padding(10);
            axisPanel.Size = new Size(970, 350);
            axisPanel.TabIndex = 0;
            // 
            // topPanel
            // 
            topPanel.BackColor = Color.WhiteSmoke;
            topPanel.Controls.Add(lblDev);
            topPanel.Controls.Add(cmbDevice);
            topPanel.Controls.Add(lblName);
            topPanel.Controls.Add(lblDeviceName);
            topPanel.Controls.Add(lblStatus);
            topPanel.Controls.Add(lblDeviceStatus);
            topPanel.Controls.Add(lblProd);
            topPanel.Controls.Add(lblProdInfo);
            topPanel.Dock = DockStyle.Top;
            topPanel.Location = new Point(10, 10);
            topPanel.Name = "topPanel";
            topPanel.Size = new Size(950, 36);
            topPanel.TabIndex = 0;
            // 
            // lblDev
            // 
            lblDev.Location = new Point(3, 10);
            lblDev.Name = "lblDev";
            lblDev.Size = new Size(64, 20);
            lblDev.TabIndex = 0;
            lblDev.Text = "当前设备:";
            lblDev.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblName
            // 
            lblName.Location = new Point(250, 12);
            lblName.Name = "lblName";
            lblName.Size = new Size(45, 20);
            lblName.TabIndex = 2;
            lblName.Text = "名称:";
            lblName.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblStatus
            // 
            lblStatus.Location = new Point(390, 12);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(40, 20);
            lblStatus.TabIndex = 4;
            lblStatus.Text = "状态:";
            lblStatus.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblProd
            // 
            lblProd.Location = new Point(510, 12);
            lblProd.Name = "lblProd";
            lblProd.Size = new Size(40, 20);
            lblProd.TabIndex = 6;
            lblProd.Text = "产量:";
            lblProd.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblA1
            // 
            lblA1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblA1.Location = new Point(20, 20);
            lblA1.Name = "lblA1";
            lblA1.Size = new Size(50, 20);
            lblA1.TabIndex = 0;
            lblA1.Text = "轴号:";
            // 
            // lblAxisValue
            // 
            lblAxisValue.Location = new Point(80, 20);
            lblAxisValue.Name = "lblAxisValue";
            lblAxisValue.Size = new Size(60, 20);
            lblAxisValue.TabIndex = 1;
            lblAxisValue.Text = "--";
            // 
            // lblA2
            // 
            lblA2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblA2.Location = new Point(180, 20);
            lblA2.Name = "lblA2";
            lblA2.Size = new Size(60, 20);
            lblA2.TabIndex = 2;
            lblA2.Text = "轴状态:";
            // 
            // lblAxisStatus
            // 
            lblAxisStatus.Location = new Point(250, 20);
            lblAxisStatus.Name = "lblAxisStatus";
            lblAxisStatus.Size = new Size(80, 20);
            lblAxisStatus.TabIndex = 3;
            lblAxisStatus.Text = "--";
            // 
            // lblPos
            // 
            lblPos.Location = new Point(20, 55);
            lblPos.Name = "lblPos";
            lblPos.Size = new Size(60, 20);
            lblPos.TabIndex = 4;
            lblPos.Text = "当前位置:";
            // 
            // lblAxisPos
            // 
            lblAxisPos.Location = new Point(90, 55);
            lblAxisPos.Name = "lblAxisPos";
            lblAxisPos.Size = new Size(80, 20);
            lblAxisPos.TabIndex = 5;
            lblAxisPos.Text = "--";
            // 
            // lblVel
            // 
            lblVel.Location = new Point(180, 55);
            lblVel.Name = "lblVel";
            lblVel.Size = new Size(60, 20);
            lblVel.TabIndex = 6;
            lblVel.Text = "当前速度:";
            // 
            // lblAxisVel
            // 
            lblAxisVel.Location = new Point(250, 55);
            lblAxisVel.Name = "lblAxisVel";
            lblAxisVel.Size = new Size(80, 20);
            lblAxisVel.TabIndex = 7;
            lblAxisVel.Text = "--";
            // 
            // lblTarget
            // 
            lblTarget.Location = new Point(20, 90);
            lblTarget.Name = "lblTarget";
            lblTarget.Size = new Size(60, 20);
            lblTarget.TabIndex = 8;
            lblTarget.Text = "目标位置:";
            // 
            // txtTargetPos
            // 
            txtTargetPos.Location = new Point(90, 87);
            txtTargetPos.Name = "txtTargetPos";
            txtTargetPos.Size = new Size(80, 23);
            txtTargetPos.TabIndex = 9;
            txtTargetPos.Text = "10000";
            // 
            // lblJog
            // 
            lblJog.Location = new Point(180, 90);
            lblJog.Name = "lblJog";
            lblJog.Size = new Size(60, 20);
            lblJog.TabIndex = 10;
            lblJog.Text = "点动速度:";
            // 
            // txtJogSpeed
            // 
            txtJogSpeed.Location = new Point(250, 87);
            txtJogSpeed.Name = "txtJogSpeed";
            txtJogSpeed.Size = new Size(80, 23);
            txtJogSpeed.TabIndex = 11;
            txtJogSpeed.Text = "10";
            // 
            // lblAcc
            // 
            lblAcc.Location = new Point(20, 125);
            lblAcc.Name = "lblAcc";
            lblAcc.Size = new Size(60, 20);
            lblAcc.TabIndex = 12;
            lblAcc.Text = "加速度:";
            // 
            // txtAcc
            // 
            txtAcc.Location = new Point(90, 122);
            txtAcc.Name = "txtAcc";
            txtAcc.Size = new Size(80, 23);
            txtAcc.TabIndex = 13;
            txtAcc.Text = "5";
            // 
            // lblDec
            // 
            lblDec.Location = new Point(180, 125);
            lblDec.Name = "lblDec";
            lblDec.Size = new Size(60, 20);
            lblDec.TabIndex = 14;
            lblDec.Text = "减速度:";
            // 
            // txtDec
            // 
            txtDec.Location = new Point(250, 122);
            txtDec.Name = "txtDec";
            txtDec.Size = new Size(80, 23);
            txtDec.TabIndex = 15;
            txtDec.Text = "5";
            // 
            // btnHome
            // 
            btnHome.Location = new Point(20, 170);
            btnHome.Name = "btnHome";
            btnHome.Size = new Size(70, 34);
            btnHome.TabIndex = 16;
            btnHome.Text = "回零";
            // 
            // btnMoveAbs
            // 
            btnMoveAbs.Location = new Point(100, 170);
            btnMoveAbs.Name = "btnMoveAbs";
            btnMoveAbs.Size = new Size(70, 34);
            btnMoveAbs.TabIndex = 17;
            btnMoveAbs.Text = "定位";
            // 
            // btnJogP
            // 
            btnJogP.Location = new Point(180, 170);
            btnJogP.Name = "btnJogP";
            btnJogP.Size = new Size(70, 34);
            btnJogP.TabIndex = 18;
            btnJogP.Text = "点动+";
            // 
            // btnJogN
            // 
            btnJogN.Location = new Point(260, 170);
            btnJogN.Name = "btnJogN";
            btnJogN.Size = new Size(70, 34);
            btnJogN.TabIndex = 19;
            btnJogN.Text = "点动-";
            // 
            // btnStopAxis
            // 
            btnStopAxis.BackColor = Color.LightCoral;
            btnStopAxis.Location = new Point(340, 170);
            btnStopAxis.Name = "btnStopAxis";
            btnStopAxis.Size = new Size(70, 34);
            btnStopAxis.TabIndex = 20;
            btnStopAxis.Text = "停止轴";
            btnStopAxis.UseVisualStyleBackColor = false;
            // 
            // btnServoOn
            // 
            btnServoOn.BackColor = Color.LightGreen;
            btnServoOn.Location = new Point(420, 170);
            btnServoOn.Name = "btnServoOn";
            btnServoOn.Size = new Size(70, 34);
            btnServoOn.TabIndex = 21;
            btnServoOn.Text = "使能";
            btnServoOn.UseVisualStyleBackColor = false;
            // 
            // btnServoOff
            // 
            btnServoOff.Location = new Point(500, 170);
            btnServoOff.Name = "btnServoOff";
            btnServoOff.Size = new Size(70, 34);
            btnServoOff.TabIndex = 22;
            btnServoOff.Text = "去使能";
            // 
            // btnAxisAlarmReset
            // 
            btnAxisAlarmReset.BackColor = Color.Gold;
            btnAxisAlarmReset.Location = new Point(580, 170);
            btnAxisAlarmReset.Name = "btnAxisAlarmReset";
            btnAxisAlarmReset.Size = new Size(70, 34);
            btnAxisAlarmReset.TabIndex = 23;
            btnAxisAlarmReset.Text = "复位报警";
            btnAxisAlarmReset.UseVisualStyleBackColor = false;
            // 
            // tabModbus
            // 
            tabModbus.Controls.Add(modbusPanel);
            tabModbus.Location = new Point(4, 26);
            tabModbus.Name = "tabModbus";
            tabModbus.Size = new Size(970, 317);
            tabModbus.TabIndex = 1;
            tabModbus.Text = "📡 Modbus";
            // 
            // modbusPanel
            // 
            modbusPanel.Controls.Add(grpWriteReg);
            modbusPanel.Controls.Add(grpCoil);
            modbusPanel.Controls.Add(grpRead);
            modbusPanel.Dock = DockStyle.Fill;
            modbusPanel.Location = new Point(0, 0);
            modbusPanel.Name = "modbusPanel";
            modbusPanel.Padding = new Padding(10);
            modbusPanel.Size = new Size(970, 317);
            modbusPanel.TabIndex = 0;
            // 
            // grpWriteReg
            // 
            grpWriteReg.Controls.Add(lblRegAddr);
            grpWriteReg.Controls.Add(numRegAddress);
            grpWriteReg.Controls.Add(lblRegType);
            grpWriteReg.Controls.Add(cmbDataType);
            grpWriteReg.Controls.Add(lblRegByte);
            grpWriteReg.Controls.Add(cmbByteOrder);
            grpWriteReg.Controls.Add(lblRegVal);
            grpWriteReg.Controls.Add(txtRegValues);
            grpWriteReg.Controls.Add(btnWriteRegister);
            grpWriteReg.Location = new Point(10, 10);
            grpWriteReg.Name = "grpWriteReg";
            grpWriteReg.Size = new Size(890, 69);
            grpWriteReg.TabIndex = 0;
            grpWriteReg.TabStop = false;
            grpWriteReg.Text = "写寄存器";
            // 
            // lblRegAddr
            // 
            lblRegAddr.Location = new Point(20, 30);
            lblRegAddr.Name = "lblRegAddr";
            lblRegAddr.Size = new Size(50, 20);
            lblRegAddr.TabIndex = 0;
            lblRegAddr.Text = "地址:";
            lblRegAddr.TextAlign = ContentAlignment.MiddleRight;
            // 
            // numRegAddress
            // 
            numRegAddress.Location = new Point(75, 27);
            numRegAddress.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numRegAddress.Name = "numRegAddress";
            numRegAddress.Size = new Size(80, 23);
            numRegAddress.TabIndex = 1;
            // 
            // lblRegType
            // 
            lblRegType.Location = new Point(170, 30);
            lblRegType.Name = "lblRegType";
            lblRegType.Size = new Size(40, 20);
            lblRegType.TabIndex = 2;
            lblRegType.Text = "类型:";
            lblRegType.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cmbDataType
            // 
            cmbDataType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDataType.Location = new Point(215, 27);
            cmbDataType.Name = "cmbDataType";
            cmbDataType.Size = new Size(90, 25);
            cmbDataType.TabIndex = 3;
            // 
            // lblRegByte
            // 
            lblRegByte.Location = new Point(320, 30);
            lblRegByte.Name = "lblRegByte";
            lblRegByte.Size = new Size(50, 20);
            lblRegByte.TabIndex = 4;
            lblRegByte.Text = "字节序:";
            lblRegByte.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cmbByteOrder
            // 
            cmbByteOrder.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbByteOrder.Location = new Point(375, 27);
            cmbByteOrder.Name = "cmbByteOrder";
            cmbByteOrder.Size = new Size(100, 25);
            cmbByteOrder.TabIndex = 5;
            // 
            // lblRegVal
            // 
            lblRegVal.Location = new Point(490, 30);
            lblRegVal.Name = "lblRegVal";
            lblRegVal.Size = new Size(90, 20);
            lblRegVal.TabIndex = 6;
            lblRegVal.Text = "值(逗号分隔):";
            lblRegVal.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtRegValues
            // 
            txtRegValues.Location = new Point(585, 27);
            txtRegValues.Name = "txtRegValues";
            txtRegValues.Size = new Size(120, 23);
            txtRegValues.TabIndex = 7;
            txtRegValues.Text = "1";
            // 
            // btnWriteRegister
            // 
            btnWriteRegister.Location = new Point(730, 22);
            btnWriteRegister.Name = "btnWriteRegister";
            btnWriteRegister.Size = new Size(120, 30);
            btnWriteRegister.TabIndex = 8;
            btnWriteRegister.Text = "写入寄存器";
            // 
            // grpCoil
            // 
            grpCoil.Controls.Add(lblCoilAddr);
            grpCoil.Controls.Add(numCoilAddress);
            grpCoil.Controls.Add(lblCoilVal);
            grpCoil.Controls.Add(cmbCoilValue);
            grpCoil.Controls.Add(btnWriteCoil);
            grpCoil.Location = new Point(8, 85);
            grpCoil.Name = "grpCoil";
            grpCoil.Size = new Size(890, 70);
            grpCoil.TabIndex = 1;
            grpCoil.TabStop = false;
            grpCoil.Text = "写线圈";
            // 
            // lblCoilAddr
            // 
            lblCoilAddr.Location = new Point(20, 30);
            lblCoilAddr.Name = "lblCoilAddr";
            lblCoilAddr.Size = new Size(50, 20);
            lblCoilAddr.TabIndex = 0;
            lblCoilAddr.Text = "地址:";
            lblCoilAddr.TextAlign = ContentAlignment.MiddleRight;
            // 
            // numCoilAddress
            // 
            numCoilAddress.Location = new Point(75, 27);
            numCoilAddress.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numCoilAddress.Name = "numCoilAddress";
            numCoilAddress.Size = new Size(80, 23);
            numCoilAddress.TabIndex = 1;
            // 
            // lblCoilVal
            // 
            lblCoilVal.Location = new Point(170, 30);
            lblCoilVal.Name = "lblCoilVal";
            lblCoilVal.Size = new Size(40, 20);
            lblCoilVal.TabIndex = 2;
            lblCoilVal.Text = "值:";
            lblCoilVal.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cmbCoilValue
            // 
            cmbCoilValue.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCoilValue.Location = new Point(215, 27);
            cmbCoilValue.Name = "cmbCoilValue";
            cmbCoilValue.Size = new Size(90, 25);
            cmbCoilValue.TabIndex = 3;
            // 
            // btnWriteCoil
            // 
            btnWriteCoil.Location = new Point(330, 22);
            btnWriteCoil.Name = "btnWriteCoil";
            btnWriteCoil.Size = new Size(120, 30);
            btnWriteCoil.TabIndex = 4;
            btnWriteCoil.Text = "写入线圈";
            // 
            // grpRead
            // 
            grpRead.Controls.Add(lblReadAddr);
            grpRead.Controls.Add(numReadAddress);
            grpRead.Controls.Add(lblReadCount);
            grpRead.Controls.Add(numReadCount);
            grpRead.Controls.Add(btnReadRegister);
            grpRead.Controls.Add(txtReadResult);
            grpRead.Location = new Point(8, 161);
            grpRead.Name = "grpRead";
            grpRead.Size = new Size(890, 133);
            grpRead.TabIndex = 2;
            grpRead.TabStop = false;
            grpRead.Text = "读寄存器";
            // 
            // lblReadAddr
            // 
            lblReadAddr.Location = new Point(20, 30);
            lblReadAddr.Name = "lblReadAddr";
            lblReadAddr.Size = new Size(50, 20);
            lblReadAddr.TabIndex = 0;
            lblReadAddr.Text = "地址:";
            lblReadAddr.TextAlign = ContentAlignment.MiddleRight;
            // 
            // numReadAddress
            // 
            numReadAddress.Location = new Point(75, 27);
            numReadAddress.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numReadAddress.Name = "numReadAddress";
            numReadAddress.Size = new Size(80, 23);
            numReadAddress.TabIndex = 1;
            // 
            // lblReadCount
            // 
            lblReadCount.Location = new Point(170, 30);
            lblReadCount.Name = "lblReadCount";
            lblReadCount.Size = new Size(50, 20);
            lblReadCount.TabIndex = 2;
            lblReadCount.Text = "数量:";
            lblReadCount.TextAlign = ContentAlignment.MiddleRight;
            // 
            // numReadCount
            // 
            numReadCount.Location = new Point(225, 27);
            numReadCount.Maximum = new decimal(new int[] { 125, 0, 0, 0 });
            numReadCount.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numReadCount.Name = "numReadCount";
            numReadCount.Size = new Size(80, 23);
            numReadCount.TabIndex = 3;
            numReadCount.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // btnReadRegister
            // 
            btnReadRegister.Location = new Point(330, 22);
            btnReadRegister.Name = "btnReadRegister";
            btnReadRegister.Size = new Size(120, 30);
            btnReadRegister.TabIndex = 4;
            btnReadRegister.Text = "读取";
            // 
            // txtReadResult
            // 
            txtReadResult.Location = new Point(20, 58);
            txtReadResult.Multiline = true;
            txtReadResult.Name = "txtReadResult";
            txtReadResult.ReadOnly = true;
            txtReadResult.ScrollBars = ScrollBars.Vertical;
            txtReadResult.Size = new Size(840, 65);
            txtReadResult.TabIndex = 5;
            // 
            // tabDevice
            // 
            tabDevice.Controls.Add(devicePanel);
            tabDevice.Location = new Point(4, 26);
            tabDevice.Name = "tabDevice";
            tabDevice.Size = new Size(970, 317);
            tabDevice.TabIndex = 2;
            tabDevice.Text = "⚙️ 设备控制";
            // 
            // devicePanel
            // 
            devicePanel.Controls.Add(lblDevTitle);
            devicePanel.Controls.Add(btnStartDevice);
            devicePanel.Controls.Add(btnStopDevice);
            devicePanel.Controls.Add(btnDeviceConfig);
            devicePanel.Dock = DockStyle.Fill;
            devicePanel.Location = new Point(0, 0);
            devicePanel.Name = "devicePanel";
            devicePanel.Padding = new Padding(20);
            devicePanel.Size = new Size(970, 317);
            devicePanel.TabIndex = 0;
            // 
            // lblDevTitle
            // 
            lblDevTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblDevTitle.Location = new Point(20, 20);
            lblDevTitle.Name = "lblDevTitle";
            lblDevTitle.Size = new Size(120, 25);
            lblDevTitle.TabIndex = 0;
            lblDevTitle.Text = "单设备控制";
            // 
            // btnStartDevice
            // 
            btnStartDevice.BackColor = Color.LightGreen;
            btnStartDevice.Location = new Point(20, 60);
            btnStartDevice.Name = "btnStartDevice";
            btnStartDevice.Size = new Size(100, 40);
            btnStartDevice.TabIndex = 1;
            btnStartDevice.Text = "▶ 启动设备";
            btnStartDevice.UseVisualStyleBackColor = false;
            // 
            // btnStopDevice
            // 
            btnStopDevice.BackColor = Color.LightCoral;
            btnStopDevice.Location = new Point(130, 60);
            btnStopDevice.Name = "btnStopDevice";
            btnStopDevice.Size = new Size(100, 40);
            btnStopDevice.TabIndex = 2;
            btnStopDevice.Text = "⏹ 停止设备";
            btnStopDevice.UseVisualStyleBackColor = false;
            // 
            // btnDeviceConfig
            // 
            btnDeviceConfig.Location = new Point(240, 60);
            btnDeviceConfig.Name = "btnDeviceConfig";
            btnDeviceConfig.Size = new Size(100, 40);
            btnDeviceConfig.TabIndex = 3;
            btnDeviceConfig.Text = "⚙️ 配置";
            // 
            // tabSystem
            // 
            tabSystem.Controls.Add(sysPanel);
            tabSystem.Location = new Point(4, 26);
            tabSystem.Name = "tabSystem";
            tabSystem.Size = new Size(970, 317);
            tabSystem.TabIndex = 3;
            tabSystem.Text = "🔄 系统模式";
            // 
            // sysPanel
            // 
            sysPanel.Controls.Add(lblSysTitle);
            sysPanel.Controls.Add(btnToggleMode);
            sysPanel.Controls.Add(btnHotReload);
            sysPanel.Controls.Add(btnSaveConfig);
            sysPanel.Controls.Add(btnDumpBlackBox);
            sysPanel.Dock = DockStyle.Fill;
            sysPanel.Location = new Point(0, 0);
            sysPanel.Name = "sysPanel";
            sysPanel.Padding = new Padding(20);
            sysPanel.Size = new Size(970, 317);
            sysPanel.TabIndex = 0;
            // 
            // lblSysTitle
            // 
            lblSysTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblSysTitle.Location = new Point(20, 20);
            lblSysTitle.Name = "lblSysTitle";
            lblSysTitle.Size = new Size(150, 25);
            lblSysTitle.TabIndex = 0;
            lblSysTitle.Text = "系统模式切换";
            // 
            // btnToggleMode
            // 
            btnToggleMode.Location = new Point(20, 60);
            btnToggleMode.Name = "btnToggleMode";
            btnToggleMode.Size = new Size(150, 40);
            btnToggleMode.TabIndex = 1;
            btnToggleMode.Text = "切换模式";
            // 
            // btnHotReload
            // 
            btnHotReload.Location = new Point(20, 110);
            btnHotReload.Name = "btnHotReload";
            btnHotReload.Size = new Size(150, 40);
            btnHotReload.TabIndex = 2;
            btnHotReload.Text = "🔄 热加载配置";
            // 
            // btnSaveConfig
            // 
            btnSaveConfig.Location = new Point(20, 160);
            btnSaveConfig.Name = "btnSaveConfig";
            btnSaveConfig.Size = new Size(150, 40);
            btnSaveConfig.TabIndex = 3;
            btnSaveConfig.Text = "💾 保存配置";
            // 
            // btnDumpBlackBox
            // 
            btnDumpBlackBox.Location = new Point(20, 210);
            btnDumpBlackBox.Name = "btnDumpBlackBox";
            btnDumpBlackBox.Size = new Size(150, 40);
            btnDumpBlackBox.TabIndex = 4;
            btnDumpBlackBox.Text = "📦 导出黑匣子";
            // 
            // timerRefresh
            // 
            timerRefresh.Interval = 500;
            // 
            // DebugToolboxForm
            // 
            ClientSize = new Size(978, 347);
            Controls.Add(tabMain);
            Name = "DebugToolboxForm";
            Text = "🔧 调试工具箱";
            FormClosing += DebugToolboxForm_FormClosing;
            tabMain.ResumeLayout(false);
            tabAxis.ResumeLayout(false);
            axisPanel.ResumeLayout(false);
            axisPanel.PerformLayout();
            topPanel.ResumeLayout(false);
            tabModbus.ResumeLayout(false);
            modbusPanel.ResumeLayout(false);
            grpWriteReg.ResumeLayout(false);
            grpWriteReg.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numRegAddress).EndInit();
            grpCoil.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)numCoilAddress).EndInit();
            grpRead.ResumeLayout(false);
            grpRead.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numReadAddress).EndInit();
            ((System.ComponentModel.ISupportInitialize)numReadCount).EndInit();
            tabDevice.ResumeLayout(false);
            devicePanel.ResumeLayout(false);
            tabSystem.ResumeLayout(false);
            sysPanel.ResumeLayout(false);
            ResumeLayout(false);
        }
        private Panel axisPanel;
        private Label lblA1;
        private Label lblA2;
        private Label lblPos;
        private Label lblVel;
        private Label lblTarget;
        private Label lblJog;
        private Label lblAcc;
        private Label lblDec;
        private Panel modbusPanel;
        private GroupBox grpWriteReg;
        private Label lblRegAddr;
        private Label lblRegType;
        private Label lblRegByte;
        private Label lblRegVal;
        private GroupBox grpCoil;
        private Label lblCoilAddr;
        private Label lblCoilVal;
        private GroupBox grpRead;
        private Label lblReadAddr;
        private Label lblReadCount;
        private Panel devicePanel;
        private Label lblDevTitle;
        private Panel sysPanel;
        private Label lblSysTitle;
        private Panel topPanel;
        private Label lblDev;
        private Label lblName;
        private Label lblStatus;
        private Label lblProd;
    }
}