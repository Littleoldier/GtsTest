namespace GtsTest.Modbus
{
    partial class ModbusConfigForm
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

        private void InitializeComponent()
        {
            this.lblProtocol = new Label();
            this.cmbProtocol = new ComboBox();
            this.grpTcp = new GroupBox();
            this.lblIp = new Label();
            this.txtIp = new TextBox();
            this.lblPort = new Label();
            this.numPort = new NumericUpDown();
            this.grpRtu = new GroupBox();
            this.lblComPort = new Label();
            this.cmbComPort = new ComboBox();
            this.btnRefreshCom = new Button();
            this.lblBaudRate = new Label();
            this.cmbBaudRate = new ComboBox();
            this.lblDataBits = new Label();
            this.cmbDataBits = new ComboBox();
            this.lblStopBits = new Label();
            this.cmbStopBits = new ComboBox();
            this.lblParity = new Label();
            this.cmbParity = new ComboBox();
            this.lblDataType = new Label();
            this.cmbDataType = new ComboBox();
            this.lblDisplayFormat = new Label();
            this.cmbDisplayFormat = new ComboBox();
            this.btnOK = new Button();
            this.btnCancel = new Button();
            this.lblStartAddress = new Label();
            this.lblRegisterCount = new Label();
            this.numStartAddress = new NumericUpDown();
            this.numRegisterCount = new NumericUpDown();
            this.lblByteOrder = new Label();
            this.cmbByteOrder = new ComboBox();

            this.grpTcp.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).BeginInit();
            this.grpRtu.SuspendLayout();
            this.SuspendLayout();

            // 起始地址
            this.lblStartAddress.AutoSize = true;
            this.lblStartAddress.Location = new Point(12, 420);
            this.lblStartAddress.Text = "起始地址:";
            
            this.numStartAddress.Location = new Point(100, 417);
            this.numStartAddress.Maximum = 65535;
            this.numStartAddress.Value = 0;

            // 寄存器数量
            this.lblRegisterCount.AutoSize = true;
            this.lblRegisterCount.Location = new Point(12, 450);
            this.lblRegisterCount.Text = "寄存器数量:";
            
            this.numRegisterCount.Location = new Point(100, 447);
            this.numRegisterCount.Minimum = 1;
            this.numRegisterCount.Maximum = 125;   // Modbus 最大连续读取 125 个寄存器
            this.numRegisterCount.Value = 1;

            // 添加到窗体（注意调整顺序和位置）
            this.Controls.Add(this.lblStartAddress);
            this.Controls.Add(this.numStartAddress);
            this.Controls.Add(this.lblRegisterCount);
            this.Controls.Add(this.numRegisterCount);
            this.Controls.Add(this.lblByteOrder);
            this.Controls.Add(this.cmbByteOrder);

            // 字节序
            this.lblByteOrder.AutoSize = true;
            this.lblByteOrder.Location = new Point(12, 390); // 调整 Y 坐标
            this.lblByteOrder.Text = "字节序:";

            this.cmbByteOrder.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbByteOrder.Items.AddRange(new object[] { "Big Endian (高字节在前)", "Little Endian (低字节在前)" });
            this.cmbByteOrder.Location = new Point(100, 387);
            this.cmbByteOrder.Size = new Size(150, 25);
            this.cmbByteOrder.SelectedIndex = 0;


            // lblProtocol
            this.lblProtocol.AutoSize = true;
            this.lblProtocol.Location = new Point(12, 20);
            this.lblProtocol.Name = "lblProtocol";
            this.lblProtocol.Size = new Size(44, 17);
            this.lblProtocol.TabIndex = 0;
            this.lblProtocol.Text = "协议:";

            // cmbProtocol
            this.cmbProtocol.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbProtocol.Items.AddRange(new object[] { "TCP", "RTU" });
            this.cmbProtocol.Location = new Point(62, 17);
            this.cmbProtocol.Name = "cmbProtocol";
            this.cmbProtocol.Size = new Size(120, 25);
            this.cmbProtocol.TabIndex = 1;
            this.cmbProtocol.SelectedIndexChanged += new EventHandler(this.CmbProtocol_SelectedIndexChanged);

            // grpTcp
            this.grpTcp.Controls.Add(this.lblIp);
            this.grpTcp.Controls.Add(this.txtIp);
            this.grpTcp.Controls.Add(this.lblPort);
            this.grpTcp.Controls.Add(this.numPort);
            this.grpTcp.Location = new Point(12, 50);
            this.grpTcp.Name = "grpTcp";
            this.grpTcp.Size = new Size(300, 80);
            this.grpTcp.TabIndex = 2;
            this.grpTcp.TabStop = false;
            this.grpTcp.Text = "TCP 参数";
            this.grpTcp.Visible = true;         // 始终可见
            this.grpTcp.Enabled = true;         // 默认启用（TCP）

            // lblIp
            this.lblIp.AutoSize = true;
            this.lblIp.Location = new Point(10, 28);
            this.lblIp.Name = "lblIp";
            this.lblIp.Size = new Size(58, 17);
            this.lblIp.TabIndex = 0;
            this.lblIp.Text = "IP 地址:";

            // txtIp
            this.txtIp.Location = new Point(80, 25);
            this.txtIp.Name = "txtIp";
            this.txtIp.Size = new Size(150, 23);
            this.txtIp.TabIndex = 1;
            this.txtIp.Text = "192.168.5.24";

            // lblPort
            this.lblPort.AutoSize = true;
            this.lblPort.Location = new Point(10, 56);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new Size(44, 17);
            this.lblPort.TabIndex = 2;
            this.lblPort.Text = "端口:";

            // numPort
            this.numPort.Location = new Point(80, 53);
            this.numPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            this.numPort.Name = "numPort";
            this.numPort.Size = new Size(120, 23);
            this.numPort.TabIndex = 3;
            this.numPort.Value = new decimal(new int[] { 502, 0, 0, 0 });

            // grpRtu
            this.grpRtu.Controls.Add(this.lblComPort);
            this.grpRtu.Controls.Add(this.cmbComPort);
            this.grpRtu.Controls.Add(this.btnRefreshCom);
            this.grpRtu.Controls.Add(this.lblBaudRate);
            this.grpRtu.Controls.Add(this.cmbBaudRate);
            this.grpRtu.Controls.Add(this.lblDataBits);
            this.grpRtu.Controls.Add(this.cmbDataBits);
            this.grpRtu.Controls.Add(this.lblStopBits);
            this.grpRtu.Controls.Add(this.cmbStopBits);
            this.grpRtu.Controls.Add(this.lblParity);
            this.grpRtu.Controls.Add(this.cmbParity);
            this.grpRtu.Location = new Point(12, 135);    // 下移，与 TCP 参数不重叠
            this.grpRtu.Name = "grpRtu";
            this.grpRtu.Size = new Size(300, 175);
            this.grpRtu.TabIndex = 3;
            this.grpRtu.TabStop = false;
            this.grpRtu.Text = "RTU 参数";
            this.grpRtu.Visible = true;          // 始终可见
            this.grpRtu.Enabled = false;         // 默认禁用（因为默认 TCP）

            // lblComPort
            this.lblComPort.AutoSize = true;
            this.lblComPort.Location = new Point(10, 28);
            this.lblComPort.Name = "lblComPort";
            this.lblComPort.Size = new Size(56, 17);
            this.lblComPort.TabIndex = 0;
            this.lblComPort.Text = "串口号:";

            // cmbComPort
            this.cmbComPort.DropDownStyle = ComboBoxStyle.DropDown;
            this.cmbComPort.Location = new Point(80, 25);
            this.cmbComPort.Name = "cmbComPort";
            this.cmbComPort.Size = new Size(120, 25);
            this.cmbComPort.TabIndex = 1;

            // btnRefreshCom
            this.btnRefreshCom.Location = new Point(210, 23);
            this.btnRefreshCom.Name = "btnRefreshCom";
            this.btnRefreshCom.Size = new Size(70, 25);
            this.btnRefreshCom.TabIndex = 2;
            this.btnRefreshCom.Text = "刷新";
            this.btnRefreshCom.UseVisualStyleBackColor = true;
            this.btnRefreshCom.Click += new EventHandler(this.BtnRefreshCom_Click);

            // lblBaudRate
            this.lblBaudRate.AutoSize = true;
            this.lblBaudRate.Location = new Point(10, 56);
            this.lblBaudRate.Name = "lblBaudRate";
            this.lblBaudRate.Size = new Size(56, 17);
            this.lblBaudRate.TabIndex = 3;
            this.lblBaudRate.Text = "波特率:";

            // cmbBaudRate
            this.cmbBaudRate.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbBaudRate.Items.AddRange(new object[] { 9600, 19200, 38400, 115200 });
            this.cmbBaudRate.Location = new Point(80, 53);
            this.cmbBaudRate.Name = "cmbBaudRate";
            this.cmbBaudRate.Size = new Size(120, 25);
            this.cmbBaudRate.TabIndex = 4;
            this.cmbBaudRate.SelectedIndex = 0;

            // lblDataBits
            this.lblDataBits.AutoSize = true;
            this.lblDataBits.Location = new Point(10, 84);
            this.lblDataBits.Name = "lblDataBits";
            this.lblDataBits.Size = new Size(56, 17);
            this.lblDataBits.TabIndex = 5;
            this.lblDataBits.Text = "数据位:";

            // cmbDataBits
            this.cmbDataBits.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbDataBits.Items.AddRange(new object[] { 5, 6, 7, 8 });
            this.cmbDataBits.Location = new Point(80, 81);
            this.cmbDataBits.Name = "cmbDataBits";
            this.cmbDataBits.Size = new Size(120, 25);
            this.cmbDataBits.TabIndex = 6;
            this.cmbDataBits.SelectedIndex = 3;

            // lblStopBits
            this.lblStopBits.AutoSize = true;
            this.lblStopBits.Location = new Point(10, 112);
            this.lblStopBits.Name = "lblStopBits";
            this.lblStopBits.Size = new Size(56, 17);
            this.lblStopBits.TabIndex = 7;
            this.lblStopBits.Text = "停止位:";

            // cmbStopBits
            this.cmbStopBits.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbStopBits.Items.AddRange(new object[] { "1", "1.5", "2" });
            this.cmbStopBits.Location = new Point(80, 109);
            this.cmbStopBits.Name = "cmbStopBits";
            this.cmbStopBits.Size = new Size(120, 25);
            this.cmbStopBits.TabIndex = 8;
            this.cmbStopBits.SelectedIndex = 0;

            // lblParity
            this.lblParity.AutoSize = true;
            this.lblParity.Location = new Point(10, 140);
            this.lblParity.Name = "lblParity";
            this.lblParity.Size = new Size(56, 17);
            this.lblParity.TabIndex = 9;
            this.lblParity.Text = "校验位:";

            // cmbParity
            this.cmbParity.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbParity.Items.AddRange(new object[] { "None", "Odd", "Even", "Mark", "Space" });
            this.cmbParity.Location = new Point(80, 137);
            this.cmbParity.Name = "cmbParity";
            this.cmbParity.Size = new Size(120, 25);
            this.cmbParity.TabIndex = 10;
            this.cmbParity.SelectedIndex = 0;

            // lblDataType
            this.lblDataType.AutoSize = true;
            this.lblDataType.Location = new Point(12, 325);   // 根据 RTU 组下移调整
            this.lblDataType.Name = "lblDataType";
            this.lblDataType.Size = new Size(68, 17);
            this.lblDataType.TabIndex = 4;
            this.lblDataType.Text = "数据类型:";

            // cmbDataType
            this.cmbDataType.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbDataType.Items.AddRange(new object[] { "Int16", "UInt16", "Int32", "UInt32", "Float", "Double" });
            this.cmbDataType.Location = new Point(100, 322);
            this.cmbDataType.Name = "cmbDataType";
            this.cmbDataType.Size = new Size(150, 25);
            this.cmbDataType.TabIndex = 5;
            this.cmbDataType.SelectedIndex = 0;

            // lblDisplayFormat
            this.lblDisplayFormat.AutoSize = true;
            this.lblDisplayFormat.Location = new Point(12, 360);
            this.lblDisplayFormat.Name = "lblDisplayFormat";
            this.lblDisplayFormat.Size = new Size(68, 17);
            this.lblDisplayFormat.TabIndex = 6;
            this.lblDisplayFormat.Text = "显示格式:";

            // cmbDisplayFormat
            this.cmbDisplayFormat.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbDisplayFormat.Items.AddRange(new object[] { "十进制", "十六进制(0x)", "十六进制", "八进制", "二进制", "ASCII" });
            this.cmbDisplayFormat.Location = new Point(100, 357);
            this.cmbDisplayFormat.Name = "cmbDisplayFormat";
            this.cmbDisplayFormat.Size = new Size(150, 25);
            this.cmbDisplayFormat.TabIndex = 7;
            this.cmbDisplayFormat.SelectedIndex = 0;

            // btnOK
            this.btnOK.DialogResult = DialogResult.OK;
            this.btnOK.Location = new Point(140, 480);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new Size(75, 30);
            this.btnOK.TabIndex = 8;
            this.btnOK.Text = "确定";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new EventHandler(this.btnOK_Click);

            // btnCancel
            this.btnCancel.DialogResult = DialogResult.Cancel;
            this.btnCancel.Location = new Point(225, 480);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(75, 30);
            this.btnCancel.TabIndex = 9;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new EventHandler(this.btnCancel_Click);

            // ModbusConfigForm
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new SizeF(7F, 17F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new Size(330, 530);   // 高度增加以容纳下移的控件
            this.Controls.Add(this.lblDisplayFormat);
            this.Controls.Add(this.cmbDisplayFormat);
            this.Controls.Add(this.lblDataType);
            this.Controls.Add(this.cmbDataType);
            this.Controls.Add(this.grpRtu);
            this.Controls.Add(this.grpTcp);
            this.Controls.Add(this.cmbProtocol);
            this.Controls.Add(this.lblProtocol);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ModbusConfigForm";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Modbus 配置";

            this.grpTcp.ResumeLayout(false);
            this.grpTcp.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).EndInit();
            this.grpRtu.ResumeLayout(false);
            this.grpRtu.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        // 控件字段声明
        private Label lblProtocol;
        private ComboBox cmbProtocol;
        private GroupBox grpTcp;
        private Label lblIp;
        private TextBox txtIp;
        private Label lblPort;
        private NumericUpDown numPort;
        private GroupBox grpRtu;
        private Label lblComPort;
        private ComboBox cmbComPort;
        private Button btnRefreshCom;
        private Label lblBaudRate;
        private ComboBox cmbBaudRate;
        private Label lblDataBits;
        private ComboBox cmbDataBits;
        private Label lblStopBits;
        private ComboBox cmbStopBits;
        private Label lblParity;
        private ComboBox cmbParity;
        private Label lblDataType;
        private ComboBox cmbDataType;
        private Label lblDisplayFormat;
        private ComboBox cmbDisplayFormat;
        private Button btnOK;
        private Button btnCancel;
        private Label lblStartAddress;
        private Label lblRegisterCount;
        private NumericUpDown numStartAddress;
        private NumericUpDown numRegisterCount;
        private Label lblByteOrder;
        private ComboBox cmbByteOrder;
    }
}