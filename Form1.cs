using System;
using System.Windows.Forms;

namespace GtsTest
{

    public partial class Form1 : Form
    {
        // 定义事件，由 Controller 订阅
        public event EventHandler OpenRequested;                    //打开设备
        public event EventHandler CloseDeviceRequested;             //关闭设备
        public event EventHandler ClearRequested;                   //清空信息栏
        public event EventHandler GetStatusRequested;               //获取状态轴信息
        public event EventHandler StartMonitorRequested;            //开启监控
        public event EventHandler StopMonitorRequested;             //停止监控
        public event EventHandler RunWorkflowRequested;             //工作流
        public event EventHandler ToggleSimulatorRequested;         //切换模式
        public string SelectedWorkflowName => cmbWorkflow.SelectedItem?.ToString() ?? "";



        public Form1()
        {
            InitializeComponent();                                              //初始化UI
            SetSimulationModeUI(GtsModel.UseSimulation);                        //设定模式
            // 绑定 UI 事件到内部触发方法
            btnOpen.Click += (s, e) => OnOpenRequested();                       //开启设备
            btnCloseDevice.Click += (s, e) => OnCloseDeviceRequested();         //关闭设备
            btnClear.Click += (s, e) => OnClearRequested();                     //清空消息栏
            btnGetStatus.Click += (s, e) => OnGetStatusRequested();             //获取轴状态信息
            btnStartMonitor.Click += (s, e) => OnStartMonitorRequested();       //实时监控
            btnStopMonitor.Click += (s, e) => OnStopMonitorRequested();         //停止监控
            btnRunWorkflow.Click += (s, e) => OnRunWorkflowRequested();         //打开工作流
            btnToggleSimulator.Click += (s, e) => OnToggleSimulatorRequested(); //切换模式

            LoadWorkflowList();                                                 //获取配置文件
        }

        // 触发打开事件
        private void OnOpenRequested() => OpenRequested?.Invoke(this, EventArgs.Empty);
        // 触发关闭事件
        private void OnCloseDeviceRequested() => CloseDeviceRequested?.Invoke(this, EventArgs.Empty);
        // 触发清空事件
        private void OnClearRequested() => ClearRequested?.Invoke(this, EventArgs.Empty);
        // 获取状态轴事件
        private void OnGetStatusRequested() => GetStatusRequested?.Invoke(this, EventArgs.Empty);
        //实时监控事件
        private void OnStartMonitorRequested() => StartMonitorRequested?.Invoke(this, EventArgs.Empty);
        //停止监控事件
        private void OnStopMonitorRequested() => StopMonitorRequested?.Invoke(this, EventArgs.Empty);
        //打开工作流控事件
        private void OnRunWorkflowRequested() => RunWorkflowRequested?.Invoke(this, EventArgs.Empty);
        //切换模式事件
        private void OnToggleSimulatorRequested() => ToggleSimulatorRequested?.Invoke(this, EventArgs.Empty);

        public short SelectedAxis => (short)numAxis.Value;

        /// <summary>
        /// 供 Controller 调用的显示方法
        /// </summary>
        /// 专门记录用户点击按钮、选择下拉框等动作
        public void ShowResult(string message)
        {
            //txtResult.Text = message;
            string timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            txtOperationLog.AppendText($"[{timeStamp}] {message}" + Environment.NewLine);
            // 写入文件（类别：Operation）
            FileLogger.Log(message, "INFO", "Operation");
        }
        // 专门记录硬件数据、网络心跳、状态变化等
        public void AppendMonitorLog(string message)
        {
            string timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            txtMonitorLog.AppendText($"[{timeStamp}] {message}" + Environment.NewLine);
            // 写入文件（类别：Monitor）
            FileLogger.Log(message, "INFO", "Monitor");
        }

        /// <summary>
        /// 清空显示
        /// </summary>
        public void ClearResult()
        {
            txtOperationLog.Clear();
            txtMonitorLog.Clear();
        }

        // 可选：显示错误消息框（也可由 Controller 直接调用 MessageBox）
        public void ShowError(string message, string type)
        {
            MessageBox.Show(message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            // 错误既属于操作类，也记录到文件，类别可定为 Operation 或 General
            FileLogger.Log(message, "ERROR", type);
        }

        public void SetSimulationModeUI(bool isSimulation)
        {
            // 安全地更新 UI（无需 Invoke，因为该方法会在主线程被调用）
            btnToggleSimulator.Text = isSimulation ? "切换到真实" : "切换到模拟";
            btnToggleSimulator.BackColor = isSimulation ? Color.LightGreen : Color.LightGray;
        }

        // 扫描 Workflows 目录，填充下拉框
        private void LoadWorkflowList()
        {
            string workflowsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Workflows");
            if (!Directory.Exists(workflowsDir))
            {
                Directory.CreateDirectory(workflowsDir);
                // 可以在这里写一个默认的 JSON 示例文件，或提示用户添加
            }

            var files = Directory.GetFiles(workflowsDir, "*.json");
            cmbWorkflow.Items.Clear();
            foreach (var file in files)
            {
                cmbWorkflow.Items.Add(Path.GetFileNameWithoutExtension(file));
            }
            if (cmbWorkflow.Items.Count > 0)
                cmbWorkflow.SelectedIndex = 0;
        }



        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // 触发停止监控事件，让 Controller 去取消线程
            StopMonitorRequested?.Invoke(this, EventArgs.Empty);
            base.OnFormClosing(e);
        }

    }
}
