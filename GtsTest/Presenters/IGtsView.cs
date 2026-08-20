using GtsTest.Modbus;
using GtsTest.Services;
using GtsTest.Services.Data;
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

        // ---------- 报警 ----------
        void UpdateAlarmList(IEnumerable<AlarmRecord> alarms);
        string GetSelectedAlarmId();

        // ---------- 通用 ----------
        void ShowMessage(string text, string caption, MessageType type);
        bool ShowConfirm(string text, string caption);

        // ---------- 视图事件（由Presenter订阅） ----------
        event EventHandler LoadView;
        event EventHandler AddDeviceClicked;
        event EventHandler RemoveDeviceClicked;
        event EventHandler StartAllClicked;
        event EventHandler StopAllClicked;
        event EventHandler EmergencyStopClicked;
        event EventHandler AlarmResetClicked;
        event EventHandler RunWorkflowClicked;
        event EventHandler StopWorkflowClicked;
        event EventHandler ToggleSimulatorClicked;
        event EventHandler ToggleModbusClicked;
        event EventHandler ConnectAllModbusClicked;
        event EventHandler DisconnectAllModbusClicked;
        event EventHandler SaveConfigClicked;
        event EventHandler AlarmAcknowledgeClicked;
        event EventHandler AlarmResolveClicked;
    }

    public enum MessageType { Info, Warning, Error, Question }

    public class DeviceListItem
    {
        public string DeviceId { get; set; }
        public string Name { get; set; }
        public bool IsOnline { get; set; }
    }
}