using System;
using System.IO.Ports;
using System.Windows.Forms;
using System.Xml.XPath;
using GtsTest.Modbus;

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
        public event EventHandler ToggleModbusRequested;             //连接/断开modbus设备
        public event EventHandler<ModbusConfig> ModbusConfigChanged;
        public event EventHandler<WriteRegisterEventArgs>? WriteRegisterRequested;
        public event EventHandler<WriteCoilEventArgs>? WriteCoilRequested;

        public Form1()
        {
            InitializeComponent();
            cmbWriteDataType.SelectedIndex = 0;      
            cmbCoilValue.SelectedIndex = 0;      // 默认选中 "ON (1)"
            cmbByteOrder.SelectedIndex = 0;      // 默认选中 "Big Endian"
            //初始化UI
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
            btnToggleModbus.Click += (s, e) => OnToggleModbusRequested();       //打开modbustcp设备
            LoadWorkflowList();                                                 //获取配置文件

            //btnApplyModbus.Click += (s, e) => {
            //    string ip = txtModbusIp.Text.Trim();
            //    int port = (int)numModbusPort.Value;
            //    // 触发事件，Controller会去执行真正的重连
            //    ModbusReconnectRequested?.Invoke(this, new ModbusReconnectArgs(ip, port));
            //};
            // 在构造函数或 Load 事件中为 btnSteModbus 绑定事件
            btnSteModbus.Click += (s, e) =>
            {
                using (var configForm = new ModbusConfigForm(_currentModbusConfig))
                {
                    if (configForm.ShowDialog() == DialogResult.OK)
                    {
                        if (configForm.IsChanged)
                        {
                            // 更新本地缓存
                            _currentModbusConfig = configForm.Config;
                            // 通知 Controller 配置已变更
                            ModbusConfigChanged?.Invoke(this, configForm.Config);

                            if (_isModbusConnected)
                            {
                                var result = MessageBox.Show(
                                    "Modbus 通讯参数已更改，需要断开重连才能生效。是否立即重连？",
                                    "配置变更",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question);

                                if (result == DialogResult.Yes)
                                {
                                    ShowResult("✅ Modbus 配置已更新，正在“连接 Modbus”应用新配置。");
                                    OnToggleModbusRequested(); // 触发连接事件
                                }
                            }
                            else
                            {
                                ShowResult("✅ Modbus 配置已保存，点击“连接 Modbus”应用新配置。");
                            }
                        }
                    }
                }
            };

            btnExportMonitor.Click += (s, e) =>
            {
                string? path = CyclicMonitorBuffer.DumpToFile("手动导出");
                if (path != null)
                    MessageBox.Show($"监控数据已导出至:\n{path}", "导出成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show("缓冲区为空，无需导出", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            btnWriteRegister.Click += BtnWriteRegister_Click;
            btnWriteCoil.Click += BtnWriteCoil_Click;
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
        //触发Modbus开启/断开事件
        private void OnToggleModbusRequested() => ToggleModbusRequested?.Invoke(this, EventArgs.Empty);
        
        public short SelectedAxis => (short)numAxis.Value;

        private ModbusConfig _currentModbusConfig = new ModbusConfig();

        private bool _isModbusConnected = false;


        /// <summary>
        /// 供 Controller 调用的显示方法
        /// </summary>
        /// 专门记录用户点击按钮、选择下拉框等动作
        public void ShowResult(string message)
        {
            txtOperationLog.AppendText( message + Environment.NewLine);
        }
        // 专门记录硬件数据、网络心跳、状态变化等
        public void AppendMonitorLog(string message)
        {
            txtMonitorLog.AppendText( message + Environment.NewLine);
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

        // 供 Controller 调用的 UI 更新方法（更新指示灯和状态栏） 
        public void UpdateModbusStatus(bool connected, ModbusConfig config, string? errorMsg = null)
        {
            _isModbusConnected = connected;
            if (connected)
            {
                string connectionInfo = config.Protocol == ModbusProtocol.Tcp
                    ? $"TCP {config.IpAddress}:{config.Port}"
                    : $"RTU {config.PortName} ({config.BaudRate}bps)";
                lblModbusStatus.Text = $"Modbus 已连接 ({connectionInfo})";
                lblModbusStatus.BackColor = Color.LimeGreen;
            }
            else
            {
                lblModbusStatus.Text = "Modbus 已断开";
                lblModbusStatus.BackColor = Color.Red;
            }

            //切换按钮状态 
            btnToggleModbus.Text = connected ? "断开 Modbus" : "连接 Modbus";
            btnToggleModbus.BackColor = connected ? Color.LightCoral : Color.LightGreen;
        }

        private void BtnWriteRegister_Click(object? sender, EventArgs e)
        {
            // 读取 UI 数据
            ushort address = (ushort)numWriteAddress.Value;
            DataType dataType = (DataType)cmbWriteDataType.SelectedIndex;
            ByteOrder byteOrder = (ByteOrder)cmbByteOrder.SelectedIndex;

            // 解析 txtWriteValues 中的值（支持逗号分隔多个）
            string[] parts = txtWriteValues.Text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            List<object> values = new List<object>();
            foreach (string part in parts)
            {
                string trimmed = part.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;

                // 根据数据类型转换
                try
                {
                    object converted = dataType switch
                    {
                        DataType.Int16 => Convert.ToInt16(trimmed),
                        DataType.UInt16 => Convert.ToUInt16(trimmed),
                        DataType.Int32 => Convert.ToInt32(trimmed),
                        DataType.UInt32 => Convert.ToUInt32(trimmed),
                        DataType.Float => Convert.ToSingle(trimmed),
                        DataType.Double => Convert.ToDouble(trimmed),
                        _ => throw new NotSupportedException($"不支持的类型: {dataType}")
                    };
                    values.Add(converted);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"值 '{trimmed}' 无法转换为 {dataType}: {ex.Message}", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            if (values.Count == 0)
            {
                MessageBox.Show("请输入至少一个有效数值", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 触发事件
            WriteRegisterRequested?.Invoke(this, new WriteRegisterEventArgs(address, dataType, byteOrder, values.ToArray()));
        }

        private void BtnWriteCoil_Click(object? sender, EventArgs e)
        {
            ushort address = (ushort)numCoilAddress.Value;
            bool value = cmbCoilValue.SelectedIndex == 0; // ON(1) -> true, OFF(0) -> false
            WriteCoilRequested?.Invoke(this, new WriteCoilEventArgs(address, value));
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // 触发停止监控事件，让 Controller 去取消线程
            StopMonitorRequested?.Invoke(this, EventArgs.Empty);
            base.OnFormClosing(e);
        }

    }
}
