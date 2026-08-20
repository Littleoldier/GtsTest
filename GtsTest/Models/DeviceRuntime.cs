using GtsTest.Core;
using GtsTest.Modbus;

namespace GtsTest.Models
{
    /// <summary>
    /// 设备运行时状态
    /// </summary>
    public class DeviceRuntime
    {
        public DeviceConfig Config { get; set; } = null!;
        public ModbusClient ModbusClient { get; set; } = null!;
        public CancellationTokenSource? WorkflowCts { get; set; }
        public Task? WorkflowTask { get; set; }
        public bool IsRunning { get; set; }
        public string CurrentStep { get; set; } = "空闲";
        public DateTime LastUpdate { get; set; }
        public object? LastModbusData { get; set; }
        public bool IsOnline { get; set; }
        public string LastError { get; set; } = "";
        public Watchdog Watchdog { get; set; } = new Watchdog(timeoutMs: 5000, checkIntervalMs: 200);
        public HashSet<int> TriggeredSignalCounts { get; set; } = new HashSet<int>();
    }
}