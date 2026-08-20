using GtsTest.Presenters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GtsTest.Controls
{
    public partial class CameraControl : UserControl, ICameraView
    {
        // ---------- 实现 ICameraView 事件 ----------
        public event Action<string> ConnectClicked;   // 带相机名称参数
        public event Action TriggerClicked;
        public event Action ContinuousToggled;
        public event Action SaveImageClicked;

        // ---------- UI 控件 ----------
        private ComboBox cmbCameras;
        private Button btnConnect, btnTrigger, btnContinuous, btnSaveImage;
        private PictureBox picImage;
        private Label lblStatusDot, lblStatusText, lblInfo;
        private Panel panelStatus;

        public CameraControl()
        {
            InitializeComponent();
        }

        // ---------- UI 构建 ----------
        private void InitializeComponent()
        {
            this.SuspendLayout();

            // 顶部工具栏
            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 45, BackColor = Color.FromArgb(240, 240, 240), Padding = new Padding(10, 8, 10, 8) };
            this.cmbCameras = new ComboBox { Location = new Point(10, 8), Size = new Size(150, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            this.cmbCameras.Items.AddRange(new object[] { "请连接相机" }); // 占位
            this.cmbCameras.SelectedIndex = 0;

            this.btnConnect = new Button { Location = new Point(170, 6), Size = new Size(80, 30), Text = "连接", BackColor = Color.LightGreen };
            this.btnTrigger = new Button { Location = new Point(260, 6), Size = new Size(80, 30), Text = "📸 触发", Enabled = false };
            this.btnContinuous = new Button { Location = new Point(350, 6), Size = new Size(100, 30), Text = "▶ 连续采集", Enabled = false };
            this.btnSaveImage = new Button { Location = new Point(460, 6), Size = new Size(80, 30), Text = "💾 保存", Enabled = false };

            this.panelStatus = new Panel { Location = new Point(560, 8), Size = new Size(150, 26) };
            this.lblStatusDot = new Label { Location = new Point(0, 2), Size = new Size(20, 20), Text = "●", ForeColor = Color.Gray, Font = new Font("Segoe UI", 12F) };
            this.lblStatusText = new Label { Location = new Point(25, 4), Size = new Size(120, 20), Text = "未连接", Font = new Font("Segoe UI", 9F), TextAlign = ContentAlignment.MiddleLeft };
            this.panelStatus.Controls.Add(lblStatusDot);
            this.panelStatus.Controls.Add(lblStatusText);

            topPanel.Controls.Add(cmbCameras);
            topPanel.Controls.Add(btnConnect);
            topPanel.Controls.Add(btnTrigger);
            topPanel.Controls.Add(btnContinuous);
            topPanel.Controls.Add(btnSaveImage);
            topPanel.Controls.Add(panelStatus);

            // 图像显示区域
            this.picImage = new PictureBox { Dock = DockStyle.Fill, BackColor = Color.FromArgb(50, 50, 50), SizeMode = PictureBoxSizeMode.Zoom, BorderStyle = BorderStyle.FixedSingle };
            this.picImage.Paint += PicImage_Paint;

            // 底部信息栏
            Panel bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 30, BackColor = Color.FromArgb(240, 240, 240), Padding = new Padding(10, 4, 10, 4) };
            this.lblInfo = new Label { Dock = DockStyle.Fill, Text = "就绪 | 请连接相机", TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 9F), ForeColor = Color.DarkGray };
            bottomPanel.Controls.Add(lblInfo);

            this.Controls.Add(picImage);
            this.Controls.Add(topPanel);
            this.Controls.Add(bottomPanel);
            this.ResumeLayout(false);

            // ---------- 绑定 UI 事件（触发 View 事件） ----------
            this.btnConnect.Click += (s, e) =>
            {
                var selected = cmbCameras.SelectedItem?.ToString();
                ConnectClicked?.Invoke(selected);
            };
            this.btnTrigger.Click += (s, e) => TriggerClicked?.Invoke();
            this.btnContinuous.Click += (s, e) => ContinuousToggled?.Invoke();
            this.btnSaveImage.Click += (s, e) => SaveImageClicked?.Invoke();
        }

        // ---------- 占位绘图 ----------
        private void PicImage_Paint(object sender, PaintEventArgs e)
        {
            if (picImage.Image != null) return;
            Graphics g = e.Graphics;
            g.Clear(Color.FromArgb(50, 50, 50));
            using (Pen pen = new Pen(Color.FromArgb(100, 255, 255, 255), 2))
            using (Font font = new Font("Segoe UI", 18F, FontStyle.Bold))
            using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                g.DrawString("📷 相机画面", font, new SolidBrush(Color.FromArgb(150, 255, 255, 255)),
                    new Rectangle(0, 0, picImage.Width, picImage.Height), sf);
                g.DrawRectangle(pen, new Rectangle(10, 10, picImage.Width - 20, picImage.Height - 20));
            }
        }

        // ================================================================
        // 实现 ICameraView 接口
        // ================================================================

        public void SetCameraList(IEnumerable<string> cameraNames)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => SetCameraList(cameraNames)));
                return;
            }

            cmbCameras.Items.Clear();
            foreach (var name in cameraNames)
            {
                cmbCameras.Items.Add(name);
            }
            if (cmbCameras.Items.Count > 0)
                cmbCameras.SelectedIndex = 0;
        }

        public void UpdateImage(Bitmap image, bool isTriggered)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => UpdateImage(image, isTriggered)));
                return;
            }

            var old = picImage.Image;
            picImage.Image = image;
            old?.Dispose();
            picImage.Invalidate();

            if (isTriggered)
            {
                lblInfo.Text = $"触发拍照 | {DateTime.Now:HH:mm:ss} | 图像已捕获";
                lblInfo.ForeColor = Color.Green;
            }
            else
            {
                lblInfo.Text = $"连续采集 | {DateTime.Now:HH:mm:ss} | 帧率: 10 fps";
                lblInfo.ForeColor = Color.DarkGray;
            }
        }

        public void UpdateConnectionStatus(bool connected)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => UpdateConnectionStatus(connected)));
                return;
            }

            btnConnect.Text = connected ? "断开" : "连接";
            btnConnect.BackColor = connected ? Color.LightCoral : Color.LightGreen;
            btnTrigger.Enabled = connected;
            btnContinuous.Enabled = connected;
            btnSaveImage.Enabled = connected;
            lblStatusDot.ForeColor = connected ? Color.Green : Color.Gray;
            lblStatusText.Text = connected ? "已连接" : "未连接";

            if (!connected)
            {
                picImage.Image = null;
                picImage.Invalidate();
                lblInfo.Text = "就绪 | 未连接";
                lblInfo.ForeColor = Color.DarkGray;
            }
        }

        public void UpdateContinuousStatus(bool isGrabbing)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => UpdateContinuousStatus(isGrabbing)));
                return;
            }

            btnContinuous.Text = isGrabbing ? "⏹ 停止采集" : "▶ 连续采集";
            btnContinuous.BackColor = isGrabbing ? Color.LightCoral : SystemColors.Control;
        }

        public void ShowSaveDialog(Bitmap image)
        {
            if (image == null && picImage.Image == null)
            {
                MessageBox.Show("没有图像可保存", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var img = image ?? picImage.Image;
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "PNG 图片|*.png|JPEG 图片|*.jpg";
                sfd.DefaultExt = "png";
                sfd.FileName = $"Camera_{DateTime.Now:yyyyMMdd_HHmmss}";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        img.Save(sfd.FileName);
                        MessageBox.Show($"图像已保存到 {sfd.FileName}", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"保存失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        public void ShowMessage(string text, string caption, MessageType type)
        {
            MessageBoxIcon icon = type switch
            {
                MessageType.Info => MessageBoxIcon.Information,
                MessageType.Warning => MessageBoxIcon.Warning,
                MessageType.Error => MessageBoxIcon.Error,
                _ => MessageBoxIcon.None
            };
            MessageBox.Show(text, caption, MessageBoxButtons.OK, icon);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                picImage?.Image?.Dispose();
                base.Dispose(disposing);
            }
        }
    }
}