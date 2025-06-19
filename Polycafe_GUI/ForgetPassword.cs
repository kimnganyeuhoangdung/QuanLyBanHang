using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using Polycafe.DTO;
using Polycafe.DAL;
using Polycafe.BLL;
using Polycafe_GUI;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace Polycafe_GUI
{
    public partial class ForgetPassword : Form
    {
        private EmployeeBLL _employeeBLL;
        private string _userEmail; // Biến để lưu Email từ form Login

        public ForgetPassword(string email)
        {
            InitializeComponent();
            _employeeBLL = new EmployeeBLL(); // Khởi tạo BLL
            _userEmail = email; // Gán Email được truyền vào

            // Khởi tạo các thuộc tính của control
            InitializeControls();
        }

        private void InitializeControls()
        {
            txtEmail.Text = _userEmail;
            txtEmail.ReadOnly = true; // Vô hiệu hoá chỉnh sửa Email
        }

        private void chkShowNewPassword_CheckedChanged(object sender, EventArgs e)
        {
            txtNewPassword.PasswordChar = chkShowNewPassword.Checked ? '\0' : '*';
        }

        private void chkShowConfirmPassword_CheckedChanged(object sender, EventArgs e)
        {
            txtConfirmPassword.PasswordChar = chkShowConfirmPassword.Checked ? '\0' : '*';
        }

        private void btnChangePassword_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text;
            string newPassword = txtNewPassword.Text;
            string confirmPassword = txtConfirmPassword.Text;

            // Kiểm tra các trường không được để trống (logic cơ bản)
            if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                MessageBox.Show("Mật khẩu mới và Xác nhận mật khẩu không được để trống.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Gọi tầng BLL để xử lý logic đổi mật khẩu
            bool success = _employeeBLL.ChangePassword(email, newPassword, confirmPassword);

            if (success)
            {
                MessageBox.Show($"Đổi mật khẩu thành công, và mật khẩu mới là '{newPassword}'", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Quay lại form Login
                this.Hide();
                Login login = new Login();
                login.Show();
                this.Close();
            }
            else
            {
                // Nếu ChangePassword trả về false, có 2 trường hợp:
                // 1. Mật khẩu mới và xác nhận không trùng khớp (đã xử lý trong BLL)
                // 2. Lỗi khi cập nhật database (DAL trả về false)
                // Trong trường hợp này, thông báo lỗi chung là an toàn nhất hoặc bạn có thể nâng cấp BLL để trả về mã lỗi cụ thể hơn.
                MessageBox.Show("Đổi mật khẩu thất bại do Mật khẩu mới và Xác nhận mật khẩu không trùng khớp, hoặc có lỗi xảy ra.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtEmail_TextChanged(object sender, EventArgs e)
        {

        }

        

        private void ForgetPassword_Load(object sender, EventArgs e)
        {
            txtNewPassword.PasswordChar = chkShowNewPassword.Checked ? '\0' : '*';
            txtConfirmPassword.PasswordChar = chkShowConfirmPassword.Checked ? '\0' : '*';

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        
    }
}

namespace Polycafe.DTO
{
    public class EmployeeDTO
    {
        public string EmployeeId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool Role { get; set; }
        public bool Status { get; set; }
    }
}

namespace Polycafe.DAL
{
    public class DBConnection
    {
        private static string connectionString = "Data Source=SD20302\\ADMINCUTE;Initial Catalog=QLPolycafe;Integrated Security=True;";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
    }

    public class EmployeeDAL
    {
        public bool UpdatePassword(string email, string newPassword)
        {
            string query = "UPDATE NhanVien SET MatKhau = @MatKhau WHERE Email = @Email";

            using (SqlConnection connection = DBConnection.GetConnection())
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MatKhau", newPassword);
                    command.Parameters.AddWithValue("@Email", email);

                    try
                    {
                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0; // Trả về true nếu có ít nhất 1 hàng bị ảnh hưởng (mật khẩu đã được cập nhật)
                    }
                    catch (SqlException ex)
                    {
                        // Ghi log lỗi (tùy chọn) hoặc xử lý ngoại lệ
                        System.Diagnostics.Debug.WriteLine("Lỗi DAL khi cập nhật mật khẩu: " + ex.Message);
                        return false;
                    }
                }
            }
        }

    }
}

namespace Polycafe.BLL
{
    public class EmployeeBLL
    {
        private EmployeeDAL _employeeDAL;

        public EmployeeBLL()
        {
            _employeeDAL = new EmployeeDAL();
        }

        public bool ChangePassword(string email, string newPassword, string confirmPassword)
        {
            // Logic nghiệp vụ: Kiểm tra mật khẩu mới và xác nhận mật khẩu có trùng khớp
            if (newPassword != confirmPassword)
            {
                // Mật khẩu không trùng khớp, không cần gọi DAL
                return false;
            }

            // Gọi DAL để cập nhật mật khẩu vào database
            return _employeeDAL.UpdatePassword(email, newPassword);
        }
    }
}
