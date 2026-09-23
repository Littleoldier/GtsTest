using GtsTest.Core;
using GtsTest.Data;
using GtsTest.Services.Data;
using GtsTest.Services.Authentication;
using GtsTest.Services.Mes;
using System;
using System.Windows.Forms;

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
                    AppLogger.GlobalLogLevel = LogLevel.Info;   // 先给默认值，防止 Load 之前有日志被吞
                    GtsModel.UseSimulation = true;

                    // ⭐ 启动时应用日志配置（appsettings.json 的 Logging 段）
                    try
                    {
                        var logCfg = LoggingConfig.Load();
                        logCfg.Apply();
                    }
                    catch (Exception ex)
                    {
                        AppLogger.Warn($"应用日志配置失败，使用默认 Info: {ex.Message}", "Program");
                    }

                    // ⭐ 初始化数据库
                    DbContextFactory.ConfigureFromAppSettings();
                    DbContextFactory.EnsureDatabase();

                    // ⭐ 初始化 MES 服务
                    MesConfig mesConfig = MesConfig.Load();
                    MesReportService mesService = new MesReportService(mesConfig);
                    AppLogger.Info($"MES 服务已创建: Protocol={mesConfig.Protocol}, Url={(mesConfig.Protocol == "SOAP" ? mesConfig.SoapEndpoint : mesConfig.ApiUrl)}", "Program");

                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);

                    var repo = new EfDataRepository();
                    var authService = new AuthenticationService(repo);

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
                        AppLogger.Info("管理员账号已存在，无需创建", "Auth");
                    }

                    Application.Run(new Form1(repo, authService, mesService));
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