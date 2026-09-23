using GtsTest.Services.Data;
using System;
using System.Collections.Generic;

namespace GtsTest.Presenters
{
    /// <summary>
    /// 主窗体视图接口
    /// </summary>
    public interface IGtsView
    {
        // ---------- 设备列表 ----------
        void UpdateDeviceList(IEnumerable<DeviceListItem> items);
        void SelectDevice(string deviceId);
        string GetSelectedDeviceId();
        string GetSelectedDeviceName();
        string GetSelectedWorkflowName();

        // ---------- 设备状态 ----------
        void UpdateDeviceOnlineStatus(string deviceId, bool isOnline);
        void UpdateDeviceProduction(string deviceId, int current, int target);
        void UpdateDeviceStep(string deviceId, string step);
        void UpdateDeviceData(string deviceId, object data);

        // ---------- 统计 ----------
        void UpdateGlobalStats(int onlineCount, int totalCount, int totalProduction);

        // ---------- 状态栏 ----------
        void UpdateStatusBar(string deviceName, bool isOnline, bool servoOn,
                             string limitStatus, bool modbusConnected, string currentStep,
                             int watchdogRemainingMs, bool watchdogTimeout);

        // ---------- 报警 ----------
        void UpdateAlarmList(IEnumerable<AlarmRecord> alarms);
        string GetSelectedAlarmId();

        // ---------- 日志 ----------
        void AppendOperationLog(string message);
        void AppendMonitorLog(string message);
        void ClearLogs();

        // ---------- 生产执行日志 ----------
        void AppendExecutionLog(string message);

        // ---------- 模式 ----------
        void SetSimulationMode(bool isSimulation);

        // ---------- 权限 ----------
        void UpdateUIByPermissions(string role);

        // ---------- 通用 ----------
        void ShowMessage(string text, string caption, MessageType type);
        bool ShowConfirm(string text, string caption);

        // ---------- 视图事件 ----------
        event EventHandler LoadView;
        event EventHandler DeviceSelected;

        // ---- 设备控制 ----
        event EventHandler AddDeviceClicked;
        event EventHandler RemoveDeviceClicked;
        event EventHandler StartAllClicked;
        event EventHandler StopAllClicked;
        event EventHandler StartSelectedClicked;
        event EventHandler StopSelectedClicked;
        event EventHandler ResetDeviceClicked;

        // ---- 报警 ----
        event EventHandler AlarmResetClicked;
        event EventHandler AlarmAcknowledgeClicked;
        event EventHandler AlarmResolveClicked;

        // ---- 安全 ----
        event EventHandler EmergencyStopClicked;

        // ---- 系统 ----
        event EventHandler SystemConfigClicked;
        event EventHandler LoginClicked;

        // ---- 生产执行 ----
        event EventHandler<string> WorkflowRunClicked;
        event EventHandler WorkflowStopClicked;
        event EventHandler<string> DeviceForWorkflowSelected;
        event EventHandler ProductionResetClicked;
        event EventHandler BindDeviceWorkflowClicked;   // 
    }

    public enum MessageType { Info, Warning, Error, Question }

    public class DeviceListItem
    {
        public string DeviceId { get; set; } = "";
        public string Name { get; set; } = "";
        public bool IsOnline { get; set; }
    }
}