using System;

namespace GtsTest.Modbus
{
    /// <summary>
    /// 写寄存器事件参数（用于 UI -> Controller 传递数据）
    /// </summary>
    public class WriteRegisterEventArgs : EventArgs
    {
        public ushort Address { get; }
        public DataType DataType { get; }
        public ByteOrder ByteOrder { get; }
        public object[] Values { get; }

        public WriteRegisterEventArgs(ushort address, DataType dataType, ByteOrder byteOrder, object[] values)
        {
            Address = address;
            DataType = dataType;
            ByteOrder = byteOrder;
            Values = values;
        }
    }
}