using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SoChiTieu.Data;
using SoChiTieu.Helpers;
using SoChiTieu.Models;

namespace SoChiTieu;

// Toàn bộ giao diện được dựng bằng code (không cần kéo thả trong Designer)
public class MainForm : Form
{
    private static readonly string[] DanhSachLoai = { "Ăn uống", "Đi lại", "Học tập", "Giải trí", "Khác" };
    private static readonly CultureInfo Vi = new("vi-VN");

    private readonly AppConfig _cfg;

    // Nhập liệu
    private readonly DateTimePicker dtpNgay = new();
    private readonly TextBox txtNoiDung = new();
    private readonly ComboBox cboLoai = new();
    private readonly NumericUpDown nudSoTien = new();
    private readonly TextBox txtGhiChu = new();

    // Tìm kiếm / lọc
    private readonly TextBox txtTim = new();
    private readonly ComboBox cboLocLoai = new();
    private readonly DateTimePicker dtpTu = new();
    private readonly DateTimePicker dtpDen = new();

    // Nút, lưới, tổng
    private readonly Button btnThem = new() { Text = "Thêm" };
    private readonly Button btnSua = new() { Text = "Sửa" };
    private readonly Button btnXoa = new() { Text = "Xóa" };
    private readonly Button btnLamMoi = new() { Text = "Làm mới" };
    private readonly Button btnTim = new() { Text = "Tìm" };
    private readonly DataGridView dgv = new();
    private readonly Label lblTong = new();

    private int _selectedId;
    private bool _binding;

    public MainForm(AppConfig cfg)
    {
        _cfg = cfg;
        BuildUi();
        Load += (_, _) => LoadData();
    }

    // ---------------------------------------------------------------- UI
    private void BuildUi()
    {
        Text = "Sổ chi tiêu cá nhân";
        ClientSize = new Size(900, 640);
        MinimumSize = new Size(900, 640);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9F);

        // --- Nhóm thông tin khoản chi
        var gbInfo = new GroupBox { Text = "Thông tin khoản chi", Location = new Point(12, 10), Size = new Size(530, 185) };
        AddLabel(gbInfo, "Ngày chi", 12, 30);
        dtpNgay.SetBounds(80, 26, 140, 25);
        dtpNgay.Format = DateTimePickerFormat.Custom;
        dtpNgay.CustomFormat = "dd/MM/yyyy";
        dtpNgay.MaxDate = DateTime.Today;

        AddLabel(gbInfo, "Loại chi", 240, 30);
        cboLoai.SetBounds(305, 26, 210, 25);
        cboLoai.DropDownStyle = ComboBoxStyle.DropDownList;
        cboLoai.Items.AddRange(DanhSachLoai);
        cboLoai.SelectedIndex = 0;

        AddLabel(gbInfo, "Nội dung", 12, 66);
        txtNoiDung.SetBounds(80, 62, 140, 25);
        txtNoiDung.MaxLength = 100;

        AddLabel(gbInfo, "Số tiền", 240, 66);
        nudSoTien.SetBounds(305, 62, 210, 25);
        nudSoTien.Minimum = 0;
        nudSoTien.Maximum = 999_999_999_999m;
        nudSoTien.Increment = 1000;
        nudSoTien.DecimalPlaces = 0;
        nudSoTien.ThousandsSeparator = true;
        nudSoTien.TextAlign = HorizontalAlignment.Right;

        AddLabel(gbInfo, "Ghi chú", 12, 102);
        txtGhiChu.SetBounds(80, 98, 435, 70);
        txtGhiChu.Multiline = true;
        txtGhiChu.MaxLength = 200;
        txtGhiChu.ScrollBars = ScrollBars.Vertical;

        gbInfo.Controls.AddRange(new Control[] { dtpNgay, cboLoai, txtNoiDung, nudSoTien, txtGhiChu });

        // --- Nhóm tìm kiếm
        var gbTim = new GroupBox { Text = "Tìm kiếm", Location = new Point(552, 10), Size = new Size(336, 185) };
        AddLabel(gbTim, "Nội dung", 12, 30);
        txtTim.SetBounds(80, 26, 240, 25);
        txtTim.PlaceholderText = "Nhập nội dung...";

