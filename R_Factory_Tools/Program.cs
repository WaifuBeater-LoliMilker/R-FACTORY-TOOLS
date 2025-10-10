using R_Factory_Tools.Utilities;

namespace R_Factory_Tools
{
    internal static class Program
    {
        private static readonly Mutex mutex = new(true, "RFactoryTools");

        [STAThread]
        private static void Main()
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
                ApplicationConfiguration.Initialize();
                Application.Run(new FormMain());
                mutex.ReleaseMutex();
            }
            else
            {
                MessageBox.Show("Hệ thống vẫn đang chạy.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Environment.Exit(0);
            }
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            MessageBox.Show($"Đã có lỗi xảy ra: {e.Exception.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            ErrorLogger.Write(e.Exception);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                MessageBox.Show($"Đã có lỗi xảy ra: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ErrorLogger.Write(ex);
            }
        }
    }
}