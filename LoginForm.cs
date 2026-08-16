using GtsTest.Services;
using System;
using System.Windows.Forms;

namespace GtsTest
{
    public partial class LoginForm : Form
    {
        private readonly IAuthenticationService _authService;
        private readonly IDataRepository _repo;

        public LoginForm(IAuthenticationService authService, IDataRepository repo)
        {
            InitializeComponent();
            _authService = authService;
            _repo = repo;
            this.btnLogin.Click += BtnLogin_Click;
            this.btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
        }

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            var user = await _authService.AuthenticateAsync(txtUser.Text.Trim(), txtPass.Text.Trim());
            if (user != null)
            {
                SessionManager.Login(user, _repo);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("用户名或密码错误，或账号已禁用", "登录失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}