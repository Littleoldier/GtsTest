using System;

namespace GtsTest.Commands
{
    public static class CommandFactory
    {
        /// <summary>
        /// 根据配置创建对应的命令实例
        /// </summary>
        public static IMotionCommand Create(GtsModel model, CommandConfig config)
        {
            return config.Type switch
            {
                "Home" => new HomeCommand(model, (short)config.Axis, config.HomePos),
                "MoveAbs" => new MoveAbsCommand(model, (short)config.Axis, config.TargetPos, config.Vel, config.Acc),
                "WaitIO" => new WaitIOCommand(model, config.IoIndex, config.ExpectValue),
                "Delay" => new DelayCommand(model, config.DelayMs),
                _ => throw new NotSupportedException($"未知命令类型: {config.Type}")
            };
        }
    }
}