using GtsTest.Commands;
using GtsTest.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;


namespace GtsTest.Controls
{

    public partial class WorkflowControl : UserControl
    {
        private List<CommandConfig> _steps = new List<CommandConfig>();
        private string _workflowName = "Default";
        private readonly GtsModel _model;
        private readonly DeviceManager _deviceManager;
        

        public WorkflowControl(GtsModel model, DeviceManager deviceManager)
        {
            _model = model;
            _deviceManager = deviceManager;
            InitializeComponent();
            LoadWorkflow();
        }

        private void InitializeComponent()
        {
            this.tableLayout = new TableLayoutPanel();
            this.toolPanel = new Panel();
            this.cmbWorkflow = new ComboBox();
            this.btnNew = new Button();
            this.btnSave = new Button();
            this.btnLoad = new Button();
            this.btnRun = new Button();
            this.btnStop = new Button();
            this.listViewSteps = new ListView();
            this.colStep = new ColumnHeader();
            this.colType = new ColumnHeader();
            this.colParams = new ColumnHeader();
            this.panelEdit = new Panel();
            this.btnAdd = new Button();
            this.btnEdit = new Button();
            this.btnDelete = new Button();
            this.btnMoveUp = new Button();
            this.btnMoveDown = new Button();

            this.SuspendLayout();

            // tableLayout
            this.tableLayout.Dock = DockStyle.Fill;
            this.tableLayout.ColumnCount = 1;
            this.tableLayout.RowCount = 3;
            this.tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            this.tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this.tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));

            // 工具栏
            this.toolPanel.Dock = DockStyle.Fill;
            this.cmbWorkflow.Location = new Point(10, 8);
            this.cmbWorkflow.Size = new Size(150, 25);
            this.cmbWorkflow.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbWorkflow.Items.Add("Default");
            this.cmbWorkflow.SelectedIndex = 0;

            this.btnNew.Location = new Point(170, 6);
            this.btnNew.Size = new Size(60, 28);
            this.btnNew.Text = "新建";
            this.btnNew.Click += BtnNew_Click;

            this.btnSave.Location = new Point(240, 6);
            this.btnSave.Size = new Size(60, 28);
            this.btnSave.Text = "保存";
            this.btnSave.Click += BtnSave_Click;

            this.btnLoad.Location = new Point(310, 6);
            this.btnLoad.Size = new Size(60, 28);
            this.btnLoad.Text = "加载";
            this.btnLoad.Click += BtnLoad_Click;

            this.btnRun.Location = new Point(400, 6);
            this.btnRun.Size = new Size(60, 28);
            this.btnRun.Text = "▶ 运行";
            this.btnRun.BackColor = Color.LightGreen;
            this.btnRun.Click += BtnRun_Click;

            this.btnStop.Location = new Point(470, 6);
            this.btnStop.Size = new Size(60, 28);
            this.btnStop.Text = "⏹ 停止";
            this.btnStop.BackColor = Color.LightCoral;
            this.btnStop.Click += BtnStop_Click;

            this.toolPanel.Controls.Add(this.cmbWorkflow);
            this.toolPanel.Controls.Add(this.btnNew);
            this.toolPanel.Controls.Add(this.btnSave);
            this.toolPanel.Controls.Add(this.btnLoad);
            this.toolPanel.Controls.Add(this.btnRun);
            this.toolPanel.Controls.Add(this.btnStop);

            // 步骤列表
            this.listViewSteps.Dock = DockStyle.Fill;
            this.listViewSteps.View = View.Details;
            this.listViewSteps.FullRowSelect = true;
            this.listViewSteps.Columns.Add(this.colStep);
            this.listViewSteps.Columns.Add(this.colType);
            this.listViewSteps.Columns.Add(this.colParams);
            this.colStep.Text = "步骤";
            this.colStep.Width = 60;
            this.colType.Text = "类型";
            this.colType.Width = 120;
            this.colParams.Text = "参数";
            this.colParams.Width = 400;
            this.listViewSteps.SelectedIndexChanged += ListViewSteps_SelectedIndexChanged;

            // 编辑面板
            this.panelEdit.Dock = DockStyle.Fill;
            this.btnAdd.Location = new Point(10, 6);
            this.btnAdd.Size = new Size(60, 28);
            this.btnAdd.Text = "➕ 添加";
            this.btnAdd.Click += BtnAdd_Click;

