using QuanTraSua.Helpers;

namespace QuanTraSua.Services;

public static class VietQrService
{
    // https://img.vietqr.io/image/{BankId}-{AccountNo}-{Template}.png?amount=..&addInfo=..&accountName=..
    public static string BuildUrl(PaymentSettings p, decimal amount, string noiDung)
    {
        var baseUrl = $"https://img.vietqr.io/image/{p.BankId}-{p.AccountNo}-{p.Template}.png";
        return $"{baseUrl}?amount={Uri.EscapeDataString(((long)amount).ToString())}" +
               $"&addInfo={Uri.EscapeDataString(noiDung)}" +
               $"&accountName={Uri.EscapeDataString(p.AccountName)}";
    }

    // Che số tài khoản khi ghi log: chỉ giữ 3 số cuối
    public static string Mask(string accountNo)
    {
        if (string.IsNullOrEmpty(accountNo)) return "";
        if (accountNo.Length <= 3) return new string('*', accountNo.Length);
        return new string('*', accountNo.Length - 3) + accountNo[^3..];
    }
}
