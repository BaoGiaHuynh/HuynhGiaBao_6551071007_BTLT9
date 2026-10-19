using Serilog;

namespace QuanTraSua;

static class Program
{
    [STAThread]
    static void Main()
    {
        var logDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "QuanTraSua", "logs");
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(Path.Combine(logDir, "log-.txt"), rollingInterval: RollingInterval.Day)
            .CreateLogger();

        try
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new FormBanHang());
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Ung dung gap loi nghiem trong");
            MessageBox.Show("Ứng dụng gặp lỗi: " + ex.Message + "\nKiểm tra chuỗi kết nối trong appsettings.json.",
                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
