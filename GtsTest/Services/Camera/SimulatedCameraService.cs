using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace GtsTest.Services.Camera
{
    /// <summary>
    /// 模拟相机服务（用于测试和演示）
    /// </summary>
    public class SimulatedCameraService : ICameraService
    {
        private readonly Random _random = new Random();
        private CancellationTokenSource _cts;
        private Task _grabTask;
        private bool _isConnected;
        private bool _isGrabbing;

        public event EventHandler<CameraImageEventArgs> ImageCaptured;
        public event EventHandler<bool> ConnectionStateChanged;

        public bool IsConnected => _isConnected;
        public bool IsGrabbing => _isGrabbing;
        public string CameraName { get; private set; } = "模拟相机";

        public async Task<bool> ConnectAsync(string cameraName = null)
        {
            if (_isConnected) return true;

            CameraName = cameraName ?? "模拟相机";
            await Task.Delay(200);
            _isConnected = true;
            ConnectionStateChanged?.Invoke(this, true);

            // ✅ 连接成功后自动开始实时预览
            StartGrabbing();

            return true;
        }

        public List<string> GetCameraList()
        {
            // 模拟返回两个相机名称
            return new List<string> { "模拟相机1 (模拟)", "模拟相机2 (模拟)" };
        }

        public void Disconnect()
        {
            StopGrabbing();
            _isConnected = false;
            ConnectionStateChanged?.Invoke(this, false);
        }

        public async Task TriggerAsync()
        {
            if (!_isConnected) return;

            var image = await Task.Run(() => GenerateSimulatedImage());
            ImageCaptured?.Invoke(this, new CameraImageEventArgs(image, isTriggered: true));
        }

        public void StartGrabbing()
        {
            if (_isGrabbing || !_isConnected) return;

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
            while (!ct.IsCancellationRequested && _isConnected)
            {
                Thread.Sleep(100);
                var image = GenerateSimulatedImage();
                ImageCaptured?.Invoke(this, new CameraImageEventArgs(image, isTriggered: false));
            }
        }

        // ---------- 模拟图像生成 ----------
        private Bitmap GenerateSimulatedImage()
        {
            int width = 640, height = 480;
            var bmp = new Bitmap(width, height);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(60, 70, 80));

                for (int i = 0; i < 8; i++)
                {
                    int x = _random.Next(0, width);
                    int y = _random.Next(0, height);
                    int size = _random.Next(20, 80);
                    var color = Color.FromArgb(_random.Next(100, 255), _random.Next(100, 255), _random.Next(100, 255));
                    using (var brush = new SolidBrush(color))
                    {
                        if (_random.Next(2) == 0)
                            g.FillEllipse(brush, x, y, size, size);
                        else
                            g.FillRectangle(brush, x, y, size, size);
                    }
                }

                using (var pen = new Pen(Color.FromArgb(60, 255, 255, 255), 2))
                {
                    g.DrawLine(pen, width / 2 - 50, height / 2, width / 2 + 50, height / 2);
                    g.DrawLine(pen, width / 2, height / 2 - 50, width / 2, height / 2 + 50);
                }

                using (var font = new Font("Consolas", 10F))
                using (var brush = new SolidBrush(Color.FromArgb(200, 0, 255, 0)))
                {
                    g.DrawString(DateTime.Now.ToString("HH:mm:ss.fff"), font, brush, new PointF(10, 10));
                }

                using (var pen = new Pen(Color.FromArgb(150, 0, 255, 0), 2))
                {
                    g.DrawRectangle(pen, 100, 80, 440, 320);
                }
            }
            return bmp;
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}