using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Polycafe_BUS;
using Polycafe_DTO;

namespace Polycafe_GUI
{
    public partial class ThongKe : UserControl
    {
        private string _vaiTro;
        private ThongkeNVBUS bus = new ThongkeNVBUS();

        private string connectionString = "Data Source=.;Initial Catalog=QLPolycafe;Integrated Security=True;";
        private nhanvienBLL nhanVienBLL;
        private ThongKeBLL thongKeBLL;

        public ThongKe(string vaiTro)
        {
            InitializeComponent();

            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            _vaiTro = vaiTro;
            ApDungPhanQuyen();

            // Ẩn tất cả các GroupBox chứa biểu đồ khi khởi tạo
            groupBox5.Visible = false; // Chứa chart1 (theo tháng)
            groupBox6.Visible = false; // Chứa chart3 (theo quý)
            groupBox7.Visible = false; // Chứa chart4 (theo tuần)
            groupBox8.Visible = false;
            groupBox9.Visible = false;

            nhanVienBLL = new nhanvienBLL(connectionString);
            thongKeBLL = new ThongKeBLL(connectionString);
        }

        private void ThongKe_Load(object sender, EventArgs e)
        {
            dataGridView1.DataSource = null;
            dataGridView1.Columns.Clear();
            dataGridView2.DataSource = null;
            dataGridView2.Columns.Clear();

            groupBox5.Visible = false;
            groupBox6.Visible = false;
            groupBox7.Visible = false;
            groupBox8.Visible = false;
            groupBox9.Visible = false;

            if (tabControl1.SelectedTab == tabPage1)
            {
                //LoadCombo();
                LoadNhanVienToComboBox();
                LoadData();
                LoadChartTheoThang();
                LoadChartTheoTuan();
                //LoadPieChartTheoQuy();
            }
            else if (tabControl1.SelectedTab == tabPage2)
            {
                LoadComboSP();
                LoadThongKeSP();
                var dtTop5 = bus.LayTop5SanPhamBanChay(dateTimePicker4.Value.Date, dateTimePicker3.Value.Date);
                VeBieuDoTop5(dtTop5);
                HienThiBieuDoPie();
            }
        }

        /////////////////////////////// Phân Quyền /////////////////////////////////

        private void ApDungPhanQuyen()
        {
            if (_vaiTro == "1")
            {
                button1.Enabled = true;
                button2.Enabled = true;
                button3.Enabled = true;
                button4.Enabled = true;
                groupBox1.Show();
                groupBox2.Show();
                groupBox3.Show();
                groupBox4.Show();
                groupBox5.Show();
                groupBox6.Show();
                groupBox7.Show();
                tabControl1.Show();
                tabPage2.Show();
            }
            else
            {
                button1.Enabled = false;
                button2.Enabled = false;
                button3.Enabled = false;
                button4.Enabled = false;
                groupBox1.Hide();
                groupBox2.Hide();
                groupBox3.Hide();
                groupBox4.Hide();
                groupBox5.Hide();
                groupBox6.Hide();
                groupBox7.Hide();
                tabControl1.Hide();
                tabPage2.Hide();
            }
        }

        ///////////////////// Thống kê nhân viên /////////////////////////////////

        private void LoadThongKe()
        {
            string maNV = comboBox1.Text.Trim();
            DateTime tuNgay = dateTimePicker1.Value;
            DateTime denNgay = dateTimePicker2.Value;

            DataTable dt = bus.GetThongKe(maNV, tuNgay, denNgay);
            dataGridView1.DataSource = dt;
        }

