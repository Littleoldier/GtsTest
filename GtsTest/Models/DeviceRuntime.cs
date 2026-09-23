using GtsTest.Commands;
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
        public Watchdog Watchdog { get; set; } = new Watchdog(timeoutMs: 6000, checkIntervalMs: 200);
        public HashSet<int> TriggeredSignalCounts { get; set; } = new HashSet<int>();

        // ========== 🆕 工作流断点恢复相关字段 ==========
        public WorkflowConfig? CurrentWorkflow { get; set; }          // 缓存当前加载的工作流
        public string? CurrentWorkflowName { get; set; }               // 当前工作流文件名（不含扩展名）
        public IMotionCommand? CurrentCommand { get; set; }          // 当前正在执行的顶级命令（SequenceCommand）
        public int CurrentStepIndex { get; set; } = 0;               // 当前正在执行的命令索引（工作流中的位置）
        public bool IsPaused { get; set; } = false;                 // 是否处于暂停状态（等待恢复）
        public DateTime LastPauseTime { get; set; }                 // 上次暂停时间.
        /// <summary>
        /// 绑定的工作流名称（用于“全部启动”时使用）
        /// </summary>
        public string? BoundWorkflowName { get; set; }
        public Dictionary<string, object> WorkflowContext { get; set; } = new Dictionary<string, object>(); // 上下文数据
        // ========== 🆕 新增：用于取消 Modbus 事件订阅的委托引用 ==========
        public EventHandler<ModbusConnectionEventArgs>? ConnectionStateChangedHandler { get; set; }
    }
}