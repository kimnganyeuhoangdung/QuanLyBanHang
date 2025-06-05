create database QLPolycafe 
use QLPolycafe

--Thẻ Lưu Động
CREATE TABLE TheLuuDong (
    MaThe CHAR(6) PRIMARY KEY,  
    ChuSoHuu NVARCHAR(100) NOT NULL,  
    TrangThai BIT NOT NULL DEFAULT 1  
);
GO
INSERT INTO TheLuuDong (MaThe, ChuSoHuu, TrangThai) VALUES
('THE001', N'Nguyễn Văn An', 1),
('THE002', N'Trần Thị Bình', 1),
('THE003', N'Phạm Minh Châu', 1),
('THE004', N'Đỗ Thành Công', 1),
('THE005', N'Lê Hải Đăng', 0),
('THE006', N'Hoàng Văn Đức', 1),
('THE007', N'Ngô Thị Hạnh', 0),
('THE008', N'Bùi Quốc Huy', 1),
('THE009', N'Lý Minh Hương', 1),
('THE010', N'Tạ Văn Hòa', 0),
('THE011', N'Vũ Thị Kim', 1),
('THE012', N'Đặng Thành Long', 1),
('THE013', N'Trương Ngọc Lan', 0),
('THE014', N'Cao Thị Mai', 1),
('THE015', N'Phan Văn Nam', 1),
('THE016', N'Kiều Bảo Ngọc', 0),
('THE017', N'Lâm Thị Oanh', 1),
('THE018', N'Đinh Văn Phúc', 1),
('THE019', N'Huỳnh Thị Quỳnh', 0),
('THE020', N'Mai Văn Quang', 1);
GO
--Nhân Viên 
CREATE TABLE NhanVien (
    MaNhanVien CHAR(6) PRIMARY KEY, 
    HoTen NVARCHAR(100) NOT NULL, 
    Email NVARCHAR(255) NOT NULL UNIQUE,  
    MatKhau NVARCHAR(255) NOT NULL,
    VaiTro BIT NOT NULL,  
    TrangThai BIT NOT NULL DEFAULT 1  
);
GO
INSERT INTO NhanVien (MaNhanVien, HoTen, Email, MatKhau, VaiTro, TrangThai) VALUES
('NV001', N'Nguyễn Thị Hoa', 'hoa.nguyen@cafe.com', 'password1', 1, 1),
('NV002', N'Trần Văn Minh', 'minh.tran@cafe.com', 'password2', 0, 1),
('NV003', N'Hoàng Thị Lan', 'lan.hoang@cafe.com', 'password3', 0, 1),
('NV004', N'Phạm Tuấn Kiệt', 'kiet.pham@cafe.com', 'password4', 0, 1),
('NV005', N'Lê Quốc Bảo', 'bao.le@cafe.com', 'password5', 0, 1),
('NV006', N'Vũ Thị Hồng', 'hong.vu@cafe.com', 'password6', 0, 1),
('NV007', N'Đặng Văn Long', 'long.dang@cafe.com', 'password7', 0, 1),
('NV008', N'Trịnh Thị Mai', 'mai.trinh@cafe.com', 'password8', 0, 1),
('NV009', N'Ngô Hoàng Nam', 'nam.ngo@cafe.com', 'password9', 0, 0),
('NV010', N'Cao Thị Oanh', 'oanh.cao@cafe.com', 'password10', 0, 1),
('NV011', N'Lý Minh Phúc', 'phuc.ly@cafe.com', 'password11', 0, 1),
('NV012', N'Tống Thị Quyên', 'quyen.tong@cafe.com', 'password12', 0, 1),
('NV013', N'Tạ Văn Sơn', 'son.ta@cafe.com', 'password13', 0, 0),
('NV014', N'Huỳnh Bảo Trâm', 'tram.huynh@cafe.com', 'password14', 0, 1),
('NV015', N'Đỗ Thị Uyên', 'uyen.do@cafe.com', 'password15', 0, 1),
('NV016', N'Mai Quốc Vinh', 'vinh.mai@cafe.com', 'password16', 0, 1),
('NV017', N'Trương Thị Xuân', 'xuan.truong@cafe.com', 'password17', 0, 0),
('NV018', N'Phan Văn Yên', 'yen.phan@cafe.com', 'password18', 0, 1),
('NV019', N'Kiều Thị Giang', 'giang.kieu@cafe.com', 'password19', 0, 1),
('NV020', N'Lâm Nhật Hào', 'hao.lam@cafe.com', 'password20', 0, 1);
GO

