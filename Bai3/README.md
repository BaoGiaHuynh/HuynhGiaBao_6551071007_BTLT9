# Kế hoạch dã ngoại cuối tuần (WinForms + EF Core + Open-Meteo)

## Chạy
1. Mở SSMS, chạy `Database/QL_DaNgoai.sql` (tạo CSDL QL_DaNgoai, bảng ChuyenDi, 8 chuyến mẫu).
2. Sửa chuỗi kết nối trong `appsettings.json` cho đúng SQL Server của bạn
   (ví dụ `Server=.\\SQLEXPRESS;...` hoặc `Server=(localdb)\\MSSQLLocalDB;...`).
3. Mở `KeHoachDaNgoai.csproj` bằng Visual Studio 2022, F5. (Cần .NET 8 SDK + workload .NET desktop.)

## Cấu trúc
- `FormChuyenDi.cs`  : toàn bộ giao diện dựng bằng code (không cần Designer), CRUD, tìm kiếm, thời tiết, đa ngôn ngữ
- `Models/`          : entity ChuyenDi
- `Data/`            : DbContext
- `Helpers/`         : đọc appsettings.json
- `Database/`        : script SQL
- Cài đặt ngôn ngữ lưu tại `%LocalAppData%\KeHoachDaNgoai\usersettings.json`
