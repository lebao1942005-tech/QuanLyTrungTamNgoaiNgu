using DAL; // Để gọi DatabaseHelper
using System.Windows;

namespace GUI
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Kiểm tra xem đã cấu hình lần nào chưa (biến tạo ở BƯỚC 1)
            if (GUI.Properties.Settings.Default.IsConfigured)
            {
                // NẠP CHUỖI KẾT NỐI VÀO DAL (biến tạo ở BƯỚC 2.1)
                DatabaseHelper.ConnectionString = GUI.Properties.Settings.Default.DbConnectionString;

                // Mở màn hình Đăng nhập
                var loginWindow = new GUI.Views.Windows.Login();
                loginWindow.Show();
            }
            else
            {
                // Chưa cấu hình -> Mở màn hình Cấu hình
                var configWindow = new GUI.Views.Windows.ConfigWindow();
                configWindow.Show();
            }
        }
    }
}