IF DB_ID('QL_TraSua') IS NULL CREATE DATABASE QL_TraSua;
GO
USE QL_TraSua;
GO
CREATE TABLE DanhMucMon (
    MaDM  INT IDENTITY(1,1) PRIMARY KEY,
    TenDM NVARCHAR(50) NOT NULL
);
CREATE TABLE MonUong (
    MaMon   INT IDENTITY(1,1) PRIMARY KEY,
    TenMon  NVARCHAR(100) NOT NULL,
    GiaBan  DECIMAL(18,0) NOT NULL,
    DangBan BIT NOT NULL DEFAULT 1,
    MaDM    INT NOT NULL FOREIGN KEY REFERENCES DanhMucMon(MaDM)
);
CREATE TABLE HoaDon (
    MaHD        INT IDENTITY(1,1) PRIMARY KEY,
    NgayLap     DATETIME NOT NULL,
    TongTien    DECIMAL(18,0) NOT NULL,
    HinhThucTT  NVARCHAR(20) NOT NULL,
    DaThanhToan BIT NOT NULL
);
CREATE TABLE ChiTietHoaDon (
    MaHD   INT NOT NULL FOREIGN KEY REFERENCES HoaDon(MaHD),
    MaMon  INT NOT NULL FOREIGN KEY REFERENCES MonUong(MaMon),
    SoLy   INT NOT NULL,
    DonGia DECIMAL(18,0) NOT NULL,
    PRIMARY KEY (MaHD, MaMon)
);
GO

INSERT INTO DanhMucMon (TenDM) VALUES (N'Trà sữa'), (N'Trà trái cây'), (N'Topping'), (N'Đồ ăn vặt');

INSERT INTO MonUong (TenMon, GiaBan, DangBan, MaDM) VALUES
(N'Trà sữa trân châu',  35000, 1, (SELECT MaDM FROM DanhMucMon WHERE TenDM = N'Trà sữa')),
(N'Trà sữa matcha',     40000, 1, (SELECT MaDM FROM DanhMucMon WHERE TenDM = N'Trà sữa')),
(N'Hồng trà sữa',       32000, 1, (SELECT MaDM FROM DanhMucMon WHERE TenDM = N'Trà sữa')),
(N'Trà sữa khoai môn',  38000, 1, (SELECT MaDM FROM DanhMucMon WHERE TenDM = N'Trà sữa')),
(N'Trà đào cam sả',     35000, 1, (SELECT MaDM FROM DanhMucMon WHERE TenDM = N'Trà trái cây')),
(N'Trà vải',            32000, 1, (SELECT MaDM FROM DanhMucMon WHERE TenDM = N'Trà trái cây')),
(N'Trà dâu tằm',        36000, 1, (SELECT MaDM FROM DanhMucMon WHERE TenDM = N'Trà trái cây')),
(N'Trân châu trắng',     5000, 1, (SELECT MaDM FROM DanhMucMon WHERE TenDM = N'Topping')),
(N'Thạch dừa',           5000, 1, (SELECT MaDM FROM DanhMucMon WHERE TenDM = N'Topping')),
(N'Pudding',             7000, 1, (SELECT MaDM FROM DanhMucMon WHERE TenDM = N'Topping')),
(N'Khoai tây chiên',    25000, 1, (SELECT MaDM FROM DanhMucMon WHERE TenDM = N'Đồ ăn vặt')),
(N'Gà viên chiên',      30000, 1, (SELECT MaDM FROM DanhMucMon WHERE TenDM = N'Đồ ăn vặt')),
(N'Bánh tráng trộn',    20000, 1, (SELECT MaDM FROM DanhMucMon WHERE TenDM = N'Đồ ăn vặt')),
(N'Xúc xích nướng',     15000, 1, (SELECT MaDM FROM DanhMucMon WHERE TenDM = N'Đồ ăn vặt'));

-- 2 hóa đơn mẫu trong ngày để xem doanh thu
DECLARE @hd INT;
INSERT INTO HoaDon (NgayLap, TongTien, HinhThucTT, DaThanhToan) VALUES (GETDATE(), 70000, N'Tiền mặt', 1);
SET @hd = SCOPE_IDENTITY();
INSERT INTO ChiTietHoaDon VALUES (@hd, (SELECT MaMon FROM MonUong WHERE TenMon = N'Trà sữa trân châu'), 2, 35000);

INSERT INTO HoaDon (NgayLap, TongTien, HinhThucTT, DaThanhToan) VALUES (GETDATE(), 65000, N'Chuyển khoản', 1);
SET @hd = SCOPE_IDENTITY();
INSERT INTO ChiTietHoaDon VALUES (@hd, (SELECT MaMon FROM MonUong WHERE TenMon = N'Trà sữa matcha'), 1, 40000);
INSERT INTO ChiTietHoaDon VALUES (@hd, (SELECT MaMon FROM MonUong WHERE TenMon = N'Khoai tây chiên'), 1, 25000);
GO
