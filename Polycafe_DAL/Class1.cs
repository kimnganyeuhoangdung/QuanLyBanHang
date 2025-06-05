using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using Polycafe_DTO;
using System.Linq;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DocumentFormat.OpenXml.Math;
namespace Polycafe_DAL
{
    public class DBUtil
    {
        private static string connectionString = "Data Source=SD20302\\ADMINCUTE;Initial Catalog=QLPolycafe;Integrated Security=True;";

        public static SqlCommand GetCommand(string sql, List<object> args, CommandType cmdType)
        {
            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand(sql, conn)
            {
                CommandType = cmdType
            };
            for (int i = 0; i < args.Count; i++)
            {
                cmd.Parameters.AddWithValue($"@param{i}", args[i]);
            }
            return cmd;
        }

        public static void Update(string sql, List<object> args, CommandType cmdType)
        {
            using (SqlCommand cmd = GetCommand(sql, args, cmdType))
            {
                cmd.Connection.Open();
                using (var transaction = cmd.Connection.BeginTransaction())
                {
                    cmd.Transaction = transaction;
                    try
                    {
                        cmd.ExecuteNonQuery();
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public static SqlDataReader Query(string sql, List<object> args, CommandType cmdType = CommandType.Text)
        {
            try {
                SqlCommand cmd = GetCommand(sql, args, cmdType);
                cmd.Connection.Open();
                return cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public static Object Value(string sql, List<object> args, CommandType cmdType = CommandType.Text)
        {
            using (SqlCommand cmd = GetCommand(sql, args, cmdType))
            {
                cmd.Connection.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var result = new object();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            string columnName = reader.GetName(i);
                            PropertyInfo propertyInfo = result.GetType().GetProperty(columnName);
                            if (propertyInfo != null)
                            {
                                var value = reader.IsDBNull(i) ? null : reader[columnName];
                                propertyInfo.SetValue(result, value);
                            }
                        }
                        return result;
                    }
                }
            }
            return null;
        }
    }

    public class LoginDAL
    {
        private static string connectionString = "Data Source=SD20302\\ADMINCUTE;Initial Catalog=QLPolycafe;Integrated Security=True;";

        public string GetMatKhau(string username)
        {
            string query = "SELECT MatKhau FROM NhanVien WHERE Email = @username";
            using (SqlCommand cmd = DBUtil.GetCommand(query, new List<object>(), CommandType.Text))
            {
                cmd.Parameters.AddWithValue("@username", username); // Ensure parameter is added
                cmd.Connection.Open();
                object result = cmd.ExecuteScalar();
                return result?.ToString().Trim();
            }
        }

        public bool GetVaiTro(string email)
        {
            string query = "SELECT VaiTro FROM NhanVien WHERE Email = @username";
            using (SqlCommand cmd = DBUtil.GetCommand(query, new List<object>(), CommandType.Text))
            {
                cmd.Parameters.AddWithValue("@username", email);
                cmd.Connection.Open();
                object result = cmd.ExecuteScalar();

                if (result != null && result != DBNull.Value)
                {
                    // Chuyển object sang bool (do cột BIT trong SQL)
                    return Convert.ToBoolean(result);
                }
                else
                {
                    return false; // hoặc xử lý khác khi không tìm thấy
                }
            }
        }
        public string LayTenTheoEmail(string email)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT HoTen FROM NhanVien WHERE Email = @email", conn);
                cmd.Parameters.AddWithValue("@email", email);

                object result = cmd.ExecuteScalar();
                return result != null ? result.ToString() : null;
            }
        }

    }
    public class NhanVienDAL
    {
        private string connectionString;

