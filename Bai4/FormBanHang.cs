using System.ComponentModel;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using QuanTraSua.Helpers;
using QuanTraSua.Services;
using Serilog;

namespace QuanTraSua;

public class MonItem
{
    public int MaMon { get; set; }
    public string TenMon { get; set; } = "";
    public decimal GiaBan { get; set; }
    public override string ToString() => $"{TenMon} - {GiaBan.ToString("N0", new CultureInfo("vi-VN"))}";
}

public class CartItem
{
    public int MaMon { get; set; }
    public string TenMon { get; set; } = "";
    public int SL { get; set; }
    public decimal DonGia { get; set; }
    public decimal ThanhTien => SL * DonGia;
}

public class FormBanHang : Form
{
    static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(8) };
    static readonly CultureInfo VN = new("vi-VN");

    readonly BindingList<CartItem> cart = new();

    ComboBox cboDanhMuc = new();
    ListBox lstMon = new();
    NumericUpDown numSoLy = new();
    Button btnThem = new(), btnXoaDong = new(), btnCK = new(), btnTienMat = new(), btnDaNhan = new();
    DataGridView grid = new();
    Label lblTong = new(), lblInfo = new();
    PictureBox picQr = new();

    public FormBanHang()
    {
        BuildUi();
        LoadDanhMuc();
        UpdateTotal();
    }

    // ================= Giao diện dựng bằng code =================
    void BuildUi()
    {
        Text = "Bán hàng - Quán trà sữa";
        ClientSize = new Size(970, 570);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        Font = new Font("Segoe UI", 9F);

        // Menu: mở các form khác
        var menu = new MenuStrip();
        var mThucDon = new ToolStripMenuItem("Thực đơn");
        mThucDon.Click += (s, e) => { using var f = new FormThucDon(); f.ShowDialog(this); LoadDanhMuc(); };
        var mHoaDon = new ToolStripMenuItem("Hóa đơn");
        mHoaDon.Click += (s, e) => { using var f = new FormHoaDon(); f.ShowDialog(this); };
        menu.Items.AddRange(new ToolStripItem[] { mThucDon, mHoaDon });
        MainMenuStrip = menu;

        // --- Nhóm: Chọn món ---
        var g1 = new GroupBox { Text = "Chọn món", Location = new Point(12, 32), Size = new Size(270, 526) };
        g1.Controls.Add(new Label { Text = "Danh mục:", Location = new Point(10, 24), AutoSize = true });
        cboDanhMuc.DropDownStyle = ComboBoxStyle.DropDownList;
        cboDanhMuc.DisplayMember = "TenDM";
        cboDanhMuc.Location = new Point(10, 44); cboDanhMuc.Width = 250;
        cboDanhMuc.SelectedIndexChanged += (s, e) => LoadMon();
        lstMon.Location = new Point(10, 78); lstMon.Size = new Size(250, 330);
        g1.Controls.Add(new Label { Text = "Số ly:", Location = new Point(10, 428), AutoSize = true });
        numSoLy.Location = new Point(60, 424); numSoLy.Width = 60; numSoLy.Minimum = 1; numSoLy.Maximum = 99; numSoLy.Value = 1;
        btnThem.Text = "Thêm vào đơn"; btnThem.Location = new Point(10, 464); btnThem.Size = new Size(250, 34);
        btnThem.Click += (s, e) => ThemVaoDon();
        g1.Controls.AddRange(new Control[] { cboDanhMuc, lstMon, numSoLy, btnThem });

        // --- Nhóm: Đơn hiện tại ---
        var g2 = new GroupBox { Text = "Đơn hiện tại", Location = new Point(292, 32), Size = new Size(380, 526) };
        grid.Location = new Point(10, 22); grid.Size = new Size(360, 400);
        grid.AutoGenerateColumns = false;
        grid.ReadOnly = true; grid.AllowUserToAddRows = false; grid.AllowUserToDeleteRows = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect; grid.MultiSelect = false;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.RowHeadersVisible = false;
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tên món", DataPropertyName = "TenMon", FillWeight = 150 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "SL", DataPropertyName = "SL", FillWeight = 35 });
        var cDonGia = new DataGridViewTextBoxColumn { HeaderText = "Đơn giá", DataPropertyName = "DonGia", FillWeight = 70 };
        cDonGia.DefaultCellStyle.Format = "N0"; cDonGia.DefaultCellStyle.FormatProvider = VN;
        var cTT = new DataGridViewTextBoxColumn { HeaderText = "Thành tiền", DataPropertyName = "ThanhTien", FillWeight = 80 };
        cTT.DefaultCellStyle.Format = "N0"; cTT.DefaultCellStyle.FormatProvider = VN;
        grid.Columns.Add(cDonGia); grid.Columns.Add(cTT);
        grid.DataSource = cart;
        cart.ListChanged += (s, e) => { UpdateTotal(); ResetPayment(); };

        btnXoaDong.Text = "Xóa dòng"; btnXoaDong.Location = new Point(10, 432); btnXoaDong.Size = new Size(110, 32);
        btnXoaDong.Click += (s, e) =>
        {
            if (grid.CurrentRow?.DataBoundItem is CartItem ci) cart.Remove(ci);
        };
        lblTong.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
        lblTong.AutoSize = true; lblTong.Location = new Point(10, 476);
        g2.Controls.AddRange(new Control[] { grid, btnXoaDong, lblTong });

        // --- Nhóm: Thanh toán ---
        var g3 = new GroupBox { Text = "Thanh toán", Location = new Point(682, 32), Size = new Size(276, 526) };
        btnCK.Text = "Thanh toán chuyển khoản"; btnCK.Location = new Point(10, 24); btnCK.Size = new Size(256, 36);
        btnCK.Click += async (s, e) => await ChuyenKhoanAsync();
        btnTienMat.Text = "Tiền mặt"; btnTienMat.Location = new Point(10, 66); btnTienMat.Size = new Size(256, 36);
        btnTienMat.Click += (s, e) => TienMat();
        picQr.Location = new Point(28, 112); picQr.Size = new Size(220, 220); picQr.SizeMode = PictureBoxSizeMode.Zoom;
        picQr.BorderStyle = BorderStyle.FixedSingle; picQr.Visible = false;
        lblInfo.Location = new Point(10, 340); lblInfo.Size = new Size(256, 104); lblInfo.Visible = false;
        btnDaNhan.Text = "Đã nhận tiền"; btnDaNhan.Location = new Point(10, 466); btnDaNhan.Size = new Size(256, 44);
        btnDaNhan.BackColor = Color.FromArgb(25, 135, 84); btnDaNhan.ForeColor = Color.White;
        btnDaNhan.Font = new Font("Segoe UI", 11F, FontStyle.Bold); btnDaNhan.FlatStyle = FlatStyle.Flat; btnDaNhan.Visible = false;
        btnDaNhan.Click += (s, e) => DaNhanTien();
        g3.Controls.AddRange(new Control[] { btnCK, btnTienMat, picQr, lblInfo, btnDaNhan });

        Controls.AddRange(new Control[] { menu, g1, g2, g3 });
    }

    // ================= Nạp dữ liệu =================
    void LoadDanhMuc()
    {
        try
        {
            using var db = new QL_TraSuaContext();
            cboDanhMuc.DataSource = db.DanhMucMons.OrderBy(d => d.MaDM).ToList();
            cboDanhMuc.DisplayMember = "TenDM";
            LoadMon();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Khong doc duoc danh muc");
            MessageBox.Show("Không kết nối được CSDL: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    void LoadMon()
    {
        lstMon.Items.Clear();
        if (cboDanhMuc.SelectedItem is not DanhMucMon dm) return;
        using var db = new QL_TraSuaContext();
        var mons = db.MonUongs.Where(m => m.MaDM == dm.MaDM && m.DangBan)
            .OrderBy(m => m.TenMon)
            .Select(m => new MonItem { MaMon = m.MaMon, TenMon = m.TenMon, GiaBan = m.GiaBan })
            .ToList();
        lstMon.Items.AddRange(mons.Cast<object>().ToArray());
    }

    // ================= Giỏ hàng =================
    void ThemVaoDon()
    {
        if (lstMon.SelectedItem is not MonItem m)
        {
            MessageBox.Show("Hãy chọn một món.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        int n = (int)numSoLy.Value;
        var exist = cart.FirstOrDefault(c => c.MaMon == m.MaMon);
        if (exist != null) { exist.SL += n; cart.ResetBindings(); }   // cộng dồn số lượng
        else cart.Add(new CartItem { MaMon = m.MaMon, TenMon = m.TenMon, SL = n, DonGia = m.GiaBan });
        numSoLy.Value = 1;
    }

    decimal Tong() => cart.Sum(c => c.ThanhTien);

    void UpdateTotal() => lblTong.Text = $"Tổng tiền: {Tong().ToString("N0", VN)} đ";

    void ResetPayment()
    {
        picQr.Image?.Dispose(); picQr.Image = null;
        picQr.Visible = false; lblInfo.Visible = false; btnDaNhan.Visible = false;
    }

    // ================= Thanh toán chuyển khoản (VietQR) =================
    int NextHoaDonId()
    {
        using var db = new QL_TraSuaContext();
        return (db.HoaDons.Max(h => (int?)h.MaHD) ?? 0) + 1;
    }

    async Task ChuyenKhoanAsync()
    {
        if (cart.Count == 0)
        {
            MessageBox.Show("Đơn hàng đang trống.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        AppConfig cfg;
        try { cfg = AppConfig.Load(); }
        catch (Exception ex)
        {
            Log.Error(ex, "Khong doc duoc appsettings.json");
            MessageBox.Show("Không đọc được appsettings.json.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        var p = cfg.Payment;
        if (string.IsNullOrWhiteSpace(p.BankId) || string.IsNullOrWhiteSpace(p.AccountNo))
        {
            MessageBox.Show("Chưa cấu hình BankId/AccountNo trong appsettings.json.", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        decimal total = Tong();
        int maHd = NextHoaDonId();
        string noiDung = $"TRASUA HD{maHd}";
        string url = VietQrService.BuildUrl(p, total, noiDung);

        // Log Information: KHÔNG ghi số tài khoản đầy đủ
        Log.Information("Tao QR: MaHD={MaHD}, SoTien={SoTien}, TaiKhoan={TK}", maHd, total, VietQrService.Mask(p.AccountNo));

        // Luôn hiện thông tin chuyển khoản dạng chữ
        lblInfo.Text = $"Chủ TK: {p.AccountName}\nNgân hàng: {p.BankId}\nSTK: {p.AccountNo}\n" +
                       $"Số tiền: {total.ToString("N0", VN)} đ\nNội dung: {noiDung}";
        lblInfo.Visible = true;
        btnCK.Enabled = false;
        Cursor = Cursors.WaitCursor;
        try
        {
            var bytes = await Http.GetByteArrayAsync(url);
            picQr.Image?.Dispose();
            picQr.Image = Image.FromStream(new MemoryStream(bytes));
            picQr.Visible = true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Khong tai duoc anh QR cho HD{MaHD}", maHd);
            picQr.Visible = false;
            MessageBox.Show("Không tải được mã QR (có thể mất mạng).\nHãy chuyển khoản thủ công theo thông tin bên dưới, " +
                            "hoặc chọn \"Tiền mặt\".", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            Cursor = Cursors.Default;
            btnCK.Enabled = true;
        }
        btnDaNhan.Visible = true;
    }

    void DaNhanTien()
    {
        var ok = MessageBox.Show("Bạn đã kiểm tra tin nhắn ngân hàng và thấy tiền đã vào tài khoản?", "Xác nhận",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (ok == DialogResult.Yes) LuuHoaDon("Chuyển khoản");
    }

    void TienMat()
    {
        if (cart.Count == 0)
        {
            MessageBox.Show("Đơn hàng đang trống.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        LuuHoaDon("Tiền mặt");
    }

    // Lưu HoaDon + ChiTietHoaDon trong CÙNG MỘT lần SaveChanges
    void LuuHoaDon(string hinhThuc)
    {
        try
        {
            using var db = new QL_TraSuaContext();
            var hd = new HoaDon
            {
                NgayLap = DateTime.Now,
                TongTien = Tong(),
                HinhThucTT = hinhThuc,
                DaThanhToan = true
            };
            foreach (var c in cart)
                hd.ChiTietHoaDons.Add(new ChiTietHoaDon { MaMon = c.MaMon, SoLy = c.SL, DonGia = c.DonGia });
            db.HoaDons.Add(hd);
            db.SaveChanges();

            Log.Information("Luu hoa don: MaHD={MaHD}, SoTien={SoTien}, HinhThuc={HinhThuc}", hd.MaHD, hd.TongTien, hinhThuc);
            cart.Clear();
            MessageBox.Show($"Đã lưu hóa đơn HD{hd.MaHD}.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Khong luu duoc hoa don");
            MessageBox.Show("Không lưu được hóa đơn: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
