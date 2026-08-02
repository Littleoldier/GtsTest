using System;

namespace GtsTest.Commands
{
    public class DelayCommand : MotionCommandBase
    {
        private readonly int _delayMs;

        public DelayCommand(GtsModel model, int delayMs) : base(model)
        {
            Name = $"延时 {delayMs} ms";
            _delayMs = delayMs;
        }

        protected override void ExecuteCore(CancellationToken ct)
        {
            // 直接利用 WaitForCondition 的等待机制
            WaitForCondition(() => false, _delayMs, ct);
        }
    }
}