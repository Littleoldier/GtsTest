using System;

namespace GtsTest.Commands
{
    /// <summary>
    /// 运动指令接口
    /// </summary>
    public interface IMotionCommand
    {
        string Name { get; }
        bool IsCompleted { get; }
        bool IsFaulted { get; }
        string FaultReason { get; }
        event Action<string> OnLog; // 实时输出日志到界面

        void Execute(CancellationToken ct); // 执行核心逻辑
        void Stop(); // 急停或中断
    }
}