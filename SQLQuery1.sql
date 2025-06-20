USE [QLPolycafe]
GO
/****** Object:  StoredProcedure [dbo].[sp_Top5SanPhamBanChay]    Script Date: 6/2/2025 3:23:44 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[sp_Top5SanPhamBanChay]
    @TuNgay DATE,
    @DenNgay DATE
AS
BEGIN
    SELECT TOP 5 
        sp.TenSanPham, 
        SUM(ct.SoLuong) AS TongSoLuong
    FROM 
        ChiTietPhieu ct
    JOIN 
        SanPham sp ON ct.MaSanPham = sp.MaSanPham
    JOIN 
        PhieuBanHang pb ON ct.MaPhieu = pb.MaPhieu
    WHERE 
        pb.NgayTao BETWEEN @TuNgay AND @DenNgay
    GROUP BY 
        sp.TenSanPham
    ORDER BY 
        TongSoLuong DESC
END

select * from NhanVien