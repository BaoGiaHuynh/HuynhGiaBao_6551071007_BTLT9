# Sổ chi tiêu cá nhân (WinForms + EF Core Database First)

## Chạy
1. Chạy `Database/QL_ChiTieu.sql` trong SSMS (hoặc SQL Server Object Explorer của Visual Studio).
2. Mở thư mục này bằng Visual Studio (File > Open > Project/Solution > SoChiTieu.csproj), nhấn F5.
3. Nếu SQL Server khác LocalDB, sửa `ConnectionStrings:DefaultConnection` trong `appsettings.json`
   (hoặc `appsettings.Development.json`, file này ghi đè và được .gitignore).

## Scaffold lại Model/DbContext (task 1)
```
dotnet ef dbcontext scaffold "Server=(localdb)\MSSQLLocalDB;Database=QL_ChiTieu;Trusted_Connection=True;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer -o Models --context QlChiTieuContext --context-dir Data -t ChiTieu --force
```

## Publish sang máy khác (task 7)
```
dotnet publish -c Release -r win-x64 --self-contained false -o publish
```
Copy thư mục `publish` sang máy kia (đã cài .NET Desktop Runtime), sửa `appsettings.json`, chạy `SoChiTieu.exe`.
Log nằm ở thư mục `Logs` cạnh file .exe (`log-yyyyMMdd.txt`).
