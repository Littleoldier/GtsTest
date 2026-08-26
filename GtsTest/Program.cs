using GtsTest.Core;
using GtsTest.Services.Data;
using GtsTest.Services.Authentication;

namespace GtsTest
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                try
                {
                    AppLogger.Initialize();
                    AppLogger.GlobalLogLevel = LogLevel.Info;
                    GtsModel.UseSimulation = true;

                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);

                    var repo = new SqliteRepository();
                    var authService = new AuthenticationService(repo);

                    // ★★★ 强制创建默认管理员并显示密码 ★★★
                    var defaultPwd = authService.EnsureDefaultAdmin();
                    if (!string.IsNullOrEmpty(defaultPwd))
                    {
                        MessageBox.Show(
                            $"默认管理员账号已创建！\n\n用户名: admin\n密码: {defaultPwd}\n\n请妥善保管此密码，登录后立即修改。",
                            "⚠️ 初始管理员密码",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );
                        AppLogger.Info($"初始管理员密码: {defaultPwd}", "Auth");
                    }
                    else
                    {
                        // 如果返回 null，说明 admin 已存在，无需重复提示
                        AppLogger.Info("管理员账号已存在，无需创建", "Auth");
                    }

                    Application.Run(new Form1(repo, authService));
                }
                finally
                {
                    AppLogger.Shutdown();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"程序启动失败: {ex.Message}\n\n{ex.StackTrace}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}