-- Bảng Loại Sản Phẩm
CREATE TABLE LoaiSanPham (
    MaLoai CHAR(6) PRIMARY KEY, 
    TenLoai NVARCHAR(100) NOT NULL UNIQUE,  
    GhiChu NVARCHAR(MAX) NULL  
);
GO
INSERT INTO LoaiSanPham (MaLoai, TenLoai, GhiChu) VALUES
('LSP001', N'Cà phê', N'Các loại cà phê nguyên chất và pha chế'),
('LSP002', N'Trà', N'Các loại trà thảo mộc và trái cây'),
('LSP003', N'Bánh ngọt', N'Bánh ngọt ăn kèm với đồ uống'),
('LSP004', N'Nước ép', N'Nước ép trái cây tươi'),
('LSP005', N'Sinh tố', N'Các loại sinh tố hoa quả'),
('LSP006', N'Nước ngọt', N'Nước uống đóng chai, nước có ga'),
('LSP007', N'Đồ ăn nhanh', N'Món ăn nhẹ như sandwich, xúc xích'),
('LSP008', N'Kem', N'Kem viên, kem ly, kem trái cây'),
('LSP009', N'Sữa chua', N'Sữa chua hoa quả, sữa chua uống'),
('LSP010', N'Sữa hạt', N'Sữa từ đậu nành, óc chó, hạnh nhân'),
('LSP011', N'Soda Ý', N'Nước soda pha chế phong cách Ý'),
('LSP012', N'Matcha', N'Đồ uống pha chế từ trà xanh Nhật Bản'),
('LSP013', N'Trà sữa', N'Trà sữa các loại, có topping'),
('LSP014', N'Thức uống đá xay', N'Đá xay vị cà phê, socola, trái cây'),
('LSP015', N'Cacao', N'Đồ uống nóng/lạnh từ bột cacao'),
('LSP016', N'Nước khoáng', N'Nước lọc và nước khoáng thiên nhiên'),
('LSP017', N'Sinh tố detox', N'Sinh tố rau củ tốt cho sức khỏe'),
('LSP018', N'Trà trái cây nhiệt đới', N'Trà kết hợp trái cây tươi'),
('LSP019', N'Bánh mặn', N'Bánh mì, bánh nhân thịt, xúc xích'),
('LSP020', N'Thức uống đặc biệt', N'Đồ uống sáng tạo theo mùa');
GO
-- Bảng Sản Phẩm
CREATE TABLE SanPham (
    MaSanPham CHAR(6) PRIMARY KEY,
    TenSanPham NVARCHAR(100) NOT NULL, 
    DonGia DECIMAL(10,0) NOT NULL CHECK (DonGia >= 0),  
    MaLoai CHAR(6) NOT NULL, 
    HinhAnh NVARCHAR(MAX) NULL, 
    TrangThai BIT NOT NULL DEFAULT 1,  
    CONSTRAINT FK_SanPham_Loai FOREIGN KEY (MaLoai) REFERENCES LoaiSanPham(MaLoai) 
        ON DELETE CASCADE ON UPDATE CASCADE
);
GO
INSERT INTO SanPham (MaSanPham, TenSanPham, DonGia, MaLoai, HinhAnh, TrangThai) VALUES
('SP001', N'Cà phê đen', 25000, 'LSP001', 'caphe_den.jpg', 1),
('SP002', N'Cà phê sữa', 30000, 'LSP001', 'caphe_sua.jpg', 1),
('SP003', N'Trà đào cam sả', 35000, 'LSP002', 'tra_dao_cam_sa.jpg', 1),
('SP004', N'Bánh Tiramisu', 40000, 'LSP003', 'banh_tiramisu.jpg', 1),
('SP005', N'Nước ép cam', 45000, 'LSP004', 'nuoc_ep_cam.jpg', 1),
('SP006', N'Sinh tố bơ', 40000, 'LSP005', 'sinh_to_bo.jpg', 1),
('SP007', N'Sinh tố xoài', 40000, 'LSP005', 'sinh_to_xoai.jpg', 1),
('SP008', N'Nước ngọt Coca', 20000, 'LSP006', 'coca.jpg', 1),
('SP009', N'Nước ngọt Pepsi', 20000, 'LSP006', 'pepsi.jpg', 1),
('SP010', N'Sandwich trứng', 30000, 'LSP007', 'sandwich_trung.jpg', 1),
('SP011', N'Xúc xích phô mai', 25000, 'LSP007', 'xuc_xich_pho_mai.jpg', 1),
('SP012', N'Kem dâu', 30000, 'LSP008', 'kem_dau.jpg',1 ),
('SP013', N'Kem vani', 30000, 'LSP008', 'kem_vani.jpg', 1),
('SP014', N'Sữa chua nếp cẩm', 25000, 'LSP009', 'sua_chua_nep_cam.jpg', 1),
('SP015', N'Sữa hạt óc chó', 35000, 'LSP010', 'sua_oc_cho.jpg', 1),
('SP016', N'Soda chanh dây', 35000, 'LSP011', 'soda_chanh_day.jpg', 1),
('SP017', N'Matcha đá xay', 45000, 'LSP012', 'matcha_da_xay.jpg', 1),
('SP018', N'Trà sữa trân châu', 40000, 'LSP013', 'tra_sua_tran_chau.jpg', 1),
('SP019', N'Cà phê đá xay socola', 50000, 'LSP014', 'caphe_socola.jpg', 1),
('SP020', N'Cacao nóng', 35000, 'LSP015', 'cacao_nong.jpg', 1);
GO
CREATE TABLE PhieuBanHang (
    MaPhieu CHAR(6) PRIMARY KEY,  
    MaThe CHAR(6) NULL,  
    MaNhanVien CHAR(6) NULL,  
    NgayTao DATETIME NOT NULL DEFAULT GETDATE(),  
    TrangThai BIT NOT NULL DEFAULT 0,  
    CONSTRAINT FK_PhieuBanHang_The FOREIGN KEY (MaThe) REFERENCES TheLuuDong(MaThe) 
        ON DELETE SET NULL ON UPDATE CASCADE,
    CONSTRAINT FK_PhieuBanHang_NhanVien FOREIGN KEY (MaNhanVien) REFERENCES NhanVien(MaNhanVien) 
        ON DELETE SET NULL ON UPDATE CASCADE
);
GO
INSERT INTO PhieuBanHang (MaPhieu, MaThe, MaNhanVien, NgayTao, TrangThai) VALUES
('PBH001', 'THE001', 'NV014', '2024-01-10 09:00:00', 1),
('PBH002', 'THE002', 'NV007', '2024-01-20 14:30:00', 1),
('PBH003', 'THE003', 'NV020', '2024-02-05 08:45:00', 1),
('PBH004', 'THE004', 'NV002', '2024-02-18 10:15:00', 1),
('PBH005', 'THE005', 'NV010', '2024-03-03 11:20:00', 1),
('PBH006', 'THE006', 'NV005', '2024-03-15 13:00:00', 1),
('PBH007', 'THE007', 'NV018', '2024-04-07 09:10:00', 1),
('PBH008', 'THE008', 'NV009', '2024-04-21 15:25:00', 1),
('PBH009', 'THE009', 'NV003', '2024-05-02 10:00:00', 1),
('PBH010', 'THE010', 'NV013', '2024-05-19 12:45:00', 1),
('PBH011', 'THE011', 'NV016', '2024-06-01 09:30:00', 1),
('PBH012', 'THE012', 'NV004', '2024-06-15 14:10:00', 1),
('PBH013', 'THE013', 'NV019', '2024-07-03 11:40:00', 1),
('PBH014', 'THE014', 'NV011', '2024-07-17 08:50:00', 1),
('PBH015', 'THE015', 'NV012', '2024-08-05 13:20:00', 1),
('PBH016', 'THE016', 'NV001', '2024-08-22 16:00:00', 1),
('PBH017', 'THE017', 'NV017', '2024-09-04 10:35:00', 1),
('PBH018', 'THE018', 'NV006', '2024-09-12 11:55:00', 1),
('PBH019', 'THE019', 'NV015', '2024-09-19 09:15:00', 1),
('PBH020', 'THE020', 'NV008', '2024-09-30 14:45:00', 1);
GO
-- Bảng Chi Tiết Phiếu
CREATE TABLE ChiTietPhieu (
    Id INT IDENTITY(1,1) PRIMARY KEY,  
    MaPhieu CHAR(6) NOT NULL,  
    MaSanPham CHAR(6) NOT NULL,  
    SoLuong INT NOT NULL CHECK (SoLuong > 0),  
    DonGia DECIMAL(10,0) NOT NULL CHECK (DonGia >= 0),  
    CONSTRAINT FK_ChiTietPhieu_Phieu FOREIGN KEY (MaPhieu) REFERENCES PhieuBanHang(MaPhieu) 
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT FK_ChiTietPhieu_SanPham FOREIGN KEY (MaSanPham) REFERENCES SanPham(MaSanPham) 
        ON DELETE CASCADE ON UPDATE CASCADE
);
GO
INSERT INTO ChiTietPhieu (MaPhieu, MaSanPham, SoLuong, DonGia) VALUES
('PBH001', 'SP001', 2, 25000),
('PBH001', 'SP003', 1, 35000),
('PBH002', 'SP002', 1, 30000),
('PBH002', 'SP005', 1, 45000),
('PBH003', 'SP004', 2, 40000),
('PBH003', 'SP001', 1, 25000),
('PBH004', 'SP002', 1, 30000),
('PBH004', 'SP003', 2, 35000),
('PBH005', 'SP005', 1, 45000),
('PBH006', 'SP001', 1, 25000),
('PBH006', 'SP004', 1, 40000),
('PBH006', 'SP003', 1, 35000),
('PBH007', 'SP002', 2, 30000),
('PBH008', 'SP001', 1, 25000),
('PBH008', 'SP005', 1, 45000),
('PBH009', 'SP004', 2, 40000),
('PBH010', 'SP003', 1, 35000),
('PBH010', 'SP002', 2, 30000),
('PBH011', 'SP005', 1, 45000),
('PBH011', 'SP001', 1, 25000),
('PBH012', 'SP002', 1, 30000),
('PBH013', 'SP004', 1, 40000),
('PBH014', 'SP003', 1, 35000),
('PBH014', 'SP005', 1, 45000),
('PBH015', 'SP002', 2, 30000),
('PBH016', 'SP001', 1, 25000),
('PBH016', 'SP003', 1, 35000),
('PBH017', 'SP004', 2, 40000),
('PBH018', 'SP005', 1, 45000),
('PBH019', 'SP002', 1, 30000),
('PBH020', 'SP001', 2, 25000),
('PBH020', 'SP004', 1, 40000);
GO

