-- Chạy script này trong SSMS / Visual Studio (SQL Server Object Explorer) trước khi chạy ứng dụng
IF DB_ID(N'QL_ChiTieu') IS NULL
    CREATE DATABASE QL_ChiTieu;
GO
USE QL_ChiTieu;
GO
IF OBJECT_ID(N'dbo.ChiTieu', N'U') IS NOT NULL DROP TABLE dbo.ChiTieu;
GO
CREATE TABLE dbo.ChiTieu (
    MaCT     INT IDENTITY(1,1) PRIMARY KEY,
    NgayChi  DATE           NOT NULL,
    NoiDung  NVARCHAR(100)  NOT NULL,
    LoaiChi  NVARCHAR(30)   NOT NULL,
    SoTien   DECIMAL(18,0)  NOT NULL,
    GhiChu   NVARCHAR(200)  NULL
);
GO
INSERT INTO dbo.ChiTieu (NgayChi, NoiDung, LoaiChi, SoTien, GhiChu) VALUES
('2026-10-01', N'Cơm trưa',        N'Ăn uống', 35000,  NULL),
('2026-10-02', N'Xăng xe',         N'Đi lại',  60000,  NULL),
('2026-10-03', N'Photo giáo trình',N'Học tập', 20000,  N'Môn C'),
('2026-10-04', N'Xem phim',        N'Giải trí',90000,  NULL),
('2026-10-05', N'Trà sữa',         N'Ăn uống', 40000,  NULL),
('2026-10-06', N'Mua sắm',         N'Khác',    150000, NULL),
('2026-10-07', N'Tiền điện',       N'Khác',    500000, N'Tháng 9'),
('2026-10-08', N'Ăn sáng',         N'Ăn uống', 25000,  NULL),
('2026-10-09', N'Vé xe buýt',      N'Đi lại',  15000,  NULL),
('2026-10-10', N'Mua sách',        N'Học tập', 120000, NULL),
('2026-10-12', N'Cà phê với bạn',  N'Ăn uống', 45000,  NULL);
GO