        public NhanVienDAL(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public bool CheckCredentials(string email, string password)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) FROM NhanVien WHERE Email = @Email AND MatKhau = @Password";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@Password", password);
                        int count = (int)command.ExecuteScalar();
                        return count > 0;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Lỗi kết nối hoặc truy vấn database: " + ex.Message);
                }
            }
        }

        public bool CheckEmailExists(string email)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) FROM NhanVien WHERE Email = @Email";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);
                        int count = (int)command.ExecuteScalar();
                        return count > 0;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Lỗi kết nối hoặc truy vấn database: " + ex.Message);
                }
            }
        }

        public bool UpdatePassword(string email, string newPassword)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "UPDATE NhanVien SET MatKhau = @NewPassword WHERE Email = @Email";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@NewPassword", newPassword);
                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Lỗi kết nối hoặc truy vấn database: " + ex.Message);
                }
            }
        }
    }
    public class qlLSP_DAL
    {

        private string connString = "Data Source=SD20302\\ADMINCUTE;Initial Catalog=QLPolycafe;Integrated Security=True;";

        public DataTable GetAll()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                try
                {
                    string query = "SELECT MaLoai, TenLoai, GhiChu FROM LoaiSanPham"; // Chọn rõ ràng các cột
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
                catch (SqlException)
                {
                    return null;
                }
                catch (Exception)
                {

                    return null; // Trả về null khi có lỗi
                }
            }
        }
        public bool GetVaiTroByEmail(string email)
        {
            string query = "SELECT VaiTro FROM NhanVien WHERE Email = @Email";

            using (SqlConnection conn = new SqlConnection(connString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Email", email);
                conn.Open();
                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    return Convert.ToBoolean(result); // true: admin, false: nhân viên
                }
            }

            return false; // nếu không tìm thấy hoặc lỗi thì coi như không có quyền
        }
        public bool Check(string maloai)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM LoaiSanPham WHERE MaLoai = @MaLoai";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@MaLoai", maloai);
                    return (int)cmd.ExecuteScalar() > 0;
                }
                catch (SqlException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"SQL Error in Check: {ex.Message}");
                    return false;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"General Error in Check: {ex.Message}");
                    return false;
                }
            }
        }

        public int Add(qlLSP lsp)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = "INSERT INTO LoaiSanPham (MaLoai, TenLoai, GhiChu) VALUES (@MaLoai, @TenLoai, @GhiChu)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@MaLoai", lsp.MaLoai ?? "");
                    cmd.Parameters.AddWithValue("@TenLoai", lsp.TenLoai ?? "");
                    cmd.Parameters.AddWithValue("@GhiChu", lsp.GhiChu ?? "");

                    return cmd.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    System.Diagnostics.Debug.WriteLine("Lỗi SQL: " + ex.Message);
                    return -1;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Write("Lỗi chung: " + ex.Message);
                    return -1;
                }
            }
        }


        public int Update(qlLSP lsp)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = "UPDATE LoaiSanPham SET TenLoai = @TenLoai, GhiChu = @GhiChu WHERE MaLoai = @MaLoai";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@MaLoai", lsp.MaLoai);
                    cmd.Parameters.AddWithValue("@TenLoai", lsp.TenLoai);
                    cmd.Parameters.AddWithValue("@GhiChu", lsp.GhiChu);
                    return cmd.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"SQL Error in Update: {ex.Message}");
                    return -1;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"General Error in Update: {ex.Message}");
                    return -1;
                }
            }
        }

        public int Delete(qlLSP lsp)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = "DELETE FROM LoaiSanPham WHERE MaLoai = @MaLoai";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@MaLoai", lsp.MaLoai);
                    return cmd.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"SQL Error in Delete: {ex.Message}");
                    if (ex.Number == 547) // Foreign Key Violation error number
                    {
                    }
                    else
                    {
                    }
                    return -1;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"General Error in Delete: {ex.Message}");
                    return -1;
                }
            }
        }

        public List<string> LoadMaLoai()
        {
            List<string> maloai = new List<string>();
            using (SqlConnection conn = new SqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT DISTINCT MaLoai FROM LoaiSanPham";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        maloai.Add(reader["MaLoai"].ToString());
                    }
                    reader.Close(); // Đóng reader
                    return maloai;
                }
                catch (SqlException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"SQL Error in LoadMaLoai: {ex.Message}");
                    return null; // Trả về null khi có lỗi
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"General Error in LoadMaLoai: {ex.Message}");
                    return null;
                }
            }
        }
    }


    public class SanPhamDAL
    {
        private static string connectionString = "Data Source=SD20302\\ADMINCUTE;Initial Catalog=QLPolycafe;Integrated Security=True;";
        public DataTable GetAllSanPham()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM SanPham", conn);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }
        public bool GetVaiTroByEmail(string email)
        {
            string query = "SELECT VaiTro FROM NhanVien WHERE Email = @Email";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Email", email);
                conn.Open();
                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    return Convert.ToBoolean(result); // true: admin, false: nhân viên
                }
            }

            return false; // nếu không tìm thấy hoặc lỗi thì coi như không có quyền
        }
        public bool AddSanPham(SanPhamDTO sanPham)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("INSERT INTO SanPham (MaSanPham, TenSanPham, MaLoai, DonGia, TrangThai, HinhAnh) VALUES (@MaSanPham, @TenSanPham, @MaLoai, @DonGia, @TrangThai, @HinhAnh)", conn);
                cmd.Parameters.AddWithValue("@MaSanPham", sanPham.MaSanPham);
                cmd.Parameters.AddWithValue("@TenSanPham", sanPham.TenSanPham);
                cmd.Parameters.AddWithValue("@MaLoai", sanPham.MaLoai);
                cmd.Parameters.AddWithValue("@DonGia", sanPham.DonGia);
                cmd.Parameters.AddWithValue("@TrangThai", sanPham.TrangThai);
                cmd.Parameters.AddWithValue("@HinhAnh", sanPham.HinhAnh);
                return cmd.ExecuteNonQuery() > 0;
            }
        }
        public bool UpdateSanPham(SanPhamDTO sanPham)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("UPDATE SanPham SET TenSanPham = @TenSanPham, MaLoai = @MaLoai, DonGia = @DonGia, TrangThai = @TrangThai, HinhAnh = @HinhAnh WHERE MaSanPham = @MaSanPham", conn);
                cmd.Parameters.AddWithValue("@MaSanPham", sanPham.MaSanPham);
                cmd.Parameters.AddWithValue("@TenSanPham", sanPham.TenSanPham);
                cmd.Parameters.AddWithValue("@MaLoai", sanPham.MaLoai);
                cmd.Parameters.AddWithValue("@DonGia", sanPham.DonGia);
                cmd.Parameters.AddWithValue("@TrangThai", sanPham.TrangThai);
                cmd.Parameters.AddWithValue("@HinhAnh", sanPham.HinhAnh);
                return cmd.ExecuteNonQuery() > 0;
            }
        }
        public bool DeleteSanPham(string maSanPham)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("DELETE FROM SanPham WHERE MaSanPham = @MaSanPham", conn);
                cmd.Parameters.AddWithValue("@MaSanPham", maSanPham);
                return cmd.ExecuteNonQuery() > 0;
            }
        }
        public DataTable SearchSanPham(string searchTerm)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                // --- CORRECTED: Select all columns for proper DataGridView display ---
                // Changed from SELECT MaSanPham to SELECT *
                // Changed search column from MaSanPham to TenSanPham (more common for text search)
                SqlCommand cmd = new SqlCommand("SELECT * FROM SanPham WHERE TenSanPham LIKE @SearchTerm OR MaSanPham LIKE @SearchTerm", conn);
                cmd.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm + "%");
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }
        public DataTable GetLoaiSanPham()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM LoaiSanPham", conn);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }
        public List<SanPhamDTO> GetByMaLoai(string maLoai)
        {
            var list = new List<SanPhamDTO>();
            string query = "SELECT * FROM SanPham WHERE MaLoai = @MaLoai";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@MaLoai", maLoai);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new SanPhamDTO
                    {
                        MaSanPham = reader["MaSanPham"].ToString(),
                        TenSanPham = reader["TenSanPham"].ToString()
                        // Gán các thuộc tính khác nếu cần
                    });
                }
            }

            return list;
        }

        public bool DeleteByMaLoai(string maLoai)
        {
            string query = "DELETE FROM SanPham WHERE MaLoai = @MaLoai";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@MaLoai", maLoai);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

    }
    // Lớp tiện ích quản lý chuỗi kết nối
    public static class DBConnection
    {
        public static string ConnectionString { get; private set; }

        static DBConnection()
        {
            ConnectionString = "Data Source=SD20302\\ADMINCUTE;Initial Catalog=QLPolycafe;Integrated Security=True;";
        }
    }

    public class SaleInvoiceDAL
    {
        // Lấy tất cả thông tin thẻ lưu động cho ComboBox
        public List<CardDTO> GetAllCards()
        {
            List<CardDTO> cards = new List<CardDTO>();
            string query = "SELECT MaThe FROM TheLuuDong WHERE TrangThai = 1"; // Chỉ lấy thẻ đang hoạt động

            using (SqlConnection connection = new SqlConnection(DBConnection.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cards.Add(new CardDTO
                            {
                                CardId = reader["MaThe"].ToString(),
                                //OwnerName = reader["ChuSoHuu"].ToString()
                            });
                        }
                    }
                }
            }
            return cards;
        }

        // Lấy tất cả thông tin nhân viên cho ComboBox
        public List<EmployeeDto> GetAllEmployees()
        {
            List<EmployeeDto> employees = new List<EmployeeDto>();
            string query = "SELECT MaNhanVien, HoTen FROM NhanVien WHERE TrangThai = 1"; // Chỉ lấy nhân viên đang hoạt động

            using (SqlConnection connection = new SqlConnection(DBConnection.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            employees.Add(new EmployeeDto
                            {
                                EmployeeId = reader["MaNhanVien"].ToString(),
                                FullName = reader["HoTen"].ToString()
                            });
                        }
                    }
                }
            }
            return employees;
        }

        // Lấy tất cả thông tin sản phẩm cho ComboBox và lấy đơn giá
        public List<ProductDTO> GetAllProducts()
        {
            List<ProductDTO> products = new List<ProductDTO>();
            string query = "SELECT MaSanPham, TenSanPham, DonGia FROM SanPham WHERE TrangThai = 1"; // Chỉ lấy sản phẩm đang hoạt động

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

        // Lấy đơn giá của một sản phẩm theo Mã sản phẩm
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

        // Lấy tất cả các phiếu bán hàng (có thể join để lấy tên thẻ, tên nhân viên)
        public List<SaleInvoiceDTO> GetAllSaleInvoices()
        {
            List<SaleInvoiceDTO> invoices = new List<SaleInvoiceDTO>();
            string query = @"
            SELECT
                pbh.MaPhieu,
                pbh.MaThe,
                tld.ChuSoHuu,
                pbh.MaNhanVien,
                nv.HoTen,
                pbh.NgayTao,
                pbh.TrangThai
            FROM PhieuBanHang pbh
            LEFT JOIN TheLuuDong tld ON pbh.MaThe = tld.MaThe
            LEFT JOIN NhanVien nv ON pbh.MaNhanVien = nv.MaNhanVien";

            using (SqlConnection connection = new SqlConnection(DBConnection.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SaleInvoiceDTO invoice = new SaleInvoiceDTO
                            {
                                InvoiceId = reader["MaPhieu"].ToString(),
                                CardId = reader["MaThe"] == DBNull.Value ? null : reader["MaThe"].ToString(),
                                CardOwnerName = reader["ChuSoHuu"] == DBNull.Value ? "N/A" : reader["ChuSoHuu"].ToString(),
                                EmployeeId = reader["MaNhanVien"] == DBNull.Value ? null : reader["MaNhanVien"].ToString(),
                                EmployeeName = reader["HoTen"] == DBNull.Value ? "N/A" : reader["HoTen"].ToString(),
                                CreatedDate = Convert.ToDateTime(reader["NgayTao"]),
                                Status = Convert.ToBoolean(reader["TrangThai"])
                                // TotalQuantity và TotalAmount sẽ được tính ở BLL hoặc khi lấy chi tiết
                            };
                            invoices.Add(invoice);
                        }
                    }
                }
            }
            return invoices;
        }

        // Lấy một phiếu bán hàng theo ID (bao gồm cả chi tiết)
        public SaleInvoiceDTO GetSaleInvoiceById(string invoiceId)
        {
            SaleInvoiceDTO invoice = null;
            string invoiceQuery = @"
            SELECT
                pbh.MaPhieu,
                pbh.MaThe,
                tld.ChuSoHuu,
                pbh.MaNhanVien,
                nv.HoTen,
                pbh.NgayTao,
                pbh.TrangThai
            FROM PhieuBanHang pbh
            LEFT JOIN TheLuuDong tld ON pbh.MaThe = tld.MaThe
            LEFT JOIN NhanVien nv ON pbh.MaNhanVien = nv.MaNhanVien
            WHERE pbh.MaPhieu = @MaPhieu";

            string detailQuery = @"
            SELECT
                ctp.Id,
                ctp.MaPhieu,
                ctp.MaSanPham,
                sp.TenSanPham,
                ctp.SoLuong,
                ctp.DonGia
            FROM ChiTietPhieu ctp
            JOIN SanPham sp ON ctp.MaSanPham = sp.MaSanPham
            WHERE ctp.MaPhieu = @MaPhieu";

            using (SqlConnection connection = new SqlConnection(DBConnection.ConnectionString))
            {
                connection.Open();

                // Lấy thông tin phiếu chính
                using (SqlCommand command = new SqlCommand(invoiceQuery, connection))
                {
                    command.Parameters.AddWithValue("@MaPhieu", invoiceId);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            invoice = new SaleInvoiceDTO
                            {
                                InvoiceId = reader["MaPhieu"].ToString(),
                                CardId = reader["MaThe"] == DBNull.Value ? null : reader["MaThe"].ToString(),
                                CardOwnerName = reader["ChuSoHuu"] == DBNull.Value ? "N/A" : reader["ChuSoHuu"].ToString(),
                                EmployeeId = reader["MaNhanVien"] == DBNull.Value ? null : reader["MaNhanVien"].ToString(),
                                EmployeeName = reader["HoTen"] == DBNull.Value ? "N/A" : reader["HoTen"].ToString(),
                                CreatedDate = Convert.ToDateTime(reader["NgayTao"]),
                                Status = Convert.ToBoolean(reader["TrangThai"])
                            };
                        }
                    }
                }

                // Lấy chi tiết phiếu nếu phiếu chính tồn tại
                if (invoice != null)
                {
                    using (SqlCommand detailCommand = new SqlCommand(detailQuery, connection))
                    {
                        detailCommand.Parameters.AddWithValue("@MaPhieu", invoiceId);
                        using (SqlDataReader detailReader = detailCommand.ExecuteReader())
                        {
                            while (detailReader.Read())
                            {
                                SaleInvoiceDetailDTO detail = new SaleInvoiceDetailDTO
                                {
                                    Id = Convert.ToInt32(detailReader["Id"]),
                                    InvoiceId = detailReader["MaPhieu"].ToString(),
                                    ProductId = detailReader["MaSanPham"].ToString(),
                                    ProductName = detailReader["TenSanPham"].ToString(),
                                    Quantity = Convert.ToInt32(detailReader["SoLuong"]),
                                    UnitPrice = Convert.ToDecimal(detailReader["DonGia"]),
                                    LineAmount = Convert.ToDecimal(detailReader["SoLuong"]) * Convert.ToDecimal(detailReader["DonGia"]) // Tính toán ở đây
                                };
                                invoice.InvoiceDetails.Add(detail);
                            }
                        }
                    }
                    // Tính toán TotalQuantity và TotalAmount sau khi có tất cả chi tiết
                    invoice.TotalQuantity = invoice.InvoiceDetails.Sum(d => d.Quantity);
                    invoice.TotalAmount = invoice.InvoiceDetails.Sum(d => d.LineAmount);
                }
            }
            return invoice;
        }

        // Thêm mới một phiếu bán hàng (có sử dụng Transaction để đảm bảo tính toàn vẹn)
        public bool AddSaleInvoice(SaleInvoiceDTO invoice)
        {
            bool success = false;
            string insertInvoiceQuery = "INSERT INTO PhieuBanHang (MaPhieu, MaThe, MaNhanVien, NgayTao, TrangThai) VALUES (@MaPhieu, @MaThe, @MaNhanVien, @NgayTao, @TrangThai)";
            string insertDetailQuery = "INSERT INTO ChiTietPhieu (MaPhieu, MaSanPham, SoLuong, DonGia) VALUES (@MaPhieu, @MaSanPham, @SoLuong, @DonGia)";

            using (SqlConnection connection = new SqlConnection(DBConnection.ConnectionString))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction(); // Bắt đầu Transaction

                try
                {
                    // Thêm phiếu bán hàng chính
                    using (SqlCommand command = new SqlCommand(insertInvoiceQuery, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@MaPhieu", invoice.InvoiceId);
                        command.Parameters.AddWithValue("@MaThe", (object)invoice.CardId ?? DBNull.Value); // Xử lý NULL
                        command.Parameters.AddWithValue("@MaNhanVien", (object)invoice.EmployeeId ?? DBNull.Value); // Xử lý NULL
                        command.Parameters.AddWithValue("@NgayTao", invoice.CreatedDate);
                        command.Parameters.AddWithValue("@TrangThai", invoice.Status);
                        command.ExecuteNonQuery();
                    }

                    // Thêm các chi tiết phiếu
                    foreach (var detail in invoice.InvoiceDetails)
                    {
                        using (SqlCommand detailCommand = new SqlCommand(insertDetailQuery, connection, transaction))
                        {
                            detailCommand.Parameters.AddWithValue("@MaPhieu", invoice.InvoiceId);
                            detailCommand.Parameters.AddWithValue("@MaSanPham", detail.ProductId);
                            detailCommand.Parameters.AddWithValue("@SoLuong", detail.Quantity);
                            detailCommand.Parameters.AddWithValue("@DonGia", detail.UnitPrice);
                            detailCommand.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit(); // Commit Transaction nếu tất cả đều thành công
                    success = true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback(); // Rollback Transaction nếu có lỗi
                                            // Ghi log lỗi (quan trọng!)
                    System.Diagnostics.Debug.WriteLine("Lỗi khi thêm phiếu bán hàng: " + ex.Message);
                    success = false;
                }
            }
            return success;
        }

        // Cập nhật một phiếu bán hàng (có sử dụng Transaction)
        public bool UpdateSaleInvoice(SaleInvoiceDTO invoice)
        {
            bool success = false;
            string updateInvoiceQuery = "UPDATE PhieuBanHang SET MaThe = @MaThe, MaNhanVien = @MaNhanVien, NgayTao = @NgayTao, TrangThai = @TrangThai WHERE MaPhieu = @MaPhieu";
            string deleteDetailsQuery = "DELETE FROM ChiTietPhieu WHERE MaPhieu = @MaPhieu";
            string insertDetailQuery = "INSERT INTO ChiTietPhieu (MaPhieu, MaSanPham, SoLuong, DonGia) VALUES (@MaPhieu, @MaSanPham, @SoLuong, @DonGia)";

            using (SqlConnection connection = new SqlConnection(DBConnection.ConnectionString))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // Cập nhật phiếu bán hàng chính
                    using (SqlCommand command = new SqlCommand(updateInvoiceQuery, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@MaPhieu", invoice.InvoiceId);
                        command.Parameters.AddWithValue("@MaThe", (object)invoice.CardId ?? DBNull.Value); // Xử lý NULL
                        command.Parameters.AddWithValue("@MaNhanVien", (object)invoice.EmployeeId ?? DBNull.Value); // Xử lý NULL
                        command.Parameters.AddWithValue("@NgayTao", invoice.CreatedDate);
                        command.Parameters.AddWithValue("@TrangThai", invoice.Status);
                        command.ExecuteNonQuery();
                    }

                    // Xóa tất cả chi tiết cũ của phiếu
                    using (SqlCommand deleteDetailCommand = new SqlCommand(deleteDetailsQuery, connection, transaction))
                    {
                        deleteDetailCommand.Parameters.AddWithValue("@MaPhieu", invoice.InvoiceId);
                        deleteDetailCommand.ExecuteNonQuery();
                    }

                    // Thêm lại các chi tiết mới
                    foreach (var detail in invoice.InvoiceDetails)
                    {
                        using (SqlCommand insertDetailCommand = new SqlCommand(insertDetailQuery, connection, transaction))
                        {
                            insertDetailCommand.Parameters.AddWithValue("@MaPhieu", invoice.InvoiceId);
                            insertDetailCommand.Parameters.AddWithValue("@MaSanPham", detail.ProductId);
                            insertDetailCommand.Parameters.AddWithValue("@SoLuong", detail.Quantity);
                            insertDetailCommand.Parameters.AddWithValue("@DonGia", detail.UnitPrice);
                            insertDetailCommand.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                    success = true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    System.Diagnostics.Debug.WriteLine("Lỗi khi cập nhật phiếu bán hàng: " + ex.Message);
                    success = false;
                }
            }
            return success;
        }

        // Xóa một phiếu bán hàng và tất cả chi tiết liên quan (có sử dụng Transaction)
        public bool DeleteSaleInvoice(string invoiceId)
        {
            bool success = false;
            string deleteDetailsQuery = "DELETE FROM ChiTietPhieu WHERE MaPhieu = @MaPhieu";
            string deleteInvoiceQuery = "DELETE FROM PhieuBanHang WHERE MaPhieu = @MaPhieu";

            using (SqlConnection connection = new SqlConnection(DBConnection.ConnectionString))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // Xóa tất cả chi tiết của phiếu trước (do FK ON DELETE CASCADE, dòng này có thể không cần nếu CSDL đã cấu hình đúng, nhưng vẫn an toàn)
                    using (SqlCommand deleteDetailCommand = new SqlCommand(deleteDetailsQuery, connection, transaction))
                    {
                        deleteDetailCommand.Parameters.AddWithValue("@MaPhieu", invoiceId);
                        deleteDetailCommand.ExecuteNonQuery();
                    }

                    // Sau đó xóa phiếu chính
                    using (SqlCommand deleteInvoiceCommand = new SqlCommand(deleteInvoiceQuery, connection, transaction))
                    {
                        deleteInvoiceCommand.Parameters.AddWithValue("@MaPhieu", invoiceId);
                        deleteInvoiceCommand.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    success = true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    System.Diagnostics.Debug.WriteLine("Lỗi khi xóa phiếu bán hàng: " + ex.Message);
                    success = false;
                }
            }
            return success;
        }

        // Thêm mới một chi tiết phiếu bán hàng riêng lẻ
        // (Thường được gọi từ BLL, sau đó BLL sẽ cập nhật TotalQuantity/TotalAmount của phiếu chính)
        public bool AddSaleInvoiceDetail(SaleInvoiceDetailDTO detail)
        {
            bool success = false;
            string insertDetailQuery = "INSERT INTO ChiTietPhieu (MaPhieu, MaSanPham, SoLuong, DonGia) VALUES (@MaPhieu, @MaSanPham, @SoLuong, @DonGia)";

            using (SqlConnection connection = new SqlConnection(DBConnection.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand(insertDetailQuery, connection))
                {
                    command.Parameters.AddWithValue("@MaPhieu", detail.InvoiceId);
                    command.Parameters.AddWithValue("@MaSanPham", detail.ProductId);
                    command.Parameters.AddWithValue("@SoLuong", detail.Quantity);
                    command.Parameters.AddWithValue("@DonGia", detail.UnitPrice);

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();
                    success = rowsAffected > 0;
                }
            }
            return success;
        }

        // Cập nhật một chi tiết phiếu bán hàng riêng lẻ
        public bool UpdateSaleInvoiceDetail(SaleInvoiceDetailDTO detail)
        {
            bool success = false;
            string updateDetailQuery = "UPDATE ChiTietPhieu SET MaSanPham = @MaSanPham, SoLuong = @SoLuong, DonGia = @DonGia WHERE Id = @Id AND MaPhieu = @MaPhieu";

            using (SqlConnection connection = new SqlConnection(DBConnection.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand(updateDetailQuery, connection))
                {
                    command.Parameters.AddWithValue("@Id", detail.Id);
                    command.Parameters.AddWithValue("@MaPhieu", detail.InvoiceId); // Đảm bảo chi tiết thuộc đúng phiếu
                    command.Parameters.AddWithValue("@MaSanPham", detail.ProductId);
                    command.Parameters.AddWithValue("@SoLuong", detail.Quantity);
                    command.Parameters.AddWithValue("@DonGia", detail.UnitPrice);

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();
                    success = rowsAffected > 0;
                }
            }
            return success;
        }

        // Xóa một chi tiết phiếu bán hàng theo ID của chi tiết
        public bool DeleteSaleInvoiceDetail(int detailId)
        {
            bool success = false;
            string deleteDetailQuery = "DELETE FROM ChiTietPhieu WHERE Id = @Id";

            using (SqlConnection connection = new SqlConnection(DBConnection.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand(deleteDetailQuery, connection))
                {
                    command.Parameters.AddWithValue("@Id", detailId);

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();
                    success = rowsAffected > 0;
                }
            }
            return success;
        }

        // Lấy tất cả chi tiết của một phiếu cụ thể
        public List<SaleInvoiceDetailDTO> GetDetailsByInvoiceId(string invoiceId)
        {
            List<SaleInvoiceDetailDTO> details = new List<SaleInvoiceDetailDTO>();
            string query = @"
            SELECT
                ctp.Id,
                ctp.MaPhieu,
                ctp.MaSanPham,
                sp.TenSanPham,
                ctp.SoLuong,
                ctp.DonGia
            FROM ChiTietPhieu ctp
            JOIN SanPham sp ON ctp.MaSanPham = sp.MaSanPham
            WHERE ctp.MaPhieu = @MaPhieu";

            using (SqlConnection connection = new SqlConnection(DBConnection.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MaPhieu", invoiceId);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            details.Add(new SaleInvoiceDetailDTO
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                InvoiceId = reader["MaPhieu"].ToString(),
                                ProductId = reader["MaSanPham"].ToString(),
                                ProductName = reader["TenSanPham"].ToString(),
                                Quantity = Convert.ToInt32(reader["SoLuong"]),
                                UnitPrice = Convert.ToDecimal(reader["DonGia"]),
                                LineAmount = Convert.ToDecimal(reader["SoLuong"]) * Convert.ToDecimal(reader["DonGia"])
                            });
                        }
                    }
                }
            }
            return details;
        }
    }
    public class QLTheLuuDongDAL
    {
        private string connectionString = "Data Source=SD20302\\ADMINCUTE;Initial Catalog=QLPolycafe;Integrated Security=True;";

        // Phương thức trợ giúp để tạo ID tuần tự tiếp theo (ví dụ: "THE001", "SP001", "PBH001")
        private string GenerateNextId(string prefix, string tableName, string idColumnName, SqlConnection connection, SqlTransaction transaction)
        {
            string query = $"SELECT MAX({idColumnName}) FROM {tableName} WHERE {idColumnName} LIKE '{prefix}%'";
            using (var cmd = new SqlCommand(query, connection, transaction))
            {
                object result = cmd.ExecuteScalar();
                string maxId = result == DBNull.Value ? null : result.ToString();

                if (string.IsNullOrEmpty(maxId))
                {
                    return $"{prefix}001"; // ID đầu tiên
                }
                else
                {
                    // Trích xuất phần số, tăng lên và định dạng
                    string numericPart = maxId.Substring(prefix.Length);
                    if (int.TryParse(numericPart, out int num))
                    {
                        return $"{prefix}{(num + 1).ToString("D3")}"; // D3 cho 3 chữ số, ví dụ: 001, 002
                    }
                    else
                    {
                        throw new Exception("Lỗi khi tạo mã ID mới: Không thể phân tích phần số.");
                    }
                }
            }
        }
        public bool GetVaiTroByEmail(string email)
        {
            string query = "SELECT VaiTro FROM NhanVien WHERE Email = @Email";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Email", email);
                conn.Open();
                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    return Convert.ToBoolean(result); // true: admin, false: nhân viên
                }
            }

            return false; // nếu không tìm thấy hoặc lỗi thì coi như không có quyền
        }

        public void AddTheLuuDong(QLTheLuuDongDTO theLuuDong)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var transaction = connection.BeginTransaction();

                try
                {
                    // 1. Thêm vào bảng TheLuuDong
                    // MaThe được cung cấp từ UI, đảm bảo nó là duy nhất hoặc xử lý trùng lặp
                    // Để đơn giản, giả sử MaThe từ UI là duy nhất. Nếu không, hãy kiểm tra sự tồn tại trước.
                    string insertThe = @"INSERT INTO TheLuuDong (MaThe, ChuSoHuu, TrangThai)
                                         VALUES (@MaThe, @ChuSoHuu, @TrangThai)";
                    using (var cmd = new SqlCommand(insertThe, connection, transaction))
                    {
                        cmd.Parameters.Add("@MaThe", SqlDbType.Char, 6).Value = theLuuDong.MaThe;
                        cmd.Parameters.Add("@ChuSoHuu", SqlDbType.NVarChar, 100).Value = theLuuDong.ChuSoHuu;
                        cmd.Parameters.Add("@TrangThai", SqlDbType.Bit).Value = theLuuDong.TrangThai;
                        cmd.ExecuteNonQuery();
                    }



                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw; // Ném lại ngoại lệ gốc
                }
            }
        }

        public void UpdateTheLuuDong(Polycafe_DTO.QLTheLuuDongDTO theLuuDong)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                const string query = @"
                UPDATE TheLuuDong
                SET ChuSoHuu = @ChuSoHuu, TrangThai = @TrangThai
                WHERE MaThe = @MaThe";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@ChuSoHuu", SqlDbType.NVarChar, 100).Value = theLuuDong.ChuSoHuu ?? (object)DBNull.Value;
                    command.Parameters.Add("@TrangThai", SqlDbType.Bit).Value = theLuuDong.TrangThai; // Truyền trực tiếp boolean
                    command.Parameters.Add("@MaThe", SqlDbType.Char, 6).Value = theLuuDong.MaThe; // MaThe là CHAR(6)

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        throw new Exception($"Không tìm thấy thẻ với Mã thẻ = {theLuuDong.MaThe}");
                    }
                }
            }
        }

        public void RemoveTheLuuDong(string maThe) // Đã thay đổi kiểu tham số thành string
        {
            using (var connection = new SqlConnection(connectionString))
            {
                const string query = "DELETE FROM TheLuuDong WHERE MaThe = @MaThe";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@MaThe", SqlDbType.Char, 6).Value = maThe; // MaThe là CHAR(6)

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        throw new Exception($"Không tìm thấy thẻ lưu động với Mã thẻ = {maThe} để xóa.");
                    }
                }
            }
        }

        public List<Polycafe_DTO.QLTheLuuDongDTO> GetAllTheLuuDong()
        {
            var list = new List<Polycafe_DTO.QLTheLuuDongDTO>();
            using (var connection = new SqlConnection(connectionString))
            {
                // Đã loại bỏ pb.NgayTao khỏi SELECT vì nó không có trong QLTheLuuDongDTO
                string query = @"
                SELECT
                t.MaThe,
                t.ChuSoHuu,
                t.TrangThai
                FROM TheLuuDong t";

                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var item = new Polycafe_DTO.QLTheLuuDongDTO
                            {
                                MaThe = reader.GetString(reader.GetOrdinal("MaThe")), // Đọc dưới dạng chuỗi
                                ChuSoHuu = reader["ChuSoHuu"].ToString(),
                                TrangThai = reader.GetBoolean(reader.GetOrdinal("TrangThai")), // Đọc dưới dạng boolean
                            };
                            list.Add(item);
                        }
                    }
                }
            }
            return list;
        }

        public List<Polycafe_DTO.QLTheLuuDongDTO> SearchTheLuuDongByMaThe(string maThe)
        {
            var list = new List<Polycafe_DTO.QLTheLuuDongDTO>();
            using (var connection = new SqlConnection(connectionString))
            {
                string query = @"
                SELECT
                t.MaThe,
                t.ChuSoHuu,
                t.TrangThai
                FROM TheLuuDong t

                WHERE t.MaThe = @MaThe"; // Thêm điều kiện WHERE để tìm kiếm theo MaThe

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@MaThe", SqlDbType.Char, 6).Value = maThe; // Thêm tham số MaThe

                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var item = new Polycafe_DTO.QLTheLuuDongDTO
                            {
                                MaThe = reader.GetString(reader.GetOrdinal("MaThe")),
                                ChuSoHuu = reader["ChuSoHuu"].ToString(),
                                TrangThai = reader.GetBoolean(reader.GetOrdinal("TrangThai")),

                            };
                            list.Add(item);
                        }
                    }
                }
            }
            return list;
        }


    }
    public class ThongkeNVDAL
    {
        private static string connectionString = "Data Source=SD20302\\ADMINCUTE;Initial Catalog=QLPolycafe;Integrated Security=True;";

        public DataTable GetNV()
        {
            DataTable dt = new DataTable();
            string query = "SELECT MaNhanVien, HoTen FROM NhanVien ";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi khi lấy dữ liệu nhân viên từ CSDL: " + ex.Message);
            }
            return dt;
        }

        public DataTable GetTK(string MaNV, DateTime startDate, DateTime endDate)
        {
            DataTable dt = new DataTable();
            string query = @"
            SELECT NV.MaNhanVien,
                   NV.HoTen,
                   PB.NgayTao,
                   PB.TrangThai,
                   COUNT(ChiTietPhieu.SoLuong) AS [So Luong Phieu],
                   SUM(ChiTietPhieu.DonGia * ChiTietPhieu.SoLuong) AS [Tong Tien]
            FROM NhanVien NV
            INNER JOIN PhieuBanHang PB ON PB.MaNhanVien = NV.MaNhanVien
            INNER JOIN ChiTietPhieu ON ChiTietPhieu.MaPhieu = PB.MaPhieu
            WHERE (@MaNV IS NULL OR NV.MaNhanVien = @MaNV)
              AND PB.NgayTao BETWEEN @TuNgay AND @DenNgay
            GROUP BY NV.MaNhanVien, NV.HoTen, PB.TrangThai, PB.NgayTao";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaNV", string.IsNullOrEmpty(MaNV) ? (object)DBNull.Value : MaNV);
                    cmd.Parameters.AddWithValue("@TuNgay", startDate);
                    cmd.Parameters.AddWithValue("@DenNgay", endDate);

                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi khi lấy dữ liệu thống kê nhân viên: " + ex.Message);
                throw new Exception("Lỗi khi lấy dữ liệu thống kê nhân viên.", ex);
            }

            return dt;
        }

        public bool GetVaiTroByEmail(string email)
        {
            string query = "SELECT VaiTro FROM NhanVien WHERE Email = @Email";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Email", email);
                conn.Open();
                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    return Convert.ToBoolean(result); // true: admin, false: nhân viên
                }
            }

            return false;
        }

        public DataTable GetThongKeDoanhThu(string maNV, DateTime tuNgay, DateTime denNgay)
        {
            DataTable dt = new DataTable();
            string procedureName = "TKDoanhThuTheoNhanVien";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(procedureName, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@1", string.IsNullOrEmpty(maNV) ? (object)DBNull.Value : maNV);
                    cmd.Parameters.AddWithValue("@2", tuNgay.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@3", denNgay.ToString("yyyy-MM-dd"));

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi khi lấy dữ liệu thống kê từ CSDL: " + ex.Message);
                throw new Exception("Lỗi khi lấy dữ liệu thống kê.", ex);
            }

            return dt;
        }

        public List<string> LoadMaNV()
        {
            List<string> maNV = new List<string>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT DISTINCT MaNhanVien FROM NhanVien";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        maNV.Add(reader["MaNhanVien"].ToString());
                    }
                    reader.Close();
                }
                catch (SqlException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Lỗi SQL: {ex.Message}");
                    return null;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Lỗi không xác định: {ex.Message}");
                    return null;
                }
            }
            return maNV;
        }

        public List<SanPham_DTO> LoadMaSP()
        {
            List<SanPham_DTO> list = new List<SanPham_DTO>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT MaSanPham, TenSanPham FROM SanPham";
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new SanPham_DTO
                    {
                        MaSP = reader["MaSanPham"].ToString(),
                        TenSP = reader["TenSanPham"].ToString()
                    });
                }
                reader.Close();
            }
            return list;
        }

        public DataTable GetTKSP(string MaSP, DateTime startDate, DateTime endDate)
        {
            DataTable dt = new DataTable();
            string query = @"
            SELECT SP.MaSanPham,
                   SP.TenSanPham,
                   PB.NgayTao,
                   SUM(CTP.SoLuong) AS SoLuongBan,
                   SUM(CTP.SoLuong * CTP.DonGia) AS TongTien
            FROM SanPham SP
            INNER JOIN ChiTietPhieu CTP ON SP.MaSanPham = CTP.MaSanPham
            INNER JOIN PhieuBanHang PB ON PB.MaPhieu = CTP.MaPhieu
            WHERE (@MaSP IS NULL OR SP.MaSanPham = @MaSP)
              AND PB.NgayTao BETWEEN @TuNgay AND @DenNgay
            GROUP BY SP.MaSanPham, SP.TenSanPham, PB.NgayTao";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaSP", string.IsNullOrEmpty(MaSP) ? (object)DBNull.Value : MaSP);
                    cmd.Parameters.AddWithValue("@TuNgay", startDate);
                    cmd.Parameters.AddWithValue("@DenNgay", endDate);

                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi khi lấy thống kê sản phẩm: " + ex.Message);
                throw new Exception("Lỗi khi lấy thống kê sản phẩm.", ex);
            }

            return dt;
        }

        public DataTable GetThongKeDoanhThuSP(string maSP, DateTime tuNgay, DateTime denNgay)
        {
            DataTable dt = new DataTable();
            string procedureName = "TKDoanhThuTheoLoaiSP";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(procedureName, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@1", string.IsNullOrEmpty(maSP) ? (object)DBNull.Value : maSP);
                    cmd.Parameters.AddWithValue("@2", tuNgay);
                    cmd.Parameters.AddWithValue("@3", denNgay);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi lấy dữ liệu thống kê: " + ex.Message);
            }

            return dt;
        }

        public DataTable GetStatisticalData(string type, DateTime? tuNgay = null, DateTime? denNgay = null)
        {
            DataTable dt = new DataTable();
            string query = "";

            switch (type.ToLower())
            {
                case "monthly":
                    query = @"
        SELECT pbh.MaNhanVien,
               DATEPART(YEAR, pbh.NgayTao) AS Nam,
               DATEPART(MONTH, pbh.NgayTao) AS Ky,
               SUM(ctpbh.SoLuong * ctpbh.DonGia) AS TongDoanhThu
        FROM PhieuBanHang AS pbh
        JOIN ChiTietPhieu AS ctpbh ON pbh.MaPhieu = ctpbh.MaPhieu
        WHERE pbh.TrangThai = 1
        GROUP BY pbh.MaNhanVien, DATEPART(YEAR, pbh.NgayTao), DATEPART(MONTH, pbh.NgayTao)
        ORDER BY pbh.MaNhanVien, Nam, Ky;";
                    break;

                case "weekly":
                    if (tuNgay == null || denNgay == null)
                        throw new ArgumentException("TuNgay và DenNgay là bắt buộc cho thống kê tuần.");

                    query = @"
        SELECT 
            pbh.MaNhanVien,
            DATEPART(YEAR, pbh.NgayTao) AS Nam,
            DATEPART(ISO_WEEK, pbh.NgayTao) AS Ky,
            SUM(ctpbh.SoLuong * ctpbh.DonGia) AS TongDoanhThu
        FROM PhieuBanHang AS pbh
        JOIN ChiTietPhieu AS ctpbh ON pbh.MaPhieu = ctpbh.MaPhieu
        WHERE 
            pbh.TrangThai = 1
            AND pbh.NgayTao BETWEEN @TuNgay AND @DenNgay
        GROUP BY 
            pbh.MaNhanVien, 
            DATEPART(YEAR, pbh.NgayTao), 
            DATEPART(ISO_WEEK, pbh.NgayTao)
        ORDER BY 
            pbh.MaNhanVien, 
            DATEPART(YEAR, pbh.NgayTao), 
            DATEPART(ISO_WEEK, pbh.NgayTao);";
                    break;

                case "quarterly":
                    query = @"
        SELECT pbh.MaNhanVien,
               DATEPART(YEAR, pbh.NgayTao) AS Nam,
               DATEPART(QUARTER, pbh.NgayTao) AS Ky,
               SUM(ctpbh.SoLuong * ctpbh.DonGia) AS TongDoanhThu
        FROM PhieuBanHang AS pbh
        JOIN ChiTietPhieu AS ctpbh ON pbh.MaPhieu = ctpbh.MaPhieu
        WHERE pbh.TrangThai = 1
        GROUP BY pbh.MaNhanVien, DATEPART(YEAR, pbh.NgayTao), DATEPART(QUARTER, pbh.NgayTao)
        ORDER BY pbh.MaNhanVien, Nam, Ky;";
                    break;

                default:
                    throw new ArgumentException("Unsupported statistic type: " + type);
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    // Nếu là thống kê theo tuần thì thêm tham số vào cmd
                    if (type.ToLower() == "weekly")
                    {
                        if (tuNgay == null || denNgay == null)
                            throw new ArgumentException("TuNgay và DenNgay là bắt buộc cho thống kê tuần.");

                        cmd.Parameters.AddWithValue("@TuNgay", tuNgay.Value.Date);
                        cmd.Parameters.AddWithValue("@DenNgay", denNgay.Value.Date);
                    }

                    conn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi lấy dữ liệu thống kê dạng " + type + ": " + ex.Message);
                throw;
            }


            return dt;
        }

        public DataTable LayTop5SanPhamBanChay(DateTime tuNgay, DateTime denNgay)
        {
            DataTable dt = new DataTable();
            string procedureName = "sp_Top5SanPhamBanChay";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(procedureName, conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TuNgay", tuNgay);
                cmd.Parameters.AddWithValue("@DenNgay", denNgay);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }

            return dt;
        }

        public DataTable LayDoanhThuTheoThang(DateTime tuNgay, DateTime denNgay)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_ThongKeTongDoanhThu", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TuNgay", tuNgay.Date);
                cmd.Parameters.AddWithValue("@DenNgay", denNgay.Date);

                try
                {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }
                catch (Exception ex)
                {
                    throw new Exception("Lỗi khi lấy doanh thu theo tháng.", ex);
                }
            }
            return dt;
        }

        public DataTable ThongKeSoPhieuTheoSanPham(string maLoai, DateTime tuNgay, DateTime denNgay)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("TKSoPhieuTheoSanPham", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@MaLoai", string.IsNullOrEmpty(maLoai) ? (object)DBNull.Value : maLoai);
                cmd.Parameters.AddWithValue("@TuNgay", tuNgay);
                cmd.Parameters.AddWithValue("@DenNgay", denNgay);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }

            return dt;
        }
    }

    public class DBConnections
    {
        // Chuỗi kết nối đến cơ sở dữ liệu PolyCafe
        private static string connectionString = "Data Source=SD20302\\ADMINCUTE;Initial Catalog=QLPolycafe;Integrated Security=True;";

        // Phương thức lấy SqlConnection
        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        // Phương thức thực thi câu lệnh SQL (INSERT, UPDATE, DELETE)
        public static int ExecuteNonQuery(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }
                    return command.ExecuteNonQuery();
                }
            }
        }

        // Phương thức thực thi câu lệnh SQL trả về dữ liệu (SELECT)
        public static DataTable ExecuteQuery(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }
                    DataTable data = new DataTable();
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    adapter.Fill(data);
                    return data;
                }
            }
        }
    }
    public class EmployeeDAL
    {
        private static string connectionString = "Data Source=SD20302\\ADMINCUTE;Initial Catalog=QLPolycafe;Integrated Security=True;";
        // Lấy tất cả nhân viên
        public List<EmployeeDTO> GetAllEmployees()
        {
            List<EmployeeDTO> employees = new List<EmployeeDTO>();
            // Câu lệnh SQL để lấy tất cả nhân viên
            string query = "SELECT MaNhanVien, HoTen, Email, MatKhau, VaiTro, TrangThai FROM NhanVien";
            DataTable data = DBConnections.ExecuteQuery(query);

            foreach (DataRow row in data.Rows)
            {
                EmployeeDTO employee = new EmployeeDTO
                (
                    row["MaNhanVien"].ToString(),
                    row["HoTen"].ToString(),
                    row["Email"].ToString(),
                    row["MatKhau"].ToString(),
                    (bool)row["VaiTro"],
                    (bool)row["TrangThai"]
                );
                employees.Add(employee);
            }
            return employees;
        }
        public bool GetVaiTroByEmail(string email)
        {
            string query = "SELECT VaiTro FROM NhanVien WHERE Email = @Email";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Email", email);
                conn.Open();
                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    return Convert.ToBoolean(result); // true: admin, false: nhân viên
                }
            }

            return false; // nếu không tìm thấy hoặc lỗi thì coi như không có quyền
        }



        // Thêm một nhân viên mới
        public bool AddEmployee(EmployeeDTO employee)
        {
            // Câu lệnh SQL để thêm nhân viên
            string query = "INSERT INTO NhanVien (MaNhanVien, HoTen, Email, MatKhau, VaiTro, TrangThai) VALUES (@MaNhanVien, @HoTen, @Email, @MatKhau, @VaiTro, @TrangThai)";
            SqlParameter[] parameters = new SqlParameter[]
            {
             new SqlParameter("@MaNhanVien", employee.EmployeeId),
             new SqlParameter("@HoTen", employee.FullName),
             new SqlParameter("@Email", employee.Email),
             new SqlParameter("@MatKhau", employee.Password),
             new SqlParameter("@VaiTro", employee.Role),
             new SqlParameter("@TrangThai", employee.Status)
            };
            int rowsAffected = DBConnections.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        // Cập nhật thông tin nhân viên
        public bool UpdateEmployee(EmployeeDTO employee)
        {
            // Câu lệnh SQL để cập nhật nhân viên
            string query = "UPDATE NhanVien SET HoTen = @HoTen, Email = @Email, MatKhau = @MatKhau, VaiTro = @VaiTro, TrangThai = @TrangThai WHERE MaNhanVien = @MaNhanVien";
            SqlParameter[] parameters = new SqlParameter[]
            {
             new SqlParameter("@HoTen", employee.FullName),
             new SqlParameter("@Email", employee.Email),
             new SqlParameter("@MatKhau", employee.Password),
             new SqlParameter("@VaiTro", employee.Role),
             new SqlParameter("@TrangThai", employee.Status),
             new SqlParameter("@MaNhanVien", employee.EmployeeId)
            };
            int rowsAffected = DBConnections.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        // Xóa nhân viên theo mã
        public bool DeleteEmployee(string employeeId)
        {
            // Câu lệnh SQL để xóa nhân viên
            string query = "DELETE FROM NhanVien WHERE MaNhanVien = @MaNhanVien";
            SqlParameter[] parameters = new SqlParameter[]
            {
             new SqlParameter("@MaNhanVien", employeeId)
            };
            int rowsAffected = DBConnections.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        // Tìm kiếm nhân viên theo Mã NV hoặc Tên NV
        public List<EmployeeDTO> SearchEmployees(string keyword)
        {
            List<EmployeeDTO> employees = new List<EmployeeDTO>();
            // Câu lệnh SQL để tìm kiếm nhân viên
            string query = "SELECT MaNhanVien, HoTen, Email, MatKhau, VaiTro, TrangThai FROM NhanVien WHERE MaNhanVien LIKE @Keyword OR HoTen LIKE @Keyword";
            SqlParameter[] parameters = new SqlParameter[]
            {
             new SqlParameter("@Keyword", "%" + keyword + "%") // Sử dụng LIKE để tìm kiếm gần đúng
            };
            DataTable data = DBConnections.ExecuteQuery(query, parameters);

            foreach (DataRow row in data.Rows)
            {
                EmployeeDTO employee = new EmployeeDTO
                (
                    row["MaNhanVien"].ToString(),
                    row["HoTen"].ToString(),
                    row["Email"].ToString(),
                    row["MatKhau"].ToString(),
                    (bool)row["VaiTro"],
                    (bool)row["TrangThai"]
                );
                employees.Add(employee);
            }
            return employees;
        }

        // Kiểm tra xem Email đã tồn tại chưa
        public bool IsEmailExist(string email, string excludeEmployeeId = null)
        {
            // Câu lệnh SQL để kiểm tra email
            string query = "SELECT COUNT(*) FROM NhanVien WHERE Email = @Email";
            List<SqlParameter> parameters = new List<SqlParameter>
         {
             new SqlParameter("@Email", email)
         };

            if (!string.IsNullOrEmpty(excludeEmployeeId))
            {
                query += " AND MaNhanVien <> @ExcludeEmployeeId";
                parameters.Add(new SqlParameter("@ExcludeEmployeeId", excludeEmployeeId));
            }

            using (SqlConnection connection = DBConnections.GetConnection())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddRange(parameters.ToArray());
                    int count = (int)command.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        // Kiểm tra xem Mã NV đã tồn tại chưa
        public bool IsEmployeeIdExist(string employeeId)
        {
            // Câu lệnh SQL để kiểm tra mã nhân viên
            string query = "SELECT COUNT(*) FROM NhanVien WHERE MaNhanVien = @MaNhanVien";
            SqlParameter[] parameters = new SqlParameter[]
            {
             new SqlParameter("@MaNhanVien", employeeId)
            };

            using (SqlConnection connection = DBConnections.GetConnection())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddRange(parameters);
                    int count = (int)command.ExecuteScalar();
                    return count > 0;
                }
            }
        }
    }
    public class HoSo_DAL
    {
        private string connString = "Data Source=SD20302\\ADMINCUTE;Initial Catalog=QLPolycafe;Integrated Security=True;";
        public DataTable GetUser(string email)
        {
            DataTable userInfo = new DataTable();
            using (SqlConnection connection = new SqlConnection(connString))
            {
                string query = "SELECT HoTen, Email, VaiTro FROM NhanVien WHERE Email = @Email";
                SqlCommand command = new SqlCommand(query, connection);

                // ✅ THÊM DÒNG NÀY:
                command.Parameters.AddWithValue("@Email", email);

                SqlDataAdapter adapter = new SqlDataAdapter(command);
                adapter.Fill(userInfo);
            }
            return userInfo;
        }


    }

}
