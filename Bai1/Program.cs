using Serilog;
using SoChiTieu.Helpers;

namespace SoChiTieu;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        AppConfig config;
        try
        {
            config = AppConfig.Load();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không đọc được appsettings.json:\n" + ex.Message, "Lỗi cấu hình",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Ghi log vào thư mục Logs cạnh file .exe, mỗi ngày một file: log-yyyyMMdd.txt
        var logPath = Path.Combine(AppContext.BaseDirectory, config.LogFolder, "log-.txt");
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day, shared: true)
            .CreateLogger();

        try
        {
            Log.Information("Ứng dụng khởi động");
            Application.Run(new MainForm(config));
        }
        finally
        {
            Log.Information("Ứng dụng đóng");
            Log.CloseAndFlush();
        }
    }
}
