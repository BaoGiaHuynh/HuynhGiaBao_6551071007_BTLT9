using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Serilog;
using QuanLyNhaTro.Data;
using QuanLyNhaTro.Helpers;
using QuanLyNhaTro.Models;
using QuanLyNhaTro.Services;

namespace QuanLyNhaTro;

// Phần tử hiển thị trong ComboBox phòng
public class RoomItem
{
    public int MaPhong { get; set; }
    public string Text { get; set; } = "";
    public override string ToString() => Text;
}

// Một dòng của DataGridView (kết quả JOIN KhachThue + PhongTro)
public class KhachRow
{
    public int MaKhach { get; set; }
    public string HoTen { get; set; } = "";
    public string SoDienThoai { get; set; } = "";
    public string TenPhong { get; set; } = "";
    public decimal GiaThue { get; set; }
    public string QueQuan { get; set; } = "";
    public bool DaDangKyTamTru { get; set; }
}

// Toàn bộ giao diện được dựng bằng code (không cần kéo thả trong Designer)
public class FormKhachThue : Form
{
    private const string TrangThaiTrong = "Trống";
    private const string TrangThaiDangThue = "Đang thuê";
    private static readonly CultureInfo Vi = new("vi-VN");

    private readonly AppConfig _cfg;

    // Thông tin khách thuê
    private readonly TextBox txtHoTen = new();
    private readonly GroupBox gbGioiTinh = new();
    private readonly RadioButton rdoNam = new() { Text = "Nam", Checked = true };
    private readonly RadioButton rdoNu = new() { Text = "Nữ" };
    private readonly DateTimePicker dtpNgaySinh = new();
    private readonly TextBox txtSDT = new();
    private readonly ComboBox cboPhong = new();
    private readonly ComboBox cboTinh = new();
    private readonly ComboBox cboPhuongXa = new();
    private readonly Button btnTaiLai = new() { Text = "↻" };
    private readonly CheckBox chkTamTru = new() { Text = "Đã đăng ký tạm trú" };

    // Nút chức năng
    private readonly Button btnThem = new() { Text = "Thêm" };
    private readonly Button btnSua = new() { Text = "Sửa" };
    private readonly Button btnXoa = new() { Text = "Xóa" };
    private readonly Button btnLamMoi = new() { Text = "Làm mới" };
    private readonly Button btnGayLoi = new() { Text = "Gây lỗi thử" };

    // Tìm kiếm / lọc
    private readonly TextBox txtTim = new();
    private readonly ComboBox cboLocPhong = new();
    private readonly ComboBox cboLocTinh = new();
    private readonly Button btnTim = new() { Text = "Tìm" };
    private readonly Button btnBoLoc = new() { Text = "Bỏ lọc" };

    private readonly DataGridView dgv = new();
    private readonly StatusStrip status = new();
    private readonly ToolStripStatusLabel lblStatus = new();

    private List<AddressItem> _provinces = new();
    private bool _provincesLoaded;
    private bool _suppress;          // chặn SelectedIndexChanged khi gán dữ liệu bằng code
    private bool _loadingFilters;
    private bool _binding;
    private bool _daBaoLoiMang;      // chỉ hiện hộp thoại lỗi mạng một lần cho đến khi tải lại thành công
    private int _wardVersion;        // bỏ qua kết quả phường/xã của yêu cầu cũ
    private int _selectedId;

    public FormKhachThue(AppConfig cfg)
    {
        _cfg = cfg;
        BuildUi();
        Load += async (_, _) => await OnLoadAsync();
    }

    // ------------------------------------------------------------ UI
    private void BuildUi()
    {
        Text = "Quản lý nhà trọ - Khách thuê";
        ClientSize = new Size(1040, 560);
        MinimumSize = new Size(1040, 560);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9F);

        // --- Khung thông tin khách thuê
        var gb = new GroupBox { Text = "Thông tin khách thuê", Location = new Point(12, 8), Size = new Size(350, 410) };

        AddLabel(gb, "Họ tên", 12, 32);
        txtHoTen.SetBounds(110, 28, 225, 25);
        txtHoTen.MaxLength = 100;

