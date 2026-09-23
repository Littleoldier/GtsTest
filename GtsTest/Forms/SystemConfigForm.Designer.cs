using GtsTest.Core;
using System.Drawing;
using System.Windows.Forms;

namespace GtsTest
{
    partial class SystemConfigForm
    {
        private System.ComponentModel.IContainer components = null;

        // ==================== 控件声明 ====================
        private TabControl tabMain;
        private TabPage tabWorkflow;
        private TabPage tabComm;          // OPC UA 通信配置
        private TabPage tabMqtt;          // 🆕 MQTT 独立 Tab
        private TabPage tabDebug;
        private TabPage tabSystemTools;
        private TabPage tabAdmin;

        // 系统工具控件
        private Button btnInit;
        private Button btnToggleMode;
        private Button btnHotReload;
        private Button btnSaveConfig;
        private Button btnDumpBlackBox;
        private Button btnClearLogs;
        private Button btnDiagnostics;



        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1050, 680);
            this.Text = "🔧 系统配置中心";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            this.BackColor = SystemColors.Control;

            tabMain = new TabControl();
            tabMain.Dock = DockStyle.Fill;
            tabMain.Padding = new Point(8, 3);
            tabMain.Font = new Font("Segoe UI", 9F);

            // ---- 定义所有 Tab 页 ----
            tabWorkflow = new TabPage { Text = "📝 工作流", Padding = new Padding(6) };
            tabComm = new TabPage { Text = "🌐 OPC UA", Padding = new Padding(6) };
            tabMqtt = new TabPage { Text = "📨 MQTT", Padding = new Padding(6) };   // 🆕 MQTT 独立
            tabDebug = new TabPage { Text = "🔧 调试工具", Padding = new Padding(6) };
            tabSystemTools = new TabPage { Text = "⚙️ 系统工具", Padding = new Padding(6) };
            tabAdmin = new TabPage { Text = "👤 用户管理", Padding = new Padding(6) };

            // ---- 添加到 TabControl ----
            tabMain.Controls.Add(tabWorkflow);
            tabMain.Controls.Add(tabComm);
            tabMain.Controls.Add(tabMqtt);
            tabMain.Controls.Add(tabDebug);
            tabMain.Controls.Add(tabSystemTools);
            tabMain.Controls.Add(tabAdmin);

            this.Controls.Add(tabMain);

            BuildSystemToolsContent();
        }

        // ================================================================
        // 系统工具 Tab 内容构建（使用 TableLayoutPanel，自适应宽度）
        // ================================================================
        private void BuildSystemToolsContent()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                AutoScroll = true,
                BackColor = Color.FromArgb(248, 248, 250)
            };

            var mainTable = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                RowCount = 0,
                Padding = new Padding(10),
                BackColor = Color.Transparent
            };
            mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            // ---- 标题 ----
            var lblTitle = new Label
            {
                Text = "⚙️ 系统工具",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.DarkSlateGray,
                AutoSize = false,
                Height = 40,
                Margin = new Padding(0, 0, 0, 10)
            };
            mainTable.Controls.Add(lblTitle, 0, mainTable.RowCount);
            mainTable.RowCount++;

            // ---- 1. 系统控制分组 ----
            var grpControl = new GroupBox
            {
                Text = "🖥️ 系统控制",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Padding = new Padding(10),
                Dock = DockStyle.Fill,
                AutoSize = true,
                MinimumSize = new Size(400, 70)
            };
            var controlTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 2,
                AutoSize = true,
                Padding = new Padding(5)
            };
            controlTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            controlTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            controlTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
            controlTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            controlTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

            btnInit = new Button
            {
                Text = "🔌 初始化运动控制卡",
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.LightBlue,
                Margin = new Padding(3)
            };
            btnHotReload = new Button
            {
                Text = "🔄 热加载配置",
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.LightYellow,
                Margin = new Padding(3)
            };
            btnSaveConfig = new Button
            {
                Text = "💾 保存配置",
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.LightGreen,
                Margin = new Padding(3)
            };

            controlTable.Controls.Add(btnInit, 0, 0);
            controlTable.Controls.Add(btnHotReload, 1, 0);
            controlTable.Controls.Add(btnSaveConfig, 2, 0);

            var permLabel = new Label
            {
                Text = "权限: 工程师/管理员",
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Margin = new Padding(3, 0, 0, 0)
            };
            controlTable.Controls.Add(permLabel, 0, 1);
            controlTable.SetColumnSpan(permLabel, 3);

            grpControl.Controls.Add(controlTable);
            mainTable.Controls.Add(grpControl, 0, mainTable.RowCount);
            mainTable.RowCount++;

            // ---- 2. 模拟模式分组 ----
            var grpMode = new GroupBox
            {
                Text = "🎯 模拟模式",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Padding = new Padding(10),
                Dock = DockStyle.Fill,
                AutoSize = true,
                MinimumSize = new Size(400, 55)
            };
            var modeTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                AutoSize = true,
                Padding = new Padding(5)
            };
            modeTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            modeTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));

            var lblModeStatus = new Label
            {
                Text = $"当前模式: {(GtsModel.UseSimulation ? "模拟模式" : "真实模式")}",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.DarkBlue,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(3)
            };
            btnToggleMode = new Button
            {
                Text = GtsModel.UseSimulation ? "切换到真实" : "切换到模拟",
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.LightGray,
                Margin = new Padding(3)
            };

            modeTable.Controls.Add(lblModeStatus, 0, 0);
            modeTable.Controls.Add(btnToggleMode, 1, 0);

            grpMode.Controls.Add(modeTable);
            mainTable.Controls.Add(grpMode, 0, mainTable.RowCount);
            mainTable.RowCount++;

            // ---- 3. 运维工具分组 ----
            var grpTools = new GroupBox
            {
                Text = "📦 运维工具",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Padding = new Padding(10),
                Dock = DockStyle.Fill,
                AutoSize = true,
                MinimumSize = new Size(400, 55)
            };
            var toolsTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                AutoSize = true,
                Padding = new Padding(5)
            };
            toolsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            toolsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            toolsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));

            btnDumpBlackBox = new Button
            {
                Text = "📤 导出黑匣子",
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.LightSalmon,
                Margin = new Padding(3)
            };
            btnClearLogs = new Button
            {
                Text = "🗑️ 清空日志",
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.LightGray,
                Margin = new Padding(3)
            };
            btnDiagnostics = new Button
            {
                Text = "📊 系统诊断",
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.LightCyan,
                Margin = new Padding(3)
            };

            toolsTable.Controls.Add(btnDumpBlackBox, 0, 0);
            toolsTable.Controls.Add(btnClearLogs, 1, 0);
            toolsTable.Controls.Add(btnDiagnostics, 2, 0);

            grpTools.Controls.Add(toolsTable);
            mainTable.Controls.Add(grpTools, 0, mainTable.RowCount);
            mainTable.RowCount++;

            panel.Controls.Add(mainTable);
            tabSystemTools.Controls.Add(panel);
        }
    }
}