        private void LoadData()
        {
            string MaNV = comboBox1.SelectedValue?.ToString();
            DateTime startDate = dateTimePicker1.Value;
            DateTime endDate = dateTimePicker2.Value;

            dataGridView1.DataSource = null;
            dataGridView1.Columns.Clear();

            try
            {
                var dataSource = bus.GetThongKe(MaNV, startDate, endDate);
                if (dataSource != null && dataSource.Rows.Count > 0)
                {
                    dataGridView1.DataSource = dataSource;
                }
                else
                {
                    dataGridView1.DataSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Đã xảy ra lỗi khi tải dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadCombo()
        {
            try
            {
                var listmaNV = bus.LoadmaNV();
                if (listmaNV != null)
                {
                    comboBox1.DataSource = listmaNV;
                    comboBox1.SelectedIndex = -1;
                }
                else
                {
                    MessageBox.Show("Không thể tải dữ liệu Mã nhân viên cho ComboBox.", "Lỗi Tải ComboBox", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi Load dữ liệu ComboBox: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string maNV = comboBox1.SelectedValue?.ToString();
                DateTime tuNgay = dateTimePicker1.Value.Date;
                DateTime denNgay = dateTimePicker2.Value.Date;

                if (tuNgay > denNgay)
                {
                    MessageBox.Show("Ngày bắt đầu không được lớn hơn ngày kết thúc.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DataTable gridData = bus.GetThongKe(maNV, tuNgay, denNgay);

                if (gridData == null || gridData.Rows.Count == 0)
                {
                    MessageBox.Show("Không có dữ liệu thống kê phù hợp.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                dataGridView1.DataSource = gridData;
                groupBox5.Visible = true;
                groupBox6.Visible = true;
                groupBox7.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Đã xảy ra lỗi khi thống kê: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            string selectedMaNhanVien = comboBox1.SelectedValue?.ToString();
            DateTime fromDate = dateTimePicker1.Value.Date;
            DateTime toDate = dateTimePicker2.Value.Date.AddDays(1).AddMilliseconds(-1);

            if (fromDate > toDate)
            {
                MessageBox.Show("Ngày 'Từ ngày' không được lớn hơn ngày 'Đến ngày'.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                chart1.Series.Clear();
                chart1.Titles.Clear();
                return;
            }
            // Gọi BLL để lấy dữ liệu thống kê
            List<ThongKeNhanVienDTO> listThongKe = thongKeBLL.GetDoanhThuTheoThang(selectedMaNhanVien, fromDate, toDate);

            if (listThongKe == null || !listThongKe.Any())
            {
                MessageBox.Show("Không có dữ liệu doanh thu cho khoảng thời gian và nhân viên đã chọn.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                chart1.Series.Clear();
                chart1.Titles.Clear();
                return;
            }

            LoadChartTheoThang(listThongKe); // Truyền List<ThongKeDoanhThuDTO> thay vì DataTable
                        
            LoadChartTheoTuan();
            LoadPieChartTheoQuy();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            LoadData();
            ClearFields();

            comboBox1.SelectedIndex = -1;
            groupBox5.Visible = false;
            groupBox6.Visible = false;
            groupBox7.Visible = false;
        }

        private void ClearFields()
        {
            dateTimePicker1.Value = DateTime.Now;
            dateTimePicker2.Value = DateTime.Now;
            comboBox1.SelectedIndex = -1;
            dateTimePicker3.Value = DateTime.Now;
            dateTimePicker4.Value = DateTime.Now;
            comboBox2.SelectedIndex = -1;
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            dataGridView1.DataSource = null;
            dataGridView1.Columns.Clear();
            dataGridView2.DataSource = null;
            dataGridView2.Columns.Clear();

            groupBox5.Visible = false;
            groupBox6.Visible = false;
            groupBox7.Visible = false;
            groupBox8.Visible = false;
            groupBox9.Visible = false;

            if (tabControl1.SelectedTab == tabPage1)
            {
                LoadNhanVienToComboBox();
                LoadData();
                LoadChartTheoThang();
                LoadChartTheoTuan();
                //LoadPieChartTheoQuy();
            }
            else if (tabControl1.SelectedTab == tabPage2)
            {
                LoadComboSP();
                LoadThongKeSP();
                VeBieuDoTop5(bus.LayTop5SanPhamBanChay(dateTimePicker4.Value.Date, dateTimePicker3.Value.Date));
                HienThiBieuDoPie();
            }
        }

        ///////////////////// Biểu đồ thống kê theo tuần /////////////////////////////

        private void LoadChartTheoTuan()
        {
            DateTime tuNgay = dateTimePicker1.Value.Date;
            DateTime denNgay = dateTimePicker2.Value.Date;

            if (tuNgay > denNgay)
            {
                MessageBox.Show("Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc.", "Lỗi chọn ngày", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataTable dt = bus.LayThongTinThongKe("Theo Tuần", tuNgay, denNgay);

            if (!dt.Columns.Contains("TongDoanhThu") || !dt.Columns.Contains("Ky") || !dt.Columns.Contains("Nam"))
            {
                MessageBox.Show("Dữ liệu không chứa đủ các cột cần thiết ('TongDoanhThu', 'Ky', 'Nam'). Vui lòng kiểm tra nguồn dữ liệu từ BUS.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            chart4.Series.Clear();
            chart4.Titles.Clear();
            if (chart4.ChartAreas.Count == 0)
            {
                chart4.ChartAreas.Add(new ChartArea("DefaultArea"));
            }
            else
            {
                chart4.ChartAreas[0].AxisY.CustomLabels.Clear();
            }

            Title weeksTitle = new Title
            {
                Text = "Biểu đồ thống kê tổng doanh thu của nhân viên theo tuần",
                Font = new Font("Arial", 14, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Alignment = ContentAlignment.TopCenter
            };
            chart4.Titles.Add(weeksTitle);

            ChartArea area = chart4.ChartAreas[0];
            area.BackColor = Color.White;
            area.BorderDashStyle = ChartDashStyle.Solid;
            area.BorderWidth = 1;
            area.BorderColor = Color.LightGray;

            area.AxisY.Title = "Tổng doanh thu (VNĐ)";
            area.AxisY.IsReversed = false;
            area.AxisY.Interval = 1;
            area.AxisY.MajorGrid.Enabled = false;
            area.AxisY.LabelStyle.Font = new Font("Arial", 9);
            area.AxisY.LabelStyle.IsStaggered = false;
            area.AxisY.MajorTickMark.Enabled = false;

            area.AxisX.Title = "Tuần";
            area.AxisX.Minimum = 0;
            area.AxisX.IsStartedFromZero = true;
            area.AxisX.MajorGrid.Enabled = true;
            area.AxisX.MajorGrid.LineColor = Color.LightGray;
            area.AxisX.LabelStyle.Format = "{0:N0}";
            area.AxisX.LabelStyle.Font = new Font("Arial", 9);

            var weeklyTotalRevenue = dt.AsEnumerable()
                .GroupBy(r => new { Ky = r.Field<int>("Ky"), Nam = r.Field<int>("Nam") })
                .Select(g =>
                {
                    DateTime firstDayOfYear = new DateTime(g.Key.Nam, 1, 1);
                    DateTime approxDate = firstDayOfYear.AddDays((g.Key.Ky - 1) * 7);
                    string thang = approxDate.ToString("MM");

                    return new
                    {
                        Ky = g.Key.Ky,
                        Nam = g.Key.Nam,
                        Thang = thang,
                        TuanLabel = $"Tuần {g.Key.Ky} (Tháng {thang})",
                        TotalRevenue = g.Sum(r => Convert.ToDouble(r.Field<object>("TongDoanhThu")))
                    };
                })
                .OrderBy(x => x.Nam)
                .ThenBy(x => x.Ky)
                .ToList();

            Series series = new Series("Tổng doanh thu theo tuần")
            {
                ChartType = SeriesChartType.Bar,
                Color = Color.DodgerBlue,
                BorderWidth = 1,
                IsValueShownAsLabel = true,
                LabelFormat = "{0:N0}",
                Font = new Font("Arial", 8, FontStyle.Bold),
            };

            foreach (var item in weeklyTotalRevenue)
            {
                int pointIndex = series.Points.AddXY(item.TuanLabel, item.TotalRevenue);

                // Hiển thị tooltip khi hover
                series.Points[pointIndex].ToolTip = $"Tuần {item.Ky} - Tháng {item.Thang}";
            }

            chart4.Series.Add(series);
            chart4.Legends.Clear();
            chart4.ChartAreas[0].RecalculateAxesScale();
        }

        ////////////////////// Biểu đồ thống kê theo tháng ////////////////////////////

        private void LoadChartTheoThang()
        {
            DateTime tuNgay = dateTimePicker1.Value.Date;
            DateTime denNgay = dateTimePicker2.Value.Date;

            if (tuNgay > denNgay)
            {
                MessageBox.Show("Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc.", "Lỗi chọn ngày", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            DataTable dt = bus.LayThongTinThongKe("Theo Tháng", tuNgay, denNgay);

            if (dt == null || dt.Rows.Count == 0 ||
                !dt.Columns.Contains("TongDoanhThu") ||
                !dt.Columns.Contains("MaNhanVien") ||
                !dt.Columns.Contains("Ky") ||
                !dt.Columns.Contains("Nam"))
            {
                MessageBox.Show("Không có dữ liệu hoặc thiếu cột cần thiết để hiển thị biểu đồ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            chart1.Series.Clear();
            chart1.Titles.Clear();
            chart1.ChartAreas.Clear();
            chart1.ChartAreas.Add(new ChartArea("DefaultArea"));

            Title title = new Title
            {
                Text = "Biểu đồ thống kê tổng doanh thu của nhân viên theo tháng",
                Font = new Font("Arial", 14, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Alignment = ContentAlignment.TopCenter
            };
            chart1.Titles.Add(title);

            ChartArea area = chart1.ChartAreas["DefaultArea"];
            area.BackColor = Color.White;
            area.BorderDashStyle = ChartDashStyle.Solid;
            area.BorderWidth = 1;
            area.BorderColor = Color.LightGray;

            area.AxisX.Interval = 1;
            area.AxisX.MajorGrid.Enabled = false;
            area.AxisX.LabelStyle.Angle = -45;
            area.AxisX.IsLabelAutoFit = true;
            area.AxisX.LabelStyle.Font = new Font("Arial", 8);
            area.AxisX.MajorTickMark.Enabled = true;
            area.AxisX.MajorTickMark.LineColor = Color.Gray;

            area.AxisY.Title = "Tổng doanh thu (VNĐ)";
            area.AxisY.Minimum = 0;
            area.AxisY.IsLabelAutoFit = true;
            area.AxisY.MajorGrid.Enabled = true;
            area.AxisY.MajorGrid.LineColor = Color.LightGray;
            area.AxisY.LabelStyle.Format = "{0:N0}";
            area.AxisY.LabelStyle.Font = new Font("Arial", 8);

            var nhanVienList = dt.AsEnumerable()
                                .Select(r => r.Field<string>("MaNhanVien"))
                                .Distinct()
                                .OrderBy(nv => nv);

            if (!nhanVienList.Any())
            {
                MessageBox.Show("Không có dữ liệu tổng doanh thu theo tháng cho nhân viên.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            List<Color> customColors = new List<Color>
            {
                Color.DodgerBlue, Color.OrangeRed, Color.ForestGreen, Color.Purple, Color.Goldenrod,
                Color.DeepPink, Color.Teal, Color.DarkSlateBlue, Color.Firebrick, Color.LimeGreen,
                Color.DarkCyan, Color.DarkViolet, Color.IndianRed, Color.MediumSeaGreen, Color.SlateBlue
            };

            int colorIndex = 0;

            foreach (var maNV in nhanVienList)
            {
                Series series = new Series(maNV)
                {
                    ChartType = SeriesChartType.Column,
                    IsValueShownAsLabel = true,
                    LabelFormat = "{0:N0}",
                    Font = new Font("Arial", 8, FontStyle.Bold),
                    Color = customColors[(colorIndex++) % customColors.Count],
                    Legend = "MyLegend"
                };
                series["PointWidth"] = "2";

                var rows = dt.AsEnumerable()
                    .Where(r => r.Field<string>("MaNhanVien") == maNV &&
                                r.Field<int>("Ky") != 0 &&
                                r.Field<int>("Nam") != 0)
                    .OrderBy(r => r.Field<int>("Nam"))
                    .ThenBy(r => r.Field<int>("Ky"));

                foreach (var row in rows)
                {
                    string tenThang = $"Tháng {row.Field<int>("Ky")} ({row.Field<int>("Nam")})";
                    double tongDoanhThu = Convert.ToDouble(row.Field<object>("TongDoanhThu"));

                    series.Points.AddXY(tenThang, tongDoanhThu);
                    series.Points.Last().ToolTip = $"{tenThang}\nNhân viên: {maNV}\nDoanh thu: {tongDoanhThu:N0} VNĐ";
                }

                chart1.Series.Add(series);
            }

            chart1.Legends.Clear();
            Legend legend = new Legend("MyLegend")
            {
                Docking = Docking.Right,
                Alignment = StringAlignment.Near,
                BackColor = Color.White,
                BorderColor = Color.LightGray,
                BorderWidth = 1,
                Font = new Font("Arial", 9)
            };
            chart1.Legends.Add(legend);

            area.RecalculateAxesScale();

            area.AxisX.LabelAutoFitStyle = LabelAutoFitStyles.IncreaseFont
                                           | LabelAutoFitStyles.DecreaseFont
                                           | LabelAutoFitStyles.WordWrap;
            area.AxisX.LabelStyle.Angle = -45;
            area.AxisX.LabelStyle.IsStaggered = true;
        }
        ////////////////////// Biểu đồ thống kê theo quý /////////////////////////////

        private void LoadPieChartTheoQuy()
        {
            DateTime tuNgay = dateTimePicker1.Value.Date;
            DateTime denNgay = dateTimePicker2.Value.Date;

            if (tuNgay > denNgay)
            {
                MessageBox.Show("Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc.", "Lỗi chọn ngày", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            DataTable dt = bus.LayThongTinThongKe("Theo Quý", tuNgay, denNgay);

            if (!dt.Columns.Contains("TongDoanhThu") || !dt.Columns.Contains("Ky") || !dt.Columns.Contains("Nam"))
            {
                MessageBox.Show("Dữ liệu không đầy đủ để hiển thị biểu đồ.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            chart3.Series.Clear();
            chart3.Titles.Clear();
            chart3.Legends.Clear();
            chart3.ChartAreas.Clear();

            ChartArea chartArea = new ChartArea("PieArea")
            {
                BackColor = Color.White
            };
            chart3.ChartAreas.Add(chartArea);

            Title title = new Title
            {
                Text = "Biểu đồ thống kê tổng doanh thu của từng nhân viên theo quý",
                Docking = Docking.Top,
                Font = new Font("Arial", 14, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
            };
            chart3.Titles.Add(title);

            var data = dt.AsEnumerable()
                .Where(r => !r.IsNull("Ky") && !r.IsNull("Nam") && !r.IsNull("TongDoanhThu"))
                .GroupBy(r => new { Quy = r.Field<int>("Ky"), Nam = r.Field<int>("Nam") })
                .Select(g => new
                {
                    QuyNam = $"Q{g.Key.Quy} ({g.Key.Nam})",
                    TongDoanhThu = g.Sum(x =>
                    {
                        double val;
                        return double.TryParse(x["TongDoanhThu"].ToString(), out val) && !double.IsNaN(val) && !double.IsInfinity(val) ? val : 0;
                    })
                })
                .OrderBy(x => x.QuyNam)
                .ToList();

            if (!data.Any())
            {
                MessageBox.Show("Không có dữ liệu để hiển thị biểu đồ tròn.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Series series = new Series("DoanhThuTheoQuy")
            {
                ChartType = SeriesChartType.Pie,
                Font = new Font("Arial", 10),
                Label = "#PERCENT{P0}\n#VALY{N0}",
                LegendText = "#AXISLABEL",
                IsValueShownAsLabel = true,
                CustomProperties = "PieLabelStyle=Outside, PieLineColor=Gray",
                BorderColor = Color.White,
                BorderWidth = 1,
                Palette = ChartColorPalette.Pastel
            };

            foreach (var item in data)
            {
                if (item.TongDoanhThu < 0)
                {
                    MessageBox.Show($"Cảnh báo: Giá trị doanh thu âm cho {item.QuyNam}. Đặt về 0 hoặc bỏ qua.", "Cảnh báo Dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (double.IsNaN(item.TongDoanhThu) || double.IsInfinity(item.TongDoanhThu))
                {
                    MessageBox.Show($"Cảnh báo: Dữ liệu doanh thu không hợp lệ (NaN/Infinity) cho {item.QuyNam}. Bỏ qua điểm này.", "Cảnh báo Dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    continue;
                }

                DataPoint point = new DataPoint
                {
                    AxisLabel = item.QuyNam,
                    YValues = new double[] { item.TongDoanhThu },
                    ToolTip = $"{item.QuyNam}: {item.TongDoanhThu:N0} VNĐ"
                };
                series.Points.Add(point);
            }

            chart3.Series.Add(series);

            Legend legend = new Legend("Legend1")
            {
                Docking = Docking.Right,
                Font = new Font("Arial", 9)
            };
            chart3.Legends.Add(legend);
        }

        /////////////////////// Tab 2 - Thống kê sản phẩm /////////////////////////////

        private void LoadThongKeSP()
        {
            string maSP = comboBox2.SelectedValue?.ToString();
            DateTime tuNgay = dateTimePicker4.Value.Date;
            DateTime denNgay = dateTimePicker3.Value.Date;

            DataTable dt = bus.Get(maSP, tuNgay, denNgay);
            dataGridView2.DataSource = dt;
        }

        private void LoadComboSP()
        {
            var listMaSP = bus.LoadmaSP();

            if (listMaSP != null)
            {
                var allProducts = new qlLSP
                {
                    MaLoai = "",
                    TenLoai = "--Tất cả sản phẩm"
                };
                listMaSP.Insert(0, allProducts);

                comboBox2.DataSource = listMaSP;
                comboBox2.DisplayMember = "TenLoai";
                comboBox2.ValueMember = "MaLoai";
                comboBox2.SelectedIndex = 0;
            }
            else
            {
                MessageBox.Show("Không thể tải dữ liệu Mã sản phẩm.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ClearFields();
            comboBox2.SelectedIndex = -1;
        }

       

        ///////////////////////////////////////////// Thống kê 5 sản phẩm bán chạy nhất /////////////////////////////

        private void VeBieuDoTop5(DataTable dt)
        {
            chart2.Series.Clear();
            chart2.ChartAreas[0].AxisX.Title = "Sản phẩm";
            chart2.ChartAreas[0].AxisY.Title = "Số lượng bán";

            chart2.Titles.Clear();
            chart2.Titles.Add("Top 5 Sản phẩm bán chạy");
            chart2.Titles[0].Font = new Font("Arial", 16, FontStyle.Bold);
            chart2.Titles[0].ForeColor = Color.DarkBlue;

            Series series = new Series("Sản phẩm")
            {
                ChartType = SeriesChartType.Column
            };

            chart2.ChartAreas[0].AxisX.LabelStyle.Angle = -45;
            chart2.ChartAreas[0].AxisX.LabelStyle.Font = new Font("Arial", 10);
            chart2.ChartAreas[0].AxisY.LabelStyle.Font = new Font("Arial", 10);

            chart2.ChartAreas[0].AxisY.Minimum = 0;
            chart2.ChartAreas[0].AxisY.Interval = 1;

            Color[] colors = new Color[] {
                Color.Blue, Color.Green, Color.Red, Color.Orange, Color.Purple,
                Color.Teal, Color.Olive, Color.Brown, Color.DarkCyan, Color.DarkMagenta
            };

            int i = 0;
            foreach (DataRow row in dt.Rows)
            {
                string tenSP = row["TenSanPham"].ToString();
                int soLuong = Convert.ToInt32(row["TongSoLuong"]);
                int pointIndex = series.Points.AddXY(tenSP, soLuong);

                series.Points[pointIndex].Color = colors[i % colors.Length];
                series.Points[pointIndex].Label = soLuong.ToString();
                series.Points[pointIndex].LabelForeColor = Color.Black;
                series.Points[pointIndex].Font = new Font("Arial", 9, FontStyle.Bold);
                series.Points[pointIndex].BorderColor = Color.DarkGray;
                series.Points[pointIndex].BorderWidth = 1;

                series.ShadowOffset = 2;
                series.ShadowColor = Color.FromArgb(128, 0, 0, 0);
                i++;
            }

            chart2.Series.Add(series);

            if (chart2.Legends.Count > 0)
            {
                chart2.Legends[0].Docking = Docking.Top;
                chart2.Legends[0].Alignment = StringAlignment.Center;
                chart2.Legends[0].Font = new Font("Arial", 10);
                chart2.Legends[0].BorderColor = Color.LightGray;
            }

            chart2.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
            chart2.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
        }

        ///////////////////////////////////////////// Biểu đồ doanh thu theo tháng /////////////////////////////

        private void HienThiBieuDoPie()
        {
            DateTime tuNgay = dateTimePicker4.Value;
            DateTime denNgay = dateTimePicker3.Value;

            DataTable dt = bus.LayDoanhThuTheoThang(tuNgay, denNgay);

            chart5.Series.Clear();
            chart5.Titles.Clear();
            chart5.Legends.Clear();
            chart5.ChartAreas.Clear();

            ChartArea chartArea = new ChartArea("BarChartArea")
            {
                BackColor = Color.Transparent
            };
            chart5.ChartAreas.Add(chartArea);

            Title title = new Title("Biểu đồ doanh thu theo tháng")
            {
                Font = new Font("Arial", 16, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Docking = Docking.Top,
            };
            chart5.Titles.Add(title);

            Series series = new Series("DoanhThuTheoThang")
            {
                ChartType = SeriesChartType.Bar, // BIỂU ĐỒ CỘT NGANG
                Font = new Font("Arial", 9, FontStyle.Bold),
                IsValueShownAsLabel = true,
                Color = Color.SeaGreen,
                BorderColor = Color.Black,
                BorderWidth = 1
            };

            if (dt == null || dt.Rows.Count == 0 || !dt.Columns.Contains("Thang") || !dt.Columns.Contains("DoanhThu"))
            {
                return;
            }

            foreach (DataRow row in dt.Rows)
            {
                if (row["Thang"] == DBNull.Value || row["DoanhThu"] == DBNull.Value)
                    continue;

                string thang = row["Thang"].ToString();
                double doanhThu;
                if (!double.TryParse(row["DoanhThu"].ToString(), out doanhThu))
                    continue;

                DataPoint point = new DataPoint
                {
                    AxisLabel = "Tháng " + thang,
                    YValues = new double[] { doanhThu },
                    ToolTip = $"Tháng {thang}: {doanhThu:N0} VNĐ"
                };
                series.Points.Add(point);
            }

            chart5.Series.Add(series);

            Legend legend = new Legend("DoanhThuLegend")
            {
                Docking = Docking.Bottom,
                Font = new Font("Arial", 10),
                BackColor = Color.Transparent
            };
            chart5.Legends.Add(legend);
            Color[] colors = new Color[] {
    Color.Blue, Color.Green, Color.Red, Color.Orange, Color.Purple,
    Color.Teal, Color.Olive, Color.Brown, Color.DarkCyan, Color.DarkMagenta
};
            // Tùy chọn trục
            chart5.ChartAreas["BarChartArea"].AxisY.Title = "Tháng";
            chart5.ChartAreas["BarChartArea"].AxisX.Title = "Doanh thu (VNĐ)";
            chart5.ChartAreas["BarChartArea"].AxisY.Interval = 1;

            // Loại bỏ các đường kẻ trên trục Y
            chart5.ChartAreas["BarChartArea"].AxisY.MajorGrid.Enabled = false;
            chart5.ChartAreas["BarChartArea"].AxisY.MinorGrid.Enabled = false; // Thêm dòng này để tắt minor grid

            // Loại bỏ các đường kẻ trên trục X
            chart5.ChartAreas["BarChartArea"].AxisX.MajorGrid.Enabled = false; // Thêm dòng này để tắt major grid
            chart5.ChartAreas["BarChartArea"].AxisX.MinorGrid.Enabled = false; // Thêm dòng này để tắt minor grid

            chart5.ChartAreas["BarChartArea"].AxisX.LabelStyle.Format = "#,##0 VNĐ";
        }

        ///////////////////////////////////////////// Biểu đồ số phiếu theo sản phẩm /////////////////////////////

        private void VeBieuDo(DataTable dt)
        {
            chart6.Series.Clear();
            chart6.ChartAreas.Clear();

            ChartArea chartArea = new ChartArea("ChartArea1");
            chart6.ChartAreas.Add(chartArea);

            Series series = new Series("Số phiếu")
            {
                ChartType = SeriesChartType.Line,
                BorderWidth = 3,
                Color = Color.DarkBlue,
                XValueType = ChartValueType.String
            };

            foreach (DataRow row in dt.Rows)
            {
                string tenSP = row["TenSanPham"].ToString();
                int soPhieu = Convert.ToInt32(row["SoPhieuXuatHien"]);

                series.Points.AddXY(tenSP, soPhieu);
            }

            chart6.Series.Add(series);

            chart6.ChartAreas[0].AxisX.Title = "Tên sản phẩm";
            chart6.ChartAreas[0].AxisY.Title = "Số phiếu bán";

            chart6.ChartAreas[0].AxisX.LabelStyle.Angle = -45;
            chart6.ChartAreas[0].AxisX.Interval = 1;
        }

        // Empty event handlers for UI controls
        
        private void button4_Click_1(object sender, EventArgs e)
        {
            try
            {
                string maloai = comboBox2.SelectedIndex >= 0 ? comboBox2.SelectedValue?.ToString() : null;
                DateTime tuNgay = dateTimePicker4.Value.Date;
                DateTime denNgay = dateTimePicker3.Value.Date;

                if (tuNgay > denNgay)
                {
                    MessageBox.Show("Ngày bắt đầu không được lớn hơn ngày kết thúc.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DataTable dtTop5 = bus.LayTop5SanPhamBanChay(tuNgay, denNgay);
                DataTable dtPhieu = bus.LayThongKeSoPhieuTheoSanPham(maloai, tuNgay, denNgay);
                DataTable dtThang = bus.LayDoanhThuTheoThang( tuNgay, denNgay);
                VeBieuDoTop5(dtTop5);
                HienThiBieuDoPie();
                VeBieuDo(dtPhieu);

                string maSP = comboBox2.SelectedIndex >= 0 ? comboBox2.SelectedValue?.ToString() : null;
                DataTable dt = bus.Get(maSP, tuNgay, denNgay);

                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("Không có dữ liệu thống kê phù hợp.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                dataGridView2.DataSource = dt;
                groupBox8.Visible = true;
                groupBox9.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Đã xảy ra lỗi khi thống kê: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            LoadChartTheoTuan();
        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            LoadChartTheoTuan();
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void LoadNhanVienToComboBox()
        {
            try
            {
                DataTable dtNhanVien = nhanVienBLL.LayDanhSachNhanVienChoComboBox();

                // Thêm mục "Tất cả nhân viên" vào đầu ComboBox
                DataRow newRow = dtNhanVien.NewRow();
                newRow["MaNhanVien"] = "Tất cả";
                newRow["HoTen"] = "Tất cả nhân viên";
                dtNhanVien.Rows.InsertAt(newRow, 0);

                comboBox1.DataSource = dtNhanVien;
                comboBox1.DisplayMember = "MaNhanVien";
                comboBox1.ValueMember = "MaNhanVien";

                comboBox1.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách nhân viên: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadChartTheoThang(List<ThongKeNhanVienDTO> listThongKe)
        {
            // Xóa các series, tiêu đề, và chart areas cũ
            chart1.Series.Clear();
            chart1.Titles.Clear();
            chart1.ChartAreas.Clear();
            chart1.ChartAreas.Add(new ChartArea("DefaultArea"));

            // Cấu hình tiêu đề biểu đồ
            Title title = new Title
            {
                Text = "Biểu đồ thống kê tổng doanh thu của nhân viên theo tháng",
                Font = new Font("Arial", 14, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Alignment = ContentAlignment.TopCenter
            };
            chart1.Titles.Add(title);

            // Cấu hình vùng biểu đồ (ChartArea)
            ChartArea area = chart1.ChartAreas["DefaultArea"];
            area.BackColor = Color.White;
            area.BorderDashStyle = ChartDashStyle.Solid;
            area.BorderWidth = 1;
            area.BorderColor = Color.LightGray;

            // Lấy tất cả các tháng/năm duy nhất có trong dữ liệu VÀ sắp xếp chúng
            var uniqueMonths = listThongKe
                                 .Select(r => new {
                                     Nam = r.Nam,
                                     Ky = r.Ky
                                 })
                                 .Distinct()
                                 .OrderBy(m => m.Nam)
                                 .ThenBy(m => m.Ky)
                                 .ToList();

            // Tạo danh sách các chuỗi nhãn cho trục X (Category Axis)
            List<string> xAxisLabels = new List<string>();
            foreach (var month in uniqueMonths)
            {
                xAxisLabels.Add($"Tháng {month.Ky} ({month.Nam})");
            }

            // Cấu hình trục X
            area.AxisX.MajorGrid.Enabled = false;
            area.AxisX.LabelStyle.Angle = -45;
            area.AxisX.IsLabelAutoFit = true;
            area.AxisX.LabelStyle.Font = new Font("Arial", 8);
            area.AxisX.MajorTickMark.Enabled = true;
            area.AxisX.MajorTickMark.LineColor = Color.Gray;
            area.AxisX.Title = "Tháng (Năm)";
            area.AxisX.TitleFont = new Font("Arial", 10, FontStyle.Bold);
            area.AxisX.Interval = 1;
            area.AxisX.LabelStyle.IsStaggered = false; // Tắt tính năng so le


            // Cấu hình trục Y
            area.AxisY.Title = "Tổng doanh thu (VNĐ)";
            area.AxisY.Minimum = 0;
            area.AxisY.IsLabelAutoFit = true;
            area.AxisY.MajorGrid.Enabled = true;
            area.AxisY.MajorGrid.LineColor = Color.LightGray;
            area.AxisY.LabelStyle.Format = "{0:N0}";
            area.AxisY.LabelStyle.Font = new Font("Arial", 8);
            area.AxisY.TitleFont = new Font("Arial", 10, FontStyle.Bold);


            // Lấy danh sách các nhân viên duy nhất có doanh thu trong dữ liệu
            var nhanVienList = listThongKe
                                  .Select(r => new { MaNhanVien = r.MaNhanVien, HoTen = r.HoTen })
                                  .Distinct()
                                  .OrderBy(nv => nv.MaNhanVien);

            if (!nhanVienList.Any())
            {
                // Trường hợp này lẽ ra đã được xử lý ở button1_Click, nhưng thêm vào để đảm bảo
                MessageBox.Show("Không có dữ liệu tổng doanh thu theo tháng cho nhân viên.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Danh sách màu tùy chỉnh cho các series
            List<Color> customColors = new List<Color>
            {
                Color.DodgerBlue, Color.OrangeRed, Color.ForestGreen, Color.Purple, Color.Goldenrod,
                Color.DeepPink, Color.Teal, Color.DarkSlateBlue, Color.Firebrick, Color.LimeGreen,
                Color.DarkCyan, Color.DarkViolet, Color.IndianRed, Color.MediumSeaGreen, Color.SlateBlue
            };
            int colorIndex = 0;

            // Tạo các series cho từng nhân viên
            foreach (var nv in nhanVienList)
            {
                string seriesName = nv.MaNhanVien;
                Series series = new Series(seriesName)
                {
                    ChartType = SeriesChartType.Column,
                    IsValueShownAsLabel = true,
                    LabelFormat = "{0:N0}",
                    Font = new Font("Arial", 8, FontStyle.Bold),
                    Color = customColors[(colorIndex++) % customColors.Count],
                    Legend = "MyLegend"
                };
                series["PointWidth"] = "0.8";
                series.XValueType = ChartValueType.String; // RẤT QUAN TRỌNG

                // Lọc dữ liệu của nhân viên hiện tại, đã sắp xếp theo năm, tháng
                var rowsForEmployee = listThongKe
                                        .Where(r => r.MaNhanVien == nv.MaNhanVien)
                                        .OrderBy(r => r.Nam)
                                        .ThenBy(r => r.Ky);

                // Duyệt qua TẤT CẢ các tháng duy nhất đã xác định trên trục X
                foreach (var monthLabel in xAxisLabels)
                {
                    int monthFromLabel = int.Parse(monthLabel.Split(' ')[1]);
                    int yearFromLabel = int.Parse(monthLabel.Split('(')[1].TrimEnd(')'));

                    // Tìm dữ liệu tương ứng cho tháng và năm này của nhân viên hiện tại
                    var currentData = rowsForEmployee.FirstOrDefault(r =>
                        r.Ky == monthFromLabel &&
                        r.Nam == yearFromLabel
                    );

                    decimal tongDoanhThu = 0; // Sử dụng decimal cho tiền tệ
                    string toolTipText = "";

                    if (currentData != null)
                    {
                        tongDoanhThu = currentData.TongDoanhThu;
                        toolTipText = $"Tháng: {monthFromLabel}/{yearFromLabel}\nNhân viên: {nv.HoTen} ({nv.MaNhanVien})\nDoanh thu: {tongDoanhThu:N0} VNĐ";
                    }
                    else
                    {
                        toolTipText = $"Tháng: {monthFromLabel}/{yearFromLabel}\nNhân viên: {nv.HoTen} ({nv.MaNhanVien})\nDoanh thu: 0 VNĐ";
                    }

                    series.Points.AddXY(monthLabel, tongDoanhThu); // Add decimal value
                    series.Points.Last().ToolTip = toolTipText;
                }
                chart1.Series.Add(series);
            }

            // Cấu hình chú giải (Legend)
            chart1.Legends.Clear();
            Legend legend = new Legend("MyLegend")
            {
                Docking = Docking.Right,
                Alignment = StringAlignment.Near,
                BackColor = Color.White,
                BorderColor = Color.LightGray,
                BorderWidth = 1,
                Font = new Font("Arial", 9)
            };
            chart1.Legends.Add(legend);

            // Điều chỉnh lại tỷ lệ trục và kiểu nhãn trục X
            area.RecalculateAxesScale();
            area.AxisX.LabelAutoFitStyle = LabelAutoFitStyles.IncreaseFont
                                         | LabelAutoFitStyles.DecreaseFont
                                         | LabelAutoFitStyles.WordWrap;
            area.AxisX.LabelStyle.Angle = -45;
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }
    }
}

