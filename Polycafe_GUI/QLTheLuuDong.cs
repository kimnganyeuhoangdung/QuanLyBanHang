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
using Polycafe_DTO;
using Polycafe_BUS;

namespace Polycafe_GUI
{
    public partial class QLTheLuuDong : UserControl
    {
        private string _vaiTro;
       
        public QLTheLuuDong(string vaiTro)
        {
            InitializeComponent();
            _vaiTro = vaiTro;
            // Đặt DropDownStyle để ngăn người dùng nhập liệu vào ComboBox
            // Tải dữ liệu ban đầu khi control được tạo
            LoadData();
            PopulateCardidComboBox();
            ApDungPhanQuyen();
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        }
        private void ApDungPhanQuyen()
        {
            if (_vaiTro == "1")
            {
                btnAdd.Enabled = true;
                btnUpdate.Enabled = true;
                btnRemove.Enabled = true;
                btnBack.Enabled = true;
                btnfound.Enabled = true;
            }

            else
            {
                btnAdd.Enabled = false;
                btnUpdate.Enabled = false;
                btnRemove.Enabled = false;
                btnBack.Enabled = true;
                btnfound.Enabled = true;

            }
        }


        // Phương thức để tải dữ liệu vào DataGridView
        private void LoadData()
        {
            try
            {
                var bus = new QLTheLuuDongBUS();
                dataGridView1.DataSource = bus.GetAllTheLuuDong();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Phương thức mới để điền dữ liệu vào comboSP
        private void PopulateCardidComboBox()
        {
            try
            {
                var bus = new QLTheLuuDongBUS();
                List<string> card = bus.GetAllTheLuuDong().Select(x => x.MaThe).ToList(); // Fixed line


                // --- Bắt đầu phần gỡ lỗi ---
                if (card != null && card.Any())
                {
                    // Đã bỏ dòng MessageBox.Show để tránh làm phiền người dùng cuối
                    // MessageBox.Show($"Đã tải {productNames.Count} sản phẩm. Sản phẩm đầu tiên: {productNames.First()}", "Thông báo gỡ lỗi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    comboBox1.DataSource = card;
                }
                else
                {
                    MessageBox.Show("Không tìm thấy sản phẩm nào để tải vào ComboBox hoặc có lỗi khi tải dữ liệu.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    comboBox1.DataSource = null; // Đảm bảo ComboBox trống
                }
                // --- Kết thúc phần gỡ lỗi ---
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách sản phẩm: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UserControl1_Load(object sender, EventArgs e)
        {
            // Sự kiện này kích hoạt khi UserControl được tải vào container của nó
            // LoadData(); // Đã gọi trong hàm tạo, không cần gọi lại ở đây trừ khi có lý do đặc biệt
        }

        private void label7_Click(object sender, EventArgs e)
        {
            // Xử lý sự kiện trống
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            // Xử lý sự kiện trống
        }

        private void ngaytao_ValueChanged(object sender, EventArgs e)
        {
            // Xử lý sự kiện trống
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                // Lấy dữ liệu từ các control trên form
                string maThe = id_card.Text.Trim(); // MaThe là CHAR(6) trong DB, nên nó là một chuỗi
                string chuSoHuu = textBox1.Text.Trim();
               

                // Kiểm tra dữ liệu đầu vào
                if (string.IsNullOrEmpty(maThe) || string.IsNullOrEmpty(chuSoHuu))
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Tạo DTO
                bool trangThai = radioButton1.Checked; // TrangThai là BIT trong DB, nên nó là một boolean
                var theLuuDong = new QLTheLuuDongDTO
                {
                    MaThe = maThe,
                    ChuSoHuu = chuSoHuu,
                    TrangThai = trangThai,
                    
                };

                // Gọi BUS để thêm vào DB
                var bus = new QLTheLuuDongBUS();
                bus.AddTheLuuDong(theLuuDong);

                MessageBox.Show("Thêm thẻ lưu động thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Làm mới form nếu cần
                // ClearForm(); // Bỏ ghi chú và thực hiện nếu cần
                LoadData();  // Tải lại dữ liệu DataGridView
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                // Lấy dữ liệu từ form
                string maThe = id_card.Text.Trim(); // MaThe là CHAR(6) trong DB, nên nó là một chuỗi
                string chuSoHuu = textBox1.Text.Trim();
                bool trangThai = radioButton1.Checked; // TrangThai là BIT trong DB, nên nó là một boolean

                // Xác thực đầu vào
                if (string.IsNullOrEmpty(maThe) || string.IsNullOrEmpty(chuSoHuu))
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ thông tin (Mã thẻ, Chủ sở hữu)!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Tạo DTO
                var theLuuDong = new QLTheLuuDongDTO
                {
                    MaThe = maThe,
                    ChuSoHuu = chuSoHuu,
                    TrangThai = trangThai,
                    // TenSanPham và SoLuong không được cập nhật trực tiếp cho bảng TheLuuDong
                    // Nếu cần cập nhật các trường này, phương thức DAL và DTO sẽ cần phản ánh điều đó cho ChiTietPhieu
                };

                // Gọi BUS để cập nhật thẻ
                var bus = new QLTheLuuDongBUS();
                bus.UpdateTheLuuDong(theLuuDong);

                MessageBox.Show("Cập nhật thẻ lưu động thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData(); // Tải lại dữ liệu DataGridView
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            try
            {
                // Lấy mã thẻ từ control
                string maThe = id_card.Text.Trim(); // MaThe là CHAR(6) trong DB, nên nó là một chuỗi

                if (string.IsNullOrEmpty(maThe))
                {
                    MessageBox.Show("Vui lòng chọn một thẻ để xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Xác nhận xóa
                var result = MessageBox.Show("Bạn có chắc muốn xóa thẻ lưu động này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    // Gọi BUS để xóa
                    var bus = new QLTheLuuDongBUS();
                    bus.RemoveTheLuuDong(maThe);
                    MessageBox.Show("Xóa thẻ lưu động thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData();  // Tải lại dữ liệu DataGridView
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            ResetForm();
        }
        private void ResetForm()
        {
            textBox1.Clear();
            id_card.Clear();

            radioButton1.Checked = true; // Đặt về "Hoạt động"
            radioButton2.Checked = false;

            comboBox1.SelectedIndex = -1; // Xóa nội dung tìm kiếm

            // Xóa hoặc chú thích các dòng không tồn tại trong context
            // selectedEmployeeId = null; // Không có dòng nào được chọn
            // txtEmployeeId.ReadOnly = false; // Cho phép nhập Mã NV khi thêm mới

            btnAdd.Enabled = true; // Cho phép nút Thêm
            btnUpdate.Enabled = false; // Vô hiệu hóa nút Cập nhật
            btnRemove.Enabled = false; 
            // Vô hiệu hóa nút Xóa (chú thích nếu không tồn tại)
            LoadData();
        }

        private void comboSP_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Xử lý sự kiện trống
        }

        private void btnSP_Click(object sender, EventArgs e)
        {
            // Xử lý sự kiện trống
        }

        private void btnXoaSP_Click(object sender, EventArgs e)
        {
            // Xử lý sự kiện trống
        }

        private void btnfound_Click(object sender, EventArgs e)
        {
            try
            {
                string searchTerm = comboBox1.Text.Trim();

                if (string.IsNullOrEmpty(searchTerm))
                {
                    MessageBox.Show("Vui lòng nhập Mã thẻ để tìm kiếm!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    LoadData();
                    PopulateCardidComboBox();
                    return;
                }

                var bus = new QLTheLuuDongBUS();
                List<QLTheLuuDongDTO> searchResults = bus.SearchTheLuuDongByMaThe(searchTerm);

                if (searchResults.Count > 0)
                {
                    dataGridView1.DataSource = searchResults;
                    MessageBox.Show($"Tìm thấy {searchResults.Count} kết quả.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    dataGridView1.DataSource = null;
                    MessageBox.Show("Không tìm thấy thẻ lưu động nào với mã thẻ này.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tìm kiếm: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void textboxTim_TextChanged(object sender, EventArgs e)
        {
            // Xử lý sự kiện trống
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Xử lý sự kiện trống
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Kiểm tra xem có hàng nào được nhấp không và hàng đó không phải là tiêu đề cột
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                // Lấy giá trị MaThe từ ô tương ứng (ví dụ: cột có tên "MaThe")
                // Đảm bảo tên cột chính xác trong DataGridView của bạn
                string maThe = row.Cells["MaThe"].Value.ToString();

                // Điền giá trị vào textbox id_card
                id_card.Text = maThe;

                // Tùy chọn: Điền các trường khác nếu cần cho chỉnh sửa
                textBox1.Text = row.Cells["ChuSoHuu"].Value.ToString();
                bool trangThai = (bool)row.Cells["TrangThai"].Value;
                radioButton1.Checked = trangThai;
                // Giả sử bạn có radioButton2 cho trạng thái "Không hoạt động"
                // if (radioButton2 != null) radioButton2.Checked = !trangThai; 
                comboBox1.Text = row.Cells["MaThe"].Value.ToString();
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {
            // Xử lý sự kiện trống
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}



