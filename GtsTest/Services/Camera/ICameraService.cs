using System;
using System.Drawing;
using System.Threading.Tasks;

namespace GtsTest.Services.Camera
{
    /// <summary>
    /// 相机服务事件参数
    /// </summary>
    public class CameraImageEventArgs : EventArgs
    {
        public Bitmap Image { get; }
        public DateTime Timestamp { get; }
        public bool IsTriggered { get; }

        public CameraImageEventArgs(Bitmap image, bool isTriggered = false)
        {
            Image = image;
            Timestamp = DateTime.Now;
            IsTriggered = isTriggered;
        }
    }

    /// <summary>
    /// 相机服务接口
    /// </summary>
    public interface ICameraService : IDisposable
    {
        // ---------- 事件 ----------
        event EventHandler<CameraImageEventArgs> ImageCaptured;
        event EventHandler<bool> ConnectionStateChanged;

        // ---------- 属性 ----------
        bool IsConnected { get; }
        bool IsGrabbing { get; }
        string CameraName { get; }

        // ---------- 相机管理 ----------
        /// <summary> 获取当前系统可用的相机列表 </summary>
        List<string> GetCameraList();

        // ---------- 方法 ----------
        Task<bool> ConnectAsync(string cameraName = null);
        void Disconnect();
        Task TriggerAsync();
        void StartGrabbing();
        void StopGrabbing();
    }
}