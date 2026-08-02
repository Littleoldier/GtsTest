using System;

namespace GtsTest.Commands
{
    public class WaitIOCommand : MotionCommandBase
    {
        private readonly int _ioIndex;
        private readonly bool _expectValue;

        public WaitIOCommand(GtsModel model, int ioIndex, bool expectValue) : base(model)
        {
            Name = $"等待 IO[{ioIndex}] == {expectValue}";
            _ioIndex = ioIndex;
            _expectValue = expectValue;
        }

        protected override void ExecuteCore(CancellationToken ct)
        {
            bool matched = false;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (!matched && sw.ElapsedMilliseconds < 5000) // 超时 5 秒
            {
                ct.ThrowIfCancellationRequested();
                bool current = _model.ReadDI(_ioIndex);
                matched = (current == _expectValue);
                Thread.Sleep(50); // IO 响应慢，50ms 查一次即可
            }
            if (!matched) throw new TimeoutException($"等待 IO[{_ioIndex}] 超时 (5s)");
        }
    }
}