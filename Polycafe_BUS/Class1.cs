using DocumentFormat.OpenXml.Bibliography;
using Polycafe_DAL;
using Polycafe_DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polycafe_BUS
{
    public class LoginBUS
    {
        private LoginDAL dal = new LoginDAL();

        public bool KiemTraDangNhap(string username, string password)
        {
            string storedPassword = dal.GetMatKhau(username);
            return storedPassword != null && storedPassword == password.Trim();
        }

        public bool LayVaiTro(string username)
        {
            return dal.GetVaiTro(username);
        }
        public string LayTenNhanVien(string email)
        {
            return dal.LayTenTheoEmail(email);
        }
    }
    public class NhanVienBLL
    {
        private NhanVienDAL nhanVienDAL;

        public NhanVienBLL(string connectionString)
        {
            nhanVienDAL = new NhanVienDAL(connectionString);
        }

        public bool CheckAccount(string email, string password)
        {
            try
            {
                return nhanVienDAL.CheckCredentials(email, password);
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu cần
                throw new Exception("Lỗi khi kiểm tra đăng nhập: " + ex.Message);
            }
        }

        public bool ChangePassword(ChangePasswordDTO changePasswordDTO)
        {
            return nhanVienDAL.UpdatePassword(changePasswordDTO.Email, changePasswordDTO.NewPassword);
        }
    }
    public class qlLSP_BUS
    {
        private qlLSP_DAL dal = new qlLSP_DAL();

        public DataTable get()
        {
            DataTable dt = dal.GetAll();
            if (dt == null)
            {
                // Nếu DAL trả về null, trả về một DataTable trống để tránh lỗi NullReferenceException
                return new DataTable();
            }
            return dt;
        }
        public bool CheckTenLoaiExists(string tenLoai)
        {
            return dal.GetAll().AsEnumerable()
                      .Any(row => row.Field<string>("TenLoai").Equals(tenLoai, StringComparison.OrdinalIgnoreCase));
        }

        public bool GetVaiTroByEmail(string email)
        {
            return dal.GetVaiTroByEmail(email);
        }
        public DataTable GetAllLoaiSanPham()
        {
            return dal.GetAll(); // Hàm này lấy toàn bộ loại sản phẩm từ DAL
        }


        public bool check(string maloai)
        {
            return dal.Check(maloai);
        }

        public bool add(qlLSP lsp)
        {
            return dal.Add(lsp) > 0;
        }

        public bool update(qlLSP lsp)
        {
            return dal.Update(lsp) > 0;
        }

        public bool delete(qlLSP lsp)
        {
            return dal.Delete(lsp) > 0;
        }

        public List<string> Loadmaloai()
        {
            return dal.LoadMaLoai();
        }
    }
    public class ThongkeNVBUS
    {
        private ThongkeNVDAL dal = new ThongkeNVDAL();

        public bool GetVaiTroByEmail(string email)
        {
            return dal.GetVaiTroByEmail(email);
        }

        public DataTable getEmp()
        {
            return dal.GetNV();
        }

        public DataTable getTK(string MaNV, DateTime startDate, DateTime endDate)
        {
            return dal.GetTK(MaNV, startDate, endDate);
        }

        public DataTable GetThongKe(string MaNV, DateTime startDate, DateTime endDate)
        {
            return dal.GetThongKeDoanhThu(MaNV, startDate, endDate);
        }

        public List<string> LoadmaNV()
        {
            return dal.LoadMaNV();
        }

        public DataTable getTKSP(string MaSP, DateTime startDate, DateTime endDate)
        {
            return dal.GetTKSP(MaSP, startDate, endDate);
        }

        public List<qlLSP> LoadmaSP()
        {
            return dal.LoadMaSP();
        }

        public DataTable LayThongTinThongKe(string type, DateTime tuNgay, DateTime denNgay)
        {
            string dalType = "";
            switch (type)
            {
                case "Theo Tháng":
                    dalType = "monthly";
                    break;
                case "Theo Tuần":
                    dalType = "weekly";
                    break;
                case "Theo Quý":
                    dalType = "quarterly";
                    break;
                default:
                    dalType = type.ToLower();
                    break;
            }

            return dal.GetStatisticalData(dalType, tuNgay, denNgay);
        }

        public DataTable LayTop5SanPhamBanChay(DateTime tuNgay, DateTime denNgay)
        {
            return dal.LayTop5SanPhamBanChay(tuNgay, denNgay);
        }

        public DataTable LayDoanhThuTheoThang(DateTime tuNgay, DateTime denNgay)
        {
            return dal.LayDoanhThuTheoThang(tuNgay, denNgay);
        }

        public DataTable Get(string maSP, DateTime tuNgay, DateTime denNgay)
        {
            return dal.GetThongKeDoanhThuSP(maSP, tuNgay, denNgay);
        }

        public DataTable LayThongKeSoPhieuTheoSanPham(string maLoai, DateTime tuNgay, DateTime denNgay)
        {
            return dal.ThongKeSoPhieuTheoSanPham(maLoai, tuNgay, denNgay);
        }
    }

    public class SanPhamBUS
    {
        private SanPhamDAL dal = new SanPhamDAL();
        public DataTable GetAllSanPham()
        {
            return dal.GetAllSanPham();
        }
        public bool IsProductLinkedToSalesDetail(string maSanPham)
        {
            return dal.IsProductInChiTietPhieu(maSanPham);
        }
        public bool CheckTenSanPhamExists(string tenLoai)
        {
            return dal.GetAllSanPham().AsEnumerable()
                          .Any(row => row.Field<string>("TenSanPham").Equals(tenLoai, StringComparison.OrdinalIgnoreCase));
            }
            public bool GetVaiTroByEmail(string email)
            {
            return dal.GetVaiTroByEmail(email);
        }

        public bool AddSanPham(SanPhamDTO sanPham)
        {
            return dal.AddSanPham(sanPham);
        }
        public bool UpdateSanPham(SanPhamDTO sanPham)
        {
            return dal.UpdateSanPham(sanPham);
        }
        public bool DeleteSanPham(string maSanPham)
        {
            return dal.DeleteSanPham(maSanPham);
        }
        public DataTable SearchSanPham(string searchTerm)
        {
            return dal.SearchSanPham(searchTerm);
        }
        public DataTable GetLoaiSanPham()
        {
            return dal.GetLoaiSanPham();
        }
        public List<SanPhamDTO> GetSanPhamByMaLoai(string maLoai)
        {
            return dal.GetByMaLoai(maLoai);
        }

        public bool DeleteSanPhamByMaLoai(string maLoai)
        {
            return dal.DeleteByMaLoai(maLoai);
        }
    }
    public class SaleInvoiceBLL
    {
        private SaleInvoiceDAL _saleInvoiceDAL;
        private ProductDAL _productDAL; // Có thể cần một DAL riêng cho sản phẩm nếu có nhiều thao tác

        public SaleInvoiceBLL()
        {
            _saleInvoiceDAL = new SaleInvoiceDAL();
            // Giả định bạn có một DAL riêng cho Product để dễ dàng lấy thông tin sản phẩm
            // Hoặc bạn có thể thêm các phương thức GetAllProducts, GetProductUnitPrice vào SaleInvoiceDAL
            // Ở đây tôi sẽ giả định ProductDAL riêng để minh họa cách chia nhỏ DAL nếu cần.
            // Nếu không, hãy sử dụng _saleInvoiceDAL.GetAllProducts() và _saleInvoiceDAL.GetProductUnitPrice()
            _productDAL = new ProductDAL(); // Bạn sẽ cần tạo lớp ProductDAL này
        }

        // --- Các phương thức hỗ trợ ComboBox ---
        public List<CardDTO> GetAllCards()
        {
            return _saleInvoiceDAL.GetAllCards();
        }

        public List<EmployeeDto> GetAllEmployees()
        {
            return _saleInvoiceDAL.GetAllEmployees();
        }

        public List<ProductDTO> GetAllProducts()
        {
            return _productDAL.GetAllProducts(); // Hoặc _saleInvoiceDAL.GetAllProducts()
        }

        public decimal GetProductUnitPrice(string productId)
        {
            return _productDAL.GetProductUnitPrice(productId); // Hoặc _saleInvoiceDAL.GetProductUnitPrice()
        }

        // --- Các phương thức cho Phiếu Bán Hàng chính ---

        public List<SaleInvoiceDTO> GetAllSaleInvoices()
        {
            return _saleInvoiceDAL.GetAllSaleInvoices();
        }

        public SaleInvoiceDTO GetSaleInvoiceById(string invoiceId)
        {
            // Khi lấy phiếu, DAL đã tự động tính toán TotalQuantity và TotalAmount
            return _saleInvoiceDAL.GetSaleInvoiceById(invoiceId);
        }

        public bool AddSaleInvoice(SaleInvoiceDTO invoice)
        {
            // Logic nghiệp vụ:
            // 1. Kiểm tra mã phiếu có trùng không (nếu MaPhieu là do người dùng nhập)
            if (_saleInvoiceDAL.GetSaleInvoiceById(invoice.InvoiceId) != null)
            {
                // Throw exception hoặc trả về false với thông báo lỗi
                throw new Exception("Mã phiếu đã tồn tại. Vui lòng chọn mã khác.");
            }

            // 2. Tính toán TotalQuantity và TotalAmount trước khi lưu
            invoice.TotalQuantity = invoice.InvoiceDetails.Sum(d => d.Quantity);
            invoice.TotalAmount = invoice.InvoiceDetails.Sum(d => d.LineAmount);

            // 3. Gọi DAL để thêm
            return _saleInvoiceDAL.AddSaleInvoice(invoice);
        }

        public bool UpdateSaleInvoice(SaleInvoiceDTO invoice)
        {
            // Logic nghiệp vụ:
            // 1. Kiểm tra phiếu có tồn tại không
            if (_saleInvoiceDAL.GetSaleInvoiceById(invoice.InvoiceId) == null)
            {
                throw new Exception("Không tìm thấy phiếu bán hàng cần cập nhật.");
            }

            // 2. Tính toán TotalQuantity và TotalAmount trước khi lưu
            invoice.TotalQuantity = invoice.InvoiceDetails.Sum(d => d.Quantity);
            invoice.TotalAmount = invoice.InvoiceDetails.Sum(d => d.LineAmount);

            // 3. Gọi DAL để cập nhật
            return _saleInvoiceDAL.UpdateSaleInvoice(invoice);
        }

        public bool DeleteSaleInvoice(string invoiceId)
        {
            // Logic nghiệp vụ:
            // 1. Kiểm tra phiếu có tồn tại không trước khi xóa
            if (_saleInvoiceDAL.GetSaleInvoiceById(invoiceId) == null)
            {
                throw new Exception("Không tìm thấy phiếu bán hàng cần xóa.");
            }

            // 2. Gọi DAL để xóa
            return _saleInvoiceDAL.DeleteSaleInvoice(invoiceId);
        }

        // --- Các phương thức cho Chi Tiết Phiếu ---

        // Lưu ý: Các thao tác Add/Update/Delete chi tiết cần được đồng bộ với phiếu chính
        // BLL sẽ đảm bảo cập nhật TotalQuantity và TotalAmount của phiếu chính

        public bool AddSaleInvoiceDetail(string invoiceId, SaleInvoiceDetailDTO detail)
        {
            // Lấy phiếu chính để kiểm tra và cập nhật
            SaleInvoiceDTO mainInvoice = _saleInvoiceDAL.GetSaleInvoiceById(invoiceId);
            if (mainInvoice == null)
            {
                throw new Exception("Phiếu bán hàng không tồn tại để thêm chi tiết.");
            }

            // Đảm bảo chi tiết có MaPhieu đúng
            detail.InvoiceId = invoiceId;

            // Thêm chi tiết vào DAL
            if (_saleInvoiceDAL.AddSaleInvoiceDetail(detail))
            {
                // Cập nhật lại thông tin phiếu chính (tổng số lượng, tổng tiền)
                // Lấy lại phiếu chính với chi tiết mới và cập nhật
                mainInvoice = _saleInvoiceDAL.GetSaleInvoiceById(invoiceId);
                mainInvoice.TotalQuantity = mainInvoice.InvoiceDetails.Sum(d => d.Quantity);
                mainInvoice.TotalAmount = mainInvoice.InvoiceDetails.Sum(d => d.LineAmount);
                // DAL UpdateSaleInvoice sẽ chỉ cập nhật các trường chính, không phải chi tiết
                // Vì chi tiết đã được thêm riêng, chúng ta chỉ cần cập nhật lại tổng tiền/tổng số lượng (nếu có trường đó trong DB)
                // Hiện tại CSDL của bạn không có TotalQuantity và TotalAmount trong PhieuBanHang,
                // nên việc cập nhật này sẽ chỉ diễn ra trong đối tượng DTO trên BLL.
                // Nếu bạn muốn lưu TotalQuantity và TotalAmount vào DB, bạn cần thêm 2 cột này vào bảng PhieuBanHang.
                // Giả định nếu không có trong DB, thì chúng ta chỉ cần cập nhật DTO trên BLL
                // để đảm bảo tính nhất quán cho phiên làm việc hiện tại.
                // Nếu có trong DB, thì bạn cần gọi _saleInvoiceDAL.UpdateSaleInvoiceHeaderOnly(mainInvoice)
                // Để đơn giản, tôi sẽ không thêm phương thức UpdateInvoiceHeaderOnly vào DAL ở đây,
                // mà giả định BLL sẽ xử lý việc tái tính toán và trả về DTO đã cập nhật.
                return true;
            }
            return false;
        }

        public bool UpdateSaleInvoiceDetail(string invoiceId, SaleInvoiceDetailDTO detail)
        {
            SaleInvoiceDTO mainInvoice = _saleInvoiceDAL.GetSaleInvoiceById(invoiceId);
            if (mainInvoice == null)
            {
                throw new Exception("Phiếu bán hàng không tồn tại để cập nhật chi tiết.");
            }

            // Đảm bảo chi tiết có MaPhieu đúng và ID hợp lệ
            detail.InvoiceId = invoiceId;

            if (_saleInvoiceDAL.UpdateSaleInvoiceDetail(detail))
            {
                // Cập nhật lại thông tin phiếu chính
                return true;
            }
            return false;
        }

        public bool DeleteSaleInvoiceDetail(string invoiceId, int detailId)
        {
            SaleInvoiceDTO mainInvoice = _saleInvoiceDAL.GetSaleInvoiceById(invoiceId);
            if (mainInvoice == null)
            {
                throw new Exception("Phiếu bán hàng không tồn tại để xóa chi tiết.");
            }

            if (_saleInvoiceDAL.DeleteSaleInvoiceDetail(detailId))
            {
                // Cập nhật lại thông tin phiếu chính
                return true;
            }
            return false;
        }

        public List<SaleInvoiceDetailDTO> GetInvoiceDetails(string invoiceId)
        {
            return _saleInvoiceDAL.GetDetailsByInvoiceId(invoiceId);
        }
    }

    // Giả định ProductDAL (nếu không, tích hợp vào SaleInvoiceDAL)
    public class ProductDAL
    {
        public List<ProductDTO> GetAllProducts()
        {
            List<ProductDTO> products = new List<ProductDTO>();
            string query = "SELECT MaSanPham, TenSanPham, DonGia FROM SanPham WHERE TrangThai = 1";

            using (SqlConnection connection = new SqlConnection(DBConnection.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            products.Add(new ProductDTO
                            {
                                ProductId = reader["MaSanPham"].ToString(),
                                ProductName = reader["TenSanPham"].ToString(),
                                UnitPrice = Convert.ToDecimal(reader["DonGia"])
                            });
                        }
                    }
                }
            }
            return products;
        }

        public decimal GetProductUnitPrice(string productId)
        {
            decimal unitPrice = 0;
            string query = "SELECT DonGia FROM SanPham WHERE MaSanPham = @MaSanPham";

            using (SqlConnection connection = new SqlConnection(DBConnection.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MaSanPham", productId);
                    connection.Open();
                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        unitPrice = Convert.ToDecimal(result);
                    }
                }
            }
            return unitPrice;
        }
    }
    public class QLTheLuuDongBUS
    {
        private QLTheLuuDongDAL theLuuDongDAL = new QLTheLuuDongDAL();

        public void AddTheLuuDong(Polycafe_DTO.QLTheLuuDongDTO theLuuDong)
        {
            theLuuDongDAL.AddTheLuuDong(theLuuDong);
        }
        public bool IsCardUsedInSalesOrder(string maThe)
        {
            // Call the DAL method to check if the card is linked
            return theLuuDongDAL.CheckCardUsageInSalesOrder(maThe);
        }
        public bool GetVaiTroByEmail(string email)
        {
            return theLuuDongDAL.GetVaiTroByEmail(email);
        }

        public void UpdateTheLuuDong(Polycafe_DTO.QLTheLuuDongDTO theLuuDong)
        {
            theLuuDongDAL.UpdateTheLuuDong(theLuuDong);
        }

        public void RemoveTheLuuDong(string maThe) // Đã thay đổi kiểu tham số thành string
        {
            theLuuDongDAL.RemoveTheLuuDong(maThe);
        }

        public List<Polycafe_DTO.QLTheLuuDongDTO> GetAllTheLuuDong()
        {
            return theLuuDongDAL.GetAllTheLuuDong();
        }

        public List<Polycafe_DTO.QLTheLuuDongDTO> SearchTheLuuDongByMaThe(string maThe)
        {
            return theLuuDongDAL.SearchTheLuuDongByMaThe(maThe);
        }

      
    }
    public class EmployeeBLL
    {
        private EmployeeDAL employeeDAL;
        public bool IsEmployeeLinkedToSalesOrders(string employeeId)
        {
            // Gọi xuống DAL để kiểm tra nhân viên có trong phiếu bán hàng không
            return employeeDAL.CheckIfEmployeeInSalesOrder(employeeId);
        }

        public EmployeeBLL()
        {
            employeeDAL = new EmployeeDAL();
        }

        // Lấy tất cả nhân viên
        public List<EmployeeDTO> GetAllEmployees()
        {
            return employeeDAL.GetAllEmployees();
        }
        public bool GetVaiTroByEmail(string email)
        {
            return employeeDAL.GetVaiTroByEmail(email);
        }



        // Thêm một nhân viên mới
        public string AddEmployee(EmployeeDTO employee)
        {
            // Kiểm tra ràng buộc dữ liệu trước khi thêm
            if (string.IsNullOrWhiteSpace(employee.EmployeeId) || employee.EmployeeId.Length > 6)
            {
                return "Mã nhân viên không được để trống và tối đa 6 ký tự.";
            }
            if (string.IsNullOrWhiteSpace(employee.FullName))
            {
                return "Họ tên không được để trống.";
            }
            if (string.IsNullOrWhiteSpace(employee.Email) || !IsValidEmail(employee.Email))
            {
                return "Email không hợp lệ hoặc để trống.";
            }
            if (string.IsNullOrWhiteSpace(employee.Password))
            {
                return "Mật khẩu không được để trống.";
            }

            // Kiểm tra trùng lặp Mã NV
            if (employeeDAL.IsEmployeeIdExist(employee.EmployeeId))
            {
                return "Mã nhân viên đã tồn tại. Vui lòng chọn mã khác.";
            }

            // Kiểm tra trùng lặp Email
            if (employeeDAL.IsEmailExist(employee.Email))
            {
                return "Email đã tồn tại. Vui lòng sử dụng email khác.";
            }

            if (employeeDAL.AddEmployee(employee))
            {
                return "SUCCESS"; // Thêm thành công
            }
            else
            {
                return "Thêm nhân viên thất bại. Vui lòng kiểm tra lại.";
            }
        }

        // Cập nhật thông tin nhân viên
        public string UpdateEmployee(EmployeeDTO employee)
        {
            // Kiểm tra ràng buộc dữ liệu
            if (string.IsNullOrWhiteSpace(employee.EmployeeId))
            {
                return "Mã nhân viên không được để trống.";
            }
            if (string.IsNullOrWhiteSpace(employee.FullName))
            {
                return "Họ tên không được để trống.";
            }
            if (string.IsNullOrWhiteSpace(employee.Email) || !IsValidEmail(employee.Email))
            {
                return "Email không hợp lệ hoặc để trống.";
            }
            if (string.IsNullOrWhiteSpace(employee.Password))
            {
                return "Mật khẩu không được để trống.";
            }

            // Kiểm tra trùng lặp Email, loại trừ chính nhân viên đang cập nhật
            if (employeeDAL.IsEmailExist(employee.Email, employee.EmployeeId))
            {
                return "Email đã tồn tại với một nhân viên khác. Vui lòng sử dụng email khác.";
            }

            if (employeeDAL.UpdateEmployee(employee))
            {
                return "SUCCESS"; // Cập nhật thành công
            }
            else
            {
                return "Cập nhật nhân viên thất bại. Vui lòng kiểm tra lại.";
            }
        }

        // Xóa nhân viên
        public string DeleteEmployee(string employeeId)
        {
            if (string.IsNullOrWhiteSpace(employeeId))
            {
                return "Mã nhân viên không được để trống để xóa.";
            }

            // Có thể thêm kiểm tra các ràng buộc khác ở đây (ví dụ: nhân viên có đang trong phiếu bán hàng nào không?)
            // Hiện tại, do database đã có ON DELETE SET NULL cho MaNhanVien trong PhieuBanHang,
            // nên việc xóa nhân viên sẽ không gây lỗi nếu có liên kết.

            if (employeeDAL.DeleteEmployee(employeeId))
            {
                return "SUCCESS"; // Xóa thành công
            }
            else
            {
                return "Xóa nhân viên thất bại. Vui lòng kiểm tra lại.";
            }
        }

        // Tìm kiếm nhân viên
        public List<EmployeeDTO> SearchEmployees(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                // Nếu từ khóa tìm kiếm rỗng, trả về tất cả nhân viên (hoặc xử lý theo yêu cầu)
                return employeeDAL.GetAllEmployees();
            }
            return employeeDAL.SearchEmployees(keyword);
        }

        // Phương thức kiểm tra định dạng email cơ bản
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }

    public class HoSo_BUS
    {
        private HoSo_DAL userData = new HoSo_DAL();
        public DataTable Getuser(string email)
        {
            return userData.GetUser(email);
        }
    }


    public class nhanvienBLL
    {
        private nhanvienDAL nhanVienDAL;

        public nhanvienBLL(string connectionString)
        {
            nhanVienDAL = new nhanvienDAL(connectionString);
        }

        public DataTable LayDanhSachNhanVienChoComboBox()
        {
            DataTable dtNhanVien = nhanVienDAL.LayTatCaNhanVien();
            // Có thể thêm logic nghiệp vụ tại đây nếu cần (ví dụ: lọc thêm, sắp xếp...)
            return dtNhanVien;
        }

    }

    public class ThongKeBLL
    {
        private ThongKeDAL thongKeDAL;

        public ThongKeBLL(string connectionString)
        {
            thongKeDAL = new ThongKeDAL(connectionString);
        }

        public List<ThongKeNhanVienDTO> GetDoanhThuTheoThang(string maNhanVien, DateTime tuNgay, DateTime denNgay)
        {
            // Có thể thêm logic kiểm tra hợp lệ của tham số tại đây
            if (tuNgay > denNgay)
            {
                // Hoặc throw exception tùy theo cách bạn muốn xử lý lỗi
                return new List<ThongKeNhanVienDTO>();
            }

            return thongKeDAL.LayThongKeDoanhThuTheoThang(maNhanVien, tuNgay, denNgay);
        }
    }

}
