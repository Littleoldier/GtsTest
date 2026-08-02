namespace GtsTest.Commands
{
    /// <summary>
    /// 单个命令的配置（相当于一块积木的说明书）
    /// </summary>
    public class CommandConfig
    {
        public string Type { get; set; } = "";          // 命令类型: "Home", "MoveAbs", "WaitIO", "Delay"
        public int Axis { get; set; } = 1;              // 轴号（Home/MoveAbs 用）
        public int TargetPos { get; set; } = 0;         // 目标位置（MoveAbs 用）
        public int HomePos { get; set; } = 0;           // 回零位置（Home 用）
        public int IoIndex { get; set; } = 0;           // IO 索引（WaitIO 用）
        public bool ExpectValue { get; set; } = true;   // 期望 IO 值（WaitIO 用）
        public int DelayMs { get; set; } = 500;         // 延时毫秒（Delay 用）
        public double Vel { get; set; } = 10;           // 速度
        public double Acc { get; set; } = 5;            // 加速度
    }
}