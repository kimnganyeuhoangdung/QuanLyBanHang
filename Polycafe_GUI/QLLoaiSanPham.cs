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
// Removed: using static System.Windows.Forms.VisualStyles.VisualStyleElement; // Not typically needed

namespace Polycafe_GUI
{
    public partial class QLLoaiSanPham : UserControl
    {
        private string _vaiTro;
        private qlLSP_BUS bus = new qlLSP_BUS(); // Consider renaming qlLSP_BUS to LoaiSanPhamBUS for consistency
        private DataTable originalData; // Để lưu trữ dữ liệu gốc cho chức năng tìm kiếm/đặt lại
        private SanPhamBUS bussp = new SanPhamBUS(); // Used for foreign key check before deleting category
        private string selectproducttype = null; // Stores the MaLoai of the currently selected row

        public QLLoaiSanPham(string vaiTro)
        {
            InitializeComponent();
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _vaiTro = vaiTro;
            ApDungPhanQuyen(); // Apply permissions on load
            LoadData(); // Load data into DataGridView on load
            LoadComboBoxLoaiSanPham(); // Populate the search combobox on load
            SetInitialButtonState(); // Set initial button states for adding new
        }

        private void SetInitialButtonState()
        {
            // Set buttons for "Add New" mode
            button1.Enabled = (_vaiTro == "1"); // Add button
            button2.Enabled = false; // Update button
            button3.Enabled = false; // Remove button
            textBox2.ReadOnly = false; // MaLoai input should be editable for new entries
            selectproducttype = null; // Clear selected ID
        }

