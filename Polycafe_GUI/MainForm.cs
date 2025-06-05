using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Polycafe_GUI
{
    public partial class MainForm : Form
    {
        private string currentVaiTro; // "1" cho Admin, "0" cho nhân viên
        // Thêm các using cần thiết cho các UserControl
        private string emailDangNhap;
        private string hoTen;
        public MainForm(string vaiTro, string emaildn, string tenNhanVien)
        {
            InitializeComponent();
            SetupSideMenu();
            currentVaiTro = vaiTro; // lấy từ kết quả đăng nhập 
            this.emailDangNhap = emaildn;
            this.hoTen = tenNhanVien;
            label2.Text =  tenNhanVien ;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //Hiển thị mặc định

            //DisplayUserControl(new QLNhanVien(currentVaiTro));

        }


        private void button8_Click(object sender, EventArgs e)
        {
            //Nếu button này không liên quan đến menu thì giữ nguyên.
        }

        private void SetupSideMenu()
        {
            foreach (Control control in flowLayoutPanel1.Controls)
            {
                if (control is Button button)
                {
                    button.Click += MenuItem_Click;
                }
            }
        }

        private void MenuItem_Click(object sender, EventArgs e)
        {
            Button clickedButton = sender as Button;
            if (clickedButton != null)
            {
                panel1.Controls.Clear();
                UserControl newContentControl = null;

                if (clickedButton.Text == "Nhân viên")
                {
                    // Truyền đúng vai trò của user hiện tại
                    newContentControl = new QLNhanVien(currentVaiTro);
                }
                else if (clickedButton.Text == "Sản phẩm")
                {
                    newContentControl = new QuanLySanPham(currentVaiTro);
                }
                else if (clickedButton.Text == "Loại sản phẩm")
                {
                    newContentControl = new QLLoaiSanPham(currentVaiTro);
                }
                else if (clickedButton.Text == "Phiếu bán hàng")
                {
                    newContentControl = new QLPhieuBanHang();
                }
                else if (clickedButton.Text == "Thẻ lưu động")
                {
                    newContentControl = new QLTheLuuDong(currentVaiTro);
                }
                else if (clickedButton.Text == "Cài đặt")
                {
                    var caiDatControl = new CaiDat(emailDangNhap);  // ✅ Truyền đúng email
                    newContentControl = caiDatControl;
                }

                else if (clickedButton.Text == "Thống kê")
                {
                    newContentControl = new ThongKe(currentVaiTro);
                }

                if (newContentControl != null)
                {
                    DisplayUserControl(newContentControl);
                }
            }
        }
        private void DisplayUserControl(UserControl control)
        {
            control.Dock = DockStyle.Fill;
            panel1.Controls.Add(control);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Thêm code xử lý cho button1 nếu cần
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Thêm code xử lý cho button2 nếu cần
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button8_Click_1(object sender, EventArgs e)
        {
            // Hiển thị MessageBox xác nhận
            DialogResult result = MessageBox.Show("Bạn có chắc chắn muốn đăng xuất không?", "Xác nhận Đăng xuất", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            // Kiểm tra kết quả từ MessageBox
            if (result == DialogResult.Yes)
            {
                // Nếu người dùng chọn Yes, ẩn Form hiện tại và hiển thị Form Đăng nhập
                this.Hide(); // Ẩn MainForm
                Login login = new Login();
                login.Show(); // Hiển thị LoginForm
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {

        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {
            foreach (Control control in flowLayoutPanel1.Controls)
            {
                if (control is Button button)
                {
                    button.Click += MenuItem_Click;

                    if (button.Text == "Nhân viên" && currentVaiTro != "1")
                    {
                        button.Visible = false;
                    }
                    if (button.Text == "Thống kê" && currentVaiTro != "1")
                    {
                        button.Visible = false;
                    }
                }
            }
        }
    }
}
