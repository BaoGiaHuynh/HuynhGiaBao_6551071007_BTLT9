namespace QuanLyNhaTro.Models;

// Giống lớp do "dotnet ef dbcontext scaffold" sinh ra từ bảng PhongTro
public class PhongTro
{
    public int MaPhong { get; set; }
    public string TenPhong { get; set; } = "";
    public decimal GiaThue { get; set; }
    public string TinhTrang { get; set; } = "Trống";   // "Trống" | "Đang thuê"

    // Navigation property: 1 phòng - nhiều khách
    public virtual ICollection<KhachThue> KhachThues { get; set; } = new List<KhachThue>();
}
