using Polycafe_BUS;
using Polycafe_DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Polycafe_GUI
{
    public partial class QLPhieuBanHang : UserControl
    {
        public QLPhieuBanHang()
        {
            InitializeComponent(); // Hàm khởi tạo component của Form
            _saleInvoiceBLL = new SaleInvoiceBLL();
            SetupDataGridViews();
        }

        private void QLPhieuBanHang_Load(object sender, EventArgs e)
        {
            LoadInitialData();
            ClearInvoiceFields();
            ClearDetailFields();
        }

        private SaleInvoiceBLL _saleInvoiceBLL;
        private List<SaleInvoiceDTO> _currentInvoiceList; // Danh sách phiếu đang hiển thị trên dgvSaleInvoices
        private List<SaleInvoiceDetailDTO> _currentDetailList; // Danh sách chi tiết đang hiển thị trên dgvSaleInvoiceDetails

        // --- Cấu hình DataGridViews ---
        private void SetupDataGridViews()
        {
            // DataGridView cho Phiếu Bán Hàng
            dgvSaleInvoices.AutoGenerateColumns = false; // Tự động tạo cột = false để tự định nghĩa
            dgvSaleInvoices.Columns.Clear();
            dgvSaleInvoices.Columns.Add(new DataGridViewTextBoxColumn() { Name = "InvoiceIdCol", HeaderText = "Mã Phiếu", DataPropertyName = "InvoiceId", ReadOnly = true });
            dgvSaleInvoices.Columns.Add(new DataGridViewTextBoxColumn() { Name = "CardOwnerNameCol", HeaderText = "Chủ Thẻ", DataPropertyName = "CardOwnerName", ReadOnly = true });
            dgvSaleInvoices.Columns.Add(new DataGridViewTextBoxColumn() { Name = "EmployeeNameCol", HeaderText = "Nhân Viên", DataPropertyName = "EmployeeName", ReadOnly = true });
            dgvSaleInvoices.Columns.Add(new DataGridViewTextBoxColumn() { Name = "CreatedDateCol", HeaderText = "Ngày Tạo", DataPropertyName = "CreatedDate", ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" } });

            // Ẩn cột TrangThai (bool)
            dgvSaleInvoices.Columns.Add(new DataGridViewCheckBoxColumn() { Name = "StatusHiddenCol", HeaderText = "Trạng Thái", DataPropertyName = "Status", Visible = false });
            // Thay bằng cột hiển thị Trạng thái (string)
            dgvSaleInvoices.Columns.Add(new DataGridViewTextBoxColumn() { Name = "DisplayStatusCol", HeaderText = "Trạng Thái", DataPropertyName = "DisplayStatus", ReadOnly = true });

            // Ẩn các cột MaThe, MaNhanVien nếu không muốn hiển thị trực tiếp
            // Thêm cột ẩn để lưu MaThe và MaNhanVien nếu cần            
            dgvSaleInvoices.Columns.Add(new DataGridViewTextBoxColumn() { Name = "CardIdHiddenCol", DataPropertyName = "CardId", Visible = false });
            dgvSaleInvoices.Columns.Add(new DataGridViewTextBoxColumn() { Name = "EmployeeIdHiddenCol", DataPropertyName = "EmployeeId", Visible = false });


            // DataGridView cho Chi Tiết Phiếu
            dgvSaleInvoiceDetails.AutoGenerateColumns = false;
            dgvSaleInvoiceDetails.Columns.Clear();
            dgvSaleInvoiceDetails.Columns.Add(new DataGridViewTextBoxColumn() { Name = "ProductIdCol", HeaderText = "Mã SP", DataPropertyName = "ProductId", ReadOnly = true });
            dgvSaleInvoiceDetails.Columns.Add(new DataGridViewTextBoxColumn() { Name = "ProductNameCol", HeaderText = "Tên SP", DataPropertyName = "ProductName", ReadOnly = true });
            dgvSaleInvoiceDetails.Columns.Add(new DataGridViewTextBoxColumn() { Name = "QuantityCol", HeaderText = "Số Lượng", DataPropertyName = "Quantity", ReadOnly = true });
            dgvSaleInvoiceDetails.Columns.Add(new DataGridViewTextBoxColumn() { Name = "UnitPriceCol", HeaderText = "Đơn Giá", DataPropertyName = "UnitPrice", ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" } });
            dgvSaleInvoiceDetails.Columns.Add(new DataGridViewTextBoxColumn() { Name = "LineAmountCol", HeaderText = "Thành Tiền", DataPropertyName = "LineAmount", ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" } });
            // Thêm cột ẩn cho ID chi tiết để phục vụ việc xóa/sửa
            dgvSaleInvoiceDetails.Columns.Add(new DataGridViewTextBoxColumn() { Name = "DetailIdHiddenCol", DataPropertyName = "Id", Visible = false });
        }

        // --- Phương thức tải dữ liệu ban đầu cho ComboBoxes và DataGridViews ---
        private void LoadInitialData()
        {
            // Load ComboBox Thẻ Lưu Động
            List<CardDTO> cards = _saleInvoiceBLL.GetAllCards();
            cboCardId.DataSource = cards;
            cboCardId.DisplayMember = "CardId"; // Hiển thị MaThe trên ComboBox
            cboCardId.ValueMember = "CardId";   // Giá trị thực của ComboBox là MaThe
            cboCardId.SelectedIndex = -1; // Chọn không có gì mặc định

            // Load ComboBox Nhân Viên
            List<EmployeeDto> employees = _saleInvoiceBLL.GetAllEmployees();
            cboEmployeeId.DataSource = employees;
            cboEmployeeId.DisplayMember = "EmployeeId"; // Hiển thị MaNhanVien
            cboEmployeeId.ValueMember = "EmployeeId";
            cboEmployeeId.SelectedIndex = -1;

            // Load ComboBox Sản Phẩm (cho chi tiết phiếu)
            List<ProductDTO> products = _saleInvoiceBLL.GetAllProducts();
            cboProductId.DataSource = products;
            cboProductId.DisplayMember = "ProductName"; // Hiển thị TenSanPham
            cboProductId.ValueMember = "ProductId";   // Giá trị thực là MaSanPham
            cboProductId.SelectedIndex = -1;

            // Đặt ngày tạo là ngày hiện tại
            dtpCreatedDate.Value = DateTime.Now;

            // Load danh sách phiếu bán hàng vào dgvSaleInvoices
            LoadSaleInvoicesToDataGridView();

            // Load MaPhieu vào ComboBox Tìm kiếm (cboFind) ---
            LoadInvoiceIdsToFindComboBox();
        }

        // --- Phương thức tải danh sách phiếu bán hàng vào DataGridView ---
        private void LoadSaleInvoicesToDataGridView()
        {
            _currentInvoiceList = _saleInvoiceBLL.GetAllSaleInvoices();
            dgvSaleInvoices.DataSource = _currentInvoiceList;
            dgvSaleInvoices.Refresh(); // Cập nhật hiển thị DataGridView
        }

        // --- Phương thức tải chi tiết phiếu vào DataGridView Chi tiết ---
        private void LoadSaleInvoiceDetailsToDataGridView(string invoiceId)
        {
            _currentDetailList = _saleInvoiceBLL.GetInvoiceDetails(invoiceId);
            dgvSaleInvoiceDetails.DataSource = _currentDetailList;
            dgvSaleInvoiceDetails.Refresh();

            // Cập nhật tổng số lượng và tổng tiền của phiếu chính
            SaleInvoiceDTO currentInvoice = _saleInvoiceBLL.GetSaleInvoiceById(invoiceId);
            if (currentInvoice != null)
            {
                //txtQuantity.Text = currentInvoice.TotalQuantity.ToString();
                txtTotalAmount.Text = currentInvoice.TotalAmount.ToString("N0"); // Định dạng số tiền
            }
            else
            {
                nudQuantity.Text = "0";
                txtTotalAmount.Text = "0";
            }
        }

        // --- Phương thức tải danh sách MaPhieu vào cboFind ---
        private void LoadInvoiceIdsToFindComboBox()
        {
            // Lấy tất cả phiếu để lấy danh sách MaPhieu
            // Bạn có thể tạo một phương thức riêng trong BLL/DAL để chỉ lấy MaPhieu nếu danh sách GetAllSaleInvoices quá lớn
            // Nhưng với mục đích này, việc lấy MaPhieu từ _currentInvoiceList (đã load cho dgvSaleInvoices) là hiệu quả.
            if (_currentInvoiceList != null && _currentInvoiceList.Any())
            {
                // Tạo một danh sách các chuỗi MaPhieu
                List<string> invoiceIds = _currentInvoiceList.Select(inv => inv.InvoiceId).ToList();

                // Thêm một mục "Tất cả phiếu" hoặc "Chọn Mã phiếu" nếu muốn
                invoiceIds.Insert(0, "-- Chọn Mã phiếu --"); // Hoặc một chuỗi gợi ý khác

                cboFind.DataSource = invoiceIds;
                cboFind.SelectedIndex = 0; // Chọn mục gợi ý mặc định
            }
            else
            {
                cboFind.DataSource = null;
            }
        }

        // --- Xóa nội dung các trường nhập liệu phiếu chính ---
        private void ClearInvoiceFields()
        {
            txtInvoiceId.Clear();
            cboCardId.SelectedIndex = -1;
            cboEmployeeId.SelectedIndex = -1;
            dtpCreatedDate.Value = DateTime.Now;
            rdoPending.Checked = true;
            // txtTotalQuantity.Text = "0";
            txtTotalAmount.Text = "0";
            txtInvoiceId.Enabled = true; // Cho phép nhập Mã phiếu khi thêm mới
            dgvSaleInvoiceDetails.DataSource = null;
            _currentDetailList = new List<SaleInvoiceDetailDTO>();

            // Kích hoạt lại các nút khi ở chế độ thêm mới ---
            btnUpdateInvoice.Enabled = true; // Cho phép cập nhật (nếu là phiếu mới, nút này sẽ chưa có tác dụng cho đến khi nhấn "Thêm")
            btnDeleteInvoice.Enabled = true;
            btnAddDetail.Enabled = true; // Cho phép thêm chi tiết
            btnUpdateDetail.Enabled = true;
            btnDeleteDetail.Enabled = true;

            // Vô hiệu hóa nút Xuất Phiếu khi reset form
            //btnExport.Enabled = false;

            // Reset cboFind về mục gợi ý
            cboFind.SelectedIndex = 0;
        }

        // --- Xóa nội dung các trường nhập liệu chi tiết phiếu ---
        private void ClearDetailFields()
        {
            cboProductId.SelectedIndex = -1;
            nudQuantity.Value = 1; // Mặc định số lượng là 1
            txtUnitPrice.Text = "0";
            txtLineAmount.Text = "0";
        }

        // --- Sự kiện chọn dòng trên dgvSaleInvoices ---
        private void dgvSaleInvoices_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvSaleInvoices.Rows[e.RowIndex];
                string invoiceId = row.Cells["InvoiceIdCol"].Value.ToString();

                // Lấy thông tin phiếu đầy đủ từ BLL
                SaleInvoiceDTO selectedInvoice = _saleInvoiceBLL.GetSaleInvoiceById(invoiceId);

                if (selectedInvoice != null)
                {
                    txtInvoiceId.Text = selectedInvoice.InvoiceId;
                    cboCardId.SelectedValue = selectedInvoice.CardId ?? string.Empty;
                    cboCardId.SelectedValue = selectedInvoice.CardId ?? string.Empty;
                    cboCardId.SelectedValue = selectedInvoice.CardId ?? string.Empty;
                    cboEmployeeId.SelectedValue = selectedInvoice.EmployeeId;
                    dtpCreatedDate.Value = selectedInvoice.CreatedDate;
                    if (selectedInvoice.Status)
                    {
                        rdoPaid.Checked = true;
                    }
                    else
                    {
                        rdoPending.Checked = true;
                    }
                    //txtTotalQuantity.Text = selectedInvoice.TotalQuantity.ToString();
                    txtTotalAmount.Text = selectedInvoice.TotalAmount.ToString("N0");

                    txtInvoiceId.Enabled = false; // Không cho phép sửa Mã phiếu khi đang cập nhật

                    // --- THÊM LOGIC KIỂM TRA TRẠNG THÁI VÀ ĐIỀU CHỈNH NÚT ---
                    bool isPaid = selectedInvoice.Status; // True nếu đã thanh toán, False nếu chờ xác nhận

                    // Nút Cập nhật phiếu chính
                    btnUpdateInvoice.Enabled = !isPaid; // Chỉ cho phép cập nhật nếu KHÔNG phải đã thanh toán (Trạng thái = 0)
                                                        // Nút Xóa phiếu chính
                    btnDeleteInvoice.Enabled = !isPaid; // Chỉ cho phép xóa nếu KHÔNG phải đã thanh toán (Trạng thái = 0)

                    // Các nút thao tác với chi tiết cũng có thể bị vô hiệu hóa
                    btnAddDetail.Enabled = !isPaid;
                    btnUpdateDetail.Enabled = !isPaid;
                    btnDeleteDetail.Enabled = !isPaid;

                    // Kích hoạt/vô hiệu hóa nút Xuất Phiếu
                    rdoPaid.Enabled = true; // Chỉ kích hoạt nếu đã thanh toán (Trạng thái = 1)

                    // Load chi tiết của phiếu này vào dgvSaleInvoiceDetails
                    LoadSaleInvoiceDetailsToDataGridView(invoiceId);
                }
            }
        }

        // --- Sự kiện chọn dòng trên dgvSaleInvoiceDetails ---
        private void dgvSaleInvoiceDetails_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvSaleInvoiceDetails.Rows[e.RowIndex];
                string productId = row.Cells["ProductIdCol"].Value.ToString();
                int quantity = Convert.ToInt32(row.Cells["QuantityCol"].Value);
                decimal unitPrice = Convert.ToDecimal(row.Cells["UnitPriceCol"].Value);
                decimal lineAmount = Convert.ToDecimal(row.Cells["LineAmountCol"].Value);

                // Điền thông tin vào các control của chi tiết
                cboProductId.SelectedValue = productId; // Chọn đúng sản phẩm trong combobox
                nudQuantity.Value = quantity;
                txtUnitPrice.Text = unitPrice.ToString();
                txtLineAmount.Text = lineAmount.ToString();
            }
        }

        // --- Sự kiện chọn sản phẩm trong ComboBox Sản Phẩm (cboProductId) ---
        private void cboProductId_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboProductId.SelectedValue != null && cboProductId.SelectedValue is string selectedProductId)
            {
                // Lấy đơn giá từ BLL
                decimal unitPrice = _saleInvoiceBLL.GetProductUnitPrice(selectedProductId);
                txtUnitPrice.Text = unitPrice.ToString("N0"); // Hiển thị đơn giá

                // Tính lại thành tiền dòng
                CalculateLineAmount();
            }
            else
            {
                txtUnitPrice.Text = "0";
                txtLineAmount.Text = "0";
            }
        }

        // --- Sự kiện thay đổi số lượng trên NumericUpDown (nudQuantity) ---
        private void nudQuantity_ValueChanged(object sender, EventArgs e)
        {
            CalculateLineAmount();
        }

        // --- Hàm tính toán Thành tiền của dòng chi tiết ---
        private void CalculateLineAmount()
        {
            if (decimal.TryParse(txtUnitPrice.Text, out decimal unitPrice) && nudQuantity.Value > 0)
            {
                decimal lineAmount = unitPrice * nudQuantity.Value;
                txtLineAmount.Text = lineAmount.ToString("N0");
            }
            else
            {
                txtLineAmount.Text = "0";
            }
        }


        // --- Các sự kiện nút cho Phiếu Bán Hàng chính ---

        private void btnAddInvoice_Click(object sender, EventArgs e)
        {
            try
            {
                // Lấy dữ liệu từ Form để tạo SaleInvoiceDTO
                SaleInvoiceDTO newInvoice = new SaleInvoiceDTO
                {
                    InvoiceId = txtInvoiceId.Text.Trim(),
                    CardId = cboCardId.SelectedValue?.ToString(), // Có thể null
                    EmployeeId = cboEmployeeId.SelectedValue?.ToString(), // Có thể null
                    CreatedDate = dtpCreatedDate.Value,
                    Status = rdoPaid.Checked // True nếu Đã thanh toán, False nếu Chờ xác nhận
                                             // InvoiceDetails sẽ được thêm sau hoặc trong quá trình cập nhật
                };

                if (string.IsNullOrEmpty(txtInvoiceId.Text))
                {
                    MessageBox.Show("Vui lòng nhập Mã phiếu.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtInvoiceId.Focus();
                    return;
                }

                if (string.IsNullOrEmpty(cboCardId.Text))
                {
                    MessageBox.Show("Vui lòng chọn Mã thẻ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cboCardId.Focus();
                    return;
                }

                if (string.IsNullOrEmpty(cboEmployeeId.Text))
                {
                    MessageBox.Show("Vui lòng chọn Mã nhân viên.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cboEmployeeId.Focus();
                    return;
                }

                // Nếu đây là thêm phiếu mới, thì các chi tiết sẽ là rỗng ban đầu hoặc từ dgvDetail nếu đã nhập
                // Giả định khi thêm phiếu mới, ChiTietPhieu sẽ được thêm sau
                // Hoặc nếu bạn muốn thêm chi tiết ngay lúc này, bạn cần lấy từ dgvSaleInvoiceDetails
                // Cách đơn giản nhất là thêm phiếu trước, sau đó người dùng thêm chi tiết.
                // Nếu bạn muốn lưu chi tiết cùng lúc, bạn cần lấy dữ liệu từ dgvSaleInvoiceDetails:
                foreach (DataGridViewRow row in dgvSaleInvoiceDetails.Rows)
                {
                    if (row.IsNewRow) continue; // Bỏ qua dòng mới nếu có
                    SaleInvoiceDetailDTO detail = new SaleInvoiceDetailDTO
                    {
                        ProductId = row.Cells["ProductIdCol"].Value.ToString(),
                        Quantity = Convert.ToInt32(row.Cells["QuantityCol"].Value),
                        UnitPrice = Convert.ToDecimal(row.Cells["UnitPriceCol"].Value),
                        LineAmount = Convert.ToDecimal(row.Cells["LineAmountCol"].Value)
                        // Id không cần vì là tự sinh
                    };
                    newInvoice.InvoiceDetails.Add(detail);
                }


                if (_saleInvoiceBLL.AddSaleInvoice(newInvoice))
                {
                    MessageBox.Show("Thêm phiếu bán hàng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadSaleInvoicesToDataGridView(); // Tải lại danh sách phiếu
                    ClearInvoiceFields(); // Xóa các trường nhập liệu
                }
                else
                {
                    MessageBox.Show("Thêm phiếu bán hàng thất bại. Vui lòng kiểm tra lại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnUpdateInvoice_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvSaleInvoices.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Vui lòng chọn một phiếu để cập nhật.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Lấy MaPhieu của phiếu đang chọn
                string selectedInvoiceId = txtInvoiceId.Text.Trim(); // Hoặc từ dgvSaleInvoices.SelectedRows[0].Cells["InvoiceIdCol"].Value.ToString();

                SaleInvoiceDTO updatedInvoice = new SaleInvoiceDTO
                {
                    InvoiceId = selectedInvoiceId,
                    CardId = cboCardId.SelectedValue?.ToString(),
                    EmployeeId = cboEmployeeId.SelectedValue?.ToString(),
                    CreatedDate = dtpCreatedDate.Value,
                    Status = rdoPaid.Checked
                };

                // Lấy chi tiết từ DataGridView Chi tiết để cập nhật cùng
                foreach (DataGridViewRow row in dgvSaleInvoiceDetails.Rows)
                {
                    if (row.IsNewRow) continue;
                    SaleInvoiceDetailDTO detail = new SaleInvoiceDetailDTO
                    {
                        Id = Convert.ToInt32(row.Cells["DetailIdHiddenCol"].Value), // Phải lấy Id của chi tiết cũ
                        InvoiceId = selectedInvoiceId,
                        ProductId = row.Cells["ProductIdCol"].Value.ToString(),
                        Quantity = Convert.ToInt32(row.Cells["QuantityCol"].Value),
                        UnitPrice = Convert.ToDecimal(row.Cells["UnitPriceCol"].Value),
                        LineAmount = Convert.ToDecimal(row.Cells["LineAmountCol"].Value)
                    };
                    updatedInvoice.InvoiceDetails.Add(detail);
                }

                if (_saleInvoiceBLL.UpdateSaleInvoice(updatedInvoice))
                {
                    MessageBox.Show("Cập nhật phiếu bán hàng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadSaleInvoicesToDataGridView(); // Tải lại danh sách phiếu
                    ClearInvoiceFields();
                }
                else
                {
                    MessageBox.Show("Cập nhật phiếu bán hàng thất bại. Vui lòng kiểm tra lại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDeleteInvoice_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvSaleInvoices.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Vui lòng chọn một phiếu để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Lấy MaPhieu từ dòng được chọn
                string invoiceIdToDelete = dgvSaleInvoices.SelectedRows[0].Cells["InvoiceIdCol"].Value.ToString();

                if (MessageBox.Show($"Bạn có chắc chắn muốn xóa phiếu '{invoiceIdToDelete}' và tất cả chi tiết liên quan không?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    if (_saleInvoiceBLL.DeleteSaleInvoice(invoiceIdToDelete))
                    {
                        MessageBox.Show("Xóa phiếu bán hàng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadSaleInvoicesToDataGridView(); // Tải lại danh sách phiếu
                        ClearInvoiceFields();
                    }
                    else
                    {
                        MessageBox.Show("Xóa phiếu bán hàng thất bại. Vui lòng kiểm tra lại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnResetInvoice_Click(object sender, EventArgs e)
        {
            ClearInvoiceFields(); // Xóa các trường nhập liệu phiếu chính và chi tiết
            LoadSaleInvoicesToDataGridView(); // Load lại toàn bộ danh sách phiếu
        }


        // --- Các sự kiện nút cho Chi Tiết Phiếu ---

        private void btnAddDetail_Click(object sender, EventArgs e)
        {
            try
            {
                // Kiểm tra xem phiếu chính đã được chọn/nhập chưa
                if (string.IsNullOrEmpty(txtInvoiceId.Text))
                {
                    MessageBox.Show("Vui lòng chọn hoặc nhập Mã Phiếu trước khi thêm chi tiết.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (cboProductId.SelectedValue == null)
                {
                    MessageBox.Show("Vui lòng chọn Sản phẩm.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (nudQuantity.Value <= 0)
                {
                    MessageBox.Show("Số lượng phải lớn hơn 0.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                SaleInvoiceDetailDTO newDetail = new SaleInvoiceDetailDTO
                {
                    InvoiceId = txtInvoiceId.Text.Trim(),
                    ProductId = cboProductId.SelectedValue.ToString(),
                    ProductName = cboProductId.Text, // Lấy tên sản phẩm từ Text của ComboBox
                    Quantity = (int)nudQuantity.Value,
                    UnitPrice = decimal.Parse(txtUnitPrice.Text),
                    LineAmount = decimal.Parse(txtLineAmount.Text)
                };

                // Thêm chi tiết vào danh sách hiện tại của DGV và cập nhật DGV
                _currentDetailList.Add(newDetail);
                dgvSaleInvoiceDetails.DataSource = null; // Xóa DataSource tạm thời
                dgvSaleInvoiceDetails.DataSource = _currentDetailList; // Gán lại để refresh
                dgvSaleInvoiceDetails.Refresh();

                // Tính toán lại tổng tiền và tổng số lượng của phiếu chính trên UI
                nudQuantity.Text = _currentDetailList.Sum(d => d.Quantity).ToString();
                txtTotalAmount.Text = _currentDetailList.Sum(d => d.LineAmount).ToString("N0");

                ClearDetailFields(); // Xóa các trường nhập liệu chi tiết
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm chi tiết phiếu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnUpdateDetail_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvSaleInvoiceDetails.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Vui lòng chọn một chi tiết để cập nhật.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (cboProductId.SelectedValue == null)
                {
                    MessageBox.Show("Vui lòng chọn Sản phẩm.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (nudQuantity.Value <= 0)
                {
                    MessageBox.Show("Số lượng phải lớn hơn 0.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Lấy ID chi tiết từ cột ẩn
                int detailIdToUpdate = Convert.ToInt32(dgvSaleInvoiceDetails.SelectedRows[0].Cells["DetailIdHiddenCol"].Value);

                // Tìm chi tiết trong danh sách hiện tại và cập nhật
                SaleInvoiceDetailDTO existingDetail = _currentDetailList.FirstOrDefault(d => d.Id == detailIdToUpdate);
                if (existingDetail != null)
                {
                    existingDetail.ProductId = cboProductId.SelectedValue.ToString();
                    existingDetail.ProductName = cboProductId.Text;
                    existingDetail.Quantity = (int)nudQuantity.Value;
                    existingDetail.UnitPrice = decimal.Parse(txtUnitPrice.Text);
                    existingDetail.LineAmount = decimal.Parse(txtLineAmount.Text);

                    dgvSaleInvoiceDetails.DataSource = null; // Xóa DataSource tạm thời
                    dgvSaleInvoiceDetails.DataSource = _currentDetailList; // Gán lại để refresh
                    dgvSaleInvoiceDetails.Refresh();

                    // Tính toán lại tổng tiền và tổng số lượng của phiếu chính trên UI
                    nudQuantity.Text = _currentDetailList.Sum(d => d.Quantity).ToString();
                    txtTotalAmount.Text = _currentDetailList.Sum(d => d.LineAmount).ToString("N0");

                    ClearDetailFields();
                }
                else
                {
                    MessageBox.Show("Không tìm thấy chi tiết cần cập nhật trong danh sách tạm thời.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật chi tiết phiếu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDeleteDetail_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvSaleInvoiceDetails.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Vui lòng chọn một chi tiết để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int detailIdToDelete = Convert.ToInt32(dgvSaleInvoiceDetails.SelectedRows[0].Cells["DetailIdHiddenCol"].Value);

                if (MessageBox.Show("Bạn có chắc chắn muốn xóa chi tiết này không?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    // Xóa chi tiết khỏi danh sách hiện tại của DGV
                    SaleInvoiceDetailDTO detailToRemove = _currentDetailList.FirstOrDefault(d => d.Id == detailIdToDelete);
                    if (detailToRemove != null)
                    {
                        _currentDetailList.Remove(detailToRemove);
                        dgvSaleInvoiceDetails.DataSource = null; // Xóa DataSource tạm thời
                        dgvSaleInvoiceDetails.DataSource = _currentDetailList; // Gán lại để refresh
                        dgvSaleInvoiceDetails.Refresh();

                        // Tính toán lại tổng tiền và tổng số lượng của phiếu chính trên UI
                        nudQuantity.Text = _currentDetailList.Sum(d => d.Quantity).ToString();
                        txtTotalAmount.Text = _currentDetailList.Sum(d => d.LineAmount).ToString("N0");

                        ClearDetailFields();
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy chi tiết cần xóa trong danh sách tạm thời.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa chi tiết phiếu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnResetDetail_Click(object sender, EventArgs e)
        {
            ClearDetailFields();
            // Không tải lại toàn bộ chi tiết, chỉ reset các trường nhập liệu
            // dgvSaleInvoiceDetails vẫn giữ nguyên các chi tiết của phiếu đang chọn
        }

        private void cboFind_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboFind.SelectedItem != null && cboFind.SelectedItem is string selectedInvoiceId)
            {
                if (selectedInvoiceId == "-- Chọn Mã phiếu --")
                {
                    ClearInvoiceFields();
                    dgvSaleInvoices.DataSource = null;
                    dgvSaleInvoiceDetails.DataSource = null;
                    LoadSaleInvoicesToDataGridView();
                    // KHI RESET HOẶC CHỌN MỤC GỢI Ý, CÁC NÚT NÊN ĐƯỢC KÍCH HOẠT LẠI ĐỂ CHO PHÉP THÊM MỚI
                    btnUpdateInvoice.Enabled = true; // Cho phép cập nhật (nếu là phiếu mới, nút này sẽ chưa có tác dụng)
                    btnDeleteInvoice.Enabled = true;
                    btnAddDetail.Enabled = true; // Cho phép thêm chi tiết (khi thêm phiếu mới)
                    btnUpdateDetail.Enabled = true;
                    btnDeleteDetail.Enabled = true;
                    return;
                }

                SaleInvoiceDTO selectedInvoice = _saleInvoiceBLL.GetSaleInvoiceById(selectedInvoiceId);

                if (selectedInvoice != null)
                {
                    txtInvoiceId.Text = selectedInvoice.InvoiceId;
                    cboCardId.SelectedValue = selectedInvoice.CardId ?? string.Empty;
                    cboEmployeeId.SelectedValue = selectedInvoice.EmployeeId;
                    dtpCreatedDate.Value = selectedInvoice.CreatedDate;
                    if (selectedInvoice.Status)
                    {
                        rdoPaid.Checked = true;
                    }
                    else
                    {
                        rdoPending.Checked = true;
                    }
                    // txtTotalQuantity.Text = selectedInvoice.TotalQuantity.ToString();
                    txtTotalAmount.Text = selectedInvoice.TotalAmount.ToString("N0");

                    txtInvoiceId.Enabled = false;

                    // --- THÊM LOGIC KIỂM TRA TRẠNG THÁI VÀ ĐIỀU CHỈNH NÚT (TƯƠNG TỰ CellClick) ---
                    bool isPaid = selectedInvoice.Status;

                    btnUpdateInvoice.Enabled = !isPaid;
                    btnDeleteInvoice.Enabled = !isPaid;
                    btnAddDetail.Enabled = !isPaid;
                    btnUpdateDetail.Enabled = !isPaid;
                    btnDeleteDetail.Enabled = !isPaid;

                    // Kích hoạt/vô hiệu hóa nút Xuất Phiếu
                    //btnExport.Enabled = isPaid;

                    LoadSaleInvoiceDetailsToDataGridView(selectedInvoice.InvoiceId);
                }
                else
                {
                    MessageBox.Show("Không tìm thấy phiếu có mã này.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ClearInvoiceFields();
                    // Đảm bảo các nút được kích hoạt lại khi không tìm thấy phiếu hoặc reset
                    btnUpdateInvoice.Enabled = true;
                    btnDeleteInvoice.Enabled = true;
                    btnAddDetail.Enabled = true;
                    btnUpdateDetail.Enabled = true;
                    btnDeleteDetail.Enabled = true;
                }
            }

        }

        //private void btnExport_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        if (dgvSaleInvoices.SelectedRows.Count == 0)
        //        {
        //            MessageBox.Show("Vui lòng chọn một phiếu bán hàng để xuất hóa đơn.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //            return;
        //        }

        //        // Lấy MaPhieu từ dòng được chọn
        //        string invoiceId = dgvSaleInvoices.SelectedRows[0].Cells["InvoiceIdCol"].Value.ToString();

        //        // Lấy toàn bộ thông tin phiếu và chi tiết từ BLL
        //        SaleInvoiceDTO invoiceToExport = _saleInvoiceBLL.GetSaleInvoiceById(invoiceId);

        //        if (invoiceToExport == null)
        //        {
        //            MessageBox.Show("Không tìm thấy thông tin phiếu để xuất hóa đơn.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //            return;
        //        }

        //        // Kiểm tra trạng thái phiếu (đảm bảo đã thanh toán)
        //        if (!invoiceToExport.Status)
        //        {
        //            MessageBox.Show("Chỉ có thể xuất hóa đơn cho các phiếu đã thanh toán.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //            return;
        //        }

        //        // Mở hộp thoại SaveFileDialog để người dùng chọn nơi lưu file
        //        SaveFileDialog saveFileDialog = new SaveFileDialog();
        //        saveFileDialog.Filter = "Text file (*.txt)|*.txt";
        //        saveFileDialog.FileName = $"HoaDon_{invoiceToExport.InvoiceId}.txt"; // Tên file mặc định
        //        saveFileDialog.Title = "Lưu hóa đơn";

        //        if (saveFileDialog.ShowDialog() == DialogResult.OK)
        //        {
        //            string filePath = saveFileDialog.FileName;

        //            // Bắt đầu tạo nội dung hóa đơn
        //            StringBuilder sb = new StringBuilder();
        //            sb.AppendLine("******************************************************************");
        //            sb.AppendLine("                         HÓA ĐƠN BÁN HÀNG                         ");
        //            sb.AppendLine("******************************************************************");
        //            sb.AppendLine($"Mã Phiếu:       {invoiceToExport.InvoiceId}");
        //            sb.AppendLine($"Mã Nhân Viên:   {invoiceToExport.EmployeeId} ({invoiceToExport.EmployeeName})");
        //            // Thẻ có thể null, cần kiểm tra để tránh lỗi
        //            sb.AppendLine($"Mã Thẻ:         {(string.IsNullOrEmpty(invoiceToExport.CardId) ? "N/A" : invoiceToExport.CardId + " (" + invoiceToExport.CardOwnerName + ")")}");
        //            sb.AppendLine($"Ngày Tạo:       {invoiceToExport.CreatedDate:dd/MM/yyyy HH:mm}");
        //            sb.AppendLine($"Trạng Thái:     {(invoiceToExport.Status ? "Đã thanh toán" : "Chờ xác nhận")}");
        //            sb.AppendLine("------------------------------------------------------------------");
        //            sb.AppendLine("Chi tiết sản phẩm:");
        //            sb.AppendLine("------------------------------------------------------------------");
        //            sb.AppendLine(string.Format("{0,-15} {1,-15} {2,10} {3,10} {4,12}", "Mã Sản phẩm", "Tên Sản phẩm", "Đơn Giá", "Số Lượng", "Thành Tiền"));
        //            sb.AppendLine("------------------------------------------------------------------");

        //            foreach (var detail in invoiceToExport.InvoiceDetails)
        //            {
        //                sb.AppendLine(string.Format("{0,-15} {1,-15} {2,9} {3,8:N0} {4,13:N0}",
        //                    detail.ProductId,
        //                    detail.ProductName,
        //                    detail.UnitPrice,
        //                    detail.Quantity,
        //                    detail.LineAmount));
        //            }
        //            sb.AppendLine("------------------------------------------------------------------");
        //            sb.AppendLine($"Tổng Số Lượng:  {invoiceToExport.TotalQuantity}");
        //            sb.AppendLine($"Tổng Tiền:      {invoiceToExport.TotalAmount:N0} VNĐ");
        //            sb.AppendLine("******************************************************************");
        //            sb.AppendLine("Cảm ơn quý khách đã mua hàng!");
        //            sb.AppendLine("******************************************************************");

        //            // Ghi nội dung vào file
        //            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8); // Sử dụng UTF8 để tránh lỗi font tiếng Việt

        //            MessageBox.Show($"Hóa đơn đã được xuất thành công tới:\n{filePath}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Lỗi khi xuất hóa đơn: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //}

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void dgvSaleInvoiceDetails_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void cboFind_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            if (cboFind.SelectedItem != null && cboFind.SelectedItem is string selectedInvoiceId)
            {
                if (selectedInvoiceId == "-- Chọn Mã phiếu --")
                {
                    ClearInvoiceFields();
                    dgvSaleInvoices.DataSource = null;
                    dgvSaleInvoiceDetails.DataSource = null;
                    LoadSaleInvoicesToDataGridView();
                    // KHI RESET HOẶC CHỌN MỤC GỢI Ý, CÁC NÚT NÊN ĐƯỢC KÍCH HOẠT LẠI ĐỂ CHO PHÉP THÊM MỚI
                    btnUpdateInvoice.Enabled = true; // Cho phép cập nhật (nếu là phiếu mới, nút này sẽ chưa có tác dụng)
                    btnDeleteInvoice.Enabled = true;
                    btnAddDetail.Enabled = true; // Cho phép thêm chi tiết (khi thêm phiếu mới)
                    btnUpdateDetail.Enabled = true;
                    btnDeleteDetail.Enabled = true;
                    return;
                }

                SaleInvoiceDTO selectedInvoice = _saleInvoiceBLL.GetSaleInvoiceById(selectedInvoiceId);

                if (selectedInvoice != null)
                {
                    txtInvoiceId.Text = selectedInvoice.InvoiceId;
                    cboCardId.SelectedValue = selectedInvoice.CardId ?? string.Empty;
                    cboEmployeeId.SelectedValue = selectedInvoice.EmployeeId;
                    dtpCreatedDate.Value = selectedInvoice.CreatedDate;
                    if (selectedInvoice.Status)
                    {
                        rdoPaid.Checked = true;
                    }
                    else
                    {
                        rdoPending.Checked = true;
                    }
                    // txtTotalQuantity.Text = selectedInvoice.TotalQuantity.ToString();
                    txtTotalAmount.Text = selectedInvoice.TotalAmount.ToString("N0");

                    txtInvoiceId.Enabled = false;

                    // --- THÊM LOGIC KIỂM TRA TRẠNG THÁI VÀ ĐIỀU CHỈNH NÚT (TƯƠNG TỰ CellClick) ---
                    bool isPaid = selectedInvoice.Status;

                    btnUpdateInvoice.Enabled = !isPaid;
                    btnDeleteInvoice.Enabled = !isPaid;
                    btnAddDetail.Enabled = !isPaid;
                    btnUpdateDetail.Enabled = !isPaid;
                    btnDeleteDetail.Enabled = !isPaid;

                    // Kích hoạt/vô hiệu hóa nút Xuất Phiếu
                    //btnExport.Enabled = isPaid;
                    foreach (DataGridViewRow row in dgvSaleInvoices.Rows)
                    {
                        if (row.Cells[0].Value.ToString() == selectedInvoice.InvoiceId)
                        {
                            dgvSaleInvoices.ClearSelection();
                            row.Selected = true;
                            dgvSaleInvoices.FirstDisplayedScrollingRowIndex = row.Index;
                            break;
                        }
                    }
                    LoadSaleInvoiceDetailsToDataGridView(selectedInvoice.InvoiceId);
                }
                else
                {
                    MessageBox.Show("Không tìm thấy phiếu có mã này.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ClearInvoiceFields();
                    // Đảm bảo các nút được kích hoạt lại khi không tìm thấy phiếu hoặc reset
                    btnUpdateInvoice.Enabled = true;
                    btnDeleteInvoice.Enabled = true;
                    btnAddDetail.Enabled = true;
                    btnUpdateDetail.Enabled = true;
                    btnDeleteDetail.Enabled = true;
                }
            }

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void rdoPaid_Click(object sender, EventArgs e)
        {
            try
            {
                // 1. Initial selection check
                if (dgvSaleInvoices.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Vui lòng chọn một phiếu bán hàng để xử lý.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Get MaPhieu from the selected row (more reliable than txtInvoiceId for current action)
                string selectedInvoiceId = dgvSaleInvoices.SelectedRows[0].Cells["InvoiceIdCol"].Value?.ToString();

                // --- Retrieve the full invoice details for validation ---
                // It's crucial to get the up-to-date details from the BLL, not just what's in the textboxes,
                // especially for validation purposes like checking if it has items.
                SaleInvoiceDTO invoiceToProcess = _saleInvoiceBLL.GetSaleInvoiceById(selectedInvoiceId);

                if (invoiceToProcess == null)
                {
                    MessageBox.Show("Không tìm thấy thông tin phiếu bán hàng được chọn.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 2. Validate for Empty Invoice Details before proceeding with payment or export
                // Check if there are any details associated with this invoice
                // (Assuming InvoiceDetails list is populated by GetSaleInvoiceById)
                if (invoiceToProcess.InvoiceDetails == null || !invoiceToProcess.InvoiceDetails.Any())
                {
                    MessageBox.Show("Phiếu bán hàng này không có chi tiết sản phẩm. Không thể thanh toán hoặc xuất hóa đơn.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // --- Prepare the updated invoice DTO based on current form values ---
                SaleInvoiceDTO updatedInvoice = new SaleInvoiceDTO
                {
                    InvoiceId = selectedInvoiceId, // Use the ID from the selected row
                    CardId = cboCardId.SelectedValue?.ToString(),
                    EmployeeId = cboEmployeeId.SelectedValue?.ToString(),
                    CreatedDate = dtpCreatedDate.Value,
                    Status = rdoPaid.Checked // This reflects the intended status change
                };

                // Populate details for 'updatedInvoice' from dgvSaleInvoiceDetails
                foreach (DataGridViewRow row in dgvSaleInvoiceDetails.Rows)
                {
                    if (row.IsNewRow) continue;

                    // Ensure all cells have values and are convertible
                    if (row.Cells["ProductIdCol"].Value == null ||
                        row.Cells["QuantityCol"].Value == null ||
                        row.Cells["UnitPriceCol"].Value == null ||
                        row.Cells["LineAmountCol"].Value == null ||
                        row.Cells["DetailIdHiddenCol"].Value == null) // Make sure DetailIdHiddenCol exists and has value
                    {
                        // Log or handle incomplete rows if necessary. For now, skip if incomplete.
                        continue;
                    }

                    SaleInvoiceDetailDTO detail = new SaleInvoiceDetailDTO
                    {
                        Id = Convert.ToInt32(row.Cells["DetailIdHiddenCol"].Value), // Must get old detail Id
                        InvoiceId = selectedInvoiceId,
                        ProductId = row.Cells["ProductIdCol"].Value.ToString(),
                        Quantity = Convert.ToInt32(row.Cells["QuantityCol"].Value),
                        UnitPrice = Convert.ToDecimal(row.Cells["UnitPriceCol"].Value),
                        LineAmount = Convert.ToDecimal(row.Cells["LineAmountCol"].Value)
                    };
                    // Initialize the list if it's null (good practice for DTOs)
                    if (updatedInvoice.InvoiceDetails == null)
                    {
                        updatedInvoice.InvoiceDetails = new List<SaleInvoiceDetailDTO>();
                    }
                    updatedInvoice.InvoiceDetails.Add(detail);
                }

                // --- Confirmation for Payment (if status is being set to Paid) ---
                // Only ask for confirmation if the current status is UNPAID (or different)
                // and the user is trying to set it to PAID via rdoPaid.Checked.
                if (!invoiceToProcess.Status && rdoPaid.Checked)
                {
                    DialogResult confirmPay = MessageBox.Show(
                        $"Bạn có chắc chắn muốn thanh toán phiếu bán hàng '{selectedInvoiceId}' không?",
                        "Xác nhận thanh toán",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (confirmPay == DialogResult.No)
                    {
                        // If user cancels, revert the radio button (optional, but good UX)
                        rdoPending.Checked = true; // Assuming you have an rdoUnpaid
                        rdoPaid.Checked = false;
                        return; // Stop the process
                    }
                }

                // --- Exporting the Invoice (Remains mostly the same) ---
                // Use 'invoiceToProcess' for export as it's the full, verified DTO from BLL.
                // If you plan to update the DTO before export (e.g., if TotalAmount is calculated here),
                // then merge 'updatedInvoice' data into 'invoiceToProcess' or use 'updatedInvoice' directly.
                // Given the previous code, invoiceToProcess is the one with EmployeeName, CardOwnerName, etc.
                // So, it's generally better to update invoiceToProcess with current form data IF you also want
                // to export changes not yet saved to DB, otherwise, export the one from DB.
                // For simplicity, let's assume 'updatedInvoice' contains all necessary info for export.

                // If the intent is to export the *currently displayed/modified* invoice,
                // ensure 'updatedInvoice' has all the auxiliary display properties (EmployeeName, CardOwnerName, etc.)
                // For now, let's stick to 'invoiceToProcess' for export as it contains auxiliary data.
                // If 'updatedInvoice' truly reflects the final state to be saved and exported,
                // then you'd need to populate the names from somewhere or retrieve them after update.

                // Let's assume you want to export the data that is *about to be saved* (from updatedInvoice)
                // but you need the names which are typically populated by GetSaleInvoiceById.
                // A better approach is to perform the UPDATE first, then GET the updated invoice for export.
                // Or, ensure your 'updatedInvoice' DTO has all required fields including names.

                // OPTION 1: Update first, then get the fresh DTO for export (more robust for export data)
                bool updateSuccess = _saleInvoiceBLL.UpdateSaleInvoice(updatedInvoice);
                if (!updateSuccess)
                {
                    MessageBox.Show("Cập nhật phiếu bán hàng thất bại. Vui lòng thử lại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Now get the fully updated invoice with all details and auxiliary names for export
                SaleInvoiceDTO finalInvoiceToExport = _saleInvoiceBLL.GetSaleInvoiceById(selectedInvoiceId);
                if (finalInvoiceToExport == null)
                {
                    MessageBox.Show("Lỗi: Không thể lấy thông tin phiếu đã cập nhật để xuất hóa đơn.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Mở hộp thoại SaveFileDialog để người dùng chọn nơi lưu file
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Text file (*.txt)|*.txt";
                saveFileDialog.FileName = $"HoaDon_{finalInvoiceToExport.InvoiceId}.txt"; // Tên file mặc định
                saveFileDialog.Title = "Lưu hóa đơn";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = saveFileDialog.FileName;

                    // Bắt đầu tạo nội dung hóa đơn
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("******************************************************************");
                    sb.AppendLine("                 HÓA ĐƠN BÁN HÀNG                                 ");
                    sb.AppendLine("******************************************************************");
                    sb.AppendLine($"Mã Phiếu:        {finalInvoiceToExport.InvoiceId}");
                    sb.AppendLine($"Mã Nhân Viên:    {finalInvoiceToExport.EmployeeId} ({finalInvoiceToExport.EmployeeName})");
                    // Thẻ có thể null, cần kiểm tra để tránh lỗi
                    sb.AppendLine($"Mã Thẻ:          {(string.IsNullOrEmpty(finalInvoiceToExport.CardId) ? "N/A" : finalInvoiceToExport.CardId + " (" + finalInvoiceToExport.CardOwnerName + ")")}");
                    sb.AppendLine($"Ngày Tạo:        {finalInvoiceToExport.CreatedDate:dd/MM/yyyy HH:mm}");
                    sb.AppendLine($"Trạng Thái:      {(finalInvoiceToExport.Status ? "Đã thanh toán" : "Chờ xác nhận")}");
                    sb.AppendLine("------------------------------------------------------------------");
                    sb.AppendLine("Chi tiết sản phẩm:");
                    sb.AppendLine("------------------------------------------------------------------");
                    sb.AppendLine(string.Format("{0,-15} {1,-15} {2,10} {3,10} {4,12}", "Mã Sản phẩm", "Tên Sản phẩm", "Đơn Giá", "Số Lượng", "Thành Tiền"));
                    sb.AppendLine("------------------------------------------------------------------");

                    foreach (var detail in finalInvoiceToExport.InvoiceDetails)
                    {
                        sb.AppendLine(string.Format("{0,-15} {1,-15} {2,9} {3,8:N0} {4,13:N0}",
                            detail.ProductId,
                            detail.ProductName, // Make sure ProductName is populated in your DTO/BLL
                            detail.UnitPrice,
                            detail.Quantity,
                            detail.LineAmount));
                    }
                    sb.AppendLine("------------------------------------------------------------------");
                    sb.AppendLine($"Tổng Số Lượng:   {finalInvoiceToExport.TotalQuantity}");
                    sb.AppendLine($"Tổng Tiền:       {finalInvoiceToExport.TotalAmount:N0} VNĐ");
                    sb.AppendLine("******************************************************************");
                    sb.AppendLine("Cảm ơn quý khách đã mua hàng!");
                    sb.AppendLine("******************************************************************");

                    // Ghi nội dung vào file
                    File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8); // Sử dụng UTF8 để tránh lỗi font tiếng Việt

                    MessageBox.Show($"Hóa đơn đã được xuất thành công tới:\n{filePath}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                // Refresh UI after successful update and export
                LoadSaleInvoicesToDataGridView(); // Tải lại danh sách phiếu
                ClearInvoiceFields();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xử lý phiếu bán hàng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void rdoPaid_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void dgvSaleInvoices_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}

