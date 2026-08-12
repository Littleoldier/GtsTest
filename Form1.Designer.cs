using GtsTest.Modbus;
using NModbus.Extensions.Functions;
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

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            flowLayoutPanel1 = new FlowLayoutPanel();
            flowLayoutPanel2 = new FlowLayoutPanel();
            btnToggleSimulator = new Button();
            btnStartMonitor = new Button();
            btnRunWorkflow = new Button();
            cmbWorkflow = new ComboBox();
            btnClear = new Button();
            btnOpen = new Button();
            btnStopMonitor = new Button();
            btnGetStatus = new Button();
            numAxis = new NumericUpDown();
            btnCloseDevice = new Button();
            flowLayoutPanel3 = new FlowLayoutPanel();
            btnSteModbus = new Button();
            btnToggleModbus = new Button();
            btnExportMonitor = new Button();
            lblModbusStatus = new Label();
            panelWrite = new Panel();
            tableLayoutWrite = new TableLayoutPanel();
            grpWriteRegister = new GroupBox();
            lblRegAddr = new Label();
            numWriteAddress = new NumericUpDown();
            lblRegDataType = new Label();
            cmbWriteDataType = new ComboBox();
            lblRegByteOrder = new Label();
            cmbByteOrder = new ComboBox();
            lblRegValue = new Label();
            txtWriteValues = new TextBox();
            btnWriteRegister = new Button();
            grpWriteCoil = new GroupBox();
            lblCoilAddr = new Label();
            numCoilAddress = new NumericUpDown();
            lblCoilValue = new Label();
            cmbCoilValue = new ComboBox();
            btnWriteCoil = new Button();
            splitContainer1 = new SplitContainer();
            txtOperationLog = new TextBox();
            panel1 = new Panel();
            label1 = new Label();
            txtMonitorLog = new TextBox();
            panel2 = new Panel();
            label2 = new Label();
            flowLayoutPanel1.SuspendLayout();
            flowLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numAxis).BeginInit();
            flowLayoutPanel3.SuspendLayout();
            panelWrite.SuspendLayout();
            tableLayoutWrite.SuspendLayout();
            grpWriteRegister.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numWriteAddress).BeginInit();
            grpWriteCoil.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numCoilAddress).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(flowLayoutPanel2);
            flowLayoutPanel1.Controls.Add(flowLayoutPanel3);
            flowLayoutPanel1.Dock = DockStyle.Top;
            flowLayoutPanel1.Location = new Point(0, 0);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(1111, 88);
            flowLayoutPanel1.TabIndex = 14;
            // 
            // flowLayoutPanel2
            // 
            flowLayoutPanel2.Controls.Add(btnToggleSimulator);
            flowLayoutPanel2.Controls.Add(btnStartMonitor);
            flowLayoutPanel2.Controls.Add(btnRunWorkflow);
            flowLayoutPanel2.Controls.Add(cmbWorkflow);
            flowLayoutPanel2.Controls.Add(btnClear);
            flowLayoutPanel2.Controls.Add(btnOpen);
            flowLayoutPanel2.Controls.Add(btnStopMonitor);
            flowLayoutPanel2.Controls.Add(btnGetStatus);
            flowLayoutPanel2.Controls.Add(numAxis);
            flowLayoutPanel2.Controls.Add(btnCloseDevice);
            flowLayoutPanel2.Location = new Point(3, 3);
            flowLayoutPanel2.Name = "flowLayoutPanel2";
            flowLayoutPanel2.Size = new Size(526, 79);
            flowLayoutPanel2.TabIndex = 22;
            // 
            // btnToggleSimulator
            // 
            btnToggleSimulator.Location = new Point(3, 3);
            btnToggleSimulator.Name = "btnToggleSimulator";
            btnToggleSimulator.Size = new Size(90, 30);
            btnToggleSimulator.TabIndex = 1;
            btnToggleSimulator.Text = "切换模式";
            btnToggleSimulator.UseVisualStyleBackColor = true;
            // 
            // btnStartMonitor
            // 
            btnStartMonitor.Location = new Point(99, 3);
            btnStartMonitor.Name = "btnStartMonitor";
            btnStartMonitor.Size = new Size(90, 30);
            btnStartMonitor.TabIndex = 15;
            btnStartMonitor.Text = "开始监控";
            btnStartMonitor.UseVisualStyleBackColor = true;
            // 
            // btnRunWorkflow
            // 
            btnRunWorkflow.Location = new Point(195, 3);
            btnRunWorkflow.Name = "btnRunWorkflow";
            btnRunWorkflow.Size = new Size(90, 30);
            btnRunWorkflow.TabIndex = 15;
            btnRunWorkflow.Text = "启动流程";
            btnRunWorkflow.UseVisualStyleBackColor = true;
            // 
            // cmbWorkflow
            // 
            cmbWorkflow.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbWorkflow.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            cmbWorkflow.FormattingEnabled = true;
            cmbWorkflow.Location = new Point(291, 3);
            cmbWorkflow.Name = "cmbWorkflow";
            cmbWorkflow.Size = new Size(120, 29);
            cmbWorkflow.TabIndex = 15;
            // 
            // btnClear
            // 
            btnClear.Location = new Point(417, 3);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(90, 29);
            btnClear.TabIndex = 15;
            btnClear.Text = "清空信息栏";
            btnClear.UseVisualStyleBackColor = true;
            // 
            // btnOpen
            // 
            btnOpen.Location = new Point(3, 39);
            btnOpen.Name = "btnOpen";
            btnOpen.Size = new Size(90, 30);
            btnOpen.TabIndex = 15;
            btnOpen.Text = "初始化";
            btnOpen.UseVisualStyleBackColor = true;
            // 
            // btnStopMonitor
            // 
            btnStopMonitor.Location = new Point(99, 39);
            btnStopMonitor.Name = "btnStopMonitor";
            btnStopMonitor.Size = new Size(90, 30);
            btnStopMonitor.TabIndex = 15;
            btnStopMonitor.Text = "停止监控";
            btnStopMonitor.UseVisualStyleBackColor = true;
            // 
            // btnGetStatus
            // 
            btnGetStatus.Location = new Point(195, 39);
            btnGetStatus.Name = "btnGetStatus";
            btnGetStatus.Size = new Size(90, 30);
            btnGetStatus.TabIndex = 15;
            btnGetStatus.Text = "获取轴信息";
            btnGetStatus.UseVisualStyleBackColor = true;
            // 
            // numAxis
            // 
            numAxis.Font = new Font("Microsoft YaHei UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 134);
            numAxis.Location = new Point(291, 39);
            numAxis.Maximum = new decimal(new int[] { 8, 0, 0, 0 });
            numAxis.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numAxis.Name = "numAxis";
            numAxis.ReadOnly = true;
            numAxis.Size = new Size(120, 32);
            numAxis.TabIndex = 16;
            numAxis.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // btnCloseDevice
            // 
            btnCloseDevice.Location = new Point(417, 39);
            btnCloseDevice.Name = "btnCloseDevice";
            btnCloseDevice.Size = new Size(90, 29);
            btnCloseDevice.TabIndex = 17;
            btnCloseDevice.Text = "关闭设备";
            btnCloseDevice.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel3
            // 
            flowLayoutPanel3.Controls.Add(btnSteModbus);
            flowLayoutPanel3.Controls.Add(btnToggleModbus);
            flowLayoutPanel3.Controls.Add(btnExportMonitor);
            flowLayoutPanel3.Controls.Add(lblModbusStatus);
            flowLayoutPanel3.Location = new Point(535, 3);
            flowLayoutPanel3.Name = "flowLayoutPanel3";
            flowLayoutPanel3.Size = new Size(498, 79);
            flowLayoutPanel3.TabIndex = 22;
            // 
            // btnSteModbus
            // 
            btnSteModbus.Location = new Point(3, 3);
            btnSteModbus.Name = "btnSteModbus";
            btnSteModbus.Size = new Size(120, 30);
            btnSteModbus.TabIndex = 17;
            btnSteModbus.Text = "设置Modbus";
            btnSteModbus.UseVisualStyleBackColor = true;
            // 
            // btnToggleModbus
            // 
            btnToggleModbus.Location = new Point(129, 3);
            btnToggleModbus.Name = "btnToggleModbus";
            btnToggleModbus.Size = new Size(120, 30);
            btnToggleModbus.TabIndex = 15;
            btnToggleModbus.Text = "连接 Modbus";
            btnToggleModbus.UseVisualStyleBackColor = true;
            // 
            // btnExportMonitor
            // 
            btnExportMonitor.Location = new Point(255, 3);
            btnExportMonitor.Name = "btnExportMonitor";
            btnExportMonitor.Size = new Size(120, 30);
            btnExportMonitor.TabIndex = 17;
            btnExportMonitor.Text = "导出监控数据";
            btnExportMonitor.UseVisualStyleBackColor = true;
            // 
            // lblModbusStatus
            // 
            lblModbusStatus.AutoSize = true;
            lblModbusStatus.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            lblModbusStatus.Location = new Point(3, 44);
            lblModbusStatus.Margin = new Padding(3, 8, 3, 3);
            lblModbusStatus.Name = "lblModbusStatus";
            lblModbusStatus.Size = new Size(126, 21);
            lblModbusStatus.TabIndex = 21;
            lblModbusStatus.Text = "Modbus 未连接";
            // 
            // panelWrite
            // 
            panelWrite.Controls.Add(tableLayoutWrite);
            panelWrite.Dock = DockStyle.Top;
            panelWrite.Location = new Point(0, 88);
            panelWrite.Name = "panelWrite";
            panelWrite.Size = new Size(1111, 93);
            panelWrite.TabIndex = 30;
            // 
            // tableLayoutWrite
            // 
            tableLayoutWrite.ColumnCount = 2;
            tableLayoutWrite.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 47.5247536F));
            tableLayoutWrite.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52.4752464F));
            tableLayoutWrite.Controls.Add(grpWriteRegister, 0, 0);
            tableLayoutWrite.Controls.Add(grpWriteCoil, 1, 0);
            tableLayoutWrite.Dock = DockStyle.Fill;
            tableLayoutWrite.Location = new Point(0, 0);
            tableLayoutWrite.Name = "tableLayoutWrite";
            tableLayoutWrite.RowCount = 1;
            tableLayoutWrite.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutWrite.Size = new Size(1111, 93);
            tableLayoutWrite.TabIndex = 0;
            // 
            // grpWriteRegister
            // 
            grpWriteRegister.Controls.Add(lblRegAddr);
            grpWriteRegister.Controls.Add(numWriteAddress);
            grpWriteRegister.Controls.Add(lblRegDataType);
            grpWriteRegister.Controls.Add(cmbWriteDataType);
            grpWriteRegister.Controls.Add(lblRegByteOrder);
            grpWriteRegister.Controls.Add(cmbByteOrder);
            grpWriteRegister.Controls.Add(lblRegValue);
            grpWriteRegister.Controls.Add(txtWriteValues);
            grpWriteRegister.Controls.Add(btnWriteRegister);
            grpWriteRegister.Dock = DockStyle.Fill;
            grpWriteRegister.Location = new Point(3, 3);
            grpWriteRegister.Name = "grpWriteRegister";
            grpWriteRegister.Size = new Size(522, 87);
            grpWriteRegister.TabIndex = 0;
            grpWriteRegister.TabStop = false;
            grpWriteRegister.Text = "写寄存器 (保持寄存器)";
            // 
            // lblRegAddr
            // 
            lblRegAddr.AutoSize = true;
            lblRegAddr.Location = new Point(10, 28);
            lblRegAddr.Name = "lblRegAddr";
            lblRegAddr.Size = new Size(44, 17);
            lblRegAddr.TabIndex = 0;
            lblRegAddr.Text = "地址：";
            // 
            // numWriteAddress
            // 
            numWriteAddress.Location = new Point(72, 25);
            numWriteAddress.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numWriteAddress.Name = "numWriteAddress";
            numWriteAddress.Size = new Size(120, 23);
            numWriteAddress.TabIndex = 1;
            // 
            // lblRegDataType
            // 
            lblRegDataType.AutoSize = true;
            lblRegDataType.Location = new Point(10, 55);
            lblRegDataType.Name = "lblRegDataType";
            lblRegDataType.Size = new Size(44, 17);
            lblRegDataType.TabIndex = 2;
            lblRegDataType.Text = "类型：";
            // 
            // cmbWriteDataType
            // 
            cmbWriteDataType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbWriteDataType.Items.AddRange(new object[] { "Int16", "UInt16", "Int32", "UInt32", "Float", "Double" });
            cmbWriteDataType.Location = new Point(72, 52);
            cmbWriteDataType.Name = "cmbWriteDataType";
            cmbWriteDataType.Size = new Size(120, 25);
            cmbWriteDataType.TabIndex = 3;
            // 
            // lblRegByteOrder
            // 
            lblRegByteOrder.AutoSize = true;
            lblRegByteOrder.Location = new Point(210, 28);
            lblRegByteOrder.Name = "lblRegByteOrder";
            lblRegByteOrder.Size = new Size(56, 17);
            lblRegByteOrder.TabIndex = 4;
            lblRegByteOrder.Text = "字节序：";
            // 
            // cmbByteOrder
            // 
            cmbByteOrder.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbByteOrder.Items.AddRange(new object[] { "Big Endian", "Little Endian" });
            cmbByteOrder.Location = new Point(272, 25);
            cmbByteOrder.Name = "cmbByteOrder";
            cmbByteOrder.Size = new Size(139, 25);
            cmbByteOrder.TabIndex = 5;
            // 
            // lblRegValue
            // 
            lblRegValue.AutoSize = true;
            lblRegValue.Location = new Point(210, 55);
            lblRegValue.Name = "lblRegValue";
            lblRegValue.Size = new Size(32, 17);
            lblRegValue.TabIndex = 6;
            lblRegValue.Text = "值：";
            // 
            // txtWriteValues
            // 
            txtWriteValues.Location = new Point(272, 52);
            txtWriteValues.Name = "txtWriteValues";
            txtWriteValues.Size = new Size(139, 23);
            txtWriteValues.TabIndex = 7;
            txtWriteValues.Text = "1";
            // 
            // btnWriteRegister
            // 
            btnWriteRegister.Location = new Point(417, 23);
            btnWriteRegister.Name = "btnWriteRegister";
            btnWriteRegister.Size = new Size(90, 28);
            btnWriteRegister.TabIndex = 8;
            btnWriteRegister.Text = "写入寄存器";
            btnWriteRegister.UseVisualStyleBackColor = true;
            // 
            // grpWriteCoil
            // 
            grpWriteCoil.Controls.Add(lblCoilAddr);
            grpWriteCoil.Controls.Add(numCoilAddress);
            grpWriteCoil.Controls.Add(lblCoilValue);
            grpWriteCoil.Controls.Add(cmbCoilValue);
            grpWriteCoil.Controls.Add(btnWriteCoil);
            grpWriteCoil.Dock = DockStyle.Fill;
            grpWriteCoil.Location = new Point(531, 3);
            grpWriteCoil.Name = "grpWriteCoil";
            grpWriteCoil.Size = new Size(577, 87);
            grpWriteCoil.TabIndex = 1;
            grpWriteCoil.TabStop = false;
            grpWriteCoil.Text = "写线圈 (数字量输出)";
            // 
            // lblCoilAddr
            // 
            lblCoilAddr.AutoSize = true;
            lblCoilAddr.Location = new Point(10, 28);
            lblCoilAddr.Name = "lblCoilAddr";
            lblCoilAddr.Size = new Size(44, 17);
            lblCoilAddr.TabIndex = 0;
            lblCoilAddr.Text = "地址：";
            // 
            // numCoilAddress
            // 
            numCoilAddress.Location = new Point(72, 25);
            numCoilAddress.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numCoilAddress.Name = "numCoilAddress";
            numCoilAddress.Size = new Size(120, 23);
            numCoilAddress.TabIndex = 1;
            // 
            // lblCoilValue
            // 
            lblCoilValue.AutoSize = true;
            lblCoilValue.Location = new Point(10, 55);
            lblCoilValue.Name = "lblCoilValue";
            lblCoilValue.Size = new Size(32, 17);
            lblCoilValue.TabIndex = 2;
            lblCoilValue.Text = "值：";
            // 
            // cmbCoilValue
            // 
            cmbCoilValue.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCoilValue.Items.AddRange(new object[] { "ON (1)", "OFF (0)" });
            cmbCoilValue.Location = new Point(72, 52);
            cmbCoilValue.Name = "cmbCoilValue";
            cmbCoilValue.Size = new Size(120, 25);
            cmbCoilValue.TabIndex = 3;
            // 
            // btnWriteCoil
            // 
            btnWriteCoil.Location = new Point(212, 23);
            btnWriteCoil.Name = "btnWriteCoil";
            btnWriteCoil.Size = new Size(90, 28);
            btnWriteCoil.TabIndex = 4;
            btnWriteCoil.Text = "写入线圈";
            btnWriteCoil.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 181);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(txtOperationLog);
            splitContainer1.Panel1.Controls.Add(panel1);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(txtMonitorLog);
            splitContainer1.Panel2.Controls.Add(panel2);
            splitContainer1.Size = new Size(1111, 547);
            splitContainer1.SplitterDistance = 530;
            splitContainer1.TabIndex = 15;
            // 
            // txtOperationLog
            // 
            txtOperationLog.Dock = DockStyle.Fill;
            txtOperationLog.Location = new Point(0, 22);
            txtOperationLog.Multiline = true;
            txtOperationLog.Name = "txtOperationLog";
            txtOperationLog.ReadOnly = true;
            txtOperationLog.ScrollBars = ScrollBars.Vertical;
            txtOperationLog.Size = new Size(530, 525);
            txtOperationLog.TabIndex = 19;
            // 
            // panel1
            // 
            panel1.Controls.Add(label1);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(530, 22);
            panel1.TabIndex = 18;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Dock = DockStyle.Left;
            label1.Location = new Point(0, 0);
            label1.Name = "label1";
            label1.Size = new Size(68, 17);
            label1.TabIndex = 14;
            label1.Text = "操作日志：";
            // 
            // txtMonitorLog
            // 
            txtMonitorLog.Dock = DockStyle.Fill;
            txtMonitorLog.Location = new Point(0, 22);
            txtMonitorLog.Multiline = true;
            txtMonitorLog.Name = "txtMonitorLog";
            txtMonitorLog.ReadOnly = true;
            txtMonitorLog.ScrollBars = ScrollBars.Vertical;
            txtMonitorLog.Size = new Size(577, 525);
            txtMonitorLog.TabIndex = 18;
            // 
            // panel2
            // 
            panel2.Controls.Add(label2);
            panel2.Dock = DockStyle.Top;
            panel2.Location = new Point(0, 0);
            panel2.Name = "panel2";
            panel2.Size = new Size(577, 22);
            panel2.TabIndex = 17;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Dock = DockStyle.Left;
            label2.Location = new Point(0, 0);
            label2.Name = "label2";
            label2.Size = new Size(68, 17);
            label2.TabIndex = 14;
            label2.Text = "实时日志：";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1111, 728);
            Controls.Add(splitContainer1);
            Controls.Add(panelWrite);
            Controls.Add(flowLayoutPanel1);
            Name = "Form1";
            Text = "Gts_Test";
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)numAxis).EndInit();
            flowLayoutPanel3.ResumeLayout(false);
            flowLayoutPanel3.PerformLayout();
            panelWrite.ResumeLayout(false);
            tableLayoutWrite.ResumeLayout(false);
            grpWriteRegister.ResumeLayout(false);
            grpWriteRegister.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numWriteAddress).EndInit();
            grpWriteCoil.ResumeLayout(false);
            grpWriteCoil.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numCoilAddress).EndInit();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ResumeLayout(false);
        }
        #endregion

        // 控件字段声明（供代码隐藏访问）
        private FlowLayoutPanel flowLayoutPanel1;
        private FlowLayoutPanel flowLayoutPanel2;
        private Button btnToggleSimulator;
        private Button btnStartMonitor;
        private Button btnRunWorkflow;
        private ComboBox cmbWorkflow;
        private Button btnClear;
        private Button btnOpen;
        private Button btnStopMonitor;
        private Button btnGetStatus;
        private NumericUpDown numAxis;
        private Button btnCloseDevice;
        private FlowLayoutPanel flowLayoutPanel3;
        private Button btnSteModbus;
        private Button btnToggleModbus;
        private Button btnExportMonitor;
        private Label lblModbusStatus;

        private Panel panelWrite;
        private TableLayoutPanel tableLayoutWrite;
        private GroupBox grpWriteRegister;
        private Label lblRegAddr;
        private NumericUpDown numWriteAddress;
        private Label lblRegDataType;
        private ComboBox cmbWriteDataType;
        private Label lblRegByteOrder;
        private ComboBox cmbByteOrder;
        private Label lblRegValue;
        private TextBox txtWriteValues;
        private Button btnWriteRegister;

        private GroupBox grpWriteCoil;
        private Label lblCoilAddr;
        private NumericUpDown numCoilAddress;
        private Label lblCoilValue;
        private ComboBox cmbCoilValue;
        private Button btnWriteCoil;

        private SplitContainer splitContainer1;
        private TextBox txtOperationLog;
        private Panel panel1;
        private Label label1;
        private TextBox txtMonitorLog;
        private Panel panel2;
        private Label label2;
    }
}