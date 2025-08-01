namespace R_Factory_Tools.Utilities
{
    public class ErrorLogger
    {
        public static void Write(Exception exception)
        {
            DateTime now = DateTime.Now;

            string logsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            string monthDirectory = Path.Combine(logsDirectory, now.ToString("yyyy-MM"));

            Directory.CreateDirectory(monthDirectory);

            string logFilePath = Path.Combine(monthDirectory, now.ToString("dd") + ".txt");

            string logMessage = $"[{now}] Error: {exception.Message}\n{exception.StackTrace}\n\n";

            File.AppendAllText(logFilePath, logMessage);
        }
    }
}