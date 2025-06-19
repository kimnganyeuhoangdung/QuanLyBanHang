using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Polycafe_BUS;
using Polycafe_DTO;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Polycafe_GUI
{
    public partial class QuanLySanPham : UserControl
    {
        private string _vaiTro;
        private string BaseImagePath => Path.Combine(Application.StartupPath, "Images");
        private string selectedproduct = null;
        SanPhamBUS bus = new SanPhamBUS();
        public QuanLySanPham(string vaiTro)
        {
            InitializeComponent();
            _vaiTro = vaiTro;
            ApDungPhanQuyen();
            LoadData();
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void ApDungPhanQuyen()
        {
            bool isAdmin = _vaiTro == "1";

            btnAdd.Enabled = isAdmin;
            btnAnh.Enabled = isAdmin;
            btnUpdate.Enabled = isAdmin;
            btnRemove.Enabled = isAdmin;

            button1.Enabled = true;
            btnBack.Enabled = true;
        }

        public void LoadData()
        {
            SanPhamBUS sanPhamBUS = new SanPhamBUS();
            DataTable dt = sanPhamBUS.GetAllSanPham();
            dataGridView1.DataSource = dt;

            dataGridView1.Columns["TrangThai"].Visible = false;

            if (!dataGridView1.Columns.Contains("Status"))
            {
                DataGridViewTextBoxColumn trangThaiColumn = new DataGridViewTextBoxColumn();
                trangThaiColumn.Name = "Status";
                trangThaiColumn.HeaderText = "TrangThai";
                dataGridView1.Columns.Add(trangThaiColumn);
            }

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["TrangThai"].Value != null)
                {
                    bool status = (bool)row.Cells["TrangThai"].Value;
                    row.Cells["Status"].Value = status ? "Còn hàng" : "Hết hàng";
                }
            }

            comBoLoaiSP.DataSource = sanPhamBUS.GetLoaiSanPham();
            comBoLoaiSP.DisplayMember = "TenLoai";
            comBoLoaiSP.ValueMember = "MaLoai";

            comBofound.DataSource = dt;
            comBofound.DisplayMember = "MaSanPham";
            comBofound.ValueMember = "MaSanPham";
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                // Lấy dữ liệu từ các controls trên form
                string maSanPham = txtMaSP.Text.Trim();
                string tenSanPham = txtTenSP.Text.Trim();
                string maLoai = comBoLoaiSP.SelectedValue?.ToString(); // Sử dụng ?. để tránh NullReferenceException
                decimal donGia = 0;
                // Cố gắng phân tích cú pháp giá trị từ txtDonGia thành decimal
                if (!decimal.TryParse(txtDonGia.Text, out donGia))
                {
                    MessageBox.Show("Đơn giá không hợp lệ. Vui lòng nhập một số.", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (bus.CheckTenSanPhamExists(txtTenSP.Text))
                {
                    MessageBox.Show("Tên sản phẩm đã tồn tại", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                // Xác định trạng thái sản phẩm
                string trangThai = radioButton1.Checked ? "0" : "1"; // Giả định "0" là còn hàng, "1" là hết hàng. Cần kiểm tra lại logic này.
                string tenAnh = pictureBox1.Tag?.ToString(); // Tên ảnh được lưu trong Tag của PictureBox

                // Kiểm tra các trường thông tin bắt buộc
                if (string.IsNullOrEmpty(maSanPham) || string.IsNullOrEmpty(tenSanPham) || string.IsNullOrEmpty(maLoai))
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ thông tin sản phẩm (Mã SP, Tên SP, Loại SP).", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                SanPhamBUS sanPhamBUS = new SanPhamBUS();

                // Tạo đối tượng SanPhamDTO từ dữ liệu nhập vào
                SanPhamDTO sanPham = new SanPhamDTO
                {
                    MaSanPham = maSanPham,
                    TenSanPham = tenSanPham,
                    MaLoai = maLoai,
                    DonGia = donGia,
                    TrangThai = trangThai,
                    HinhAnh = tenAnh
                };

                // Gọi phương thức thêm sản phẩm từ lớp BUS
                if (sanPhamBUS.AddSanPham(sanPham))
                {
                    MessageBox.Show("Thêm sản phẩm thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData();    // Tải lại dữ liệu sau khi thêm
                    ClearForm();   // Xóa dữ liệu trên form
                }
                else
                {
                    MessageBox.Show("Thêm sản phẩm thất bại. Có thể mã sản phẩm đã tồn tại hoặc có lỗi dữ liệu.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                // Xử lý các ngoại lệ và hiển thị thông báo lỗi chi tiết
                MessageBox.Show("Lỗi khi thêm sản phẩm: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAnh_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string originalPath = ofd.FileName;
                string imageName = Path.GetFileName(originalPath);

                if (!Directory.Exists(BaseImagePath))
                    Directory.CreateDirectory(BaseImagePath);

                string newImagePath = Path.Combine(BaseImagePath, imageName);
                File.Copy(originalPath, newImagePath, true);

                pictureBox1.Image = Image.FromFile(newImagePath);
                pictureBox1.Tag = imageName;
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                // We use selectedProduct to ensure we are updating the product that was originally selected
                txtMaSP.ReadOnly = true; // This line is actually redundant here, but harmless
                if (string.IsNullOrEmpty(selectedproduct))
                {
                    MessageBox.Show("Vui lòng chọn sản phẩm cần cập nhật.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                string tenSanPham = txtTenSP.Text.Trim();
                string maLoai = comBoLoaiSP.SelectedValue?.ToString();
                decimal donGia;

                if (!decimal.TryParse(txtDonGia.Text, out donGia) || donGia < 0)
                {
                    MessageBox.Show("Đơn giá không hợp lệ. Vui lòng nhập một số không âm.", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtDonGia.Focus();
                    return;
                }

                // Standardize TrangThai: "0" = Còn hàng, "1" = Hết hàng
                string trangThai = radioButton1.Checked ? "0" : "1";
                string tenAnh = pictureBox1.Tag?.ToString();

                // **THIS IS THE CRITICAL FIX**
                SanPhamDTO sp = new SanPhamDTO
                {
                    MaSanPham = selectedproduct, // <--- UNCOMMENT AND ENSURE THIS LINE IS ACTIVE
                    TenSanPham = tenSanPham,
                    MaLoai = maLoai,
                    DonGia = donGia,
                    TrangThai = trangThai,
                    HinhAnh = tenAnh
                };

                SanPhamBUS bus = new SanPhamBUS();

                string tensp = "";
                if (dataGridView1.SelectedRows.Count > 0)
                {
                    // Assuming "TenLoai" is the column name in your DataGridView
                    tensp = dataGridView1.SelectedRows[0].Cells["TenSanPham"].Value?.ToString();
                }
                
                if (!tensp.Equals(tensp, StringComparison.OrdinalIgnoreCase))
                {
                    // If the new name is different, then check if this new name already exists in the database
                    // for *any other* product type.
                    if (bus.CheckTenSanPhamExists(tensp)) // Assuming this method exists in your BUS
                    {
                        MessageBox.Show("Tên loại đã tồn tại cho loại sản phẩm khác. Vui lòng nhập tên khác.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        txtTenSP.Focus();
                        return; // Stop the update process
                    }
                }

                if (bus.UpdateSanPham(sp))
                {
                    MessageBox.Show("Cập nhật sản phẩm thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData();
                    ClearForm();
                }
                else
                {
                    MessageBox.Show("Cập nhật thất bại. Có lỗi dữ liệu hoặc hệ thống.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật sản phẩm: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                txtMaSP.Text = row.Cells["MaSanPham"].Value.ToString();
                selectedproduct = txtMaSP.Text; // ⚠️ Gán lại để dùng khi cập nhật
                txtTenSP.Text = row.Cells["TenSanPham"].Value.ToString();
                comBoLoaiSP.SelectedValue = row.Cells["MaLoai"].Value.ToString();
                txtDonGia.Text = row.Cells["DonGia"].Value.ToString();

                bool trangThai = (bool)row.Cells["TrangThai"].Value;
                //radioButton1.Checked = trangThai == false;
                //radioButton2.Checked = trangThai != true;

                if (trangThai)
                {
                    radioButton2.Checked = true;
                }
                else
                {
                    radioButton1.Checked = true;
                }

                    txtMaSP.ReadOnly = true;

                string tenAnh = row.Cells["HinhAnh"].Value?.ToString();
                if (!string.IsNullOrEmpty(tenAnh))
                {
                    string imagePath = Path.Combine(BaseImagePath, tenAnh);
                    if (File.Exists(imagePath))
                    {
                        pictureBox1.Image = Image.FromFile(imagePath);
                        pictureBox1.Tag = tenAnh;
                    }
                    else
                    {
                        pictureBox1.Image = null;
                        pictureBox1.Tag = null;
                        MessageBox.Show("Không tìm thấy file ảnh: " + imagePath, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    pictureBox1.Image = null;
                    pictureBox1.Tag = null;
                }
            }
        }

        private void ClearForm()
        {
            txtMaSP.Clear();
            txtTenSP.Clear();
            txtDonGia.Clear();
            pictureBox1.Image = null;
            pictureBox1.Tag = null;
            radioButton1.Checked = true;
            radioButton2.Checked = false;
            comBofound.SelectedIndex = -1;
            comBoLoaiSP.SelectedIndex = -1;
            txtMaSP.ReadOnly = false;
            selectedproduct = null;


        }

        // Các sự kiện không sử dụng
        private void groupBox1_Enter(object sender, EventArgs e) { }
        private void radioButton2_CheckedChanged(object sender, EventArgs e) { }
        private void label8_Click(object sender, EventArgs e) { }
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e) { }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) { }
        private void label6_Click(object sender, EventArgs e) { }
        private void txtDonGia_TextChanged(object sender, EventArgs e) { }
        private void groupBox2_Enter(object sender, EventArgs e) { }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void label4_Click(object sender, EventArgs e) { }
        private void radioButton1_CheckedChanged(object sender, EventArgs e) { }
        private void button1_Click(object sender, EventArgs e) 
        {
            string maCanTim = comBofound.SelectedValue?.ToString();

            if (string.IsNullOrEmpty(maCanTim))
            {
                MessageBox.Show("Vui lòng chọn mã sản phẩm cần tìm.", "Thông báo");
                return;
            }

            bool timThay = false;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["MaSanPham"].Value != null && row.Cells["MaSanPham"].Value.ToString() == maCanTim)
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
        private void btnRemove_Click(object sender, EventArgs e)
        {
            try
            {
                string maSanPham = txtMaSP.Text.Trim();

                if (string.IsNullOrEmpty(maSanPham))
                {
                    MessageBox.Show("Vui lòng chọn sản phẩm để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                SanPhamBUS bus = new SanPhamBUS();

                // Kiểm tra ràng buộc trước khi xóa
                if (bus.IsProductLinkedToSalesDetail(maSanPham))
                {
                    MessageBox.Show("Sản phẩm đang tồn tại trong chi tiết phiếu bán hàng và không thể xóa!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DialogResult result = MessageBox.Show("Bạn có chắc chắn muốn xóa sản phẩm này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    if (bus.DeleteSanPham(maSanPham))
                    {
                        MessageBox.Show("Xóa sản phẩm thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadData();
                        ClearForm();
                    }
                    else
                    {
                        MessageBox.Show("Xóa sản phẩm thất bại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa sản phẩm: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            LoadData();
            ClearForm();
            comBoLoaiSP.SelectedIndex = -1;
            comBofound.SelectedIndex = -1;
        }
        private void pictureBox1_Click(object sender, EventArgs e) { }
        private void QuanLySanPham_Load(object sender, EventArgs e) => LoadData();
    }
}
