using Serilog;
using QuanLyNhaTro.Helpers;

namespace QuanLyNhaTro;

internal static class Program
{
    private const string ThongBaoLoi = "Đã xảy ra lỗi, thông tin đã được ghi lại.";

    [STAThread]
    static void Main()
    {
        // Phải gọi trước khi tạo bất kỳ form nào
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

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

        // Log vào thư mục Logs cạnh file .exe, mỗi ngày một file log-yyyyMMdd.txt
        var logPath = Path.Combine(AppContext.BaseDirectory, config.LogFolder, "log-.txt");
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day, shared: true)
            .CreateLogger();

        // Task 7: đăng ký bắt lỗi toàn cục
        Application.ThreadException += (_, e) =>
        {
            Log.Error(e.Exception, "Lỗi chưa bắt (UI thread)");
            MessageBox.Show(ThongBaoLoi, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        };
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            Log.Fatal(e.ExceptionObject as Exception, "Lỗi chưa bắt (AppDomain), IsTerminating={Terminating}", e.IsTerminating);
            Log.CloseAndFlush();
            MessageBox.Show(ThongBaoLoi, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        };
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            Log.Error(e.Exception, "Lỗi Task không được quan sát");
            e.SetObserved();
        };

        ApplicationConfiguration.Initialize();

        try
        {
            Log.Information("Ứng dụng khởi động");
            Application.Run(new FormKhachThue(config));
        }
        finally
        {
            Log.Information("Ứng dụng đóng");
            Log.CloseAndFlush();
        }
    }
}