            this.btnEdit.Location = new Point(80, 6);
            this.btnEdit.Size = new Size(60, 28);
            this.btnEdit.Text = "✏️ 编辑";
            this.btnEdit.Enabled = false;
            this.btnEdit.Click += BtnEdit_Click;

            this.btnDelete.Location = new Point(150, 6);
            this.btnDelete.Size = new Size(60, 28);
            this.btnDelete.Text = "🗑 删除";
            this.btnDelete.Enabled = false;
            this.btnDelete.Click += BtnDelete_Click;

            this.btnMoveUp.Location = new Point(220, 6);
            this.btnMoveUp.Size = new Size(60, 28);
            this.btnMoveUp.Text = "⬆ 上移";
            this.btnMoveUp.Enabled = false;
            this.btnMoveUp.Click += BtnMoveUp_Click;

            this.btnMoveDown.Location = new Point(290, 6);
            this.btnMoveDown.Size = new Size(60, 28);
            this.btnMoveDown.Text = "⬇ 下移";
            this.btnMoveDown.Enabled = false;
            this.btnMoveDown.Click += BtnMoveDown_Click;

            this.panelEdit.Controls.Add(this.btnAdd);
            this.panelEdit.Controls.Add(this.btnEdit);
            this.panelEdit.Controls.Add(this.btnDelete);
            this.panelEdit.Controls.Add(this.btnMoveUp);
            this.panelEdit.Controls.Add(this.btnMoveDown);

            this.tableLayout.Controls.Add(this.toolPanel, 0, 0);
            this.tableLayout.Controls.Add(this.listViewSteps, 0, 1);
            this.tableLayout.Controls.Add(this.panelEdit, 0, 2);