        AddLabel(gbTim, "Loại chi", 12, 66);
        cboLocLoai.SetBounds(80, 62, 240, 25);
        cboLocLoai.DropDownStyle = ComboBoxStyle.DropDownList;
        cboLocLoai.Items.Add("Tất cả");
        cboLocLoai.Items.AddRange(DanhSachLoai);
        cboLocLoai.SelectedIndex = 0;

        AddLabel(gbTim, "Từ ngày", 12, 102);
        dtpTu.SetBounds(80, 98, 240, 25);
        dtpTu.Format = DateTimePickerFormat.Custom;
        dtpTu.CustomFormat = "dd/MM/yyyy";
        dtpTu.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        AddLabel(gbTim, "Đến ngày", 12, 138);
        dtpDen.SetBounds(80, 134, 160, 25);
        dtpDen.Format = DateTimePickerFormat.Custom;
        dtpDen.CustomFormat = "dd/MM/yyyy";
        dtpDen.Value = DateTime.Today;

        btnTim.SetBounds(250, 132, 70, 28);
        gbTim.Controls.AddRange(new Control[] { txtTim, cboLocLoai, dtpTu, dtpDen, btnTim });

        // --- Nút chức năng
        int x = 12;
        foreach (var b in new[] { btnThem, btnSua, btnXoa, btnLamMoi })
        {
            b.SetBounds(x, 205, 90, 30);
            x += 100;
        }

        // --- Lưới
        dgv.SetBounds(12, 245, 876, 340);
        dgv.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        dgv.AutoGenerateColumns = false;
        dgv.AllowUserToAddRows = false;
        dgv.AllowUserToDeleteRows = false;
        dgv.ReadOnly = true;
        dgv.MultiSelect = false;
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgv.RowHeadersVisible = true;
        dgv.BackgroundColor = Color.White;
        dgv.ColumnHeadersDefaultCellStyle.Font = new Font(Font, FontStyle.Bold);
        dgv.EnableHeadersVisualStyles = false;
        dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.WhiteSmoke;
        dgv.Columns.Add(MakeCol("Mã", nameof(ChiTieu.MaCT), 60, "000"));
        dgv.Columns.Add(MakeCol("Ngày chi", nameof(ChiTieu.NgayChi), 100, "dd/MM/yyyy"));
        dgv.Columns.Add(MakeCol("Nội dung", nameof(ChiTieu.NoiDung), 200));
        dgv.Columns.Add(MakeCol("Loại chi", nameof(ChiTieu.LoaiChi), 100));
        dgv.Columns.Add(MakeCol("Số tiền", nameof(ChiTieu.SoTien), 110, "N0", DataGridViewContentAlignment.MiddleRight));
        var colGhiChu = MakeCol("Ghi chú", nameof(ChiTieu.GhiChu), 100);
        colGhiChu.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        dgv.Columns.Add(colGhiChu);

        // --- Tổng chi
        lblTong.AutoSize = false;
        lblTong.TextAlign = ContentAlignment.MiddleRight;
        lblTong.SetBounds(488, 595, 400, 32);
        lblTong.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        lblTong.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        lblTong.ForeColor = Color.Firebrick;
        lblTong.Text = "Tổng chi: 0 đ";

        Controls.AddRange(new Control[] { gbInfo, gbTim, btnThem, btnSua, btnXoa, btnLamMoi, dgv, lblTong });

