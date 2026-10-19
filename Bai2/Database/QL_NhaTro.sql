-- Chạy script này trong SSMS trước khi chạy ứng dụng
IF DB_ID(N'QL_NhaTro') IS NULL
    CREATE DATABASE QL_NhaTro;
GO
USE QL_NhaTro;
GO
IF OBJECT_ID(N'dbo.KhachThue', N'U') IS NOT NULL DROP TABLE dbo.KhachThue;
IF OBJECT_ID(N'dbo.PhongTro', N'U') IS NOT NULL DROP TABLE dbo.PhongTro;
GO
CREATE TABLE dbo.PhongTro (
    MaPhong   INT IDENTITY(1,1) PRIMARY KEY,
    TenPhong  NVARCHAR(20)  NOT NULL,
    GiaThue   DECIMAL(18,0) NOT NULL,
    TinhTrang NVARCHAR(20)  NOT NULL DEFAULT N'Trống'   -- 'Trống' | 'Đang thuê'
);
CREATE TABLE dbo.KhachThue (
    MaKhach        INT IDENTITY(1,1) PRIMARY KEY,
    HoTen          NVARCHAR(100) NOT NULL,
    GioiTinh       NVARCHAR(5)   NOT NULL,
    NgaySinh       DATE          NOT NULL,
    SoDienThoai    NVARCHAR(15)  NOT NULL,
    MaTinh         INT           NULL,
    TenTinh        NVARCHAR(100) NULL,
    MaPhuongXa     INT           NULL,
    TenPhuongXa    NVARCHAR(100) NULL,
    DaDangKyTamTru BIT           NOT NULL DEFAULT 0,
    MaPhong        INT           NOT NULL,
    CONSTRAINT FK_KhachThue_PhongTro FOREIGN KEY (MaPhong) REFERENCES dbo.PhongTro(MaPhong)
);
GO
INSERT INTO dbo.PhongTro (TenPhong, GiaThue, TinhTrang) VALUES
(N'P01', 1500000, N'Đang thuê'),
(N'P02', 1600000, N'Đang thuê'),
(N'P03', 1800000, N'Đang thuê'),
(N'P04', 2000000, N'Đang thuê'),
(N'P05', 1700000, N'Trống'),
(N'P06', 1900000, N'Trống');
GO
INSERT INTO dbo.KhachThue (HoTen, GioiTinh, NgaySinh, SoDienThoai, MaTinh, TenTinh, MaPhuongXa, TenPhuongXa, DaDangKyTamTru, MaPhong) VALUES
(N'Nguyễn Văn An',   N'Nam', '2004-05-12', '0912345678', 1,    N'Thành phố Hà Nội', 4,    N'Phường Ba Đình',  1, 3),
(N'Nguyễn Thị Lan',  N'Nữ',  '2005-02-20', '0987000111', 1,    N'Thành phố Hà Nội', 4,    N'Phường Ba Đình',  0, 3),
(N'Trần Thị Bình',   N'Nữ',  '2003-09-03', '0876543210', NULL, N'Tỉnh Ninh Bình',   NULL, N'Phường Nam Định', 0, 1),
(N'Lê Văn Cường',    N'Nam', '2002-11-25', '0901234567', NULL, N'Tỉnh Thanh Hóa',   NULL, N'Phường Hạc Thành',1, 2),
(N'Phạm Minh Đức',   N'Nam', '2001-07-08', '0987654321', NULL, N'Tỉnh Thanh Hóa',   NULL, N'Phường Hạc Thành',0, 4);
GO