        AddLabel(gb, "Giới tính", 12, 68);
        gbGioiTinh.SetBounds(110, 52, 225, 40);
        rdoNam.SetBounds(15, 14, 80, 22);
        rdoNu.SetBounds(110, 14, 80, 22);
        gbGioiTinh.Controls.AddRange(new Control[] { rdoNam, rdoNu });

        AddLabel(gb, "Ngày sinh", 12, 108);
        dtpNgaySinh.SetBounds(110, 104, 225, 25);
        dtpNgaySinh.Format = DateTimePickerFormat.Custom;
        dtpNgaySinh.CustomFormat = "dd/MM/yyyy";
        dtpNgaySinh.MinDate = new DateTime(1900, 1, 1);
        dtpNgaySinh.MaxDate = DateTime.Today;
        dtpNgaySinh.Value = DateTime.Today.AddYears(-20);

        AddLabel(gb, "Số điện thoại", 12, 144);
        txtSDT.SetBounds(110, 140, 225, 25);
        txtSDT.MaxLength = 15;

        AddLabel(gb, "Phòng", 12, 180);
        cboPhong.SetBounds(110, 176, 225, 25);
        cboPhong.DropDownStyle = ComboBoxStyle.DropDownList;

        AddLabel(gb, "Tỉnh/Thành", 12, 216);
        cboTinh.SetBounds(110, 212, 190, 25);
        cboTinh.DropDownStyle = ComboBoxStyle.DropDownList;
        btnTaiLai.SetBounds(305, 211, 30, 27);

        AddLabel(gb, "Phường/Xã", 12, 252);
        cboPhuongXa.SetBounds(110, 248, 225, 25);
        cboPhuongXa.DropDownStyle = ComboBoxStyle.DropDownList;

        chkTamTru.SetBounds(110, 284, 225, 24);
        chkTamTru.AutoSize = false;

        int x = 12;
        foreach (var b in new[] { btnThem, btnSua, btnXoa, btnLamMoi })
        {
            b.SetBounds(x, 330, 75, 30);
            gb.Controls.Add(b);
            x += 81;
        }
        btnGayLoi.SetBounds(12, 368, 110, 28);

        gb.Controls.AddRange(new Control[] { txtHoTen, gbGioiTinh, dtpNgaySinh, txtSDT, cboPhong,
            cboTinh, btnTaiLai, cboPhuongXa, chkTamTru, btnGayLoi });

        // --- Khung tìm kiếm / lọc
        txtTim.SetBounds(374, 12, 280, 25);
        txtTim.PlaceholderText = "Tìm theo họ tên hoặc SĐT";
        btnTim.SetBounds(664, 10, 70, 28);
        btnBoLoc.SetBounds(742, 10, 80, 28);

        var lblLocPhong = new Label { Text = "Phòng:", Location = new Point(374, 50), AutoSize = true };
        cboLocPhong.SetBounds(425, 46, 160, 25);
        cboLocPhong.DropDownStyle = ComboBoxStyle.DropDownList;
        var lblLocTinh = new Label { Text = "Tỉnh/Thành:", Location = new Point(600, 50), AutoSize = true };
        cboLocTinh.SetBounds(680, 46, 220, 25);
        cboLocTinh.DropDownStyle = ComboBoxStyle.DropDownList;

