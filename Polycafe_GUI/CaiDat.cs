using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using Polycafe_BUS;
using Polycafe_DTO;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;
using System.IO;

namespace Polycafe_GUI
{

    public partial class CaiDat : UserControl
    {

        private string connectionString = "Data Source=.;Initial Catalog=QLPolyCafe;Integrated Security=True;";
        private NhanVienBLL nhanVienBLL;
        private HoSo_BUS bus = new HoSo_BUS();
        private string userEmail;
        private bool isWebViewInitialized = false;


        public CaiDat(string email)
        {
            InitializeComponent();
            nhanVienBLL = new NhanVienBLL(connectionString);
            this.userEmail = email;
            LoadUser();
            LoadGioiThieuPolyCafe(); // Gọi phương thức này khi Form được khởi tạo
            InitializeWebView2();
        }

        private void LoadGioiThieuPolyCafe()
        {

            string gioiThieu = "\r\n\r\n" +
                     "\r\n\r\n" +
                     "\r\n" +
                     "        Chào mừng bạn đến với Hệ thống quản lý PolyCafe \r\n\r\n" +
                     "        Giải pháp phần mềm toàn diện được phát triển bởi Nhóm 4.\r\n\r\n" +
                     "        Các thành viên:\r\n" +
                     "            * Nguyễn Huỳnh Kim Ngân\r\n" +
                     "            * Trịnh Minh Uyên\r\n" +
                     "            * Võ Phan Hoàng Dung\r\n" +
                     "            * Bùi Nhật Huy\r\n\r\n" +
                     "        PolyCafe được thiết kế đặc biệt nhằm tối ưu hóa và đơn giản hóa mọi\r\n " +
                     "        quy trình vận hành trong các quán cà phê, từ quản lý nhân viên, sản phẩm,\r\n" +
                     "        loại sản phẩm, thẻ lưu động, phiếu bán hàng, thống kê theo nhân viên \r\n" +
                     "        hoặc theo sản phẩm. Với giao diện trực quan và các\r\n" +
                     "        tính năng mạnh mẽ, hệ thống hứa hẹn sẽ mang lại hiệu quả vượt trội,\r\n " +
                     "        giúp chủ quán dễ dàng kiểm soát và phát triển công việc kinh doanh\r\n " +
                     "        của mình.";



            label11.Text = gioiThieu;
            label11.AutoSize = true;
            label11.Padding = new Padding(10);
            label11.MaximumSize = new Size(this.ClientSize.Width - 40, 0); // Gò chiều rộng để tự xuống dòng


        }

        private void LoadUser()
        {

            DataTable userInfo = bus.Getuser(userEmail);
            if (userInfo.Rows.Count > 0)
            {
                textBox5.Text = userInfo.Rows[0]["HoTen"].ToString();
                textBox6.Text = userInfo.Rows[0]["Email"].ToString();
                textBox7.Text = Convert.ToBoolean(userInfo.Rows[0]["VaiTro"]) ? "Quản Lý" : "Nhân viên Bán Hàng ";

                txtEmail.Text = userInfo.Rows[0]["Email"].ToString();
            }
            else
            {
                MessageBox.Show("Không tìm thấy người dùng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void chkShowOldPassword_CheckedChanged(object sender, EventArgs e)
        {
            txtOldPassword.PasswordChar = chkShowOldPassword.Checked ? '\0' : '*';
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
            string email = txtEmail.Text.Trim();
            string oldPassword = txtOldPassword.Text;
            string newPassword = txtNewPassword.Text;
            string confirmPassword = txtConfirmPassword.Text;

            ChangePasswordDTO changePassword = new ChangePasswordDTO()
            {
                Email = email,
                OldPassword = oldPassword,
                NewPassword = newPassword,
                ConfirmPassword = confirmPassword
            };

            try
            {
                // Kiểm tra các trường dữ liệu đầu vào
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Kiểm tra tài khoản và mật khẩu hiện tại
                if (!nhanVienBLL.CheckAccount(email, oldPassword))
                {
                    MessageBox.Show("Sai tài khoản hoặc mật khẩu hiện tại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Kiểm tra mật khẩu mới và xác nhận mật khẩu
                if (newPassword != confirmPassword)
                {
                    MessageBox.Show("Mật khẩu mới và xác nhận mật khẩu không trùng khớp!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Nếu không có lỗi nào xảy ra, hiển thị messagebox xác nhận
                DialogResult result = MessageBox.Show("Bạn có chắc chắn muốn đổi mật khẩu?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    if (nhanVienBLL.ChangePassword(changePassword))
                    {
                        MessageBox.Show("Đổi mật khẩu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        txtOldPassword.Text = "";
                        txtNewPassword.Text = "";
                        txtConfirmPassword.Text = "";
                    }
                    else
                    {
                        MessageBox.Show("Đã có lỗi xảy ra khi đổi mật khẩu!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                // Nếu người dùng chọn "No" trong messagebox xác nhận, không thực hiện đổi mật khẩu
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void InitializeWebView2()
        {
            // Đặt UserDataFolder vào một vị trí có quyền ghi (ví dụ: %LOCALAPPDATA%\YourAppName)
            string appName = "Polycafe_GUI"; // Thay thế bằng tên ứng dụng của bạn
            string userDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appName);

            // Kiểm tra và tạo thư mục nếu chưa tồn tại
            if (!Directory.Exists(userDataFolder))
            {
                Directory.CreateDirectory(userDataFolder);
            }

            // Tạo môi trường WebView2 với UserDataFolder đã chỉ định
            var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
            await wbsGuide.EnsureCoreWebView2Async(env);

            // Tải trang web của bạn
            // wbsGuide.Source = new Uri("https://www.google.com"); // Thay thế bằng URL của bạn

            if (wbsGuide == null)
            {
                MessageBox.Show("WebView2 control not found or initialized.");
                return;
            }

            try
            {
                await wbsGuide.EnsureCoreWebView2Async(null);
                wbsGuide.CoreWebView2.Navigate("http://quanlypolycafe.atwebpages.com/");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi khởi tạo WebView2: {ex.Message}");
                return;
            }
        }

        private void wbsGuide_Click(object sender, EventArgs e)
        {

        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == tabPage2 && !isWebViewInitialized)
            {
                this.BeginInvoke((MethodInvoker)(async () =>
                {
                    try
                    {
                        await wbsGuide.EnsureCoreWebView2Async(null);
                        isWebViewInitialized = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("WebView2 Error: " + ex.Message);
                    }
                }));
            }
        }
    }

}


