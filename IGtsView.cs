using GtsTest.Modbus;
using GtsTest.Services;
using System;
using System.Collections.Generic;

namespace GtsTest.Presenters
{
    /// <summary>
    /// 主窗体视图接口，所有UI操作都通过此接口与Presenter交互
    /// </summary>
    public interface IGtsView
    {
        // ---------- 设备列表 ----------
        void UpdateDeviceList(IEnumerable<DeviceListItem> items);
        void SelectDevice(string deviceId);
        string GetSelectedDeviceId();

        // ---------- 实时数据 ----------
        void UpdateCurrentDevice(string deviceName);
        void UpdateDeviceData(string deviceId, object data);
        void UpdateDeviceStep(string deviceId, string step);
        void UpdateDeviceProduction(string deviceId, int current, int target);
        void UpdateDeviceOnlineStatus(string deviceId, bool isOnline);
        void UpdateAxisInfo(string deviceId, short axis, bool isOnline, double pos, double vel);

        // ---------- 全局统计 ----------
        void UpdateGlobalStats(int onlineCount, int totalCount, int totalProduction);

        // ---------- 日志 ----------
        void AppendOperationLog(string message);
        void AppendMonitorLog(string message);
        void ClearLogs();

        // ---------- 模式状态 ----------
        void SetSimulationMode(bool isSimulation);

        // ---------- 状态栏 ----------
        void UpdateStatusBar(string deviceName, bool isOnline, bool servoOn,
                             string limitStatus, bool modbusConnected, string currentStep,
                             int watchdogRemainingMs, bool watchdogTimeout);

        // ---------- 视图事件（由Presenter订阅） ----------
        event EventHandler LoadView;
        event EventHandler AddDeviceClicked;
        event EventHandler RemoveDeviceClicked;
        event EventHandler StartAllClicked;
        event EventHandler StopAllClicked;
        event EventHandler StartDeviceClicked;
        event EventHandler StopDeviceClicked;
        event EventHandler DeviceConfigClicked;
        event EventHandler ToggleSimulatorClicked;
        event EventHandler RunWorkflowClicked;
        event EventHandler StopWorkflowClicked;
        event EventHandler EmergencyStopClicked;
        event EventHandler SaveConfigClicked;
        event EventHandler ToggleModbusClicked;
        event EventHandler ConnectAllModbusClicked;
        event EventHandler DisconnectAllModbusClicked;
        event EventHandler HomeAxisClicked;
        event EventHandler MoveAbsClicked;
        event EventHandler JogPositiveClicked;
        event EventHandler JogNegativeClicked;
        event EventHandler StopAxisClicked;
        event EventHandler ServoOnClicked;
        event EventHandler ServoOffClicked;
        event EventHandler AlarmResetClicked;

        // 写寄存器/线圈事件（携带参数）
        event EventHandler<WriteRegisterEventArgs> WriteRegisterRequested;
        event EventHandler<WriteCoilEventArgs> WriteCoilRequested;

        event EventHandler AlarmAcknowledgeClicked;
        event EventHandler AlarmResolveClicked;
        void UpdateAlarmList(IEnumerable<AlarmRecord> alarms);
        string GetSelectedAlarmId();

        // 其他...
        void ShowMessage(string text, string caption, MessageType type);
        bool ShowConfirm(string text, string caption);
    }

    public enum MessageType { Info, Warning, Error, Question }

    public class DeviceListItem
    {
        public string DeviceId { get; set; }
        public string Name { get; set; }
        public bool IsOnline { get; set; }
    }

    // 事件参数类（与原来相同）
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