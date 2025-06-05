using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Polycafe_GUI
{
    public partial class Welcome : Form
    {
        private int dotCount = 0;
        public Welcome()
        {
            InitializeComponent();
            //Cấu hình kiểu cho thanh progress bar
            progressBar1.Style = ProgressBarStyle.Marquee;
            progressBar1.MarqueeAnimationSpeed = 30;
            //Thực hiện thao tác load chờ 3 giây
            Task.Delay(3000).ContinueWith(t =>
            {
                Invoke(new Action(() =>
                {
                    // Mở form chính
                    Login login = new Login();
                    login.Show();

                    // Đóng form Welcome
                    this.Hide(); // hoặc this.Close(); nếu không cần nữa
                }));
            });
            timer1.Interval = 500;
            timer1.Start();
        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        private void Welcome_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Ngăn chặn người dùng ngắt ứng dụng
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
            }
        }

        private void Welcome_Load(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            dotCount = (dotCount + 1) % 4; // 0 -> 3
            label1.Text = "Welcome To PolyCafe" + new string('.', dotCount);
        }
    }
}
