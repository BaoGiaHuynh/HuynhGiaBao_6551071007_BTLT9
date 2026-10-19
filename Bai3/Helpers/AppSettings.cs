using System.Text.Json;

namespace KeHoachDaNgoai.Helpers;

public static class AppSettings
{
    public static string ConnectionString { get; }

    static AppSettings()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        ConnectionString = doc.RootElement.GetProperty("ConnectionString").GetString();
    }
}