        // --- Sự kiện
        btnThem.Click += (_, _) => Them();
        btnSua.Click += (_, _) => Sua();
        btnXoa.Click += (_, _) => Xoa();
        btnLamMoi.Click += (_, _) => LamMoi();
        btnTim.Click += (_, _) => LoadData();
        txtTim.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; LoadData(); }
        };
        cboLocLoai.SelectedIndexChanged += (_, _) => { if (IsHandleCreated) LoadData(); };
        dgv.SelectionChanged += (_, _) => DocDongDangChon();
    }

    private static void AddLabel(Control parent, string text, int x, int y)
    {
        parent.Controls.Add(new Label { Text = text, Location = new Point(x, y), AutoSize = true });
    }

    private static DataGridViewTextBoxColumn MakeCol(string header, string prop, int width,
        string format = null, DataGridViewContentAlignment? align = null)
    {
        var c = new DataGridViewTextBoxColumn
        {
            HeaderText = header,
            DataPropertyName = prop,
            Width = width,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        if (format != null) c.DefaultCellStyle.Format = format;
        if (align.HasValue) c.DefaultCellStyle.Alignment = align.Value;
        return c;
    }

    // ---------------------------------------------------------------- Dữ liệu
    private QlChiTieuContext NewDb() => new(_cfg.ConnectionStrings.DefaultConnection);

    private void BaoLoiCsdl(Exception ex, string thaoTac)
    {
        Log.Error(ex, "Lỗi CSDL khi {ThaoTac}", thaoTac);
        MessageBox.Show(
            "Không thể truy cập cơ sở dữ liệu. Vui lòng kiểm tra SQL Server và chuỗi kết nối trong appsettings.json.",
            "Lỗi cơ sở dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    // Tải dữ liệu theo bộ lọc hiện tại (từ khóa, loại chi, khoảng ngày) và cập nhật Tổng chi
    private void LoadData()
    {
        try
        {
            using var db = NewDb();
            IQueryable<ChiTieu> q = db.ChiTieus.AsNoTracking();

            var tuKhoa = txtTim.Text.Trim();
            if (tuKhoa.Length > 0) q = q.Where(x => x.NoiDung.Contains(tuKhoa));

            if (cboLocLoai.SelectedIndex > 0)
            {
                var loai = cboLocLoai.SelectedItem.ToString();
                q = q.Where(x => x.LoaiChi == loai);
            }

            var tu = dtpTu.Value.Date;
            var den = dtpDen.Value.Date;
            q = q.Where(x => x.NgayChi >= tu && x.NgayChi <= den);

            var list = q.OrderByDescending(x => x.NgayChi).ThenByDescending(x => x.MaCT).ToList();

            _binding = true;
            dgv.DataSource = list;
            dgv.ClearSelection();
            _binding = false;

            lblTong.Text = "Tổng chi: " + list.Sum(x => x.SoTien).ToString("N0", Vi) + " đ";
        }
        catch (Exception ex)
        {
            _binding = false;
            BaoLoiCsdl(ex, "tải dữ liệu");
        }
    }

    private void DocDongDangChon()
    {
        if (_binding) return;
        if (dgv.CurrentRow?.DataBoundItem is not ChiTieu ct || !dgv.CurrentRow.Selected) return;

        _selectedId = ct.MaCT;
        dtpNgay.Value = ct.NgayChi.Date > DateTime.Today ? DateTime.Today : ct.NgayChi.Date;
        txtNoiDung.Text = ct.NoiDung;
        var idx = cboLoai.Items.IndexOf(ct.LoaiChi);
        cboLoai.SelectedIndex = idx >= 0 ? idx : 0;
        nudSoTien.Value = Math.Min(Math.Max(ct.SoTien, nudSoTien.Minimum), nudSoTien.Maximum);
        txtGhiChu.Text = ct.GhiChu ?? "";
    }

    private void XoaForm()
    {
        _selectedId = 0;
        dtpNgay.Value = DateTime.Today;
        txtNoiDung.Clear();
        cboLoai.SelectedIndex = 0;
        nudSoTien.Value = 0;
        txtGhiChu.Clear();
        txtNoiDung.Focus();
    }

    private bool KiemTraNhap()
    {
        if (string.IsNullOrWhiteSpace(txtNoiDung.Text))
        {
            MessageBox.Show("Nội dung không được để trống.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtNoiDung.Focus();
            return false;
        }
        if (nudSoTien.Value <= 0)
        {
            MessageBox.Show("Số tiền phải lớn hơn 0.", "Dữ liệu không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            nudSoTien.Focus();
            return false;
        }
        if (dtpNgay.Value.Date > DateTime.Today)
        {
            MessageBox.Show("Ngày chi không được lớn hơn hôm nay.", "Dữ liệu không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            dtpNgay.Focus();
            return false;
        }
        return true;
    }

    // ---------------------------------------------------------------- CRUD
    private void Them()
    {
        if (!KiemTraNhap()) return;
        try
        {
            using var db = NewDb();
            var ct = new ChiTieu
            {
                NgayChi = dtpNgay.Value.Date,
                NoiDung = txtNoiDung.Text.Trim(),
                LoaiChi = cboLoai.SelectedItem.ToString(),
                SoTien = nudSoTien.Value,
                GhiChu = string.IsNullOrWhiteSpace(txtGhiChu.Text) ? null : txtGhiChu.Text.Trim()
            };
            db.ChiTieus.Add(ct);
            db.SaveChanges();
            Log.Information("Thêm khoản chi MaCT={MaCT}, {NoiDung}, {LoaiChi}, {SoTien}", ct.MaCT, ct.NoiDung, ct.LoaiChi, ct.SoTien);
        }
        catch (Exception ex)
        {
            BaoLoiCsdl(ex, "thêm khoản chi");
            return;
        }

        // Nếu ngày vừa thêm nằm ngoài khoảng lọc thì mở rộng khoảng lọc để thấy dòng mới
        var ngay = dtpNgay.Value.Date;
        if (ngay < dtpTu.Value.Date) dtpTu.Value = ngay;
        if (ngay > dtpDen.Value.Date) dtpDen.Value = ngay;
        LoadData();
        XoaForm();
    }

    private void Sua()
    {
        if (_selectedId == 0)
        {
            MessageBox.Show("Hãy chọn một dòng trong danh sách để sửa.", "Chưa chọn dòng", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (!KiemTraNhap()) return;
        try
        {
            using var db = NewDb();
            var ct = db.ChiTieus.Find(_selectedId);
            if (ct == null)
            {
                MessageBox.Show("Khoản chi này không còn tồn tại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
                XoaForm();
                return;
            }
            ct.NgayChi = dtpNgay.Value.Date;
            ct.NoiDung = txtNoiDung.Text.Trim();
            ct.LoaiChi = cboLoai.SelectedItem.ToString();
            ct.SoTien = nudSoTien.Value;
            ct.GhiChu = string.IsNullOrWhiteSpace(txtGhiChu.Text) ? null : txtGhiChu.Text.Trim();
            db.SaveChanges();
            Log.Information("Sửa khoản chi MaCT={MaCT}, {NoiDung}, {LoaiChi}, {SoTien}", ct.MaCT, ct.NoiDung, ct.LoaiChi, ct.SoTien);
        }
        catch (Exception ex)
        {
            BaoLoiCsdl(ex, "sửa khoản chi");
            return;
        }

        var ngay = dtpNgay.Value.Date;
        if (ngay < dtpTu.Value.Date) dtpTu.Value = ngay;
        if (ngay > dtpDen.Value.Date) dtpDen.Value = ngay;
        LoadData();
        XoaForm();
    }

    private void Xoa()
    {
        if (_selectedId == 0)
        {
            MessageBox.Show("Hãy chọn một dòng trong danh sách để xóa.", "Chưa chọn dòng", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        var hoi = MessageBox.Show($"Bạn có chắc muốn xóa khoản chi \"{txtNoiDung.Text}\"?", "Xác nhận xóa",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (hoi != DialogResult.Yes) return;

        try
        {
            using var db = NewDb();
            var ct = db.ChiTieus.Find(_selectedId);
            if (ct != null)
            {
                db.ChiTieus.Remove(ct);
                db.SaveChanges();
                Log.Information("Xóa khoản chi MaCT={MaCT}, {NoiDung}, {SoTien}", ct.MaCT, ct.NoiDung, ct.SoTien);
            }
        }
        catch (Exception ex)
        {
            BaoLoiCsdl(ex, "xóa khoản chi");
            return;
        }
        LoadData();
        XoaForm();
    }

    private void LamMoi()
    {
        txtTim.Clear();
        cboLocLoai.SelectedIndex = 0;
        dtpTu.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        dtpDen.Value = DateTime.Today;
        XoaForm();
        LoadData();
    }
}
