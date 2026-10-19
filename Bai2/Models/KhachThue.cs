namespace QuanLyNhaTro.Models;

// Giống lớp do "dotnet ef dbcontext scaffold" sinh ra từ bảng KhachThue
public class KhachThue
{
    public int MaKhach { get; set; }
    public string HoTen { get; set; } = "";
    public string GioiTinh { get; set; } = "Nam";
    public DateTime NgaySinh { get; set; }
    public string SoDienThoai { get; set; } = "";
    public int? MaTinh { get; set; }
    public string TenTinh { get; set; }
    public int? MaPhuongXa { get; set; }
    public string TenPhuongXa { get; set; }
    public bool DaDangKyTamTru { get; set; }
    public int MaPhong { get; set; }

    // Navigation property: mỗi khách thuộc 1 phòng
    public virtual PhongTro MaPhongNavigation { get; set; }
}
