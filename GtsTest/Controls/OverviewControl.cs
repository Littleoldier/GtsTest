using GtsTest.Core;
using GtsTest.Models;
using GtsTest.Services.Alarm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace GtsTest.Controls
{
    public partial class OverviewControl : UserControl
    {
        private readonly DeviceManager _deviceManager;
        private readonly IAlarmManager _alarmManager;
        private System.Windows.Forms.Timer _refreshTimer;
        private Dictionary<string, Chart> _trendCharts = new();

        // ---------- 构造函数 ----------

        /// <summary>
        /// 无参构造函数（仅供 Visual Studio 设计器使用）
        /// </summary>
        public OverviewControl()
        {
            InitializeComponent();
            // 设计时显示占位数据
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                // 显示设计时占位信息
                this.flowDeviceCards.Controls.Clear();
                var placeholder = new Label { Text = "设计视图 - 运行时显示设备卡片", AutoSize = false, Size = new Size(200, 50), TextAlign = ContentAlignment.MiddleCenter };
                this.flowDeviceCards.Controls.Add(placeholder);
            }
        }

        /// <summary>
        /// 运行时构造函数（依赖注入）
        /// </summary>
        /// <param name="deviceManager">设备管理器</param>
        /// <param name="alarmManager">报警管理器（预留，暂未使用）</param>
        public OverviewControl(DeviceManager deviceManager, IAlarmManager alarmManager) : this()
        {
            _deviceManager = deviceManager ?? throw new ArgumentNullException(nameof(deviceManager));
            _alarmManager = alarmManager;  // 可为 null，暂不强制

            // 启动定时器
            _refreshTimer = new System.Windows.Forms.Timer();
            _refreshTimer.Interval = 1000;
            _refreshTimer.Tick += RefreshTimer_Tick;
            _refreshTimer.Start();

            // 立即刷新一次
            RefreshOverview();
        }

        // ---------- UI 初始化 ----------
        private void InitializeComponent()
        {
            this.tableLayoutMain = new TableLayoutPanel();
            this.flowDeviceCards = new FlowLayoutPanel();
            this.panelTrend = new Panel();
            this.chartTrend = new Chart();
            this.chartTrendArea = new ChartArea();
            this.seriesTrend = new Series();

            this.SuspendLayout();

            // tableLayoutMain
            this.tableLayoutMain.Dock = DockStyle.Fill;
            this.tableLayoutMain.ColumnCount = 1;
            this.tableLayoutMain.RowCount = 2;
            this.tableLayoutMain.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));
            this.tableLayoutMain.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
            this.tableLayoutMain.Controls.Add(this.flowDeviceCards, 0, 0);
            this.tableLayoutMain.Controls.Add(this.panelTrend, 0, 1);

            // flowDeviceCards
            this.flowDeviceCards.Dock = DockStyle.Fill;
            this.flowDeviceCards.FlowDirection = FlowDirection.LeftToRight;
            this.flowDeviceCards.Padding = new Padding(10);
            this.flowDeviceCards.AutoScroll = true;
            this.flowDeviceCards.WrapContents = true;

            // panelTrend
            this.panelTrend.Dock = DockStyle.Fill;
            this.panelTrend.Controls.Add(this.chartTrend);
            this.panelTrend.Padding = new Padding(10);

            // chartTrend
            this.chartTrend.Dock = DockStyle.Fill;
            this.chartTrend.ChartAreas.Add(this.chartTrendArea);
            this.chartTrend.Series.Add(this.seriesTrend);
            this.chartTrendArea.AxisX.Title = "时间";
            this.chartTrendArea.AxisY.Title = "总产量";
            this.seriesTrend.ChartType = SeriesChartType.Line;
            this.seriesTrend.Name = "总产量";
            this.seriesTrend.Color = Color.FromArgb(0, 120, 215);

            this.Controls.Add(this.tableLayoutMain);
            this.ResumeLayout(false);
        }

        // ---------- 定时刷新 ----------
        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            // 仅在控件可见且非设计模式时刷新
            if (this.Visible && !this.DesignMode)
            {
                RefreshOverview();
            }
        }

        // ---------- 刷新数据 ----------
        public void RefreshOverview()
        {
            if (_deviceManager == null) return;  // 设计时保护

            // 更新设备卡片
            this.flowDeviceCards.Controls.Clear();
            var devices = _deviceManager.GetAllDevices();
            foreach (var runtime in devices)
            {
                var card = CreateDeviceCard(runtime);
                this.flowDeviceCards.Controls.Add(card);
            }

            // 更新趋势图
            UpdateTrendChart();
        }

        // ---------- 创建设备卡片 ----------
        private Panel CreateDeviceCard(DeviceRuntime runtime)
        {
            var config = runtime.Config;
            var panel = new Panel();
            panel.Size = new Size(180, 120);
            panel.BorderStyle = BorderStyle.FixedSingle;
            panel.BackColor = Color.White;
            panel.Padding = new Padding(8);

            // 名称
            var lblName = new Label();
            lblName.Text = config.Name;
            lblName.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblName.Dock = DockStyle.Top;
            lblName.Height = 25;

            // 状态灯
            var lblStatus = new Label();
            lblStatus.Text = runtime.IsOnline ? "● 在线" : "○ 离线";
            lblStatus.ForeColor = runtime.IsOnline ? Color.Green : Color.Red;
            lblStatus.Dock = DockStyle.Top;
            lblStatus.Height = 20;
            lblStatus.Font = new Font("Segoe UI", 9F);

            // 产量进度条
            var progressBar = new ProgressBar();
            progressBar.Dock = DockStyle.Top;
            progressBar.Height = 20;
            progressBar.Maximum = Math.Max(config.TargetCount, 1);
            progressBar.Value = Math.Min(config.CurrentCount, config.TargetCount);
            progressBar.ForeColor = config.CurrentCount >= config.TargetCount ? Color.Green : Color.DodgerBlue;

            // 产量文字
            var lblProd = new Label();
            lblProd.Text = $"{config.CurrentCount} / {config.TargetCount}";
            lblProd.Dock = DockStyle.Top;
            lblProd.Height = 20;
            lblProd.TextAlign = ContentAlignment.MiddleCenter;
            lblProd.Font = new Font("Segoe UI", 9F);

            // 步骤
            var lblStep = new Label();
            lblStep.Text = $"步骤: {runtime.CurrentStep}";
            lblStep.Dock = DockStyle.Top;
            lblStep.Height = 18;
            lblStep.Font = new Font("Segoe UI", 8F);
            lblStep.ForeColor = Color.Gray;

            panel.Controls.Add(lblStep);
            panel.Controls.Add(lblProd);
            panel.Controls.Add(progressBar);
            panel.Controls.Add(lblStatus);
            panel.Controls.Add(lblName);

            return panel;
        }

        // ---------- 更新趋势图 ----------
        private void UpdateTrendChart()
        {
            if (_deviceManager == null) return;
            var devices = _deviceManager.GetAllDevices();
            int total = devices.Sum(d => d.Config.CurrentCount);
            this.seriesTrend.Points.AddXY(DateTime.Now, total);
            if (this.seriesTrend.Points.Count > 100)
                this.seriesTrend.Points.RemoveAt(0);
        }

        // ---------- 释放资源 ----------
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _refreshTimer?.Stop();
                _refreshTimer?.Dispose();
            }
            base.Dispose(disposing);
        }

        // ---------- 控件字段 ----------
        private TableLayoutPanel tableLayoutMain;
        private FlowLayoutPanel flowDeviceCards;
        private Panel panelTrend;
        private Chart chartTrend;
        private ChartArea chartTrendArea;
        private Series seriesTrend;
    }
}