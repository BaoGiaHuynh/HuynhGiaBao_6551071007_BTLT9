using System.Text.Json;

namespace QuanTraSua.Helpers;

public class PaymentSettings
{
    public string BankId { get; set; } = "";
    public string AccountNo { get; set; } = "";
    public string AccountName { get; set; } = "";
    public string Template { get; set; } = "compact2";
}

public class AppConfig
{
    public string ConnectionString { get; set; } = "";
    public string ShopName { get; set; } = "Quán trà sữa";
    public PaymentSettings Payment { get; set; } = new();

    // Đọc lại file mỗi lần gọi => đổi tài khoản trong appsettings.json là QR mới dùng ngay, không cần sửa code
    public static AppConfig Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        var json = File.ReadAllText(path);
        var opt = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };
        return JsonSerializer.Deserialize<AppConfig>(json, opt) ?? new AppConfig();
    }
}
