using GtsTest.Services;

namespace GtsTest
{
    internal static class Program
    {
        [STAThread]
        static void Main()
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

                Application.Run(new Form1(repo, authService));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"≥Ã–Ú∆Ù∂Ø ß∞‹: {ex.Message}\n\n{ex.StackTrace}", "∆Ù∂Ø¥ÌŒÛ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}