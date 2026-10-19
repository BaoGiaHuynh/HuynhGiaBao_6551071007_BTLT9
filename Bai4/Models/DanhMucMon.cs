namespace QuanTraSua;

public class DanhMucMon
{
    public int MaDM { get; set; }
    public string TenDM { get; set; }
    public List<MonUong> MonUongs { get; set; } = new();
}
