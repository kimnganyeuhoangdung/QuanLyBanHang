using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Test.DAL;
using Test.DTO;
using Test.BLL;

namespace Polycafe_GUI
{
    public partial class Thongke_Employee : UserControl
    {
        private ThongKe_NhanVien_BLL _thongKeBLL;
        private string userEmail; // Renamed to userEmail to be consistent with parameter name
        private string maNhanVien; // Added to store the employee ID

        public Thongke_Employee(string email)
        {
            InitializeComponent();
            _thongKeBLL = new ThongKe_NhanVien_BLL();
            this.userEmail = email; // Assign the passed email to userEmail field

            LoadUserAndStatistics(); // Call a combined method to load user info and statistics
        }

        private void LoadUserAndStatistics()
        {
            // 1. Get user information (MaNhanVien, HoTen) using the email
            DataTable userInfo = _thongKeBLL.Getuser(userEmail);
            if (userInfo.Rows.Count > 0)
            {
                maNhanVien = userInfo.Rows[0]["MaNhanVien"].ToString(); // Get MaNhanVien
                textBox1.Text = maNhanVien; // Display MaNhanVien
                textBox2.Text = userInfo.Rows[0]["HoTen"].ToString(); // Display HoTen
            }
            else
            {
                MessageBox.Show("Không tìm thấy thông tin người dùng với email này!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                // Optionally disable controls or clear data if user not found
                textBox1.Text = string.Empty;
                textBox2.Text = "Không tìm thấy";
                dataGridView1.DataSource = null;
                textBox3.Text = "0 VNĐ";
                chart1.Series.Clear();
                chart1.Titles.Clear();
                return; // Exit if no user info
            }

            // 2. Load statistics for the retrieved employee ID
            LoadStatisticsForEmployee();
        }

        private void LoadStatisticsForEmployee()
        {
            // Now maNhanVien is available from LoadUserAndStatistics
            if (string.IsNullOrEmpty(maNhanVien))
            {
                MessageBox.Show("Không có mã nhân viên để tải thống kê.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Khoảng thời gian mặc định cho "toàn bộ thời gian"
            DateTime tuNgay = new DateTime(2000, 1, 1);
            DateTime denNgay = DateTime.Now;

            string tenNhanVienFromBLL = string.Empty; // To capture the employee name returned from BLL
            DataTable dtResult = new DataTable();

            try
            {
                // Gọi phương thức BLL để lấy dữ liệu bán hàng chi tiết
                dtResult = _thongKeBLL.ThongKeDoanhThuCuaNhanVien(maNhanVien, tuNgay, denNgay, out tenNhanVienFromBLL);

                // No need to set textBox2.Text here again, it's already set in LoadUserAndStatistics

                // Kiểm tra nếu không có dữ liệu bán hàng
                if (dtResult.Rows.Count == 0)
                {
                    dataGridView1.DataSource = null;
                    MessageBox.Show($"Nhân viên {textBox2.Text} (Mã: {maNhanVien}) chưa có doanh thu nào.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    textBox3.Text = "0 VNĐ"; // Đặt tổng tiền về 0
                    chart1.Series.Clear();
                    chart1.Titles.Clear();
                    return; // Thoát nếu không có dữ liệu
                }

                // Hiển thị dữ liệu lên DataGridView
                dataGridView1.DataSource = dtResult;

                // Tùy chỉnh hiển thị cột ngày tháng và tự động căn chỉnh
                if (dataGridView1.Columns.Contains("NgayLapPhieu"))
                {
                    dataGridView1.Columns["NgayLapPhieu"].DefaultCellStyle.Format = "dd/MM/yyyy";
                }
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                // Hiển thị dữ liệu lên biểu đồ và nhận tổng doanh thu
                decimal overallTotal = HienThiBieuDo(dtResult);

                // Gán tổng doanh thu vào textBox3 với định dạng tiền tệ
                textBox3.Text = $"{overallTotal:N0} VNĐ";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Đã xảy ra lỗi khi tải dữ liệu thống kê: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox3.Text = "Lỗi"; // Hiển thị lỗi
                dataGridView1.DataSource = null;
                chart1.Series.Clear();
                chart1.Titles.Clear();
            }
        }

        private decimal HienThiBieuDo(DataTable dt)
        {
            chart1.Series.Clear();
            chart1.ChartAreas.Clear();
            chart1.Titles.Clear();

            ChartArea area = new ChartArea("Area1");
            chart1.ChartAreas.Add(area);

            Series series = new Series("Doanh thu")
            {
                ChartType = SeriesChartType.Column,
                XValueType = ChartValueType.String
            };

            var monthlyTotals = dt.AsEnumerable()
                                .GroupBy(row => new
                                {
                                    Year = ((DateTime)row["NgayLapPhieu"]).Year,
                                    Month = ((DateTime)row["NgayLapPhieu"]).Month
                                })
                                .Select(g => new
                                {
                                    Nam = g.Key.Year,
                                    Thang = g.Key.Month,
                                    TongTienTrongThang = g.Sum(row => Convert.ToDecimal(row["TongTien"]))
                                })
                                .OrderBy(x => x.Nam)
                                .ThenBy(x => x.Thang)
                                .ToList();

            decimal tongDoanhThuOverall = 0;

            foreach (var monthlyData in monthlyTotals)
            {
                series.Points.AddXY($"{monthlyData.Thang}/{monthlyData.Nam % 100}", monthlyData.TongTienTrongThang);
                tongDoanhThuOverall += monthlyData.TongTienTrongThang;
            }

            chart1.Series.Add(series);
            chart1.Titles.Add("Biểu Đồ Thống Kê Tổng Doanh Thu Của Nhân Viên Qua Mỗi Tháng");
            chart1.ChartAreas[0].AxisX.Title = "Tháng/Năm";
            chart1.ChartAreas[0].AxisY.Title = "Tổng doanh thu (VNĐ)";
            chart1.ChartAreas[0].AxisX.Interval = 1;
            chart1.ChartAreas[0].AxisX.LabelStyle.Angle = -45;

            return tongDoanhThuOverall;
        }

        // Clean up unused/redundant event handlers
        private void textBox1_TextChanged(object sender, EventArgs e) { /* No action needed */ }
        private void textBox2_TextChanged(object sender, EventArgs e) { /* No action needed */ }
        private void groupBox2_Enter(object sender, EventArgs e) { /* No action needed */ }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) { /* No action needed */ }
        private void groupBox3_Enter(object sender, EventArgs e) { /* No action needed */ }
        private void chart3_Click(object sender, EventArgs e) { /* No action needed */ } // This one seems to refer to chart3, ensure it's correct
        private void chart1_Click_1(object sender, EventArgs e) { /* No action needed */ } // Duplicate, should be removed
        private void textBox3_TextChanged(object sender, EventArgs e) { /* No action needed */ }
        private void textBox1_TextChanged_1(object sender, EventArgs e) { /* No action needed */ } // Duplicate, should be removed
        private void textBox2_TextChanged_1(object sender, EventArgs e) { /* No action needed */ } // Duplicate, should be removed
    }
}


namespace Test.DAL
{
    public class ThongKe_NhanVien_DAL
    {
        // Centralize connection string (consider moving to App.config)
        private static string connectionString = "Data Source=.;Initial Catalog=QLPolycafe;Integrated Security=True;";

        public DataTable GetUser(string email)
        {
            DataTable userInfo = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open(); // Open connection here
                // Assumes "MaNhanVien", "HoTen", "Email" are columns in NhanVien table
                string query = "SELECT MaNhanVien, HoTen, Email, VaiTro FROM NhanVien WHERE Email = @Email";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Email", email);
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                adapter.Fill(userInfo);
            }
            return userInfo;
        }

        public DataTable ThongKeDoanhThuCuaNhanVien(string maNV, DateTime tuNgay, DateTime denNgay, out string tenNV)
        {
            DataTable dt = new DataTable();
            tenNV = string.Empty;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                // Ensure this Stored Procedure name matches exactly what's in your SQL Server database.
                // If it's "TKTongDoanhThuMotNgayCuaNhanVien", then use that.
                using (SqlCommand cmd = new SqlCommand("TKDoanhThuCuaNhanVien", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@MaNhanVien", maNV);
                    // Pass these dates even if the SP doesn't use them (as per your comment)
                    cmd.Parameters.AddWithValue("@TuNgay", tuNgay.Date);
                    cmd.Parameters.AddWithValue("@DenNgay", denNgay.Date);

                    SqlParameter tenNVParam = new SqlParameter("@TenNhanVien", SqlDbType.NVarChar, 100)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(tenNVParam);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }

                    tenNV = tenNVParam.Value == DBNull.Value ? string.Empty : tenNVParam.Value.ToString();
                }
            }
            return dt;
        }

        // Removed the TKTongDoanhThuMotNgayCuaNhanVien method as it's redundant and unused.
        // If you need it for a different purpose, keep it and ensure its usage is clear.
    }
}

namespace Test.DTO
{
    public class NhanVien_ThongKe_DTO
    {
        public string MaNhanVien { get; set; }
        public string TenNhanVien { get; set; }
        public decimal TongTien { get; set; }
        public int SoLy { get; set; }
        public int SoLuongPhieu { get; set; }
        public DateTime NgayLapPhieu { get; set; } // Kiểm tra nếu cột này có trong SP của bạn
        public bool TrangThai { get; set; }
    }
   

}

namespace Test.BLL
{
    public class ThongKe_NhanVien_BLL
    {
        private ThongKe_NhanVien_DAL _dal = new ThongKe_NhanVien_DAL();
        // Removed userData as _dal is sufficient

        public DataTable ThongKeDoanhThuCuaNhanVien(string maNV, DateTime tuNgay, DateTime denNgay, out string tenNV)
        {
            return _dal.ThongKeDoanhThuCuaNhanVien(maNV, tuNgay, denNgay, out tenNV);
        }

        // Removed the TKTongDoanhThuMotNgayCuaNhanVien method as it's redundant and unused.
        public DataTable Getuser(string email)
        {
            return _dal.GetUser(email); // Use the single _dal instance
        }
    }
}