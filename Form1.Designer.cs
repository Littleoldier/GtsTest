namespace GtsTest
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            flowLayoutPanel1 = new FlowLayoutPanel();
            btnToggleSimulator = new Button();
            btnStartMonitor = new Button();
            btnRunWorkflow = new Button();
            cmbWorkflow = new ComboBox();
            btnClear = new Button();
            btnOpen = new Button();
            btnStopMonitor = new Button();
            btnGetStatus = new Button();
            numAxis = new NumericUpDown();
            splitContainer1 = new SplitContainer();
            txtOperationLog = new TextBox();
            panel1 = new Panel();
            label1 = new Label();
            txtMonitorLog = new TextBox();
            panel2 = new Panel();
            label2 = new Label();
            flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numAxis).BeginInit();
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
            flowLayoutPanel1.Controls.Add(btnToggleSimulator);
            flowLayoutPanel1.Controls.Add(btnStartMonitor);
            flowLayoutPanel1.Controls.Add(btnRunWorkflow);
            flowLayoutPanel1.Controls.Add(cmbWorkflow);
            flowLayoutPanel1.Controls.Add(btnClear);
            flowLayoutPanel1.Controls.Add(btnOpen);
            flowLayoutPanel1.Controls.Add(btnStopMonitor);
            flowLayoutPanel1.Controls.Add(btnGetStatus);
            flowLayoutPanel1.Controls.Add(numAxis);
            flowLayoutPanel1.Dock = DockStyle.Top;
            flowLayoutPanel1.Location = new Point(0, 0);
            flowLayoutPanel1.Margin = new Padding(25, 3, 25, 3);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(643, 74);
            flowLayoutPanel1.TabIndex = 14;
            // 
            // btnToggleSimulator
            // 
            btnToggleSimulator.Location = new Point(3, 3);
            btnToggleSimulator.Margin = new Padding(3, 3, 25, 3);
            btnToggleSimulator.Name = "btnToggleSimulator";
            btnToggleSimulator.Size = new Size(90, 30);
            btnToggleSimulator.TabIndex = 15;
            btnToggleSimulator.Text = "切换模式";
            btnToggleSimulator.UseVisualStyleBackColor = true;
            // 
            // btnStartMonitor
            // 
            btnStartMonitor.Location = new Point(121, 3);
            btnStartMonitor.Margin = new Padding(3, 3, 25, 3);
            btnStartMonitor.Name = "btnStartMonitor";
            btnStartMonitor.Size = new Size(90, 30);
            btnStartMonitor.TabIndex = 15;
            btnStartMonitor.Text = "开始监控";
            btnStartMonitor.UseVisualStyleBackColor = true;
            // 
            // btnRunWorkflow
            // 
            btnRunWorkflow.Location = new Point(239, 3);
            btnRunWorkflow.Margin = new Padding(3, 3, 25, 3);
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
            cmbWorkflow.Location = new Point(357, 3);
            cmbWorkflow.Margin = new Padding(3, 3, 45, 3);
            cmbWorkflow.Name = "cmbWorkflow";
            cmbWorkflow.Size = new Size(120, 29);
            cmbWorkflow.TabIndex = 15;
            // 
            // btnClear
            // 
            btnClear.Location = new Point(525, 3);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(90, 29);
            btnClear.TabIndex = 15;
            btnClear.Text = "清空信息栏";
            btnClear.UseVisualStyleBackColor = true;
            // 
            // btnOpen
            // 
            btnOpen.Location = new Point(3, 39);
            btnOpen.Margin = new Padding(3, 3, 25, 3);
            btnOpen.Name = "btnOpen";
            btnOpen.Size = new Size(90, 30);
            btnOpen.TabIndex = 15;
            btnOpen.Text = "初始化";
            btnOpen.UseVisualStyleBackColor = true;
            // 
            // btnStopMonitor
            // 
            btnStopMonitor.Location = new Point(121, 39);
            btnStopMonitor.Margin = new Padding(3, 3, 25, 3);
            btnStopMonitor.Name = "btnStopMonitor";
            btnStopMonitor.Size = new Size(90, 30);
            btnStopMonitor.TabIndex = 15;
            btnStopMonitor.Text = "停止监控";
            btnStopMonitor.UseVisualStyleBackColor = true;
            // 
            // btnGetStatus
            // 
            btnGetStatus.Location = new Point(239, 39);
            btnGetStatus.Margin = new Padding(3, 3, 25, 3);
            btnGetStatus.Name = "btnGetStatus";
            btnGetStatus.Size = new Size(90, 30);
            btnGetStatus.TabIndex = 15;
            btnGetStatus.Text = "获取轴信息";
            btnGetStatus.UseVisualStyleBackColor = true;
            // 
            // numAxis
            // 
            numAxis.Font = new Font("Microsoft YaHei UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 134);
            numAxis.Location = new Point(357, 39);
            numAxis.Maximum = new decimal(new int[] { 8, 0, 0, 0 });
            numAxis.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numAxis.Name = "numAxis";
            numAxis.ReadOnly = true;
            numAxis.Size = new Size(120, 32);
            numAxis.TabIndex = 16;
            numAxis.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 74);
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
            splitContainer1.Size = new Size(643, 609);
            splitContainer1.SplitterDistance = 312;
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
            txtOperationLog.Size = new Size(312, 587);
            txtOperationLog.TabIndex = 19;
            // 
            // panel1
            // 
            panel1.Controls.Add(label1);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(312, 22);
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
            txtMonitorLog.Size = new Size(327, 587);
            txtMonitorLog.TabIndex = 18;
            txtMonitorLog.Tag = "";
            // 
            // panel2
            // 
            panel2.Controls.Add(label2);
            panel2.Dock = DockStyle.Top;
            panel2.Location = new Point(0, 0);
            panel2.Name = "panel2";
            panel2.Size = new Size(327, 22);
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
            ClientSize = new Size(643, 683);
            Controls.Add(splitContainer1);
            Controls.Add(flowLayoutPanel1);
            Name = "Form1";
            Text = "Gts_Test";
            flowLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)numAxis).EndInit();
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
        private FlowLayoutPanel flowLayoutPanel1;
        private Button btnOpen;
        private Button btnClear;
        private Button btnGetStatus;
        private Button btnStartMonitor;
        private Button btnStopMonitor;
        private Button btnRunWorkflow;
        private Button btnToggleSimulator;
        private ComboBox cmbWorkflow;
        private NumericUpDown numAxis;
        private SplitContainer splitContainer1;
        private Panel panel1;
        private Label label1;
        private TextBox txtOperationLog;
        private TextBox txtMonitorLog;
        private Panel panel2;
        private Label label2;
    }
}
