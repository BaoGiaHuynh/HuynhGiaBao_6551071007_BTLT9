using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

// Đổi namespace cho trùng với project của bạn
namespace KeHoachDaNgoai;

public class FormChuyenDi : Form
{
    // ===== Cấu hình chung =====
    static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(8) };
    static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "KeHoachDaNgoai", "usersettings.json");

    string lang = "vi";
    CultureInfo Culture => new(lang == "vi" ? "vi-VN" : "en-US");

    // ===== Từ điển đa ngôn ngữ (key -> vi, en) =====
    static readonly Dictionary<string, (string vi, string en)> L = new()
    {
        ["title"] = ("Kế hoạch dã ngoại cuối tuần", "Weekend Outing Planner"),
        ["info"] = ("Thông tin chuyến đi", "Trip information"),
        ["ten"] = ("Tên chuyến", "Trip name"),
        ["diadiem"] = ("Địa điểm", "Destination"),
        ["ngaydi"] = ("Ngày đi", "Date"),
        ["songuoi"] = ("Số người", "People"),
        ["nganSach"] = ("Ngân sách", "Budget"),
        ["pt"] = ("Phương tiện", "Transport"),
        ["done"] = ("Đã hoàn thành", "Completed"),
        ["add"] = ("Thêm", "Add"),
        ["edit"] = ("Sửa", "Edit"),
        ["del"] = ("Xóa", "Delete"),
        ["clear"] = ("Làm mới", "Clear"),
        ["search"] = ("Tìm tên / địa điểm:", "Search name / place:"),
        ["onlyUndone"] = ("Chỉ hiện chuyến chưa hoàn thành", "Only unfinished trips"),
        ["from"] = ("Từ", "From"),
        ["to"] = ("Đến", "To"),
        ["find"] = ("Tìm", "Search"),
        ["weather"] = ("Xem thời tiết", "View weather"),
        ["lang"] = ("Ngôn ngữ", "Language"),
        ["forecast"] = ("Dự báo thời tiết ngày {0} tại {1}", "Weather forecast for {0} in {1}"),
        ["temp"] = ("Nhiệt độ: {0}° - {1}°C", "Temp: {0}° - {1}°C"),
        ["rain"] = ("Khả năng mưa: {0}%", "Chance of rain: {0}%"),
        ["warn"] = ("Nên mang áo mưa hoặc cân nhắc đổi lịch", "Bring a raincoat or consider rescheduling"),
        ["nodata"] = ("Chưa có dữ liệu dự báo", "No forecast data yet"),
        ["neterr"] = ("Không kết nối được mạng. Vui lòng thử lại.", "Network error. Please try again."),
        ["notfound"] = ("Không tìm thấy địa điểm, dùng Đà Lạt thay thế", "Place not found, using Da Lat instead"),
        ["noplace"] = ("Vui lòng nhập địa điểm.", "Please enter a destination."),
        ["errName"] = ("Tên chuyến và địa điểm không được để trống.", "Name and destination are required."),
        ["errDate"] = ("Ngày đi không được nhỏ hơn hôm nay.", "Date cannot be before today."),
        ["errPeople"] = ("Số người phải >= 1.", "People must be >= 1."),
        ["errBudget"] = ("Ngân sách phải >= 0.", "Budget must be >= 0."),
        ["pick"] = ("Hãy chọn một chuyến trong danh sách.", "Please select a trip in the list."),
        ["confirmDel"] = ("Bạn có chắc muốn xóa chuyến này?", "Delete this trip?"),
        ["notice"] = ("Thông báo", "Notice"),
        ["colMa"] = ("Mã", "ID"),
        ["colTen"] = ("Tên chuyến", "Name"),
        ["colDd"] = ("Địa điểm", "Destination"),
        ["colNgay"] = ("Ngày đi", "Date"),
        ["colNguoi"] = ("Số người", "People"),
        ["colNs"] = ("Ngân sách", "Budget"),
        ["colPt"] = ("Phương tiện", "Transport"),
        ["colDone"] = ("Hoàn thành", "Done"),
    };
    string T(string key) => lang == "vi" ? L[key].vi : L[key].en;

    // ===== Controls =====
    TextBox txtTen = new(), txtDiaDiem = new(), txtTim = new();
    DateTimePicker dtNgay = new(), dtTu = new(), dtDen = new();
    NumericUpDown numNguoi = new(), numNganSach = new();
    ComboBox cboPt = new(), cboLang = new();
    CheckBox chkDone = new(), chkChuaXong = new();
    Button btnThem = new(), btnSua = new(), btnXoa = new(), btnMoi = new(), btnTim = new(), btnWeather = new();
    DataGridView grid = new();
    ProgressBar progress = new();
    Label lblForecast = new(), lblIcon = new(), lblDesc = new(), lblTemp = new(), lblRain = new(), lblWarn = new();
    GroupBox grpInfo = new();
    Label lblTitle = new(), lblLang = new(), lblSearch = new(), lblTu = new(), lblDen = new();
    Label lblTen = new(), lblDd = new(), lblNgay = new(), lblNguoi = new(), lblNs = new(), lblPt = new();
    Panel pnlWeather = new(), pnlWarn = new();

    public FormChuyenDi()
    {
        LoadSettings();
        BuildUi();
        ApplyLanguage();
        LoadGrid();
    }

    // ===== Dựng giao diện bằng code =====
    void BuildUi()
    {
        Size = new Size(920, 640);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9F);

        lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        lblTitle.AutoSize = true; lblTitle.Location = new Point(12, 8);

        lblLang.AutoSize = true; lblLang.Location = new Point(700, 14);
        cboLang.DropDownStyle = ComboBoxStyle.DropDownList;
        cboLang.Items.AddRange(new object[] { "Tiếng Việt", "English" });
        cboLang.SelectedIndex = lang == "vi" ? 0 : 1;
        cboLang.Location = new Point(780, 10); cboLang.Width = 110;
        cboLang.SelectedIndexChanged += (s, e) =>
        {
            lang = cboLang.SelectedIndex == 0 ? "vi" : "en";
            SaveSettings();
            ApplyLanguage();
        };

        // --- Nhóm thông tin ---
        grpInfo.Location = new Point(12, 40); grpInfo.Size = new Size(380, 290);
        int y = 22;
        AddRow(grpInfo, lblTen, txtTen, ref y);
        AddRow(grpInfo, lblDd, txtDiaDiem, ref y);
        dtNgay.Format = DateTimePickerFormat.Short;
        AddRow(grpInfo, lblNgay, dtNgay, ref y);
        numNguoi.Minimum = 0; numNguoi.Maximum = 1000; numNguoi.Value = 1;
        AddRow(grpInfo, lblNguoi, numNguoi, ref y);
        numNganSach.Maximum = 1_000_000_000; numNganSach.Increment = 100000; numNganSach.ThousandsSeparator = true;
        AddRow(grpInfo, lblNs, numNganSach, ref y);
        cboPt.DropDownStyle = ComboBoxStyle.DropDownList;
        cboPt.Items.AddRange(new object[] { "Xe máy", "Ô tô", "Xe khách", "Tàu hỏa" });
        cboPt.SelectedIndex = 0;
        AddRow(grpInfo, lblPt, cboPt, ref y);
        chkDone.AutoSize = true; chkDone.Location = new Point(110, y); grpInfo.Controls.Add(chkDone);
        y += 32;
        int x = 10;
        foreach (var b in new[] { btnThem, btnSua, btnXoa, btnMoi })
        {
            b.Location = new Point(x, y); b.Size = new Size(82, 28); x += 90; grpInfo.Controls.Add(b);
        }
        btnThem.Click += (s, e) => Them();
        btnSua.Click += (s, e) => Sua();
        btnXoa.Click += (s, e) => Xoa();
        btnMoi.Click += (s, e) => LamMoi();

        // --- Thời tiết ---
        btnWeather.Location = new Point(410, 40); btnWeather.Size = new Size(130, 28);
        btnWeather.Click += async (s, e) => await ShowWeatherAsync();
        progress.Location = new Point(550, 44); progress.Size = new Size(340, 20);
        progress.Style = ProgressBarStyle.Marquee; progress.MarqueeAnimationSpeed = 0;

        lblForecast.Location = new Point(410, 78); lblForecast.AutoSize = true;
        pnlWeather.Location = new Point(410, 100); pnlWeather.Size = new Size(480, 130);
        pnlWeather.BorderStyle = BorderStyle.FixedSingle;
        lblIcon.Font = new Font("Segoe UI Emoji", 40F); lblIcon.Location = new Point(10, 20); lblIcon.AutoSize = true;
        lblDesc.Font = new Font("Segoe UI", 14F, FontStyle.Bold); lblDesc.Location = new Point(130, 15); lblDesc.AutoSize = true;
        lblTemp.Location = new Point(132, 55); lblTemp.AutoSize = true;
        lblRain.Location = new Point(132, 80); lblRain.AutoSize = true;
        pnlWeather.Controls.AddRange(new Control[] { lblIcon, lblDesc, lblTemp, lblRain });

        pnlWarn.Location = new Point(410, 238); pnlWarn.Size = new Size(480, 36);
        pnlWarn.BackColor = Color.FromArgb(255, 224, 178); pnlWarn.Visible = false;
        lblWarn.AutoSize = true; lblWarn.Location = new Point(8, 9); lblWarn.ForeColor = Color.FromArgb(150, 70, 0);
        pnlWarn.Controls.Add(lblWarn);

        // --- Tìm kiếm / lọc ---
        lblSearch.Location = new Point(12, 345); lblSearch.AutoSize = true;
        txtTim.Location = new Point(130, 341); txtTim.Width = 160;
        chkChuaXong.Location = new Point(305, 343); chkChuaXong.AutoSize = true;
        lblTu.Location = new Point(12, 375); lblTu.AutoSize = true;
        dtTu.Location = new Point(60, 371); dtTu.Width = 110; dtTu.Format = DateTimePickerFormat.Short; dtTu.ShowCheckBox = true; dtTu.Checked = false;
        lblDen.Location = new Point(185, 375); lblDen.AutoSize = true;
        dtDen.Location = new Point(235, 371); dtDen.Width = 110; dtDen.Format = DateTimePickerFormat.Short; dtDen.ShowCheckBox = true; dtDen.Checked = false;
        btnTim.Location = new Point(360, 369); btnTim.Size = new Size(80, 27);
        btnTim.Click += (s, e) => LoadGrid();

        // --- Grid ---
        grid.Location = new Point(12, 405); grid.Size = new Size(880, 190);
        grid.ReadOnly = true; grid.AllowUserToAddRows = false; grid.AllowUserToDeleteRows = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect; grid.MultiSelect = false;
        grid.RowHeadersVisible = true; grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "MaChuyen", DataPropertyName = "MaChuyen", FillWeight = 40 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "TenChuyen", DataPropertyName = "TenChuyen", FillWeight = 120 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "DiaDiem", DataPropertyName = "DiaDiem", FillWeight = 80 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "NgayDi", DataPropertyName = "NgayDi", FillWeight = 80 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "SoNguoi", DataPropertyName = "SoNguoi", FillWeight = 50 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "NganSach", DataPropertyName = "NganSach", FillWeight = 80 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "PhuongTien", DataPropertyName = "PhuongTien", FillWeight = 70 });
        grid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "DaHoanThanh", DataPropertyName = "DaHoanThanh", FillWeight = 60 });
        grid.AutoGenerateColumns = false;
        grid.SelectionChanged += (s, e) => FillFromSelected();

        Controls.AddRange(new Control[] {
            lblTitle, lblLang, cboLang, grpInfo, btnWeather, progress, lblForecast, pnlWeather, pnlWarn,
            lblSearch, txtTim, chkChuaXong, lblTu, dtTu, lblDen, dtDen, btnTim, grid });
    }

    void AddRow(Control parent, Label lbl, Control input, ref int y)
    {
        lbl.AutoSize = true; lbl.Location = new Point(10, y + 3);
        input.Location = new Point(110, y); input.Width = 250;
        parent.Controls.Add(lbl); parent.Controls.Add(input);
        y += 34;
    }

    // ===== Đa ngôn ngữ =====
    void ApplyLanguage()
    {
        var c = Culture;
        CultureInfo.CurrentCulture = c; CultureInfo.CurrentUICulture = c;
        Text = T("title"); lblTitle.Text = T("title"); lblLang.Text = T("lang");
        grpInfo.Text = T("info");
        lblTen.Text = T("ten"); lblDd.Text = T("diadiem"); lblNgay.Text = T("ngaydi");
        lblNguoi.Text = T("songuoi"); lblNs.Text = T("nganSach"); lblPt.Text = T("pt");
        chkDone.Text = T("done");
        btnThem.Text = T("add"); btnSua.Text = T("edit"); btnXoa.Text = T("del"); btnMoi.Text = T("clear");
        lblSearch.Text = T("search"); chkChuaXong.Text = T("onlyUndone");
        lblTu.Text = T("from"); lblDen.Text = T("to"); btnTim.Text = T("find");
        btnWeather.Text = T("weather"); lblWarn.Text = T("warn");

        grid.Columns["MaChuyen"]!.HeaderText = T("colMa");
        grid.Columns["TenChuyen"]!.HeaderText = T("colTen");
        grid.Columns["DiaDiem"]!.HeaderText = T("colDd");
        grid.Columns["NgayDi"]!.HeaderText = T("colNgay");
        grid.Columns["SoNguoi"]!.HeaderText = T("colNguoi");
        grid.Columns["NganSach"]!.HeaderText = T("colNs");
        grid.Columns["PhuongTien"]!.HeaderText = T("colPt");
        grid.Columns["DaHoanThanh"]!.HeaderText = T("colDone");

        // Định dạng ngày & tiền theo culture
        var dateFmt = lang == "vi" ? "dd/MM/yyyy" : "MM/dd/yyyy";
        grid.Columns["NgayDi"]!.DefaultCellStyle.Format = dateFmt;
        grid.Columns["NgayDi"]!.DefaultCellStyle.FormatProvider = c;
        grid.Columns["NganSach"]!.DefaultCellStyle.Format = "N0";
        grid.Columns["NganSach"]!.DefaultCellStyle.FormatProvider = c;
        dtNgay.CustomFormat = dateFmt; dtNgay.Format = DateTimePickerFormat.Custom;
        dtTu.CustomFormat = dateFmt; dtTu.Format = DateTimePickerFormat.Custom;
        dtDen.CustomFormat = dateFmt; dtDen.Format = DateTimePickerFormat.Custom;
        grid.Refresh();
        lblForecast.Text = ""; lblDesc.Text = ""; lblTemp.Text = ""; lblRain.Text = ""; lblIcon.Text = ""; pnlWarn.Visible = false;
    }

    void LoadSettings()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var doc = JsonDocument.Parse(File.ReadAllText(SettingsPath));
                if (doc.RootElement.TryGetProperty("Language", out var v)) lang = v.GetString() == "en" ? "en" : "vi";
            }
        }
        catch { /* bỏ qua file lỗi */ }
    }

    void SaveSettings()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
            File.WriteAllText(SettingsPath, JsonSerializer.Serialize(new { Language = lang }));
        }
        catch { }
    }

    // ===== CRUD + tìm kiếm =====
    void LoadGrid()
    {
        using var db = new QL_DaNgoaiContext();
        var q = db.ChuyenDis.AsQueryable();
        var kw = txtTim.Text.Trim();
        if (kw != "") q = q.Where(x => x.TenChuyen.Contains(kw) || x.DiaDiem.Contains(kw));
        if (chkChuaXong.Checked) q = q.Where(x => !x.DaHoanThanh);
        if (dtTu.Checked) { var d = DateOnly.FromDateTime(dtTu.Value); q = q.Where(x => x.NgayDi >= d); }
        if (dtDen.Checked) { var d = DateOnly.FromDateTime(dtDen.Value); q = q.Where(x => x.NgayDi <= d); }

        var list = q.OrderBy(x => x.NgayDi).AsEnumerable()
            .Select(x => new
            {
                x.MaChuyen, x.TenChuyen, x.DiaDiem,
                NgayDi = x.NgayDi.ToDateTime(TimeOnly.MinValue), // DateTime để format theo culture
                x.SoNguoi, x.NganSach, x.PhuongTien, x.DaHoanThanh
            }).ToList();
        grid.DataSource = list;
    }

    bool Validate(out string error)
    {
        error = "";
        if (txtTen.Text.Trim() == "" || txtDiaDiem.Text.Trim() == "") error = T("errName");
        else if (numNguoi.Value < 1) error = T("errPeople");
        else if (numNganSach.Value < 0) error = T("errBudget");
        return error == "";
    }

    void Them()
    {
        if (!Validate(out var err)) { Warn(err); return; }
        if (dtNgay.Value.Date < DateTime.Today) { Warn(T("errDate")); return; }
        using var db = new QL_DaNgoaiContext();
        db.ChuyenDis.Add(new ChuyenDi
        {
            TenChuyen = txtTen.Text.Trim(), DiaDiem = txtDiaDiem.Text.Trim(),
            NgayDi = DateOnly.FromDateTime(dtNgay.Value), SoNguoi = (int)numNguoi.Value,
            NganSach = numNganSach.Value, PhuongTien = cboPt.Text, DaHoanThanh = chkDone.Checked
        });
        db.SaveChanges();
        LoadGrid(); LamMoi();
    }

    int? SelectedId() => grid.CurrentRow?.Cells["MaChuyen"].Value is int id ? id : null;

    void Sua()
    {
        if (SelectedId() is not int id) { Warn(T("pick")); return; }
        if (!Validate(out var err)) { Warn(err); return; }
        using var db = new QL_DaNgoaiContext();
        var c = db.ChuyenDis.Find(id); if (c == null) return;
        c.TenChuyen = txtTen.Text.Trim(); c.DiaDiem = txtDiaDiem.Text.Trim();
        c.NgayDi = DateOnly.FromDateTime(dtNgay.Value); c.SoNguoi = (int)numNguoi.Value;
        c.NganSach = numNganSach.Value; c.PhuongTien = cboPt.Text; c.DaHoanThanh = chkDone.Checked;
        db.SaveChanges();
        LoadGrid();
    }

    void Xoa()
    {
        if (SelectedId() is not int id) { Warn(T("pick")); return; }
        if (MessageBox.Show(T("confirmDel"), T("notice"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        using var db = new QL_DaNgoaiContext();
        var c = db.ChuyenDis.Find(id); if (c == null) return;
        db.ChuyenDis.Remove(c); db.SaveChanges();
        LoadGrid(); LamMoi();
    }

    void LamMoi()
    {
        txtTen.Clear(); txtDiaDiem.Clear(); dtNgay.Value = DateTime.Today;
        numNguoi.Value = 1; numNganSach.Value = 0; cboPt.SelectedIndex = 0; chkDone.Checked = false;
        txtTen.Focus();
    }

    void FillFromSelected()
    {
        var r = grid.CurrentRow; if (r == null || r.Cells["MaChuyen"].Value == null) return;
        txtTen.Text = r.Cells["TenChuyen"].Value?.ToString();
        txtDiaDiem.Text = r.Cells["DiaDiem"].Value?.ToString();
        dtNgay.Value = (DateTime)r.Cells["NgayDi"].Value;
        numNguoi.Value = Math.Max(numNguoi.Minimum, Convert.ToDecimal(r.Cells["SoNguoi"].Value));
        numNganSach.Value = Convert.ToDecimal(r.Cells["NganSach"].Value);
        cboPt.Text = r.Cells["PhuongTien"].Value?.ToString();
        chkDone.Checked = (bool)r.Cells["DaHoanThanh"].Value;
    }

    void Warn(string msg) => MessageBox.Show(msg, T("notice"), MessageBoxButtons.OK, MessageBoxIcon.Warning);

    // ===== Thời tiết (Open-Meteo) =====
    async Task<(double lat, double lon, string name)?> GeocodeAsync(string place)
    {
        var url = $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(place)}&count=1&language=vi";
        using var doc = JsonDocument.Parse(await Http.GetStringAsync(url));
        if (!doc.RootElement.TryGetProperty("results", out var rs) || rs.GetArrayLength() == 0) return null;
        var r = rs[0];
        return (r.GetProperty("latitude").GetDouble(), r.GetProperty("longitude").GetDouble(), r.GetProperty("name").GetString() ?? place);
    }

    static (string icon, string vi, string en) Describe(int code) => code switch
    {
        0 => ("☀️", "Trời quang", "Clear sky"),
        >= 1 and <= 3 => ("⛅", "Có mây", "Cloudy"),
        45 or 48 => ("🌫️", "Sương mù", "Fog"),
        (>= 51 and <= 67) or (>= 80 and <= 82) => ("🌧️", "Mưa", "Rain"),
        >= 95 and <= 99 => ("⛈️", "Dông", "Thunderstorm"),
        _ => ("☁️", "Không rõ", "Unknown")
    };

    async Task ShowWeatherAsync()
    {
        var place = txtDiaDiem.Text.Trim();
        if (place == "") { Warn(T("noplace")); return; }
        var date = dtNgay.Value.Date;

        btnWeather.Enabled = false;
        progress.MarqueeAnimationSpeed = 30; // chạy Marquee
        pnlWarn.Visible = false;
        try
        {
            var geo = await GeocodeAsync(place);
            string note = "";
            if (geo == null) { geo = await GeocodeAsync("Đà Lạt"); note = T("notfound"); place = "Đà Lạt"; }
            if (geo == null) throw new Exception("geo");
            var (lat, lon, name) = geo.Value;

            var inv = CultureInfo.InvariantCulture;
            var url = $"https://api.open-meteo.com/v1/forecast?latitude={lat.ToString(inv)}&longitude={lon.ToString(inv)}" +
                      "&daily=weather_code,temperature_2m_max,temperature_2m_min,precipitation_probability_max" +
                      "&timezone=auto&forecast_days=16";
            using var doc = JsonDocument.Parse(await Http.GetStringAsync(url));
            var daily = doc.RootElement.GetProperty("daily");
            var times = daily.GetProperty("time").EnumerateArray().Select(t => t.GetString()).ToList();
            int i = times.IndexOf(date.ToString("yyyy-MM-dd"));

            lblForecast.Text = string.Format(T("forecast"), date.ToString("d", Culture), place) + (note != "" ? $"  ({note})" : "");
            if (i < 0)
            {
                lblIcon.Text = "❔"; lblDesc.Text = T("nodata"); lblTemp.Text = ""; lblRain.Text = "";
                return;
            }
            int code = daily.GetProperty("weather_code")[i].GetInt32();
            double tmax = daily.GetProperty("temperature_2m_max")[i].GetDouble();
            double tmin = daily.GetProperty("temperature_2m_min")[i].GetDouble();
            int rain = daily.GetProperty("precipitation_probability_max")[i].ValueKind == JsonValueKind.Null
                ? 0 : daily.GetProperty("precipitation_probability_max")[i].GetInt32();

            var d = Describe(code);
            lblIcon.Text = d.icon;
            lblDesc.Text = lang == "vi" ? d.vi : d.en;
            lblTemp.Text = string.Format(T("temp"), Math.Round(tmin), Math.Round(tmax));
            lblRain.Text = string.Format(T("rain"), rain);
            pnlWarn.Visible = rain >= 60;
        }
        catch
        {
            lblForecast.Text = ""; lblIcon.Text = "⚠️"; lblDesc.Text = ""; lblTemp.Text = T("neterr"); lblRain.Text = "";
        }
        finally
        {
            progress.MarqueeAnimationSpeed = 0;
            btnWeather.Enabled = true;
        }
    }
}