        private void LoadComboBoxLoaiSanPham()
        {
            try
            {
                DataTable dsLoaiSP = bus.GetAllLoaiSanPham(); // busLSP là đối tượng của lớp BUS
                if (dsLoaiSP != null && dsLoaiSP.Rows.Count > 0)
                {
                    comboBox1.DataSource = dsLoaiSP;
                    comboBox1.DisplayMember = "TenLoai"; // Hiển thị tên loại sản phẩm
                    comboBox1.ValueMember = "MaLoai";    // Giá trị thực sự là mã loại
                    comboBox1.SelectedIndex = -1; // Clear initial selection
                }
                else
                {
                    comboBox1.DataSource = null; // Ensure combobox is empty if no data
                }
                comboBox1.DropDownStyle = ComboBoxStyle.DropDownList; // Không cho nhập tay
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu cho ComboBox loại sản phẩm: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApDungPhanQuyen()
        {
            if (_vaiTro == "1") // Admin role
            {
                button1.Visible = true; // Add
                button2.Visible = true; // Update
                button3.Visible = true; // Remove
                button4.Visible = true; // Reset
                button5.Visible = true; // Find
            }
            else // Non-admin role
            {
                button1.Visible = false;
                button2.Visible = false;
                button3.Visible = false;
                button4.Visible = true;
                button5.Visible = true;
                // Buttons are disabled by default or re-enabled by SetInitialButtonState
                // which handles the 'Enabled' property. 'Visible' hides them completely.
                // Depending on your UI design, you might use Enabled = false instead of Visible = false
            }
            // Initial state set in constructor to ensure proper enabled/disabled state for visible buttons
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
                    MessageBox.Show("Không có dữ liệu loại sản phẩm để hiển thị.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    dataGridView1.DataSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi truy xuất dữ liệu loại sản phẩm: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                dataGridView1.DataSource = null;
            }
        }

        private void ClearFields()
        {
            textBox2.Clear(); // MaLoai
            textBox3.Clear(); // TenLoai
            textBox4.Clear(); // GhiChu
            comboBox1.SelectedIndex = -1; // Clear search combobox selection
            selectproducttype = null; // Crucially clear the selected ID
            textBox2.ReadOnly = false; // Allow editing MaLoai for new entries
            ApDungPhanQuyen(); // Re-apply permissions to handle button states based on role
                               // Then, specifically set button states for "add new" mode if admin
            if (_vaiTro == "1")
            {
                button1.Enabled = true; // Add
                button2.Enabled = false; // Update
                button3.Enabled = false; // Remove
            }
        }

        private void button1_Click(object sender, EventArgs e) // Thêm (Add)
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("Vui lòng không để trống Mã loại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox2.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(textBox3.Text))
            {
                MessageBox.Show("Vui lòng không để trống Tên loại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox3.Focus();
                return;
            }

            // Check if MaLoai already exists
            if (bus.check(textBox2.Text.Trim())) // Assuming check(MaLoai) checks for existence
            {
                MessageBox.Show("Mã loại đã tồn tại. Vui lòng nhập mã khác.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                textBox2.Focus();
                return;
            }
            // Check if TenLoai already exists (case-insensitive for new entries)
            if (bus.CheckTenLoaiExists(textBox3.Text.Trim()))
            {
                MessageBox.Show("Tên loại đã tồn tại. Vui lòng nhập tên khác.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                textBox3.Focus();
                return;
            }

            var loai = new qlLSP // Consider renaming qlLSP to LoaiSanPhamDTO
            {
                MaLoai = textBox2.Text.Trim(),
                TenLoai = textBox3.Text.Trim(),
                GhiChu = textBox4.Text.Trim()
            };

            try
            {
                if (bus.add(loai))
                {
                    MessageBox.Show("Thêm loại sản phẩm thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData();
                    LoadComboBoxLoaiSanPham(); // Refresh combobox
                    ClearFields();
                }
                else
                {
                    MessageBox.Show("Thêm loại sản phẩm thất bại. Vui lòng kiểm tra lại dữ liệu.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm loại sản phẩm: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e) // Cập nhật (Update)
        {
            // Ensure MaLoai is not editable during update.
            // This should typically be set to true when a row is selected in the DataGridView.
            textBox2.ReadOnly = true;

            // VALIDATION: Ensure a category is selected for update
            // 'selectproducttype' should hold the MaLoai of the currently selected row from DataGridView.
            if (string.IsNullOrEmpty(selectproducttype))
            {
                MessageBox.Show("Vui lòng chọn một loại sản phẩm để cập nhật.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Get the current values from the textboxes
            string maLoaiToUpdate = textBox2.Text.Trim(); // This should already be the MaLoai from the selected row
            string tenLoaiMoi = textBox3.Text.Trim();
            string ghiChuMoi = textBox4.Text.Trim();

            // Input validation for required fields
            if (string.IsNullOrWhiteSpace(maLoaiToUpdate))
            {
                MessageBox.Show("Mã loại không được để trống.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox2.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(tenLoaiMoi))
            {
                MessageBox.Show("Tên loại không được để trống.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox3.Focus();
                return;
            }

            // CRUCIAL: Get the original 'TenLoai' of the selected item from the DataGridView.
            // This is vital to allow updates to other fields even if the name itself isn't changing.
            string originalTenLoai = "";
            if (dataGridView1.SelectedRows.Count > 0)
            {
                // Assuming "TenLoai" is the column name in your DataGridView
                originalTenLoai = dataGridView1.SelectedRows[0].Cells["TenLoai"].Value?.ToString();
            }
            else
            {
                // Fallback: If no row is explicitly selected, try to get the original name using the MaLoaiToUpdate
                // This might involve another BUS call, but is less ideal than relying on selected row.
                // For robustness, ensure dataGridView1.SelectedRows[0] is always reliable after a CellClick.
                // If 'selectproducttype' is guaranteed to be the ID of a selected row, this branch may not be strictly necessary.
                // However, if the grid is filtered or repopulated without re-selecting, this could be an issue.
                // For simplicity, we assume the DataGridView's selected row is accurate here.
            }


            // Logic for checking duplicate "Tên loại":
            // Only check if the 'tenLoaiMoi' is different from the 'originalTenLoai'.
            // If they are the same, it means the user didn't change the name, so no duplicate check is needed for itself.
            if (!tenLoaiMoi.Equals(originalTenLoai, StringComparison.OrdinalIgnoreCase))
            {
                // If the new name is different, then check if this new name already exists in the database
                // for *any other* product type.
                if (bus.CheckTenLoaiExists(tenLoaiMoi)) // Assuming this method exists in your BUS
                {
                    MessageBox.Show("Tên loại đã tồn tại cho loại sản phẩm khác. Vui lòng nhập tên khác.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    textBox3.Focus();
                    return; // Stop the update process
                }
            }

            // At this point, the name is either unchanged, or it's a new name that doesn't conflict.
            // Now, verify the MaLoai exists for update (if your update method requires it)
           

            // Create DTO object with updated information
            var loai = new qlLSP // Remember to rename qlLSP to LoaiSanPhamDTO for better practice
            {
                MaLoai = maLoaiToUpdate, // Use the MaLoai from the textbox (which was from the selected row)
                TenLoai = tenLoaiMoi,
                GhiChu = ghiChuMoi
                // Assuming 'TrangThai' (status) is handled elsewhere, or implied by 'GhiChu' if it's not a direct field.
                // If 'TrangThai' is a radio button, add it here similar to QLTheLuuDong:
                // TrangThai = radioButtonActive.Checked // Example if you have a status field
            };

            try
            {
                // Call the BUS layer to perform the update
                if (bus.update(loai)) // Assuming 'update' method returns true on success
                {
                    MessageBox.Show("Cập nhật loại sản phẩm thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData(); // Reload DataGridView to reflect changes
                    LoadComboBoxLoaiSanPham(); // Refresh combobox (assuming this is your search/filter combobox)
                    ClearFields(); // Clear input fields
                    SetInitialButtonState(); // Reset button states (e.g., enable Add, disable Update/Remove)
                }
                else
                {
                    MessageBox.Show("Cập nhật loại sản phẩm thất bại. Có lỗi xảy ra hoặc dữ liệu không hợp lệ.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật loại sản phẩm: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object sender, EventArgs e) // Xóa (Delete)
        {
            if (string.IsNullOrWhiteSpace(textBox2.Text) || string.IsNullOrEmpty(selectproducttype))
            {
                MessageBox.Show("Vui lòng chọn một loại sản phẩm để xóa.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string maLoai = textBox2.Text.Trim(); // Use the value from the textbox for consistency

            // Check for foreign key constraint: Are there any products associated with this category?
            List<SanPhamDTO> dsSanPham = bussp.GetSanPhamByMaLoai(maLoai);

            if (dsSanPham != null && dsSanPham.Count > 0)
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

            // Confirmation before deletion
            var result = MessageBox.Show($"Bạn có chắc muốn xóa loại sản phẩm có mã '{maLoai}' không?", "Xác nhận Xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                var loaiSanPham = new qlLSP { MaLoai = maLoai }; // DTO for deletion, only MaLoai is needed

                try
                {
                    if (bus.delete(loaiSanPham))
                    {
                        MessageBox.Show("Xóa loại sản phẩm thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadData();
                        LoadComboBoxLoaiSanPham(); // Refresh combobox
                        ClearFields();
                    }
                    else
                    {
                        MessageBox.Show("Xóa thất bại. Có thể loại sản phẩm này đang được sử dụng ở nơi khác hoặc không tồn tại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xóa loại sản phẩm: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button4_Click(object sender, EventArgs e) // Đặt lại / Hiển thị tất cả (Reset / Show All)
        {
            LoadData(); // Tải lại toàn bộ dữ liệu gốc
            ClearFields(); // Xóa các trường nhập liệu
            SetInitialButtonState(); // Reset button states to initial "add new" mode
        }

        private void button5_Click(object sender, EventArgs e) // Tìm kiếm (Search)
        {
            string maCanTim = comboBox1.SelectedValue?.ToString();

            if (string.IsNullOrEmpty(maCanTim))
            {
                MessageBox.Show("Vui lòng chọn Mã loại sản phẩm cần tìm.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LoadData(); // Show all if search term is empty
                return;
            }

            // Filter the originalData (or query the BUS for a specific item if needed)
            DataView dv = new DataView(originalData);
            dv.RowFilter = $"MaLoai = '{maCanTim}'"; // Filter by MaLoai
            dataGridView1.DataSource = dv;

            if (dv.Count > 0)
            {
                MessageBox.Show($"Tìm thấy {dv.Count} kết quả.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // Optionally scroll to the first found row
                dataGridView1.ClearSelection();
                dataGridView1.Rows[0].Selected = true;
                dataGridView1.FirstDisplayedScrollingRowIndex = 0;
            }
            else
            {
                MessageBox.Show("Không tìm thấy loại sản phẩm nào với mã này.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dataGridView1.DataSource = null; // Clear display if not found
            }
        }

        // Event handler cho DataGridView cell click để điền dữ liệu vào các TextBox
        private void dataGridView1_CellClick_1(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                // Lấy hàng hiện tại mà người dùng đã click
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                // Lấy MaLoai từ dòng được chọn
                string maLoaiSelected = row.Cells["MaLoai"].Value?.ToString();

                // Điền giá trị vào các TextBox
                textBox2.Text = maLoaiSelected;
                textBox3.Text = row.Cells["TenLoai"].Value?.ToString();
                textBox4.Text = row.Cells["GhiChu"].Value?.ToString();

                // Cập nhật biến selectproducttype với Mã loại của dòng đã chọn
                selectproducttype = maLoaiSelected; // <--- CRITICAL FIX APPLIED HERE

                // Đặt TextBox Mã loại là ReadOnly để ngăn chỉnh sửa mã khi cập nhật
                textBox2.ReadOnly = true;

                // Adjust button states after a row is selected (for admin users)
                if (_vaiTro == "1")
                {
                    button1.Enabled = false; // Disable Add button
                    button2.Enabled = true;  // Enable Update button
                    button3.Enabled = true;  // Enable Remove button
                }
            }
        }

        private void QLLoaiSanPham_Load_1(object sender, EventArgs e)
        {
            // LoadData() and LoadComboBoxLoaiSanPham() are already called in the constructor,
            // so typically no need to call them here again unless there's a specific reason.
            // This event fires when the UserControl is loaded onto its parent.
            dataGridView1.AutoGenerateColumns = true; // Make sure columns are generated
        }

        // Empty event handlers - can be left as is if no logic is required for them
        private void groupBox1_Enter(object sender, EventArgs e) { }
        private void textBox4_TextChanged(object sender, EventArgs e) { }
    }
}