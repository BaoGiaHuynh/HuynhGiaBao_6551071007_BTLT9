using System.Text.Json;

namespace QuanLyNhaTro.Services;

// Phần tử Tỉnh/Thành hoặc Phường/Xã. Tên thuộc tính viết thường để khớp
// DisplayMember = "name", ValueMember = "code" và khớp JSON của API.
public class AddressItem
{
    public string name { get; set; } = "";
    public int code { get; set; }
    public override string ToString() => name;
}

public class ProvinceDetail
{
    public List<AddressItem> wards { get; set; } = new();
}

// Gọi API hành chính Việt Nam: https://provinces.open-api.vn/api/v2/
public static class AddressService
{
    // Một HttpClient dùng chung, Timeout 10 giây
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(10) };
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };
    private const string BaseUrl = "https://provinces.open-api.vn/api/v2/";

    // Có thể ném HttpRequestException / TaskCanceledException (hết thời gian) / JsonException
    public static async Task<List<AddressItem>> GetProvincesAsync()
    {
        var text = await GetStringAsync(BaseUrl);
        return JsonSerializer.Deserialize<List<AddressItem>>(text, Json) ?? new List<AddressItem>();
    }

    public static async Task<List<AddressItem>> GetWardsAsync(int provinceCode)
    {
        var text = await GetStringAsync($"{BaseUrl}p/{provinceCode}?depth=2");
        var detail = JsonSerializer.Deserialize<ProvinceDetail>(text, Json);
        return detail?.wards ?? new List<AddressItem>();
    }

    private static async Task<string> GetStringAsync(string url)
    {
        using var resp = await Http.GetAsync(url);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadAsStringAsync();
    }
}
