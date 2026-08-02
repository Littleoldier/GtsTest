namespace GtsTest
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            //ApplicationConfiguration.Initialize();
            //Application.Run(new Form1());

            // 初始化日志系统（放在最前面，确保任何日志输出前已准备好）
            FileLogger.Initialize();

            // ==== 启用模拟模式 ====
            GtsModel.UseSimulation = true;   // 设置为 false 则使用真实硬件

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 创建 Model、View、Controller
            GtsModel model = new GtsModel();
            Form1 view = new Form1();
            GtsController controller = new GtsController(model, view);

            // 启动 View
            Application.Run(view);
        }
    }
}