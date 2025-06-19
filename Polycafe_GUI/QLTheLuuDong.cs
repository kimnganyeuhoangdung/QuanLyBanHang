using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data; // Still needed for DataTable if you use it elsewhere or for DataView later
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
        private string selectcard = null; // Stores the MaThe of the currently selected card
        private List<QLTheLuuDongDTO> allCardData; // Cache for original data to help with search/reset

        public QLTheLuuDong(string vaiTro)
        {
            InitializeComponent();
            _vaiTro = vaiTro;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Call initial load and setup methods
            LoadData(); // Load data into DataGridView and populate allCardData
            PopulateCardidComboBox(); // Populate the search combobox
            ApDungPhanQuyen(); // Apply permissions
            ////SetInitialButtonState(); // Set initial button states
        }

        private void ApDungPhanQuyen()
        {
            if(_vaiTro == "1")
            {
                btnAdd.Enabled = true;
                btnRemove.Enabled = true;
                btnUpdate.Enabled = true;
                btnBack.Enabled = true;
            }
            else
            {
                btnAdd.Enabled = true;
                btnRemove.Enabled = false;
                btnUpdate.Enabled = true;
                btnBack.Enabled = true;
            }
        }

        private void SetInitialButtonState()
        {
            bool isAdmin = (_vaiTro == "1");

            btnAdd.Enabled = isAdmin; // Only admin can add initially
            btnUpdate.Enabled = false; // Disabled until a row is selected
            btnRemove.Enabled = false; // Disabled until a row is selected
            id_card.ReadOnly = false; // MaThe should be editable for new entries
            selectcard = null; // Clear the selected card ID
            ClearFields(); // Clear input fields
            LoadData(); // Ensure grid is refreshed to all data
            PopulateCardidComboBox(); // Refresh search combobox
        }

        // Method to load data into DataGridView
        private void LoadData()
        {
            try
            {
                var bus = new QLTheLuuDongBUS();
                allCardData = bus.GetAllTheLuuDong(); // Assuming this returns List<QLTheLuuDongDTO>

                if (allCardData != null && allCardData.Count > 0) // Use .Count for List<T>
                {
                    dataGridView1.DataSource = allCardData;

                    dataGridView1.Columns["TrangThai"].Visible = false;
                    dataGridView1.Columns["Status"].HeaderText = "TrangThai";
                }
                else
                {
                    dataGridView1.DataSource = null;
                    // MessageBox.Show("Không có dữ liệu thẻ lưu động để hiển thị.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu thẻ lưu động: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                dataGridView1.DataSource = null;
            }
        }

        // Method to populate comboBox1 with card IDs
        private void PopulateCardidComboBox()
        {
            try
            {
                var bus = new QLTheLuuDongBUS();
                List<QLTheLuuDongDTO> allCards = bus.GetAllTheLuuDong(); // Should return List<QLTheLuuDongDTO>
                List<string> cardIds = allCards.Select(x => x.MaThe).ToList();

                if (cardIds != null && cardIds.Any())
                {
                    comboBox1.DataSource = cardIds;
                    comboBox1.SelectedIndex = -1; // Clear initial selection
                }
                else
                {
                    comboBox1.DataSource = null;
                }
                comboBox1.DropDownStyle = ComboBoxStyle.DropDownList; // Prevent manual input
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách Mã thẻ vào ComboBox: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearFields()
        {
            id_card.Clear();
            textBox1.Clear();
            radioButton1.Checked = true; // Default to 'Active'
            radioButton2.Checked = false;
            comboBox1.SelectedIndex = -1; // Clear search combobox selection
            selectcard = null; // Crucially clear the selected ID
            id_card.ReadOnly = false; // Allow editing MaThe for new entries
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                string maThe = id_card.Text.Trim();
                string chuSoHuu = textBox1.Text.Trim();

                // Input validation
                if (string.IsNullOrEmpty(maThe))
                {
                    MessageBox.Show("Vui lòng nhập Mã thẻ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    id_card.Focus();
                    return;
                }
                if (string.IsNullOrEmpty(chuSoHuu))
                {
                    MessageBox.Show("Vui lòng nhập Chủ sở hữu.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBox1.Focus();
                    return;
                }

                var bus = new QLTheLuuDongBUS();
                // Check if card ID already exists (RE-ENABLED THIS CRUCIAL CHECK)

                bool trangThai = radioButton1.Checked; // true if Active, false if Inactive

                var theLuuDong = new QLTheLuuDongDTO
                {
                    MaThe = maThe,
                    ChuSoHuu = chuSoHuu,
                    TrangThai = trangThai
                };

                bus.AddTheLuuDong(theLuuDong);

                MessageBox.Show("Thêm thẻ lưu động thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                ClearFields(); // Clear input fields
                LoadData(); // Ensure grid is refreshed to all data
                PopulateCardidComboBox();
            }// Refresh search combobox            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm thẻ lưu động: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            // Ensure MaThe is not editable during update, it's set from selection
            id_card.ReadOnly = true;

            try
            {
                // Validation: A card must be selected for update
                if (string.IsNullOrEmpty(selectcard))
                {
                    MessageBox.Show("Vui lòng chọn một thẻ để cập nhật!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Get data from form controls
                string maTheToUpdate = id_card.Text.Trim(); // Use the ID from the textbox (which came from selection)
                string chuSoHuu = textBox1.Text.Trim();
                bool trangThai = radioButton1.Checked;

                // Basic input validation for non-ID fields
                if (string.IsNullOrEmpty(chuSoHuu))
                {
                    MessageBox.Show("Vui lòng nhập Chủ sở hữu.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBox1.Focus();
                    return;
                }

                // Create DTO with the ID for update
                var theLuuDong = new QLTheLuuDongDTO
                {
                    MaThe = maTheToUpdate, // <--- CRITICAL FIX: Assign the MaThe
                    ChuSoHuu = chuSoHuu,
                    TrangThai = trangThai
                };

                var bus = new QLTheLuuDongBUS();
                bus.UpdateTheLuuDong(theLuuDong);

                MessageBox.Show("Cập nhật thẻ lưu động thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearFields(); // Clear input fields
                LoadData(); // Ensure grid is refreshed to all data
                PopulateCardidComboBox();
            }// Refresh search combobox            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật thẻ lưu động: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            try
            {
                string maThe = id_card.Text.Trim();

                if (string.IsNullOrEmpty(maThe) || string.IsNullOrEmpty(selectcard))
                {
                    MessageBox.Show("Vui lòng chọn một thẻ để xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var bus = new QLTheLuuDongBUS();

                // Foreign Key Constraint Check
                if (bus.IsCardUsedInSalesOrder(maThe)) // Assuming this method exists in QLTheLuuDongBUS
                {
                    MessageBox.Show("Thẻ này hiện đang được sử dụng trong các phiếu bán hàng và không thể xóa!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var result = MessageBox.Show($"Bạn có chắc muốn xóa thẻ lưu động có mã '{maThe}' không?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    bus.RemoveTheLuuDong(maThe);
                    MessageBox.Show("Xóa thẻ lưu động thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearFields(); // Clear input fields
                    LoadData(); // Ensure grid is refreshed to all data
                    PopulateCardidComboBox(); // Refresh search combobox
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa thẻ lưu động: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            // Reset to initial state for adding new entries
            ClearFields(); // Clear input fields
            LoadData(); // Ensure grid is refreshed to all data
            PopulateCardidComboBox(); // Refresh search combobox
        }

        private void btnfound_Click(object sender, EventArgs e)
        {
            try
            {
                string searchTerm = comboBox1.Text.Trim();

                if (string.IsNullOrEmpty(searchTerm))
                {
                    MessageBox.Show("Vui lòng chọn Mã thẻ để tìm kiếm!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    LoadData(); // If search term is empty, show all data
                    return;
                }

                var bus = new QLTheLuuDongBUS();
                // Assuming SearchTheLuuDongByMaThe returns List<QLTheLuuDongDTO>
                List<QLTheLuuDongDTO> searchResults = bus.SearchTheLuuDongByMaThe(searchTerm);

                if (searchResults != null && searchResults.Count > 0)
                {
                    dataGridView1.DataSource = searchResults;
                    MessageBox.Show($"Tìm thấy {searchResults.Count} kết quả.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // Clear selection and select the first found item if applicable
                    dataGridView1.ClearSelection();
                    dataGridView1.Rows[0].Selected = true;
                    // Optional: Scroll to the selected row
                    // dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.Rows[0].Index;
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

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                string maThe = row.Cells["MaThe"].Value?.ToString(); // Use null-conditional operator for safety

                // Populate textboxes
                id_card.Text = maThe;
                textBox1.Text = row.Cells["ChuSoHuu"].Value?.ToString(); // Use null-conditional operator
                bool trangThai = (bool)row.Cells["TrangThai"].Value;
                radioButton1.Checked = trangThai;
                radioButton2.Checked = !trangThai; // Assuming radioButton2 is for "Inactive"

                // Set selectcard for update/delete operations
                selectcard = maThe; // <--- CRITICAL FIX: Assign the selected card ID

                // Make MaThe textbox read-only when a row is selected for update/delete
                id_card.ReadOnly = true;

                // Adjust button states for update/delete mode (for admin users)
                if (_vaiTro == "1")
                {
                    btnAdd.Enabled = false;    // Disable Add
                    btnUpdate.Enabled = true;  // Enable Update
                    btnRemove.Enabled = true;  // Enable Remove
                }
            }
        }

        // --- Unused/Empty Event Handlers (can be removed if truly unused) ---
        private void UserControl1_Load(object sender, EventArgs e) { /* Already handled in constructor */ }
        private void label7_Click(object sender, EventArgs e) { }
        private void numericUpDown1_ValueChanged(object sender, EventArgs e) { }
        private void ngaytao_ValueChanged(object sender, EventArgs e) { }
        private void comboSP_SelectedIndexChanged(object sender, EventArgs e) { }
        private void btnSP_Click(object sender, EventArgs e) { }
        private void btnXoaSP_Click(object sender, EventArgs e) { }
        private void textboxTim_TextChanged(object sender, EventArgs e) { }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) { }
        private void groupBox1_Enter(object sender, EventArgs e) { }
        private void label4_Click(object sender, EventArgs e) { }
        private void radioButton1_CheckedChanged(object sender, EventArgs e) { }
        private void radioButton2_CheckedChanged(object sender, EventArgs e) { }
    }
}