using Microsoft.Extensions.Configuration;

namespace QuanLyNhaTro.Helpers;

// Cấu hình kiểu strongly-typed
public class ConnectionStringsSection
{
    public string DefaultConnection { get; set; } = "";
}

public class AppConfig
{
    public ConnectionStringsSection ConnectionStrings { get; set; } = new();
    public string LogFolder { get; set; } = "Logs";

    public static AppConfig Load()
    {
        // Chỉ đọc appsettings.json (+ appsettings.Development.json nếu có và đang đặt môi trường Development)
        var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
        if (string.Equals(env, "Development", StringComparison.OrdinalIgnoreCase))
            builder.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false);
        var config = builder.Build();

        var result = new AppConfig();
        config.GetSection("ConnectionStrings").Bind(result.ConnectionStrings);
        var logFolder = config["AppSettings:LogFolder"];
        if (!string.IsNullOrWhiteSpace(logFolder)) result.LogFolder = logFolder;
        return result;
    }
}
