using BLL;
using DTO;
using System;
using System.Windows;

namespace GUI.Views.Windows
{
    /// <summary>
    /// Interaction logic for ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindow : Window
    {
        // Gọi BLL để xử lý logic kết nối
        private readonly SystemConfigBLL _bll = new SystemConfigBLL();

        public ConfigWindow()
        {
            InitializeComponent();

            // Set trạng thái ban đầu cho giao diện (ẩn/hiện ô pass theo radio button mặc định)
            UpdateUIState();
        }

        // --- 1. XỬ LÝ GIAO DIỆN (Ẩn hiện ô nhập User/Pass) ---

        private void rbAuth_Checked(object sender, RoutedEventArgs e)
        {
            // Sự kiện này kích hoạt khi người dùng bấm chọn RadioButton
            UpdateUIState();
        }

        private void UpdateUIState()
        {
            // Kiểm tra xem control đã được khởi tạo chưa để tránh lỗi null khi form mới load
            if (grpSqlAuth == null) return;

            if (rbWindows.IsChecked == true)
            {
                // Nếu chọn Windows Auth -> Ẩn phần nhập User/Pass
                grpSqlAuth.Visibility = Visibility.Collapsed;
            }
            else
            {
                // Nếu chọn SQL Auth -> Hiện phần nhập User/Pass
                grpSqlAuth.Visibility = Visibility.Visible;
            }
        }

        // --- 2. LẤY DỮ LIỆU TỪ FORM ---

        private SystemConfig GetConfigFromUI()
        {
            return new SystemConfig
            {
                ServerName = txtServer.Text.Trim(),
                DatabaseName = txtDbName.Text.Trim(),
                UseWindowsAuth = rbWindows.IsChecked == true,
                UserName = txtUser.Text.Trim(),
                Password = txtPass.Password.Trim()
            };
        }

        // --- 3. XỬ LÝ NÚT BẤM ---

        // Nút Thoát (X)
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Nút Kiểm tra kết nối
        private void btnCheck_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Đổi con trỏ chuột thành hình đồng hồ cát (Loading)
                this.Cursor = System.Windows.Input.Cursors.Wait;

                var config = GetConfigFromUI();
                string connStr = _bll.BuildConnectionString(config);

                if (_bll.TestConnection(connStr))
                {
                    MessageBox.Show("Kết nối đến SQL Server THÀNH CÔNG!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Kết nối THẤT BẠI. Vui lòng kiểm tra lại Tên Server, Database hoặc Tài khoản.", "Lỗi kết nối", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            finally
            {
                // Trả lại con trỏ chuột bình thường
                this.Cursor = System.Windows.Input.Cursors.Arrow;
            }
        }

        // Nút Lưu cấu hình
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var config = GetConfigFromUI();
            string connStr = _bll.BuildConnectionString(config);

            // Bước 1: Test kết nối trước khi lưu cho chắc ăn
            if (!_bll.TestConnection(connStr))
            {
                var result = MessageBox.Show("Cảnh báo: Không thể kết nối đến Server với thông tin này.\nBạn có chắc chắn muốn lưu không?",
                                             "Kết nối thất bại",
                                             MessageBoxButton.YesNo,
                                             MessageBoxImage.Warning);

                if (result == MessageBoxResult.No) return;
            }

            // Bước 2: Lưu vào Properties.Settings
            try
            {
                // Gán giá trị
                GUI.Properties.Settings.Default.DbConnectionString = connStr;
                GUI.Properties.Settings.Default.IsConfigured = true;

                // Lệnh Save() thực sự ghi xuống ổ cứng
                GUI.Properties.Settings.Default.Save();

                MessageBox.Show("Lưu cấu hình thành công!\nỨng dụng sẽ tự động khởi động lại.", "Hoàn tất", MessageBoxButton.OK, MessageBoxImage.Information);

                // Bước 3: Khởi động lại ứng dụng
                // Lấy đường dẫn file .exe hiện tại và chạy nó
                System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);

                // Tắt instance hiện tại
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu cấu hình: " + ex.Message);
            }
        }
    }
}