using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace QuanTraSua;

public class FormThucDon : Form
{
    static readonly CultureInfo VN = new("vi-VN");

    TextBox txtTen = new(), txtTim = new();
    ComboBox cboDM = new(), cboLoc = new();
    NumericUpDown numGia = new();
    CheckBox chkDangBan = new();
    Button btnThem = new(), btnSua = new(), btnXoa = new(), btnMoi = new(), btnTim = new();
    DataGridView grid = new();
    List<DanhMucMon> dms = new();

    public FormThucDon()
    {
        BuildUi();
        LoadDanhMuc();
        LoadGrid();
    }

    void BuildUi()
    {
        Text = "Quản lý thực đơn";
        ClientSize = new Size(820, 540);
        StartPosition = FormStartPosition.CenterParent;
        Font = new Font("Segoe UI", 9F);

        var g = new GroupBox { Text = "Thông tin món", Location = new Point(12, 8), Size = new Size(796, 130) };
        g.Controls.Add(new Label { Text = "Tên món:", Location = new Point(12, 28), AutoSize = true });
        txtTen.Location = new Point(90, 24); txtTen.Width = 250;
        g.Controls.Add(new Label { Text = "Danh mục:", Location = new Point(12, 62), AutoSize = true });
        cboDM.Location = new Point(90, 58); cboDM.Width = 250; cboDM.DropDownStyle = ComboBoxStyle.DropDownList;
        g.Controls.Add(new Label { Text = "Giá bán:", Location = new Point(370, 28), AutoSize = true });
        numGia.Location = new Point(440, 24); numGia.Width = 140; numGia.Maximum = 10_000_000;
        numGia.Increment = 1000; numGia.ThousandsSeparator = true;
        chkDangBan.Text = "Đang bán"; chkDangBan.Checked = true; chkDangBan.AutoSize = true; chkDangBan.Location = new Point(440, 60);
        int x = 12;
        foreach (var b in new[] { btnThem, btnSua, btnXoa, btnMoi })
        {
            b.Location = new Point(x, 92); b.Size = new Size(90, 28); x += 98; g.Controls.Add(b);
        }
        btnThem.Text = "Thêm"; btnSua.Text = "Sửa"; btnXoa.Text = "Xóa"; btnMoi.Text = "Làm mới";
        btnThem.Click += (s, e) => Them();
        btnSua.Click += (s, e) => Sua();
        btnXoa.Click += (s, e) => Xoa();
        btnMoi.Click += (s, e) => LamMoi();
        g.Controls.AddRange(new Control[] { txtTen, cboDM, numGia, chkDangBan });

        var g2 = new GroupBox { Text = "Tìm kiếm", Location = new Point(12, 146), Size = new Size(796, 60) };
        g2.Controls.Add(new Label { Text = "Tên món:", Location = new Point(12, 28), AutoSize = true });
        txtTim.Location = new Point(90, 24); txtTim.Width = 230;
        g2.Controls.Add(new Label { Text = "Danh mục:", Location = new Point(340, 28), AutoSize = true });
        cboLoc.Location = new Point(410, 24); cboLoc.Width = 200; cboLoc.DropDownStyle = ComboBoxStyle.DropDownList;
        btnTim.Text = "Tìm"; btnTim.Location = new Point(630, 22); btnTim.Size = new Size(90, 28);
        btnTim.Click += (s, e) => LoadGrid();
        g2.Controls.AddRange(new Control[] { txtTim, cboLoc, btnTim });

        grid.Location = new Point(12, 214); grid.Size = new Size(796, 314);
        grid.AutoGenerateColumns = false; grid.ReadOnly = true;
        grid.AllowUserToAddRows = false; grid.AllowUserToDeleteRows = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect; grid.MultiSelect = false;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "MaMon", HeaderText = "Mã", DataPropertyName = "MaMon", FillWeight = 40 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "TenMon", HeaderText = "Tên món", DataPropertyName = "TenMon", FillWeight = 160 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "TenDM", HeaderText = "Danh mục", DataPropertyName = "TenDM", FillWeight = 90 });
        var cGia = new DataGridViewTextBoxColumn { Name = "GiaBan", HeaderText = "Giá bán", DataPropertyName = "GiaBan", FillWeight = 70 };
        cGia.DefaultCellStyle.Format = "N0"; cGia.DefaultCellStyle.FormatProvider = VN;
        grid.Columns.Add(cGia);
        grid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "DangBan", HeaderText = "Đang bán", DataPropertyName = "DangBan", FillWeight = 50 });
        grid.SelectionChanged += (s, e) => FillFromSelected();

        Controls.AddRange(new Control[] { g, g2, grid });
    }

    void LoadDanhMuc()
    {
        using var db = new QL_TraSuaContext();
        dms = db.DanhMucMons.OrderBy(d => d.MaDM).ToList();
        cboDM.DataSource = dms.ToList(); cboDM.DisplayMember = "TenDM";
        var loc = new List<DanhMucMon> { new DanhMucMon { MaDM = 0, TenDM = "(Tất cả)" } };
        loc.AddRange(dms);
        cboLoc.DataSource = loc; cboLoc.DisplayMember = "TenDM";
    }

    void LoadGrid()
    {
        using var db = new QL_TraSuaContext();
        var q = db.MonUongs.AsQueryable();
        var kw = txtTim.Text.Trim();
        if (kw != "") q = q.Where(m => m.TenMon.Contains(kw));
        if (cboLoc.SelectedItem is DanhMucMon dm && dm.MaDM != 0) q = q.Where(m => m.MaDM == dm.MaDM);

        // join MonUong với DanhMucMon để hiện tên danh mục
        grid.DataSource = q.OrderBy(m => m.MaMon)
            .Select(m => new { m.MaMon, m.TenMon, TenDM = m.DanhMuc.TenDM, m.GiaBan, m.DangBan })
            .ToList();
    }

    bool Validate(out string err)
    {
        err = "";
        if (txtTen.Text.Trim() == "") err = "Tên món không được để trống.";
        else if (cboDM.SelectedItem is not DanhMucMon) err = "Hãy chọn danh mục.";
        else if (numGia.Value <= 0) err = "Giá bán phải lớn hơn 0.";
        return err == "";
    }

    int? SelectedId() => grid.CurrentRow?.Cells["MaMon"].Value is int id ? id : null;

    void Them()
    {
        if (!Validate(out var err)) { Warn(err); return; }
        using var db = new QL_TraSuaContext();
        db.MonUongs.Add(new MonUong
        {
            TenMon = txtTen.Text.Trim(), GiaBan = numGia.Value, DangBan = chkDangBan.Checked,
            MaDM = ((DanhMucMon)cboDM.SelectedItem).MaDM
        });
        db.SaveChanges();
        LoadGrid(); LamMoi();
    }

    void Sua()
    {
        if (SelectedId() is not int id) { Warn("Hãy chọn một món trong danh sách."); return; }
        if (!Validate(out var err)) { Warn(err); return; }
        using var db = new QL_TraSuaContext();
        var m = db.MonUongs.Find(id); if (m == null) return;
        m.TenMon = txtTen.Text.Trim(); m.GiaBan = numGia.Value; m.DangBan = chkDangBan.Checked;
        m.MaDM = ((DanhMucMon)cboDM.SelectedItem).MaDM;
        db.SaveChanges();
        LoadGrid();
    }

    void Xoa()
    {
        if (SelectedId() is not int id) { Warn("Hãy chọn một món trong danh sách."); return; }
        if (MessageBox.Show("Bạn có chắc muốn xóa món này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        try
        {
            using var db = new QL_TraSuaContext();
            if (db.ChiTietHoaDons.Any(c => c.MaMon == id))
            {
                Warn("Món này đã có trong hóa đơn nên không thể xóa.\nHãy bỏ chọn \"Đang bán\" rồi bấm Sửa.");
                return;
            }
            var m = db.MonUongs.Find(id); if (m == null) return;
            db.MonUongs.Remove(m); db.SaveChanges();
            LoadGrid(); LamMoi();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Khong xoa duoc mon {MaMon}", id);
            Warn("Không xóa được: " + ex.Message);
        }
    }

    void LamMoi()
    {
        txtTen.Clear(); numGia.Value = 0; chkDangBan.Checked = true;
        if (cboDM.Items.Count > 0) cboDM.SelectedIndex = 0;
        txtTen.Focus();
    }

    void FillFromSelected()
    {
        var r = grid.CurrentRow;
        if (r == null || r.Cells["MaMon"].Value == null) return;
        txtTen.Text = r.Cells["TenMon"].Value?.ToString();
        var tenDm = r.Cells["TenDM"].Value?.ToString();
        var dm = (cboDM.DataSource as List<DanhMucMon>)?.FirstOrDefault(d => d.TenDM == tenDm);
        if (dm != null) cboDM.SelectedItem = dm;
        numGia.Value = Math.Min(numGia.Maximum, Convert.ToDecimal(r.Cells["GiaBan"].Value));
        chkDangBan.Checked = (bool)r.Cells["DangBan"].Value;
    }

    void Warn(string msg) => MessageBox.Show(msg, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}
