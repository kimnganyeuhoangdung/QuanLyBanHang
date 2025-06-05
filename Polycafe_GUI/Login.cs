using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Polycafe_BUS;
using Polycafe_DTO;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Polycafe_GUI
{
    public partial class Login : Form
    {
        private LoginBUS bus = new LoginBUS();
        bool isUserHovered = false;
        bool isPassHovered = false;

        private bool isHover = false;
        private bool isClicking = false;

        bool isHoverExit = false;
        bool isClickingExit = false;

        private string fullText = "     Hello, Please login to get started! ";
        private int position = 0;
        public static class AuthUtil //Lưu lại thông tin khi đăng nhập thành công
        {
            public static LoginDTO CurrentUser { get; set; }
        }
        public Login()
        {
            InitializeComponent();
            this.AcceptButton = btnLogin;
            timer1.Interval = 200; // 200ms mỗi lần chạy
            timer1.Start();
        }
       
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                panel4_Click(null, null); // Gọi sự kiện đăng nhập của panel3
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            int radius = 30; // Độ cong góc
            GraphicsPath path = new GraphicsPath();
            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(this.Width - radius, 0, radius, radius, 270, 90);
            path.AddArc(this.Width - radius, this.Height - radius, radius, radius, 0, 90);
            path.AddArc(0, this.Height - radius, radius, radius, 90, 90);
            path.CloseAllFigures();
            this.Region = new Region(path);
        }
        private void RoundControl(Control c, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(c.Width - radius, 0, radius, radius, 270, 90);
            path.AddArc(c.Width - radius, c.Height - radius, radius, radius, 0, 90);
            path.AddArc(0, c.Height - radius, radius, radius, 90, 90);
            path.CloseAllFigures();
            c.Region = new Region(path);
        }
        private void MakePanelRounded(Control panel)
        {
            int radius = panel.Height / 2; // viên thuốc = bo nửa chiều cao
            GraphicsPath path = new GraphicsPath();
            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(panel.Width - radius, 0, radius, radius, 270, 90);
            path.AddArc(panel.Width - radius, panel.Height - radius, radius, radius, 0, 90);
            path.AddArc(0, panel.Height - radius, radius, radius, 90, 90);
            path.CloseAllFigures();
            panel.Region = new Region(path);
        }


        private void Form1_Load_1(object sender, EventArgs e)
        {
           
            // Bo góc cho nút
            RoundControl(btnExit, 20);


            user.MouseEnter += user_MouseEnter;
            user.MouseLeave += user_MouseLeave;
            pass.MouseEnter += pass_MouseEnter;
            pass.MouseLeave += pass_MouseLeave;

            pass.PasswordChar = show.Checked ? '\0' : '*';
            string savedUser = Properties.Settings.Default.SavedUser;
            string savedPass = Properties.Settings.Default.SavedPass;

            if (!string.IsNullOrEmpty(savedUser) && !string.IsNullOrEmpty(savedPass))
            {
                user.Text = savedUser;
                pass.Text = savedPass;
                chkremember.Checked = true;
            }
            else
            {
                chkremember.Checked = false;
            }

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void user_TextChanged_1(object sender, EventArgs e)
        {

        }


       

        private void btnExit_Click_1(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Confirm exit the program?",
                                                "Exit",
                                                MessageBoxButtons.YesNo,
                                                MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void show_CheckedChanged_1(object sender, EventArgs e)
        {
            pass.PasswordChar = show.Checked ? '\0' : '*';

        }

        private void pass_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void panel1_Paint_1(object sender, PaintEventArgs e)
        {

        }

        private void Login_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Ngăn người dùng thoát bằng nút X mặc định
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
            }
        }

        private void chkremember_CheckedChanged(object sender, EventArgs e)
        {
            if (chkremember.Checked)
            {
                Properties.Settings.Default.SavedUser = user.Text;
                Properties.Settings.Default.SavedPass = pass.Text;
                Properties.Settings.Default.Save();
            }
            else
            {
                Properties.Settings.Default.SavedUser = "";
                Properties.Settings.Default.SavedPass = "";
                Properties.Settings.Default.Save();
            }
        }

        private void LinkForget_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string userEmail = user.Text; // Lấy email từ textbox hoặc biến lưu trữ trong LoginForm
            ForgetPassword forgetPassword = new ForgetPassword(userEmail);
            forgetPassword.Show(); // Hiển thị ForgetPassword
            this.Hide(); // Ẩn LoginForm
        }
        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            
        }

        private void user_MouseEnter(object sender, EventArgs e)
        {
            isUserHovered = true;
            panel1.Invalidate();
        }

        private void user_MouseLeave(object sender, EventArgs e)
        {
            isUserHovered = false;
            panel1.Invalidate();
        }

        private void pass_MouseEnter(object sender, EventArgs e)
        {
            isPassHovered = true;
            panel2.Invalidate();
        }

        private void pass_MouseLeave(object sender, EventArgs e)
        {
            isPassHovered = false;
            panel2.Invalidate();
        }

        private void label2_Click_1(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            position++;
            if (position > fullText.Length)
                position = 0;

            label3.Text = fullText.Substring(position) + fullText.Substring(0, position);
        }


        private void panel3_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color baseColor = isClicking
                ? ColorTranslator.FromHtml("#5D4037")
                : isHover ? ColorTranslator.FromHtml("#A47148")
                          : ColorTranslator.FromHtml("#3E2723");

            using (SolidBrush brush = new SolidBrush(baseColor))
            {
                GraphicsPath path = new GraphicsPath();
                int radius = panel3.Height;
                path.AddArc(0, 0, radius, radius, 90, 180);
                path.AddArc(panel3.Width - radius, 0, radius, radius, 270, 180);
                path.CloseAllFigures();

                g.FillPath(brush, path);

                // Vẽ chữ
                TextRenderer.DrawText(g, "Đăng nhập", panel3.Font, panel3.ClientRectangle,
                    Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        private void panel3_MouseEnter(object sender, EventArgs e)
        {
            isHover = true;
            panel3.Invalidate();
        }
        private void panel3_MouseLeave(object sender, EventArgs e)
        {
            isHover = false;
            panel3.Invalidate();
        }
        private void panel3_MouseDown(object sender, MouseEventArgs e)
        {
            isClicking = true;
            panel3.Invalidate();
        }
        private void panel3_MouseUp(object sender, MouseEventArgs e)
        {
            isClicking = false;
            panel3.Invalidate();
            btnLogin_Click(sender, e); // Gọi đăng nhập
        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Màu tùy trạng thái
            Color baseColor = isClicking
                ? ColorTranslator.FromHtml("#5D4037") // màu nâu đậm khi click
                : isHover ? ColorTranslator.FromHtml("#A47148") // caramel khi hover
                          : ColorTranslator.FromHtml("#3E2723"); // nâu đậm mặc định

            using (SolidBrush brush = new SolidBrush(baseColor))
            {
                GraphicsPath path = new GraphicsPath();
                int radius = panel4.Height / 2; // Viên thuốc = 1/2 chiều cao

                // Tạo hình viên thuốc
                path.AddArc(0, 0, radius * 2, radius * 2, 90, 180);
                path.AddArc(panel4.Width - radius * 2, 0, radius * 2, radius * 2, 270, 180);
                path.CloseAllFigures();

                g.FillPath(brush, path);

                // Vẽ chữ
                TextRenderer.DrawText(
                    g,
                    "Đăng nhập",
                    panel4.Font,
                    panel4.ClientRectangle,
                    Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        private void panel4_MouseEnter(object sender, EventArgs e)
        {
            isHover = true;
            panel4.Invalidate();
        }

        private void panel4_MouseLeave(object sender, EventArgs e)
        {
            isHover = false;
            isClicking = false;
            panel4.Invalidate();
        }

        private void panel4_MouseDown(object sender, MouseEventArgs e)
        {
            isClicking = true;
            panel4.Invalidate();
        }

        private void panel4_MouseUp(object sender, MouseEventArgs e)
        {
            isClicking = false;
            panel3.Invalidate();
            btnLogin_Click(sender, e); // Gọi đăng nhập
        }

        private void panel5_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Màu theo trạng thái
            Color baseColor = isClickingExit
                ? ColorTranslator.FromHtml("#837060") // Nâu mocha khi click
                : isHoverExit ? ColorTranslator.FromHtml("#A47148") // Vân gỗ khi hover
                              : ColorTranslator.FromHtml("#837060"); // Nâu mocha mặc định

            using (SolidBrush brush = new SolidBrush(baseColor))
            {
                GraphicsPath path = new GraphicsPath();
                int radius = panel5.Height / 2;

                path.AddArc(0, 0, radius * 2, radius * 2, 90, 180);
                path.AddArc(panel5.Width - radius * 2, 0, radius * 2, radius * 2, 270, 180);
                path.CloseAllFigures();

                g.FillPath(brush, path);

                // Vẽ chữ
                TextRenderer.DrawText(
                    g,
                    "Thoát",
                    panel5.Font,
                    panel5.ClientRectangle,
                    Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        private void panel5_MouseEnter(object sender, EventArgs e)
        {
            isHoverExit = true;
            panel5.Invalidate();
        }

        private void panel5_MouseLeave(object sender, EventArgs e)
        {
            isHoverExit = false;
            isClickingExit = false;
            panel5.Invalidate();
        }

        private void panel5_MouseDown(object sender, MouseEventArgs e)
        {
            isClickingExit = true;
            panel5.Invalidate();
        }

        private void panel5_MouseUp(object sender, MouseEventArgs e)
        {
            isClickingExit = false;
            panel5.Invalidate();
            btnExit_Click_1(sender, e); 
        }

        private void panel5_Click(object sender, EventArgs e)
        {

        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = user.Text.Trim();
            string password = pass.Text.Trim();

            if (bus.KiemTraDangNhap(username, password))
            {
                // Ghi nhớ tài khoản nếu được chọn
                if (chkremember.Checked)
                {
                    Properties.Settings.Default.SavedUser = username;
                    Properties.Settings.Default.SavedPass = password;
                    Properties.Settings.Default.Save();
                }
                else
                {
                    Properties.Settings.Default.SavedUser = "";
                    Properties.Settings.Default.SavedPass = "";
                    Properties.Settings.Default.Save();
                }

                // Lấy vai trò người dùng (giả sử trả về bool)
                bool vaiTroNguoiDung = bus.LayVaiTro(username);

                // Tạo DTO, có thể giữ vai trò dạng string hoặc bool
                LoginDTO nhanVien = new LoginDTO
                {
                    Email = username,
                    MatKhau = password,
                    VaiTro = vaiTroNguoiDung ? "1" : "0"
                };

                // Hiển thị thông báo đăng nhập thành công
                MessageBox.Show($"Đăng nhập thành công! Vai trò: {(vaiTroNguoiDung ? "Admin" : "User")}",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Mở MainForm truyền tham số vai trò dưới dạng string "1" hoặc "0"
                MainForm mainForm = new MainForm(vaiTroNguoiDung ? "1" : "0", username, username);
                mainForm.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Sai tên đăng nhập hoặc mật khẩu!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void panel4_Click(object sender, EventArgs e)
        {

        }
    }
}

