using GtsTest.Services.Camera;
using System;
using System.Linq;

namespace GtsTest.Presenters
{
    public class CameraPresenter : IDisposable
    {
        private readonly ICameraView _view;
        private readonly ICameraService _service;
        private bool _disposed;

        public CameraPresenter(ICameraView view, ICameraService service)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _service = service ?? throw new ArgumentNullException(nameof(service));

            // 订阅 View 事件
            _view.ConnectClicked += OnConnect;
            _view.TriggerClicked += OnTrigger;
            _view.ContinuousToggled += OnContinuous;
            _view.SaveImageClicked += OnSaveImage;

            // 订阅 Service 事件
            _service.ImageCaptured += OnImageCaptured;
            _service.ConnectionStateChanged += OnConnectionChanged;

            // 初始化相机列表
            var list = _service.GetCameraList();
            _view.SetCameraList(list);

            // 初始化界面状态
            _view.UpdateConnectionStatus(false);
        }

        private async void OnConnect(string cameraName)
        {
            if (string.IsNullOrEmpty(cameraName))
            {
                _view.ShowMessage("请选择一个相机", "提示", MessageType.Warning);
                return;
            }

            if (_service.IsConnected)
            {
                _service.Disconnect();
                return;
            }

            _view.ShowMessage($"正在连接 {cameraName}...", "提示", MessageType.Info);
            bool success = await _service.ConnectAsync(cameraName);
            if (!success)
            {
                _view.ShowMessage("相机连接失败，请检查设备", "错误", MessageType.Error);
            }
        }

        private async void OnTrigger()
        {
            if (!_service.IsConnected)
            {
                _view.ShowMessage("请先连接相机", "提示", MessageType.Warning);
                return;
            }

            await _service.TriggerAsync();
        }

        private void OnContinuous()
        {
            if (!_service.IsConnected)
            {
                _view.ShowMessage("请先连接相机", "提示", MessageType.Warning);
                return;
            }

            if (_service.IsGrabbing)
            {
                _service.StopGrabbing();
            }
            else
            {
                _service.StartGrabbing();
            }

            _view.UpdateContinuousStatus(_service.IsGrabbing);
        }

        private void OnSaveImage()
        {
            // View 自行处理保存对话框
            _view.ShowSaveDialog(null);
        }

        private void OnImageCaptured(object sender, CameraImageEventArgs e)
        {
            _view.UpdateImage(e.Image, e.IsTriggered);
        }

        private void OnConnectionChanged(object sender, bool connected)
        {
            _view.UpdateConnectionStatus(connected);
            if (connected)
            {
                // 连接成功后服务会自动开始采集，更新按钮状态
                _view.UpdateContinuousStatus(true);
            }
            else
            {
                _view.UpdateContinuousStatus(false);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _view.ConnectClicked -= OnConnect;
            _view.TriggerClicked -= OnTrigger;
            _view.ContinuousToggled -= OnContinuous;
            _view.SaveImageClicked -= OnSaveImage;
            _service.ImageCaptured -= OnImageCaptured;
            _service.ConnectionStateChanged -= OnConnectionChanged;
            _service.Dispose();
        }
    }
}