using System;
using System.IO; // Thêm thư viện này để kiểm tra File.Exists
using System.Threading.Tasks;
using System.Windows.Forms;

// Đảm bảo bạn đã thêm tham chiếu đến AxWindowsMediaPlayer
// using AxWMPLib; // Có thể không cần dòng này nếu đã kéo thả control

namespace Polycafe_GUI
{
    public partial class Welcome : Form
    {
        private int dotCount = 0;
        private string tempVideoPath = string.Empty; // Khai báo biến để lưu đường dẫn tệp tạm thời

        public Welcome()
        {
            InitializeComponent();
            progressBar1.Style = ProgressBarStyle.Marquee;
            progressBar1.MarqueeAnimationSpeed = 30;

            // Di chuyển logic xử lý video vào Form_Load
            Task.Delay(10000).ContinueWith(t =>
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

        private void Welcome_Load(object sender, EventArgs e)
        {
            // Tên của tệp video trong Resources.
            // Ví dụ: nếu bạn thêm file "Untitled - Made with FlexClip.mp4",
            // thì tên trong Resources sẽ là "Untitled___Made_with_FlexClip".
            // Bạn có thể kiểm tra tên chính xác bằng cách mở file .resx trong Solution Explorer
            // hoặc xem qua IntelliSense khi gõ Polycafe_GUI.Properties.Resources.
            byte[] videoBytes = Polycafe_GUI.Properties.Resources.Untitled_Made_with_FlexClip; // THAY ĐỔI TÊN NÀY CHO PHÙ HỢP VỚI TÊN RESOURCE CỦA BẠN

            if (videoBytes != null && videoBytes.Length > 0)
            {
                // Tạo một tệp tạm thời để lưu trữ video
                tempVideoPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".mp4"); // Đặt đuôi tệp cho đúng định dạng video

                try
                {
                    File.WriteAllBytes(tempVideoPath, videoBytes);

                    // Phát video từ tệp tạm thời
                    axWindowsMediaPlayer1.URL = tempVideoPath;
                    axWindowsMediaPlayer1.Ctlcontrols.play(); // Tự động phát
                                                              // Ẩn thanh công cụ điều khiển
                    axWindowsMediaPlayer1.uiMode = "none";

                    // Hẹn giờ để đóng form sau khi video kết thúc hoặc sau một thời gian nhất định
                    // Bạn có thể dùng sự kiện PlayStateChange để biết khi video kết thúc
                    // hoặc tiếp tục dùng Task.Delay như cũ nếu video có thời lượng cố định.
                  
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi phát video từ tài nguyên: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Không tìm thấy dữ liệu video trong tài nguyên.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Welcome_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Ngăn chặn người dùng ngắt ứng dụng trong khi đang load
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
            }

            // Dọn dẹp tệp tạm thời khi Form đóng hoàn toàn
            if (!string.IsNullOrEmpty(tempVideoPath) && File.Exists(tempVideoPath))
            {
                try
                {
                    // Đảm bảo trình phát đã dừng trước khi xóa tệp
                    if (axWindowsMediaPlayer1 != null && axWindowsMediaPlayer1.playState != WMPLib.WMPPlayState.wmppsStopped)
                    {
                        axWindowsMediaPlayer1.Ctlcontrols.stop();
                    }
                    File.Delete(tempVideoPath);
                }
                catch (Exception ex)
                {
                    // Ghi log lỗi nếu không thể xóa tệp (có thể do tệp đang được sử dụng)
                    Console.WriteLine("Could not delete temporary video file: " + ex.Message);
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            // Các hoạt động của timer (ví dụ: cập nhật label trạng thái)
            // dotCount = (dotCount + 1) % 4;
            // label1.Text = "Welcome To PolyCafe" + new string('.', dotCount);
        }

        // Các sự kiện khác của controls không thay đổi
        private void progressBar1_Click(object sender, EventArgs e) { }
        private void pictureBox2_Click(object sender, EventArgs e) { }
        private void pictureBox1_Click(object sender, EventArgs e) { }
        private void axWindowsMediaPlayer1_Enter(object sender, EventArgs e) { }
        private void axWindowsMediaPlayer1_Enter_1(object sender, EventArgs e) { }
    }
}