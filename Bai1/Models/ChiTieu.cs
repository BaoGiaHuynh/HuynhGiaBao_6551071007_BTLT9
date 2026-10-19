namespace SoChiTieu.Models;

// Giống lớp mà "dotnet ef dbcontext scaffold" sinh ra từ bảng ChiTieu
public class ChiTieu
{
    public int MaCT { get; set; }
    public DateTime NgayChi { get; set; }
    public string NoiDung { get; set; } = "";
    public string LoaiChi { get; set; } = "";
    public decimal SoTien { get; set; }
    public string GhiChu { get; set; }
}
