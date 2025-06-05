using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polycafe_DTO
{
    public class LoginDTO
    {
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string MatKhau { get; set; }
        public string VaiTro { get; set; }

    }
    public class ChangePasswordDTO
    {
        public string Email { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
    public class qlLSP
    {
        public string MaLoai { get; set; }
        public string TenLoai { get; set; }
        public string GhiChu { get; set; }
    }

    public class ThongKeNhanVien_DTO
    {
        public string MaNV { get; set; }
        public string TenNV { get; set; }
        public decimal TongTien { get; set; }
        public int SoLuongPhieu { get; set; }
        public DateTime NgayLapPhieu { get; set; }
        public string TrangThai { get; set; }
        public int SoLy { get; set; }
    }
    public class NhanVien_DTO
    {
        public string MaNV { get; set; }
        public string TenNV { get; set; }
    }
    public class SanPham_DTO
    {
        public string MaSP { get; set; }
        public string TenSP { get; set; }
    }

    public class ThongKeSanPham_DTO
    {
        public string MaSP { get; set; }
        public string TenSP { get; set; }
        public int SoLuongBan { get; set; }
        public decimal TongTien { get; set; }
        public DateTime NgayLapPhieu { get; set; }
    }
    public class SanPhamDTO
    {
        public string MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public string MaLoai { get; set; }
        public decimal DonGia { get; set; }
        public string TrangThai { get; set; }
        public string HinhAnh { get; set; }
    }
    // DTO for TheLuuDong (Card)
    public class CardDTO
    {
        public string CardId { get; set; } // Mã thẻ
        public string OwnerName { get; set; } // Chủ sở hữu
                                              // public bool Status { get; set; } // Có thể thêm TrangThai nếu cần
    }

    // DTO for NhanVien (Employee)
    public class EmployeeDto
    {
        public string EmployeeId { get; set; } // Mã nhân viên
        public string FullName { get; set; } // Họ tên
                                             // public string Email { get; set; } // Các trường khác nếu cần
                                             // public string Password { get; set; }
                                             // public bool Role { get; set; }
                                             // public bool Status { get; set; }
    }

    // DTO for SanPham (Product)
    public class ProductDTO
    {
        public string ProductId { get; set; } // Mã sản phẩm
        public string ProductName { get; set; } // Tên sản phẩm
        public decimal UnitPrice { get; set; } // Đơn giá
                                               // public string CategoryId { get; set; } // MaLoai
                                               // public string ImagePath { get; set; } // HinhAnh
                                               // public bool Status { get; set; } // TrangThai
    }

    // DTO for ChiTietPhieu (Sale Invoice Detail)
    public class SaleInvoiceDetailDTO
    {
        public int Id { get; set; } // ID tự sinh từ SQL Server
        public string InvoiceId { get; set; } // Khóa ngoại tới PhieuBanHang (MaPhieu)
        public string ProductId { get; set; } // Mã sản phẩm (MaSanPham)
        public string ProductName { get; set; } // Tên sản phẩm (TenSanPham), để hiển thị trên UI
        public int Quantity { get; set; } // Số lượng (SoLuong)
        public decimal UnitPrice { get; set; } // Đơn giá (DonGia)
        public decimal LineAmount { get; set; } // Thành tiền = SoLuong * DonGia
    }

    // DTO for PhieuBanHang (Sale Invoice)
    public class SaleInvoiceDTO
    {
        public string InvoiceId { get; set; } // Mã phiếu (MaPhieu)
        public string CardId { get; set; } // Mã thẻ (MaThe)
        public string CardOwnerName { get; set; } // Tên chủ thẻ (ChuSoHuu) - để hiển thị
        public string EmployeeId { get; set; } // Mã nhân viên (MaNhanVien)
        public string EmployeeName { get; set; } // Tên nhân viên (HoTen) - để hiển thị
        public DateTime CreatedDate { get; set; } // Ngày tạo (NgayTao)
        public bool Status { get; set; } // Trạng thái (TrangThai) - 0: Chờ xác nhận, 1: Đã thanh toán

        // Calculated fields (không có trong CSDL trực tiếp)
        public int TotalQuantity { get; set; } // Tổng số lượng sản phẩm trong phiếu
        public decimal TotalAmount { get; set; } // Tổng tiền của phiếu

        // List of invoice details
        public List<SaleInvoiceDetailDTO> InvoiceDetails { get; set; }

        public SaleInvoiceDTO()
        {
            InvoiceDetails = new List<SaleInvoiceDetailDTO>();
        }
    }
    public class QLTheLuuDongDTO
    {
        public string MaThe { get; set; } // Đã thay đổi từ int sang string để phù hợp với CHAR(6) trong DB
        public string ChuSoHuu { get; set; }
        public bool TrangThai { get; set; } // Đã thay đổi từ string sang bool để phù hợp với BIT trong DB

    }
    public class EmployeeDTO
    {
        public string EmployeeId { get; set; } // Mã NV
        public string FullName { get; set; } // Họ tên
        public string Email { get; set; } // Email
        public string Password { get; set; } // Mật khẩu
        public bool Role { get; set; } // Vai trò: True (1) = Quản lý, False (0) = Nhân viên
        public bool Status { get; set; } // Trạng thái: True (1) = Hoạt động, False (0) = Không hoạt động

        // Constructor mặc định
        public EmployeeDTO() { }

        // Constructor với đầy đủ các thuộc tính
        public EmployeeDTO(string employeeId, string fullName, string email, string password, bool role, bool status)
        {
            EmployeeId = employeeId;
            FullName = fullName;
            Email = email;
            Password = password;
            Role = role;
            Status = status;
        }
    }
    public class HoSo_DTO
    {
        public string HoTen { get; set; }
        public string Email { get; set; }
        public bool VaiTro { get; set; }
        public HoSo_DTO() { }

        public HoSo_DTO(string userName, string email, bool role)
        {
            HoTen = userName;
            Email = email;
            VaiTro = role;
        }

    }
}
