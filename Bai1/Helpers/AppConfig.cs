using Microsoft.Extensions.Configuration;

namespace SoChiTieu.Helpers;

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
        var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";

        // appsettings.Development.json (đã .gitignore) chứa chuỗi kết nối thật, ghi đè appsettings.json
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
            .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: false)
            .Build();

        var result = new AppConfig();
        config.GetSection("ConnectionStrings").Bind(result.ConnectionStrings);
        var logFolder = config["AppSettings:LogFolder"];
        if (!string.IsNullOrWhiteSpace(logFolder)) result.LogFolder = logFolder;
        return result;
    }
}
