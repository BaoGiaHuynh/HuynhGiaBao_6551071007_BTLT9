# Quản lý nhà trọ (WinForms + EF Core Database First + API hành chính VN)

## Chạy
1. Chạy `Database/QL_NhaTro.sql` trong SSMS.
2. Mở `QuanLyNhaTro.csproj` bằng Visual Studio, nhấn F5.
3. Đổi SQL Server: sửa `ConnectionStrings:DefaultConnection` trong `appsettings.json`
   (đồng thời xóa/sửa `appsettings.Development.json` nếu bạn tự tạo file này, vì nó ghi đè khi đặt môi trường Development).

## Scaffold lại (task 1)
```
dotnet ef dbcontext scaffold "Server=localhost\SQLEXPRESS;Database=QL_NhaTro;Trusted_Connection=True;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer -o Models --context QlNhaTroContext --context-dir Data --force
```

## Publish
```
dotnet publish -c Release -r win-x64 --self-contained false -o bin\Release\net10.0-windows\publish\win-x64
```
Log: thư mục `Logs` cạnh file .exe (`log-yyyyMMdd.txt`).

## Thử các kịch bản trong "Kết quả mong đợi"
- Tắt Wi-Fi rồi mở form: app không văng, hiện thông báo, Tỉnh/Phường gõ tay được, vẫn lưu khách.
- Bấm "Gây lỗi thử": hiện hộp thoại thân thiện, app chạy tiếp, log có stack trace.
  (Nếu đang chạy bằng F5 mà Visual Studio dừng lại ở dòng ném lỗi, nhấn F5/Continue, hoặc chạy bằng Ctrl+F5.)
