using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using GUI.Views.Windows;
using BLL;

namespace GUI.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        // KHỞI TẠO BLL TẠI ĐÂY
        private readonly LoginBLL _loginBLL = new LoginBLL();

        [ObservableProperty]
        private string _email;

        [ObservableProperty]
        private string _selectedRole;

        public LoginViewModel()
        {
        }

        [RelayCommand]
        private void ForgotPassword()
        {
            var forgotPasswordWindow = new ForgotPasswordWindow();
            forgotPasswordWindow.Show();
            CloseCurrentWindow();
        }

        [RelayCommand]
        private void Login(PasswordBox passwordBox)
        {
            string password = passwordBox.Password;

            // 1. Validate
            if (string.IsNullOrEmpty(SelectedRole) || SelectedRole == "Chọn vai trò của bạn")
            {
                MessageBox.Show("Vui lòng chọn vai trò đăng nhập.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(Email))
            {
                MessageBox.Show("Vui lòng nhập Email.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Mapping Role: UI (Tiếng Việt) -> Database (Tiếng Anh)
            string dbRole = SelectedRole;
            if (SelectedRole == "Giáo viên") dbRole = "Teacher";
            if (SelectedRole == "Admin") dbRole = "Admin";

            // 3. Gọi BLL Login
            bool isLoginSuccess = false;
            try
            {
                isLoginSuccess = _loginBLL.Login(Email, password, dbRole);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Lỗi kết nối: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 4. Xử lý kết quả phân quyền
            if (isLoginSuccess)
            {
                if (dbRole == "Admin")
                {
                    // Mở trang Admin
                    var mainWindow = new AdminMainWindow();
                    mainWindow.Show();
                    CloseCurrentWindow();
                }
                else if (dbRole == "Teacher")
                {
                    // TODO: Sau này tạo xong TeacherMainWindow thì bỏ comment dòng dưới
                    // var teacherWindow = new TeacherMainWindow();
                    // teacherWindow.Show();
                    // CloseCurrentWindow();

                    // Tạm thời hiện thông báo
                    MessageBox.Show($"Xin chào Giáo viên: {Email}.\nGiao diện giáo viên đang được phát triển!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Thông tin đăng nhập không đúng!", "Thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseCurrentWindow()
        {
            var window = Application.Current.Windows.OfType<Window>()
                                .FirstOrDefault(w => w.DataContext == this);
            window?.Close();
        }
    }
}