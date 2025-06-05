using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient; // Đảm bảo đã import
using Polycafe_BUS;
using Polycafe_DTO;
using static System.Windows.Forms.VisualStyles.VisualStyleElement; // Có thể không cần thiết nếu không sử dụng VisualStyleElement

namespace Polycafe_GUI
{
    public partial class QLLoaiSanPham : UserControl
    {
        private string _vaiTro;
        private qlLSP_BUS bus = new qlLSP_BUS();
        private DataTable originalData; // Để lưu trữ dữ liệu gốc cho chức năng tìm kiếm/đặt lại
        SanPhamBUS bussp = new SanPhamBUS();

        public QLLoaiSanPham(string vaiTro)
        {
            InitializeComponent();
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _vaiTro = vaiTro;
            ApDungPhanQuyen();

        }
        private void LoadComboBoxLoaiSanPham()
        { 
            DataTable dsLoaiSP = bus.GetAllLoaiSanPham(); // busLSP là đối tượng của lớp BUS
            comboBox1.DataSource = dsLoaiSP;
            comboBox1.DisplayMember = "TenLoai"; // Hiển thị tên loại sản phẩm
            comboBox1.ValueMember = "MaLoai";     // Giá trị thực sự là mã loại
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList; // Không cho nhập tay
        }

        private void ApDungPhanQuyen()
        {
            if (_vaiTro == "1")
            {
                button1.Enabled = true;
                button2.Enabled = true;
                button3.Enabled = true;
                button4.Enabled = true;
                button5.Enabled = true;
            }

            else
            {
                button1.Enabled = false;
                button2.Enabled = false;
                button3.Enabled = false;
                button4.Enabled = true;
                button5.Enabled = true;

            }
        }

        private void LoadData()
        {
            try
            {
                originalData = bus.get(); // Lấy dữ liệu mẫu từ CSDL

                if (originalData != null && originalData.Rows.Count > 0)
                {
                    dataGridView1.DataSource = originalData;
                }
                else
                {
                    MessageBox.Show("Không có dữ liệu để hiển thị.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    dataGridView1.DataSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi truy xuất dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                dataGridView1.DataSource = null;
            }
        }


    

        private void ClearFields()
        {
            textBox4.Clear();
            textBox3.Clear();
            textBox2.Clear();
            comboBox1.SelectedIndex = -1;
        }

     

        private void button1_Click(object sender, EventArgs e) // Thêm
        {
            if (string.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("Vui lòng không để trống Mã loại", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (bus.check(textBox2.Text))
            {
                MessageBox.Show("Mã loại đã tồn tại", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var loai = new qlLSP
            {
                MaLoai = textBox2.Text,
                TenLoai = textBox3.Text,
                GhiChu = textBox4.Text
            };

            if (bus.add(loai))
            {
                MessageBox.Show("Thêm thành công", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
                ClearFields();
            }
            else
            {
                MessageBox.Show("Thêm thất bại", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e) // Cập nhật
        {
            if (string.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("Vui lòng nhập Mã loại để cập nhật", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!bus.check(textBox2.Text))
            {
                MessageBox.Show("Mã loại không tồn tại để cập nhật", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var loai = new qlLSP
            {
                MaLoai = textBox2.Text,
                TenLoai = textBox3.Text,
                GhiChu = textBox4.Text
            };

            if (bus.update(loai))
            {
                MessageBox.Show("Cập nhật thành công", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
                ClearFields();
            }
            else
            {
                MessageBox.Show("Cập nhật thất bại", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object sender, EventArgs e) // Xóa
        {
            if (string.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("Vui lòng chọn Mã loại để xóa", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string maLoai = textBox2.Text;

            // Kiểm tra xem có sản phẩm nào thuộc mã loại này không
            List<SanPhamDTO> dsSanPham = bussp.GetSanPhamByMaLoai(maLoai);

            if (dsSanPham.Count > 0)
            {
                MessageBox.Show(
                    $"Không thể xóa loại sản phẩm '{maLoai}' vì đang có {dsSanPham.Count} sản phẩm liên quan.\n" +
                    "Vui lòng xóa toàn bộ sản phẩm thuộc loại này trước.",
                    "Không thể xóa",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            var loaiSanPham = new qlLSP { MaLoai = maLoai };

            if (MessageBox.Show($"Bạn có chắc muốn xóa loại sản phẩm có mã '{maLoai}' không?", "Xác nhận Xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (bus.delete(loaiSanPham))
                {
                    MessageBox.Show("Xóa loại sản phẩm thành công", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData();
                    ClearFields();
                }
                else
                {
                    MessageBox.Show("Xóa thất bại. Có thể loại sản phẩm này đang được sử dụng ở nơi khác.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private void button4_Click(object sender, EventArgs e) // Đặt lại / Hiển thị tất cả
        {
            LoadData(); // Tải lại toàn bộ dữ liệu gốc
            ClearFields(); // Xóa các trường nhập liệu
            comboBox1.SelectedIndex = -1; // Đặt lại ComboBox
        }

        private void button5_Click(object sender, EventArgs e) // Tìm kiếm
        {
            string maCanTim = comboBox1.SelectedValue?.ToString();

            if (string.IsNullOrEmpty(maCanTim))
            {
                MessageBox.Show("Vui lòng chọn mã sản phẩm cần tìm.", "Thông báo");
                return;
            }

            bool timThay = false;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["MaLoai"].Value != null && row.Cells["MaLoai"].Value.ToString() == maCanTim)
                {
                    dataGridView1.ClearSelection(); // Bỏ chọn cũ
                    row.Selected = true; // Tô dòng được tìm thấy
                    dataGridView1.FirstDisplayedScrollingRowIndex = row.Index; // Cuộn đến dòng
                    timThay = true;
                    break;
                }
            }

            if (!timThay)
            {
                MessageBox.Show("Không tìm thấy sản phẩm.", "Kết quả");
            }
        }

        // Event handler cho DataGridView cell click để điền dữ liệu vào các TextBox

        private void dataGridView1_CellClick_1(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                // Lấy hàng hiện tại mà người dùng đã click
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                textBox2.Text = row.Cells["MaLoai"].Value?.ToString();
                textBox3.Text = row.Cells["TenLoai"].Value?.ToString();
                textBox4.Text = row.Cells["GhiChu"].Value?.ToString();
            }
        }
      
        private void QLLoaiSanPham_Load_1(object sender, EventArgs e)
        {
            LoadData();
            // Đặt AutoGenerateColumns của DataGridView thành true nếu bạn muốn nó tự động tạo cột
            // Nếu bạn đã tự định nghĩa cột trong designer, hãy đảm bảo DataPropertyName của chúng khớp với tên cột trong database.
            dataGridView1.AutoGenerateColumns = true;
            LoadComboBoxLoaiSanPham(); // Tải dữ liệu cho ComboBox loại sản phẩm
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }
    }
}





