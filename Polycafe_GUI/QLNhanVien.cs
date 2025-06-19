using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Polycafe_BUS;
using Polycafe_DTO;

namespace Polycafe_GUI
{
    public partial class QLNhanVien : UserControl
    {
        private string _vaiTro;
        private EmployeeBLL employeeBLL;
        private string selectedEmployeeId = null;

        public QLNhanVien(string vaiTro)
        {
            InitializeComponent();
            _vaiTro = vaiTro;
            employeeBLL = new EmployeeBLL();
            LoadEmployeeData();
            SetupComboBoxRole();
            ApDungPhanQuyen();
            dgvEmployee.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void QLNhanVien_Load(object sender, EventArgs e) { }

        private void ApDungPhanQuyen()
        {
            if (_vaiTro == "1")
            {
                btnAdd.Enabled = true;
                btnUpdate.Enabled = true;
                btnDelete.Enabled = true;
                btnFind.Enabled = true;
                btnReset.Enabled = true;
            }

            else
            {
                btnAdd.Enabled = false;
                btnUpdate.Enabled = false;
                btnDelete.Enabled = false;
                btnFind.Enabled = false;
                btnReset.Enabled = false;
                groupBox1.Hide();
                groupBox2.Hide();
                groupBox3.Hide();
                groupBox4.Hide();
                txtEmail.Hide();
                txtPassword.Hide();
                dgvEmployee.Hide();

            }
        }

        private void LoadEmployeeData()
        {
            dgvEmployee.DataSource = employeeBLL.GetAllEmployees();
            dgvEmployee.Columns["EmployeeId"].HeaderText = "Mã NV";

            dgvEmployee.Columns["FullName"].HeaderText = "Họ tên";
            dgvEmployee.Columns["Email"].HeaderText = "Email";
            dgvEmployee.Columns["Password"].HeaderText = "Mật khẩu";

            dgvEmployee.Columns["Role"].HeaderText = "Vai trò";
            dgvEmployee.Columns["Role"].Visible = false;
            dgvEmployee.Columns["DisplayRole"].HeaderText = "Vai trò";

            dgvEmployee.Columns["Status"].HeaderText = "Trạng thái";
            dgvEmployee.Columns["Status"].Visible = false;
            dgvEmployee.Columns["DisplayStatus"].HeaderText = "Trạng thái";
        }

        private void SetupComboBoxRole()
        {
            cboRole.Items.Clear();
            cboRole.DisplayMember = "Text";
            cboRole.ValueMember = "Value";
            cboRole.Items.Add(new { Text = "Quản lý", Value = true });
            cboRole.Items.Add(new { Text = "Nhân viên", Value = false });
            cboRole.SelectedIndex = 0;
        }

        private EmployeeDTO GetEmployeeDataFromForm()
        {
            string employeeId = txtEmployeeId.Text.Trim();
            bool role = (bool)((dynamic)cboRole.SelectedItem).Value;
            bool status = rdoActive.Checked;

            return new EmployeeDTO(
                employeeId,
                txtEmployeeName.Text.Trim(),
                txtEmail.Text.Trim(),
                txtPassword.Text.Trim(),
                role,
                status
            );
        }

        private void DisplayEmployeeDataOnForm(EmployeeDTO employee)
        {
            if (employee == null) return;

            txtEmployeeId.Text = employee.EmployeeId;
            txtEmployeeName.Text = employee.FullName;
            txtEmail.Text = employee.Email;
            txtPassword.Text = employee.Password;

            foreach (var item in cboRole.Items)
            {
                if (((dynamic)item).Value == employee.Role)
                {
                    cboRole.SelectedItem = item;
                    break;
                }
            }

            rdoActive.Checked = employee.Status;
            rdoInactive.Checked = !employee.Status;
        }

        private void ClearForm()
        {
            txtEmployeeId.Clear();
            txtEmployeeName.Clear();
            txtEmail.Clear();
            txtPassword.Clear();
            cboRole.SelectedIndex = 0;
            rdoActive.Checked = true;
            selectedEmployeeId = null;

            txtEmployeeId.ReadOnly = false;
            btnAdd.Enabled = (_vaiTro == "1");
            btnUpdate.Enabled = false;
            btnDelete.Enabled = false;
            dgvEmployee.ClearSelection();

            // ✅ Cho phép nhập lại Email và Password khi thêm mới
            txtEmail.Enabled = true;
            txtPassword.Enabled = true;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            EmployeeDTO newEmp = GetEmployeeDataFromForm();
            string result = employeeBLL.AddEmployee(newEmp);

            if (result == "SUCCESS")
            {
                MessageBox.Show("Thêm nhân viên thành công!", "Thông báo");
                LoadEmployeeData();
                ClearForm();

                // Khóa Email và Mật khẩu sau khi thêm thành công
                txtEmail.Enabled = false;
                txtPassword.Enabled = false;
            }
            else
            {
                MessageBox.Show("Lỗi: " + result, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            txtEmployeeId.ReadOnly = true; // Cho phép sửa mã nhân viên
            if (selectedEmployeeId == null)
            {
                MessageBox.Show("Vui lòng chọn nhân viên cần cập nhật.");
                return;
            }
            

            EmployeeDTO updated = GetEmployeeDataFromForm();
            updated.EmployeeId = selectedEmployeeId;
            string result = employeeBLL.UpdateEmployee(updated);

            if (result == "SUCCESS")
            {
                MessageBox.Show("Cập nhật thành công!");
                LoadEmployeeData();
                ClearForm();
            }
            else
            {
                MessageBox.Show("Lỗi: " + result, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selectedEmployeeId == null)
            {
                MessageBox.Show("Vui lòng chọn nhân viên cần xóa.");
                return;
            }

            // --- New Restriction Logic ---
            if (employeeBLL.IsEmployeeLinkedToSalesOrders(selectedEmployeeId))
            {
                MessageBox.Show("Không thể xóa nhân viên này vì đã có phiếu bán hàng liên quan.", "Lỗi xóa", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // Stop the deletion process
            }
            // --- End New Restriction Logic ---

            DialogResult confirm = MessageBox.Show("Bạn có chắc muốn xóa nhân viên này?", "Xác nhận", MessageBoxButtons.YesNo);

            if (confirm == DialogResult.Yes)
            {
                string result = employeeBLL.DeleteEmployee(selectedEmployeeId);
                if (result == "SUCCESS")
                {
                    MessageBox.Show("Xóa thành công!");
                    LoadEmployeeData();
                    ClearForm();
                }
                else
                {
                    MessageBox.Show("Lỗi: " + result, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnFind_Click(object sender, EventArgs e)
        {
            string keyword = txtFind.Text.Trim();
            var result = employeeBLL.SearchEmployees(keyword);

            if (result != null && result.Count > 0)
            {
                dgvEmployee.DataSource = result;
                MessageBox.Show($"Tìm thấy {result.Count} kết quả.");
            }
            else
            {
                dgvEmployee.DataSource = null;
                MessageBox.Show("Không tìm thấy nhân viên nào.");
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            LoadEmployeeData();
            ClearForm();
        }

        private void dgvEmployee_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            selectedEmployeeId = dgvEmployee.Rows[e.RowIndex].Cells["EmployeeId"].Value.ToString();

            EmployeeDTO employee = new EmployeeDTO
            (
                dgvEmployee.Rows[e.RowIndex].Cells["EmployeeId"].Value.ToString(),
                dgvEmployee.Rows[e.RowIndex].Cells["FullName"].Value.ToString(),
                dgvEmployee.Rows[e.RowIndex].Cells["Email"].Value.ToString(),
                dgvEmployee.Rows[e.RowIndex].Cells["Password"].Value.ToString(),
                (bool)dgvEmployee.Rows[e.RowIndex].Cells["Role"].Value,
                (bool)dgvEmployee.Rows[e.RowIndex].Cells["Status"].Value
            );

            DisplayEmployeeDataOnForm(employee);

            txtEmployeeId.ReadOnly = true;

            // ✅ Khi chọn từ danh sách thì không cho sửa Email và Mật khẩu
            txtEmail.Enabled = false;
            txtPassword.Enabled = false;

            if (_vaiTro == "1")
            {
                btnUpdate.Enabled = true;
                btnDelete.Enabled = true;
            }
        }
        // Inside your EmployeeBLL or a new SalesOrderBLL
        private bool IsEmployeeLinkedToSalesOrders(string employeeId)
        {
            // This method would query your database (or in-memory data if it's a small app)
            // to see if any sales orders have this employeeId.
            // Example: SELECT COUNT(*) FROM PhieuBanHang WHERE EmployeeId = @employeeId
            // If count > 0, return true; otherwise, false.

            // For demonstration, let's assume this returns true if employeeId is "NV001"
            // You'll replace this with actual database/data source logic.
            if (employeeId == "") // Replace with actual logic to check sales orders
            {
                return true;
            }
            return false;
        }

        private void dgvEmployee_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvEmployee.Columns[e.ColumnIndex].Name == "Password" && e.Value != null)
            {
                e.Value = new string('*', e.Value.ToString().Length);
                e.FormattingApplied = true;
            }
        }
        private void groupBox3_Enter(object sender, EventArgs e)
        {
            // Không cần xử lý gì ở đây, chỉ để tránh lỗi khi groupBox3 được click
        }
        private void groupBox1_Enter(object sender, EventArgs e)
        {
            // Không cần xử lý gì ở đây, chỉ để tránh lỗi khi groupBox1 được click
        }
        private void groupBox2_Enter(object sender, EventArgs e)
        {
            // Không cần xử lý gì ở đây, chỉ để tránh lỗi khi groupBox2 được click
        }
        private void groupBox4_Enter(object sender, EventArgs e)
        {
            // Không cần xử lý gì ở đây, chỉ để tránh lỗi khi groupBox2 được click
        }
        private void txtEmail_TextChanged(object sender, EventArgs e)
        {
            // Không cần xử lý gì ở đây, chỉ để tránh lỗi khi groupBox2 được click
        }

        private void dgvEmployee_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void txtFind_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {
            txtPassword.PasswordChar = '*'; // Hiển thị mật khẩu dưới dạng dấu *
        }

        private void groupBox1_Enter_1(object sender, EventArgs e)
        {

        }
    }
}
