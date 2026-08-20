using Emgu.CV;
using Emgu.CV.CvEnum;
using GtsTest.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace GtsTest.Services.Camera
{
    /// <summary>
    /// 基于 Emgu.CV 4.13.0 的真实相机服务（自动选择后端）
    /// </summary>
    public class RealCameraService : ICameraService
    {
        private VideoCapture _capture;
        private CancellationTokenSource _cts;
        private Task _grabTask;
        private bool _isConnected;
        private bool _isGrabbing;
        private readonly object _lock = new object();

        public event EventHandler<CameraImageEventArgs> ImageCaptured;
        public event EventHandler<bool> ConnectionStateChanged;

        public bool IsConnected => _isConnected;
        public bool IsGrabbing => _isGrabbing;
        public string CameraName { get; private set; } = "";

        /// <summary>
        /// 枚举系统中可用的相机（通过尝试打开每个索引）
        /// </summary>
        public List<string> GetCameraList()
        {
            var list = new List<string>();
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    // 使用默认后端（Windows 下为 DirectShow）
                    using (var cap = new VideoCapture(i))
                    {
                        if (cap.IsOpened)
                        {
                            // 测试读取一帧以验证可用性
                            using (var mat = new Mat())
                            {
                                cap.Read(mat);
                                if (!mat.IsEmpty)
                                    list.Add($"相机{i}");
                                else
                                    list.Add($"相机{i} (未响应)");
                            }
                        }
                    }
                }
                catch
                {
                    // 忽略无法打开的索引
                }
            }
            if (list.Count == 0)
                list.Add("未检测到相机");
            return list;
        }

        public async Task<bool> ConnectAsync(string cameraName = null)
        {
            if (_isConnected) return true;

            int index = 0;
            if (!string.IsNullOrEmpty(cameraName))
            {
                var parts = cameraName.Split(' ');
                if (parts.Length > 0 && int.TryParse(parts[0].Replace("相机", ""), out int idx))
                    index = idx;
            }

            try
            {
                // 使用默认后端创建 VideoCapture
                _capture = new VideoCapture(index);
                if (!_capture.IsOpened)
                {
                    _capture?.Dispose();
                    _capture = null;
                    return false;
                }

                // 设置分辨率（可选）
                _capture.Set(CapProp.FrameWidth, 640);
                _capture.Set(CapProp.FrameHeight, 480);

                CameraName = cameraName ?? $"相机{index}";
                _isConnected = true;
                ConnectionStateChanged?.Invoke(this, true);

                // 连接成功后自动开始采集
                StartGrabbing();
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.Error($"相机连接失败: {ex.Message}", "Camera");
                return false;
            }
        }

        public void Disconnect()
        {
            StopGrabbing();
            lock (_lock)
            {
                _capture?.Dispose();
                _capture = null;
            }
            _isConnected = false;
            ConnectionStateChanged?.Invoke(this, false);
        }

        public async Task TriggerAsync()
        {
            if (!_isConnected || _capture == null) return;

            try
            {
                var frame = await Task.Run(() =>
                {
                    lock (_lock)
                    {
                        if (_capture == null || !_capture.IsOpened) return null;
                        using (var mat = new Mat())
                        {
                            _capture.Read(mat);
                            if (mat.IsEmpty) return null;
                            return mat.ToBitmap();
                        }
                    }
                });
                if (frame != null)
                    ImageCaptured?.Invoke(this, new CameraImageEventArgs(frame, isTriggered: true));
            }
            catch (Exception ex)
            {
                AppLogger.Error($"触发拍照异常: {ex.Message}", "Camera");
            }
        }

        public void StartGrabbing()
        {
            if (_isGrabbing || !_isConnected || _capture == null) return;

            _isGrabbing = true;
            _cts = new CancellationTokenSource();
            _grabTask = Task.Run(() => GrabLoop(_cts.Token));
        }

        public void StopGrabbing()
        {
            _isGrabbing = false;
            _cts?.Cancel();
            try { _grabTask?.Wait(500); } catch { }
            _cts?.Dispose();
            _cts = null;
        }

        private void GrabLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && _isConnected && _capture != null)
            {
                try
                {
                    lock (_lock)
                    {
                        if (_capture == null || !_capture.IsOpened) break;
                        using (var mat = new Mat())
                        {
                            _capture.Read(mat);
                            if (!mat.IsEmpty)
                            {
                                var bitmap = mat.ToBitmap();
                                ImageCaptured?.Invoke(this, new CameraImageEventArgs(bitmap, isTriggered: false));
                            }
                        }
                    }
                    Thread.Sleep(33); // ~30fps
                }
                catch (Exception ex)
                {
                    AppLogger.Warn($"采集异常: {ex.Message}", "Camera");
                    Thread.Sleep(100);
                }
            }
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}