SELECT 
   t.MaThe, 
    t.ChuSoHuu, 
   pb.NgayTao, 
   t.TrangThai,
   sp.TenSanPham,
   ct.SoLuong
   FROM TheLuuDong t
   INNER JOIN PhieuBanHang pb ON t.MaThe = pb.MaThe
   INNER JOIN ChiTietPhieu ct ON pb.MaPhieu = ct.MaPhieu
   INNER JOIN SanPham sp ON ct.MaSanPham = sp.MaSanPham



    SELECT NV.MaNhanVien,
        NV.HoTen,
        PB.NgayTao,
        PB.TrangThai,
        COUNT(ChiTietPhieu.SoLuong) AS [SoLuongPhieu],
        SUM(ChiTietPhieu.DonGia * ChiTietPhieu.SoLuong) AS [TongTien]
 FROM NhanVien NV
 INNER JOIN PhieuBanHang PB ON PB.MaNhanVien = NV.MaNhanVien
 INNER JOIN ChiTietPhieu ON ChiTietPhieu.MaPhieu = PB.MaPhieu
 WHERE (NV.MaNhanVien IS NULL OR NV.MaNhanVien = NV.MaNhanVien)
       AND PB.NgayTao BETWEEN PB.NgayTao AND PB.NgayTao
 GROUP BY NV.MaNhanVien, NV.HoTen, PB.TrangThai, PB.NgayTao

SELECT * FROM ChiTietPhieu