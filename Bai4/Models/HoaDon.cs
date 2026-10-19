namespace QuanTraSua;

public class HoaDon
{
    public int MaHD { get; set; }
    public DateTime NgayLap { get; set; }
    public decimal TongTien { get; set; }
    public string HinhThucTT { get; set; }   // "Tiền mặt" / "Chuyển khoản"
    public bool DaThanhToan { get; set; }
    public List<ChiTietHoaDon> ChiTietHoaDons { get; set; } = new();
}
