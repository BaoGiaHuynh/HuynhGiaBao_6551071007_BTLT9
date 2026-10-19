# Bán hàng quán trà sữa (WinForms + EF Core + VietQR + Serilog)

## Chạy
1. SSMS: chạy `Database/QL_TraSua.sql` (4 bảng, 4 danh mục, 14 món, 2 hóa đơn mẫu).
2. Sửa `appsettings.json`:
   - `ConnectionString`: đúng SQL Server của bạn.
   - `Payment`: `BankId` (VCB, MB, ACB, TCB, BIDV...), `AccountNo`, `AccountName`, `Template` (compact2...).
     **Dùng tài khoản của chính bạn.** File này nằm trong `.gitignore`, chỉ commit `appsettings.example.json`.
3. Mở `QuanTraSua.csproj` bằng Visual Studio 2022 (.NET 8) và nhấn F5.

Đổi thông tin ngân hàng trong appsettings.json (file trong thư mục bin\Debug\net8.0-windows nếu đang chạy) -> QR mới dùng tài khoản mới, không cần sửa code.

## Cấu trúc
- `FormBanHang.cs`  : lập đơn, thanh toán tiền mặt / chuyển khoản VietQR
- `FormThucDon.cs`  : CRUD món + tìm kiếm (join danh mục)
- `FormHoaDon.cs`   : danh sách hóa đơn theo ngày + chi tiết + doanh thu ngày
- `Helpers/AppConfig.cs`, `Services/VietQrService.cs`, `Models/`, `Data/`, `Database/`
- Log (Serilog): `%LocalAppData%\QuanTraSua\logs\log-YYYYMMDD.txt` (Information khi tạo QR, Error khi tải QR lỗi; số tài khoản được che)
