using GtsTest.Core;
using System;

namespace GtsTest.Commands
{
    public static class CommandFactory
    {
        public static IMotionCommand Create(GtsModel model, DeviceManager deviceManager, CommandConfig config)
        {
            return config.Type switch
            {
                "Home" => new HomeCommand(model, (short)config.Axis, config.HomePos),
                "MoveAbs" => new MoveAbsCommand(model, (short)config.Axis, config.TargetPos, config.Vel, config.Acc),
                "WaitIO" => new WaitIOCommand(model, config.IoIndex, config.ExpectValue),
                "Delay" => new DelayCommand(model, config.DelayMs),

                // 跨设备信号命令
                "WriteSignal" => new WriteSignalCommand(model, deviceManager, config.TargetDevice, config.SignalAddress, config.SignalValue),
                "WaitSignal" => new WaitSignalCommand(model, deviceManager, config.TargetDevice, config.SignalAddress, config.ExpectValue),

                // 🆕 视觉触发命令
                "TriggerVision" => new TriggerVisionCommand(model, deviceManager, config),

                // 默认：未知命令类型
                _ => throw new NotSupportedException($"未知命令类型: {config.Type}")
            };
        }
    }
}