        // --- Lưới
        dgv.SetBounds(374, 82, 654, 440);
        dgv.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        dgv.AutoGenerateColumns = false;
        dgv.AllowUserToAddRows = false;
        dgv.AllowUserToDeleteRows = false;
        dgv.ReadOnly = true;
        dgv.MultiSelect = false;
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgv.BackgroundColor = Color.White;
        dgv.EnableHeadersVisualStyles = false;
        dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.WhiteSmoke;
        dgv.ColumnHeadersDefaultCellStyle.Font = new Font(Font, FontStyle.Bold);
        dgv.Columns.Add(MakeCol("Họ tên", nameof(KhachRow.HoTen), 150));
        dgv.Columns.Add(MakeCol("SĐT", nameof(KhachRow.SoDienThoai), 95));
        dgv.Columns.Add(MakeCol("Phòng", nameof(KhachRow.TenPhong), 60));
        var colGia = MakeCol("Giá thuê", nameof(KhachRow.GiaThue), 100);
        colGia.DefaultCellStyle.Format = "#,##0 \"đ\"";
        colGia.DefaultCellStyle.FormatProvider = Vi;
        colGia.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        dgv.Columns.Add(colGia);
        var colQue = MakeCol("Quê quán", nameof(KhachRow.QueQuan), 150);
        colQue.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        dgv.Columns.Add(colQue);
        dgv.Columns.Add(new DataGridViewCheckBoxColumn
        {
            HeaderText = "Tạm trú",
            DataPropertyName = nameof(KhachRow.DaDangKyTamTru),
            Width = 65,
            SortMode = DataGridViewColumnSortMode.NotSortable
        });

        // --- Thanh trạng thái
        status.Items.Add(lblStatus);
        status.Dock = DockStyle.Bottom;
        lblStatus.Text = "Sẵn sàng";

        Controls.AddRange(new Control[] { gb, txtTim, btnTim, btnBoLoc, lblLocPhong, cboLocPhong,
            lblLocTinh, cboLocTinh, dgv, status });