            this.Controls.Add(this.tableLayout);
            this.ResumeLayout(false);
        }

        private void ListViewSteps_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool hasSelected = this.listViewSteps.SelectedItems.Count > 0;
            this.btnEdit.Enabled = hasSelected;
            this.btnDelete.Enabled = hasSelected;
            this.btnMoveUp.Enabled = hasSelected && this.listViewSteps.SelectedIndices[0] > 0;
            this.btnMoveDown.Enabled = hasSelected && this.listViewSteps.SelectedIndices[0] < this.listViewSteps.Items.Count - 1;
        }

        private void LoadWorkflow()
        {
            // 从文件加载工作流
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Workflows", _workflowName + ".json");
            if (File.Exists(path))
            {
                try
                {
                    string json = File.ReadAllText(path);
                    var config = System.Text.Json.JsonSerializer.Deserialize<WorkflowConfig>(json);
                    if (config != null)
                    {
                        _steps = config.Commands;
                        RefreshStepList();
                        return;
                    }
                }
                catch { }
            }
            // 默认步骤
            _steps = new List<CommandConfig>
            {
                new CommandConfig { Type = "Home", Axis = 1, HomePos = 0 },
                new CommandConfig { Type = "MoveAbs", Axis = 1, TargetPos = 10000, Vel = 20, Acc = 10 },
                new CommandConfig { Type = "Delay", DelayMs = 500 },
                new CommandConfig { Type = "WaitIO", IoIndex = 0, ExpectValue = true }
            };
            RefreshStepList();
        }

        private void RefreshStepList()
        {
            this.listViewSteps.Items.Clear();
            int idx = 1;
            foreach (var step in _steps)
            {
                var item = new ListViewItem(idx.ToString());
                item.SubItems.Add(step.Type);
                string param = "";
                switch (step.Type)
                {
                    case "Home": param = $"轴{step.Axis} 位置={step.HomePos}"; break;
                    case "MoveAbs": param = $"轴{step.Axis} 目标={step.TargetPos} 速度={step.Vel} 加速度={step.Acc}"; break;
                    case "Delay": param = $"{step.DelayMs}ms"; break;
                    case "WaitIO": param = $"IO{step.IoIndex} = {step.ExpectValue}"; break;
                    default:
                        param = "未知参数";
                        break;
                }
                item.SubItems.Add(param);
                this.listViewSteps.Items.Add(item);
                idx++;
            }
            // 清除选中状态
            this.btnEdit.Enabled = false;
            this.btnDelete.Enabled = false;
            this.btnMoveUp.Enabled = false;
            this.btnMoveDown.Enabled = false;
        }

        private void BtnNew_Click(object sender, EventArgs e)
        {
            _steps.Clear();
            RefreshStepList();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            var config = new WorkflowConfig { Name = _workflowName, Commands = _steps };
            string json = System.Text.Json.JsonSerializer.Serialize(config, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Workflows");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, _workflowName + ".json");
            File.WriteAllText(path, json);
            MessageBox.Show($"工作流已保存到 {path}", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "JSON文件|*.json";
                ofd.InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Workflows");
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string json = File.ReadAllText(ofd.FileName);
                        var config = System.Text.Json.JsonSerializer.Deserialize<WorkflowConfig>(json);
                        if (config != null)
                        {
                            _workflowName = config.Name;
                            _steps = config.Commands;
                            RefreshStepList();
                            MessageBox.Show("工作流加载成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"加载失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnRun_Click(object sender, EventArgs e)
        {
            // 运行工作流（调用Presenter的方法，通过事件实现）
            // 此处暂不实现，由主窗体处理
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            // 停止工作流
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var dialog = new CommandConfigDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _steps.Add(dialog.Config);
                    RefreshStepList();
                }
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (this.listViewSteps.SelectedIndices.Count == 0) return;
            int idx = this.listViewSteps.SelectedIndices[0];
            var config = _steps[idx];
            using (var dialog = new CommandConfigDialog(config))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _steps[idx] = dialog.Config;
                    RefreshStepList();
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (this.listViewSteps.SelectedIndices.Count == 0) return;
            int idx = this.listViewSteps.SelectedIndices[0];
            if (MessageBox.Show($"确定删除步骤 {idx + 1} 吗？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _steps.RemoveAt(idx);
                RefreshStepList();
            }
        }

        private void BtnMoveUp_Click(object sender, EventArgs e)
        {
            if (this.listViewSteps.SelectedIndices.Count == 0) return;
            int idx = this.listViewSteps.SelectedIndices[0];
            if (idx > 0)
            {
                var temp = _steps[idx - 1];
                _steps[idx - 1] = _steps[idx];
                _steps[idx] = temp;
                RefreshStepList();
                this.listViewSteps.Items[idx - 1].Selected = true;
            }
        }

        private void BtnMoveDown_Click(object sender, EventArgs e)
        {
            if (this.listViewSteps.SelectedIndices.Count == 0) return;
            int idx = this.listViewSteps.SelectedIndices[0];
            if (idx < _steps.Count - 1)
            {
                var temp = _steps[idx + 1];
                _steps[idx + 1] = _steps[idx];
                _steps[idx] = temp;
                RefreshStepList();
                this.listViewSteps.Items[idx + 1].Selected = true;
            }
        }

        // 辅助对话框（简化）
        public class CommandConfigDialog : Form
        {
            public CommandConfig Config { get; private set; }
            private ComboBox cmbType;
            private NumericUpDown cmbAxis;       // ← 改为 NumericUpDown
            private NumericUpDown numTarget, numHome, numDelay, numVel, numAcc;
            private Button btnOK, btnCancel;

            public CommandConfigDialog(CommandConfig existing = null)
            {
                Config = existing ?? new CommandConfig { Type = "Home", Axis = 1 };
                InitializeComponent();
                LoadConfig();
            }

            private void InitializeComponent()
            {
                this.Text = "编辑指令";
                this.Size = new Size(400, 300);
                this.FormBorderStyle = FormBorderStyle.FixedDialog;
                this.MaximizeBox = false;
                this.MinimizeBox = false;
                this.StartPosition = FormStartPosition.CenterParent;

                TableLayoutPanel layout = new TableLayoutPanel();
                layout.Dock = DockStyle.Fill;
                layout.ColumnCount = 2;
                layout.RowCount = 7;
                layout.Padding = new Padding(10);

                layout.Controls.Add(new Label { Text = "类型:", TextAlign = ContentAlignment.MiddleRight }, 0, 0);
                this.cmbType = new ComboBox();
                this.cmbType.DropDownStyle = ComboBoxStyle.DropDownList;
                this.cmbType.Items.AddRange(new object[] { "Home", "MoveAbs", "Delay", "WaitIO" });
                layout.Controls.Add(this.cmbType, 1, 0);

                layout.Controls.Add(new Label { Text = "轴号:", TextAlign = ContentAlignment.MiddleRight }, 0, 1);
                this.cmbAxis = new NumericUpDown();   // ← 这里已经是 NumericUpDown
                this.cmbAxis.Minimum = 1;
                this.cmbAxis.Maximum = 8;
                this.cmbAxis.Value = 1;
                layout.Controls.Add(this.cmbAxis, 1, 1);

                layout.Controls.Add(new Label { Text = "目标位置:", TextAlign = ContentAlignment.MiddleRight }, 0, 2);
                this.numTarget = new NumericUpDown();
                this.numTarget.Maximum = 100000;
                layout.Controls.Add(this.numTarget, 1, 2);

                layout.Controls.Add(new Label { Text = "回零位置:", TextAlign = ContentAlignment.MiddleRight }, 0, 3);
                this.numHome = new NumericUpDown();
                this.numHome.Maximum = 100000;
                layout.Controls.Add(this.numHome, 1, 3);

                layout.Controls.Add(new Label { Text = "延时(ms):", TextAlign = ContentAlignment.MiddleRight }, 0, 4);
                this.numDelay = new NumericUpDown();
                this.numDelay.Maximum = 60000;
                this.numDelay.Value = 500;
                layout.Controls.Add(this.numDelay, 1, 4);

                layout.Controls.Add(new Label { Text = "速度:", TextAlign = ContentAlignment.MiddleRight }, 0, 5);
                this.numVel = new NumericUpDown();
                this.numVel.DecimalPlaces = 1;
                this.numVel.Increment = 0.5m;
                this.numVel.Maximum = 100;
                this.numVel.Value = 10;
                layout.Controls.Add(this.numVel, 1, 5);

                layout.Controls.Add(new Label { Text = "加速度:", TextAlign = ContentAlignment.MiddleRight }, 0, 6);
                this.numAcc = new NumericUpDown();
                this.numAcc.DecimalPlaces = 1;
                this.numAcc.Increment = 0.5m;
                this.numAcc.Maximum = 50;
                this.numAcc.Value = 5;
                layout.Controls.Add(this.numAcc, 1, 6);

                FlowLayoutPanel btnPanel = new FlowLayoutPanel();
                btnPanel.Dock = DockStyle.Bottom;
                btnPanel.FlowDirection = FlowDirection.RightToLeft;
                btnPanel.Height = 40;
                btnPanel.Padding = new Padding(10);
                this.btnOK = new Button { Text = "确定", DialogResult = DialogResult.OK };
                this.btnCancel = new Button { Text = "取消", DialogResult = DialogResult.Cancel };
                btnPanel.Controls.Add(this.btnOK);
                btnPanel.Controls.Add(this.btnCancel);
                this.Controls.Add(layout);
                this.Controls.Add(btnPanel);
                for (int i = 0; i < 7; i++)
                    layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
                this.AcceptButton = btnOK;
                this.CancelButton = btnCancel;
            }

            private void LoadConfig()
            {
                this.cmbType.SelectedItem = Config.Type;
                this.cmbAxis.Value = Config.Axis;
                this.numTarget.Value = Config.TargetPos;
                this.numHome.Value = Config.HomePos;
                this.numDelay.Value = Config.DelayMs;
                this.numVel.Value = (decimal)Config.Vel;
                this.numAcc.Value = (decimal)Config.Acc;
            }

            protected override void OnFormClosing(FormClosingEventArgs e)
            {
                if (this.DialogResult == DialogResult.OK)
                {
                    Config.Type = this.cmbType.SelectedItem.ToString();
                    Config.Axis = (int)this.cmbAxis.Value;
                    Config.TargetPos = (int)this.numTarget.Value;
                    Config.HomePos = (int)this.numHome.Value;
                    Config.DelayMs = (int)this.numDelay.Value;
                    Config.Vel = (double)this.numVel.Value;
                    Config.Acc = (double)this.numAcc.Value;
                }
                base.OnFormClosing(e);
            }
        }

        private TableLayoutPanel tableLayout;
        private Panel toolPanel, panelEdit;
        private ComboBox cmbWorkflow;
        private Button btnNew, btnSave, btnLoad, btnRun, btnStop;
        private Button btnAdd, btnEdit, btnDelete, btnMoveUp, btnMoveDown;
        private ListView listViewSteps;
        private ColumnHeader colStep, colType, colParams;
    }
}