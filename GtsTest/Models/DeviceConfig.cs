using GtsTest.Modbus;

namespace GtsTest.Models
{
    /// <summary>
    /// 设备配置（每个从机独立）
    /// </summary>
    public class DeviceConfig
    {
        public string DeviceId { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "设备1";
        public bool Enabled { get; set; } = true;
        public ModbusConfig Modbus { get; set; } = new ModbusConfig();
        public string WorkflowName { get; set; } = "";
        public short Axis { get; set; } = 1;
        public int TargetCount { get; set; } = 1000;
        public int CurrentCount { get; set; } = 0;
        public int SignalStartAddress { get; set; } = 100;

        // 工作流参数
        public int WorkPosition { get; set; } = 1000;
        public int HomePosition { get; set; } = 0;
        public double MoveSpeed { get; set; } = 20.0;
        public double MoveAcc { get; set; } = 10.0;
        public int CycleDelayMs { get; set; } = 500;
    }
}