        // --- Sự kiện
        btnThem.Click += (_, _) => Them();
        btnSua.Click += (_, _) => Sua();
        btnXoa.Click += (_, _) => Xoa();
        btnLamMoi.Click += (_, _) => LamMoi();
        btnGayLoi.Click += (_, _) =>
            throw new InvalidOperationException("Lỗi thử nghiệm: người dùng bấm nút 'Gây lỗi thử'");
        btnTim.Click += (_, _) => LoadGrid();
        btnBoLoc.Click += (_, _) => BoLoc();
        txtTim.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; LoadGrid(); }
        };
        cboLocPhong.SelectedIndexChanged += (_, _) => { if (!_loadingFilters) LoadGrid(); };
        cboLocTinh.SelectedIndexChanged += (_, _) => { if (!_loadingFilters) LoadGrid(); };
        cboTinh.SelectedIndexChanged += CboTinh_SelectedIndexChanged;
        btnTaiLai.Click += async (_, _) => await LoadProvincesAsync();
        dgv.SelectionChanged += Dgv_SelectionChanged;
    }

    private static void AddLabel(Control parent, string text, int x, int y) =>
        parent.Controls.Add(new Label { Text = text, Location = new Point(x, y), AutoSize = true });

    private static DataGridViewTextBoxColumn MakeCol(string header, string prop, int width) =>
        new()
        {
            HeaderText = header,
            DataPropertyName = prop,
            Width = width,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };

    // ------------------------------------------------------------ Khởi động
    private async Task OnLoadAsync()
    {
        RefreshAll();
        await LoadProvincesAsync();
    }

    private QlNhaTroContext NewDb() => new(_cfg.ConnectionStrings.DefaultConnection);

    private void BaoLoiCsdl(Exception ex, string thaoTac)
    {
        Log.Error(ex, "Lỗi CSDL khi {ThaoTac}", thaoTac);
        MessageBox.Show(
            "Không thể truy cập cơ sở dữ liệu. Vui lòng kiểm tra SQL Server và chuỗi kết nối trong appsettings.json.",
            "Lỗi cơ sở dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private void RefreshAll()
    {
        LoadPhongFilter();
        LoadTinhFilter();
        LoadPhongForm(null);
        LoadGrid();
    }

    // ------------------------------------------------------------ API địa chỉ (async/await)
    private void BindCombo(ComboBox cbo, List<AddressItem> items)
    {
        _suppress = true;
        try
        {
            cbo.DataSource = null;
            cbo.DisplayMember = "name";
            cbo.ValueMember = "code";
            cbo.DataSource = items;
        }
        finally { _suppress = false; }
    }

    // Hiện "Đang tải…" và khóa ComboBox trong lúc chờ API
    private void ShowLoading(ComboBox cbo)
    {
        BindCombo(cbo, new List<AddressItem> { new() { name = "Đang tải…", code = 0 } });
        cbo.DropDownStyle = ComboBoxStyle.DropDownList;
        cbo.Enabled = false;
    }

    // Chế độ chịu lỗi: ComboBox thành ô nhập tay
    private void MakeEditable(ComboBox cbo, string text)
    {
        _suppress = true;
        try
        {
            cbo.DataSource = null;
            cbo.Items.Clear();
            cbo.DropDownStyle = ComboBoxStyle.DropDown;
            cbo.Enabled = true;
            cbo.Text = text ?? "";
        }
        finally { _suppress = false; }
    }

    private void BaoLoiMang(Exception ex, string viec)
    {
        Log.Error(ex, "Lỗi mạng khi {Viec}", viec);
        const string msg = "Không tải được danh sách địa chỉ, vui lòng kiểm tra mạng";
        lblStatus.Text = msg + " (bạn vẫn có thể gõ tay Tỉnh/Thành và Phường/Xã)";
        if (!_daBaoLoiMang)
        {
            _daBaoLoiMang = true;
            MessageBox.Show(msg + ".\nBạn vẫn có thể gõ tay Tỉnh/Thành và Phường/Xã để lưu khách.",
                "Mất kết nối mạng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async Task LoadProvincesAsync()
    {
        _wardVersion++;
        _provincesLoaded = false;
        ShowLoading(cboTinh);
        ShowLoading(cboPhuongXa);
        btnTaiLai.Enabled = false;
        lblStatus.Text = "Đang tải danh sách tỉnh/thành…";
        try
        {
            var list = await AddressService.GetProvincesAsync();
            _provinces = list;
            _provincesLoaded = true;
            BindCombo(cboTinh, list);
            cboTinh.Enabled = true;
            cboTinh.SelectedIndex = -1;
            MakeWardsEmpty();
            _daBaoLoiMang = false;
            lblStatus.Text = $"Đã tải {list.Count} tỉnh/thành từ provinces.open-api.vn";
            Log.Information("Đã tải {Count} tỉnh/thành từ API", list.Count);
        }
        catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
        {
            BaoLoiMang(ex, "tải danh sách tỉnh/thành");
            MakeEditable(cboTinh, "");
            MakeEditable(cboPhuongXa, "");
        }
        catch (Exception ex)   // ví dụ JSON sai định dạng
        {
            BaoLoiMang(ex, "xử lý dữ liệu tỉnh/thành");
            MakeEditable(cboTinh, "");
            MakeEditable(cboPhuongXa, "");
        }
        finally
        {
            btnTaiLai.Enabled = true;
        }
    }

    private void MakeWardsEmpty()
    {
        _suppress = true;
        try
        {
            cboPhuongXa.DataSource = null;
            cboPhuongXa.Items.Clear();
            cboPhuongXa.DropDownStyle = ComboBoxStyle.DropDownList;
            cboPhuongXa.Enabled = true;
        }
        finally { _suppress = false; }
    }

    private async void CboTinh_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_suppress || !_provincesLoaded) return;
        if (cboTinh.SelectedItem is AddressItem a && a.code != 0)
            await LoadWardsAsync(a.code, null, null);
        else
        {
            _wardVersion++;
            MakeWardsEmpty();
        }
    }

    // Tải phường/xã của một tỉnh. Nếu lỗi mạng -> cho gõ tay.
    private async Task LoadWardsAsync(int provinceCode, int? selectCode, string fallbackName)
    {
        int ver = ++_wardVersion;
        ShowLoading(cboPhuongXa);
        lblStatus.Text = "Đang tải danh sách phường/xã…";
        try
        {
            var wards = await AddressService.GetWardsAsync(provinceCode);
            if (ver != _wardVersion) return;     // đã chọn tỉnh khác trong lúc chờ
            BindCombo(cboPhuongXa, wards);
            cboPhuongXa.Enabled = true;

            int idx = selectCode.HasValue ? wards.FindIndex(w => w.code == selectCode.Value) : -1;
            if (idx < 0 && !string.IsNullOrWhiteSpace(fallbackName))
                idx = wards.FindIndex(w => w.name == fallbackName);
            cboPhuongXa.SelectedIndex = idx;

            _daBaoLoiMang = false;
            lblStatus.Text = $"Đã tải {wards.Count} phường/xã";
        }
        catch (Exception ex)
        {
            if (ver != _wardVersion) return;
            BaoLoiMang(ex, "tải danh sách phường/xã");
            MakeEditable(cboPhuongXa, fallbackName);
        }
    }

    // Đọc lựa chọn của ComboBox địa chỉ: (mã, tên). Nếu gõ tay thì mã = null.
    private static (int? code, string name) LayChon(ComboBox cbo)
    {
        if (cbo.SelectedItem is AddressItem a && a.code != 0 &&
            (cbo.DropDownStyle == ComboBoxStyle.DropDownList || a.name == cbo.Text.Trim()))
            return (a.code, a.name);
        var text = cbo.DropDownStyle == ComboBoxStyle.DropDownList ? "" : cbo.Text.Trim();
        return (null, text);
    }

    // Hiển thị lại địa chỉ của khách đang chọn
    private async Task HienDiaChiAsync(KhachThue k)
    {
        AddressItem match = null;
        if (_provincesLoaded)
            match = k.MaTinh.HasValue
                ? _provinces.FirstOrDefault(p => p.code == k.MaTinh.Value)
                : _provinces.FirstOrDefault(p => p.name == k.TenTinh);

        if (match != null)
        {
            _suppress = true;
            try
            {
                cboTinh.DropDownStyle = ComboBoxStyle.DropDownList;
                cboTinh.SelectedValue = match.code;
            }
            finally { _suppress = false; }
            await LoadWardsAsync(match.code, k.MaPhuongXa, k.TenPhuongXa);
        }
        else
        {
            _wardVersion++;
            if (cboTinh.Enabled || !_provincesLoaded)
            {
                _suppress = true;
                try
                {
                    cboTinh.DropDownStyle = ComboBoxStyle.DropDown;
                    cboTinh.SelectedIndex = -1;
                    cboTinh.Text = k.TenTinh ?? "";
                }
                finally { _suppress = false; }
            }
            MakeEditable(cboPhuongXa, k.TenPhuongXa);
        }
    }

    // ------------------------------------------------------------ Tải dữ liệu từ CSDL
    private void LoadPhongForm(int? includeRoomId)
    {
        try
        {
            using var db = NewDb();
            var items = db.PhongTros.AsNoTracking()
                .Where(p => p.TinhTrang == TrangThaiTrong || (includeRoomId != null && p.MaPhong == includeRoomId))
                .OrderBy(p => p.TenPhong)
                .ToList()
                .Select(p => new RoomItem
                {
                    MaPhong = p.MaPhong,
                    Text = $"{p.TenPhong} - {p.GiaThue.ToString("N0", Vi)} đ"
                })
                .ToList();

            cboPhong.DataSource = null;
            cboPhong.DisplayMember = "Text";
            cboPhong.ValueMember = "MaPhong";
            cboPhong.DataSource = items;
            cboPhong.SelectedIndex = -1;
        }
        catch (Exception ex) { BaoLoiCsdl(ex, "tải danh sách phòng"); }
    }

    private void LoadPhongFilter()
    {
        try
        {
            _loadingFilters = true;
            int? old = cboLocPhong.SelectedValue is int v ? v : null;
            using var db = NewDb();
            var items = new List<RoomItem> { new() { MaPhong = 0, Text = "Tất cả" } };
            items.AddRange(db.PhongTros.AsNoTracking().OrderBy(p => p.TenPhong).ToList()
                .Select(p => new RoomItem { MaPhong = p.MaPhong, Text = p.TenPhong }));

            cboLocPhong.DataSource = null;
            cboLocPhong.DisplayMember = "Text";
            cboLocPhong.ValueMember = "MaPhong";
            cboLocPhong.DataSource = items;
            int idx = old.HasValue ? items.FindIndex(i => i.MaPhong == old.Value) : 0;
            cboLocPhong.SelectedIndex = idx < 0 ? 0 : idx;
        }
        catch (Exception ex) { BaoLoiCsdl(ex, "tải bộ lọc phòng"); }
        finally { _loadingFilters = false; }
    }

    // Lọc theo tỉnh lấy từ chính dữ liệu khách (chạy được cả khi mất mạng)
    private void LoadTinhFilter()
    {
        try
        {
            _loadingFilters = true;
            var old = cboLocTinh.SelectedItem as string;
            using var db = NewDb();
            var items = new List<string> { "Tất cả" };
            items.AddRange(db.KhachThues.AsNoTracking()
                .Where(k => k.TenTinh != null && k.TenTinh != "")
                .Select(k => k.TenTinh).Distinct().OrderBy(t => t).ToList());

            cboLocTinh.DataSource = null;
            cboLocTinh.DataSource = items;
            int idx = old == null ? 0 : items.IndexOf(old);
            cboLocTinh.SelectedIndex = idx < 0 ? 0 : idx;
        }
        catch (Exception ex) { BaoLoiCsdl(ex, "tải bộ lọc tỉnh"); }
        finally { _loadingFilters = false; }
    }

    // JOIN KhachThue với PhongTro, áp dụng bộ lọc hiện tại
    private void LoadGrid()
    {
        try
        {
            using var db = NewDb();
            IQueryable<KhachThue> q = db.KhachThues.AsNoTracking();

            var kw = txtTim.Text.Trim();
            if (kw.Length > 0)
                q = q.Where(k => k.HoTen.Contains(kw) || k.SoDienThoai.Contains(kw));

            if (cboLocPhong.SelectedValue is int maPhong && maPhong != 0)
                q = q.Where(k => k.MaPhong == maPhong);

            if (cboLocTinh.SelectedIndex > 0 && cboLocTinh.SelectedItem is string tinh)
                q = q.Where(k => k.TenTinh == tinh);

            var data = q.OrderBy(k => k.HoTen)
                .Select(k => new
                {
                    k.MaKhach,
                    k.HoTen,
                    k.SoDienThoai,
                    TenPhong = k.MaPhongNavigation.TenPhong,
                    GiaThue = k.MaPhongNavigation.GiaThue,
                    k.TenPhuongXa,
                    k.TenTinh,
                    k.DaDangKyTamTru
                })
                .ToList();

            var rows = data.Select(d => new KhachRow
            {
                MaKhach = d.MaKhach,
                HoTen = d.HoTen,
                SoDienThoai = d.SoDienThoai,
                TenPhong = d.TenPhong,
                GiaThue = d.GiaThue,
                QueQuan = string.Join(" - ", new[] { d.TenPhuongXa, d.TenTinh }.Where(s => !string.IsNullOrWhiteSpace(s))),
                DaDangKyTamTru = d.DaDangKyTamTru
            }).ToList();

            _binding = true;
            try
            {
                dgv.DataSource = rows;
                dgv.ClearSelection();
            }
            finally { _binding = false; }
        }
        catch (Exception ex)
        {
            _binding = false;
            BaoLoiCsdl(ex, "tải danh sách khách thuê");
        }
    }

    private void BoLoc()
    {
        _loadingFilters = true;
        txtTim.Clear();
        if (cboLocPhong.Items.Count > 0) cboLocPhong.SelectedIndex = 0;
        if (cboLocTinh.Items.Count > 0) cboLocTinh.SelectedIndex = 0;
        _loadingFilters = false;
        LoadGrid();
    }

    // ------------------------------------------------------------ Chọn dòng -> đổ lên form
    private async void Dgv_SelectionChanged(object sender, EventArgs e)
    {
        if (_binding) return;
        if (dgv.CurrentRow?.DataBoundItem is not KhachRow row || !dgv.CurrentRow.Selected) return;

        try
        {
            KhachThue k;
            using (var db = NewDb())
                k = db.KhachThues.AsNoTracking().FirstOrDefault(x => x.MaKhach == row.MaKhach);
            if (k == null) return;

            _selectedId = k.MaKhach;
            txtHoTen.Text = k.HoTen;
            rdoNu.Checked = k.GioiTinh == "Nữ";
            rdoNam.Checked = !rdoNu.Checked;
            var ns = k.NgaySinh.Date;
            dtpNgaySinh.Value = ns < dtpNgaySinh.MinDate ? dtpNgaySinh.MinDate
                : ns > dtpNgaySinh.MaxDate ? dtpNgaySinh.MaxDate : ns;
            txtSDT.Text = k.SoDienThoai;
            chkTamTru.Checked = k.DaDangKyTamTru;

            LoadPhongForm(k.MaPhong);       // gồm cả phòng hiện tại của khách
            cboPhong.SelectedValue = k.MaPhong;

            await HienDiaChiAsync(k);
        }
        catch (Exception ex) when (ex is not HttpRequestException && ex is not TaskCanceledException)
        {
            BaoLoiCsdl(ex, "đọc thông tin khách");
        }
    }

    // ------------------------------------------------------------ Kiểm tra nhập liệu
    private bool KiemTraNhap(out int maPhong, out (int? code, string name) tinh, out (int? code, string name) px)
    {
        maPhong = 0;
        tinh = default;
        px = default;

        if (string.IsNullOrWhiteSpace(txtHoTen.Text))
            return Canh("Họ tên không được để trống.", txtHoTen);
        if (!Regex.IsMatch(txtSDT.Text.Trim(), @"^\d{9,11}$"))
            return Canh("Số điện thoại phải gồm 9-11 chữ số.", txtSDT);
        if (cboPhong.SelectedValue is not int mp)
            return Canh("Vui lòng chọn phòng.", cboPhong);
        maPhong = mp;

        tinh = LayChon(cboTinh);
        if (string.IsNullOrWhiteSpace(tinh.name))
            return Canh("Vui lòng chọn (hoặc nhập) Tỉnh/Thành.", cboTinh);
        px = LayChon(cboPhuongXa);
        if (string.IsNullOrWhiteSpace(px.name))
            return Canh("Vui lòng chọn (hoặc nhập) Phường/Xã.", cboPhuongXa);
        return true;
    }

    private static bool Canh(string msg, Control focus)
    {
        MessageBox.Show(msg, "Dữ liệu chưa hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        focus.Focus();
        return false;
    }

    private void GanDuLieu(KhachThue k, int maPhong, (int? code, string name) tinh, (int? code, string name) px)
    {
        k.HoTen = txtHoTen.Text.Trim();
        k.GioiTinh = rdoNu.Checked ? "Nữ" : "Nam";
        k.NgaySinh = dtpNgaySinh.Value.Date;
        k.SoDienThoai = txtSDT.Text.Trim();
        k.MaPhong = maPhong;
        k.MaTinh = tinh.code;
        k.TenTinh = tinh.name;
        k.MaPhuongXa = px.code;
        k.TenPhuongXa = px.name;
        k.DaDangKyTamTru = chkTamTru.Checked;
    }

    // ------------------------------------------------------------ CRUD trên 2 bảng
    private void Them()
    {
        if (!KiemTraNhap(out var maPhong, out var tinh, out var px)) return;
        try
        {
            using var db = NewDb();
            var phong = db.PhongTros.Find(maPhong);
            if (phong == null || phong.TinhTrang != TrangThaiTrong)
            {
                MessageBox.Show("Phòng này đã có người thuê. Vui lòng chọn phòng khác.", "Phòng không hợp lệ",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                RefreshAll();
                return;
            }

            var k = new KhachThue();
            GanDuLieu(k, maPhong, tinh, px);
            db.KhachThues.Add(k);
            phong.TinhTrang = TrangThaiDangThue;   // cập nhật bảng PhongTro cùng một lần lưu (một transaction)
            db.SaveChanges();
            Log.Information("Thêm khách MaKhach={MaKhach}, {HoTen}, phòng {Phong}", k.MaKhach, k.HoTen, phong.TenPhong);
        }
        catch (Exception ex)
        {
            BaoLoiCsdl(ex, "thêm khách thuê");
            return;
        }
        LamMoi();
    }

    private void Sua()
    {
        if (_selectedId == 0)
        {
            MessageBox.Show("Hãy chọn một khách trong danh sách để sửa.", "Chưa chọn dòng",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (!KiemTraNhap(out var maPhong, out var tinh, out var px)) return;
        try
        {
            using var db = NewDb();
            var k = db.KhachThues.Find(_selectedId);
            if (k == null)
            {
                MessageBox.Show("Khách này không còn tồn tại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LamMoi();
                return;
            }

            int phongCu = k.MaPhong;
            if (maPhong != phongCu)
            {
                var phongMoi = db.PhongTros.Find(maPhong);
                if (phongMoi == null || phongMoi.TinhTrang != TrangThaiTrong)
                {
                    MessageBox.Show("Phòng mới đã có người thuê. Vui lòng chọn phòng khác.", "Phòng không hợp lệ",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                phongMoi.TinhTrang = TrangThaiDangThue;

                // Phòng cũ: nếu không còn khách nào khác thì trả về "Trống"
                bool conKhach = db.KhachThues.Any(x => x.MaPhong == phongCu && x.MaKhach != k.MaKhach);
                if (!conKhach)
                {
                    var phongCuEntity = db.PhongTros.Find(phongCu);
                    if (phongCuEntity != null) phongCuEntity.TinhTrang = TrangThaiTrong;
                }
            }

            GanDuLieu(k, maPhong, tinh, px);
            db.SaveChanges();
            Log.Information("Sửa khách MaKhach={MaKhach}, {HoTen}, phòng {Phong}", k.MaKhach, k.HoTen, maPhong);
        }
        catch (Exception ex)
        {
            BaoLoiCsdl(ex, "sửa khách thuê");
            return;
        }
        LamMoi();
    }

    private void Xoa()
    {
        if (_selectedId == 0)
        {
            MessageBox.Show("Hãy chọn một khách trong danh sách để xóa.", "Chưa chọn dòng",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        var hoi = MessageBox.Show($"Bạn có chắc muốn xóa khách \"{txtHoTen.Text}\"?", "Xác nhận xóa",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (hoi != DialogResult.Yes) return;

        try
        {
            using var db = NewDb();
            var k = db.KhachThues.Find(_selectedId);
            if (k != null)
            {
                int maPhong = k.MaPhong;
                db.KhachThues.Remove(k);

                // Người cuối cùng của phòng bị xóa -> phòng trở lại "Trống"
                bool conKhach = db.KhachThues.Any(x => x.MaPhong == maPhong && x.MaKhach != k.MaKhach);
                if (!conKhach)
                {
                    var phong = db.PhongTros.Find(maPhong);
                    if (phong != null) phong.TinhTrang = TrangThaiTrong;
                }
                db.SaveChanges();
                Log.Information("Xóa khách MaKhach={MaKhach}, {HoTen}, phòng {MaPhong}", k.MaKhach, k.HoTen, maPhong);
            }
        }
        catch (Exception ex)
        {
            BaoLoiCsdl(ex, "xóa khách thuê");
            return;
        }
        LamMoi();
    }

    // Xóa trắng form và nạp lại dữ liệu
    private void LamMoi()
    {
        _selectedId = 0;
        txtHoTen.Clear();
        rdoNam.Checked = true;
        dtpNgaySinh.Value = DateTime.Today.AddYears(-20);
        txtSDT.Clear();
        chkTamTru.Checked = false;

        _wardVersion++;
        if (_provincesLoaded)
        {
            _suppress = true;
            try
            {
                cboTinh.DropDownStyle = ComboBoxStyle.DropDownList;
                cboTinh.SelectedIndex = -1;
            }
            finally { _suppress = false; }
            MakeWardsEmpty();
        }
        else if (cboTinh.Enabled)
        {
            MakeEditable(cboTinh, "");
            MakeEditable(cboPhuongXa, "");
        }

        RefreshAll();
        txtHoTen.Focus();
    }
}
