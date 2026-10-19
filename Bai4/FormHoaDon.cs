using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace QuanTraSua;

public class FormHoaDon : Form
{
    static readonly CultureInfo VN = new("vi-VN");

    DateTimePicker dtp = new();
    DataGridView gridHD = new(), gridCT = new();
    Label lblDoanhThu = new();

    public FormHoaDon()
    {
        BuildUi();
        LoadHoaDon();
    }

    void BuildUi()
    {
        Text = "Danh sách hóa đơn";
        ClientSize = new Size(820, 520);
        StartPosition = FormStartPosition.CenterParent;
        Font = new Font("Segoe UI", 9F);

        Controls.Add(new Label { Text = "Ngày:", Location = new Point(12, 16), AutoSize = true });
        dtp.Location = new Point(56, 12); dtp.Width = 130; dtp.Format = DateTimePickerFormat.Short;
        dtp.ValueChanged += (s, e) => LoadHoaDon();
        lblDoanhThu.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        lblDoanhThu.AutoSize = true; lblDoanhThu.Location = new Point(220, 12);

        gridHD.Location = new Point(12, 48); gridHD.Size = new Size(796, 230);
        Setup(gridHD);
        gridHD.Columns.Add(new DataGridViewTextBoxColumn { Name = "MaHD", HeaderText = "Mã HD", DataPropertyName = "MaHD", FillWeight = 50 });
        var cGio = new DataGridViewTextBoxColumn { HeaderText = "Giờ lập", DataPropertyName = "NgayLap", FillWeight = 100 };
        cGio.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";
        gridHD.Columns.Add(cGio);
        gridHD.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Hình thức", DataPropertyName = "HinhThucTT", FillWeight = 80 });
        var cTong = new DataGridViewTextBoxColumn { HeaderText = "Tổng tiền", DataPropertyName = "TongTien", FillWeight = 80 };
        cTong.DefaultCellStyle.Format = "N0"; cTong.DefaultCellStyle.FormatProvider = VN;
        gridHD.Columns.Add(cTong);
        gridHD.SelectionChanged += (s, e) => LoadChiTiet();

        Controls.Add(new Label { Text = "Chi tiết hóa đơn:", Location = new Point(12, 288), AutoSize = true });
        gridCT.Location = new Point(12, 310); gridCT.Size = new Size(796, 198);
        Setup(gridCT);
        gridCT.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tên món", DataPropertyName = "TenMon", FillWeight = 150 });
        gridCT.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "SL", DataPropertyName = "SoLy", FillWeight = 40 });
        var cDG = new DataGridViewTextBoxColumn { HeaderText = "Đơn giá", DataPropertyName = "DonGia", FillWeight = 70 };
        cDG.DefaultCellStyle.Format = "N0"; cDG.DefaultCellStyle.FormatProvider = VN;
        var cTT = new DataGridViewTextBoxColumn { HeaderText = "Thành tiền", DataPropertyName = "ThanhTien", FillWeight = 80 };
        cTT.DefaultCellStyle.Format = "N0"; cTT.DefaultCellStyle.FormatProvider = VN;
        gridCT.Columns.Add(cDG); gridCT.Columns.Add(cTT);

        Controls.AddRange(new Control[] { dtp, lblDoanhThu, gridHD, gridCT });
    }

    static void Setup(DataGridView g)
    {
        g.AutoGenerateColumns = false; g.ReadOnly = true;
        g.AllowUserToAddRows = false; g.AllowUserToDeleteRows = false;
        g.SelectionMode = DataGridViewSelectionMode.FullRowSelect; g.MultiSelect = false;
        g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        g.RowHeadersVisible = false;
    }

    void LoadHoaDon()
    {
        var d = dtp.Value.Date; var next = d.AddDays(1);
        using var db = new QL_TraSuaContext();
        var list = db.HoaDons.Where(h => h.NgayLap >= d && h.NgayLap < next)
            .OrderByDescending(h => h.NgayLap)
            .Select(h => new { h.MaHD, h.NgayLap, h.HinhThucTT, h.TongTien, h.DaThanhToan })
            .ToList();
        gridHD.DataSource = list;
        var doanhThu = list.Where(h => h.DaThanhToan).Sum(h => h.TongTien);
        lblDoanhThu.Text = $"Doanh thu ngày: {doanhThu.ToString("N0", VN)} đ";
        LoadChiTiet();
    }

    void LoadChiTiet()
    {
        if (gridHD.CurrentRow?.Cells["MaHD"].Value is not int id) { gridCT.DataSource = null; return; }
        using var db = new QL_TraSuaContext();
        gridCT.DataSource = db.ChiTietHoaDons.Where(c => c.MaHD == id)
            .Select(c => new { c.MonUong.TenMon, c.SoLy, c.DonGia, ThanhTien = c.SoLy * c.DonGia })
            .ToList();
    }
}
