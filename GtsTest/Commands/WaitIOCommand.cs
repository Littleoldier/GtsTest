using GtsTest.Core;
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
            var sw = System.Diagnostics.Stopwatch.StartNew();//用于测量从进入循环到当前时刻的耗时，以便判断是否超过 5 秒超时限制。
            while (!matched && sw.ElapsedMilliseconds < 5000) // 超时 5 秒
            {
                //作用：检查传入的取消令牌 CancellationToken ct 是否已被请求取消。
                //用途：如果外部（例如用户点击取消或上层逻辑）要求终止操作，
                //则此方法会抛出 OperationCanceledException，从而立即退出命令的执行，
                //避免无限等待。这是异步编程中常见的协作式取消机制。
                ct.ThrowIfCancellationRequested();

                //作用：通过 _model 对象读取指定索引 _ioIndex 的数字输入（DI）的当前布尔值（true 或 false）。
                //用途：获取该 IO 点的实时状态，用于和期望值 _expectValue 进行比较。
                bool current = _model.ReadDI(_ioIndex);

                //作用：将读取到的当前值 current 与期望值 _expectValue 比较，若相等则 matched 为 true，否则为 false。
                //用途：决定是否满足等待条件，若满足则退出循环（循环条件!matched 不成立）。
                matched = (current == _expectValue);
                Thread.Sleep(50); // IO 响应慢，50ms 查一次即可
            }

            //位置：这段代码在 while 循环之后执行。
            //作用：如果循环结束后 matched 仍为 false，说明在 5 秒内始终没有读到期望的 IO 值，
            //于是抛出 TimeoutException 异常，通知上层调用者等待失败。
            if (!matched) throw new TimeoutException($"等待 IO[{_ioIndex}] 超时 (5s)");

        }
    }
}