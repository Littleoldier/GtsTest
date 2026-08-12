using System;

namespace GtsTest.Modbus
{
    /// <summary>
    /// 写线圈事件参数（用于 UI -> Controller 传递数据）
    /// </summary>
    public class WriteCoilEventArgs : EventArgs
    {
        public ushort Address { get; }
        public bool Value { get; }

        public WriteCoilEventArgs(ushort address, bool value)
        {
            Address = address;
            Value = value;
        }
    }
}