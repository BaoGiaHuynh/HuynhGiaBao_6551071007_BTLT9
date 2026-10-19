CREATE DATABASE QL_DaNgoai;
GO
USE QL_DaNgoai;
GO
CREATE TABLE ChuyenDi (
    MaChuyen     INT IDENTITY(1,1) PRIMARY KEY,
    TenChuyen    NVARCHAR(100) NOT NULL,
    DiaDiem      NVARCHAR(100) NOT NULL,
    NgayDi       DATE NOT NULL,
    SoNguoi      INT NOT NULL,
    NganSach     DECIMAL(18,0) NOT NULL,
    PhuongTien   NVARCHAR(30) NOT NULL,
    DaHoanThanh  BIT NOT NULL DEFAULT 0,
    GhiChu       NVARCHAR(200) NULL
);
GO
-- 8 chuyến, ngày đi từ hôm nay đến vài tuần tới
INSERT INTO ChuyenDi (TenChuyen, DiaDiem, NgayDi, SoNguoi, NganSach, PhuongTien, DaHoanThanh, GhiChu) VALUES
(N'Cắm trại đồi chè',   N'Đà Lạt',   CAST(DATEADD(DAY,1,GETDATE()) AS DATE),  6, 3500000, N'Ô tô',     0, NULL),
(N'Tắm biển Mũi Né',    N'Phan Thiết',CAST(DATEADD(DAY,3,GETDATE()) AS DATE),  4, 2500000, N'Xe khách', 0, NULL),
(N'Trekking Sa Pa',     N'Sa Pa',    CAST(DATEADD(DAY,5,GETDATE()) AS DATE),  5, 4500000, N'Tàu hỏa',  0, NULL),
(N'Dạo phố cổ',         N'Hội An',   CAST(DATEADD(DAY,7,GETDATE()) AS DATE),  3, 3000000, N'Xe máy',   0, NULL),
(N'Biển Vũng Tàu',      N'Vũng Tàu', CAST(DATEADD(DAY,10,GETDATE()) AS DATE), 8, 2000000, N'Ô tô',     0, NULL),
(N'Cắm trại hồ Dầu Tiếng',N'Tây Ninh',CAST(DATEADD(DAY,14,GETDATE()) AS DATE),6, 1500000, N'Xe máy',   0, NULL),
(N'Thác Bản Giốc',      N'Cao Bằng', CAST(DATEADD(DAY,17,GETDATE()) AS DATE), 4, 5000000, N'Xe khách', 0, NULL),
(N'Đảo Phú Quốc',       N'Phú Quốc', CAST(DATEADD(DAY,21,GETDATE()) AS DATE), 5, 6000000, N'Ô tô',     0, NULL);
GO
