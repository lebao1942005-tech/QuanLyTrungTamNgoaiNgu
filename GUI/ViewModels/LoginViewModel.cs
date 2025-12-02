using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using GUI.Views.Windows;
using BLL;
using DTO;          // Cần để dùng UserDTO
using GUI.Utilities; // Cần để dùng UserSession

namespace GUI.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly LoginBLL _loginBLL = new LoginBLL();

        [ObservableProperty]
        private string _email;

        [ObservableProperty]
        private string _selectedRole;

        public LoginViewModel() { }

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

            // 1. Validate (Giữ nguyên)
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

            // 2. Mapping Role
            string dbRole = SelectedRole;
            if (SelectedRole == "Giáo viên") dbRole = "Teacher";
            if (SelectedRole == "Admin") dbRole = "Admin";

            // 3. Gọi BLL Login
            try
            {
                // SỬA: Nhận về UserDTO thay vì bool
                UserDTO user = _loginBLL.Login(Email, password, dbRole);

                if (user != null)
                {
                    // --- QUAN TRỌNG: LƯU SESSION ---
                    UserSession.CurrentUsername = user.Username;
                    UserSession.Role = user.Role;
                    UserSession.CurrentTeacherID = user.TeacherID;
                    // Session này sẽ được dùng ở màn hình Lớp học để lọc danh sách
                    // --------------------------------

                    // 4. Điều hướng
                    if (user.Role == "Admin")
                    {
                        var mainWindow = new AdminMainWindow();
                        mainWindow.Show();
                        CloseCurrentWindow();
                    }
                    else if (user.Role == "Teacher")
                    {
                        // Mở màn hình giáo viên
                        var teacherWindow = new TeacherMainWindow();
                        teacherWindow.Show();
                        CloseCurrentWindow();
                    }
                }
                else
                {
                    MessageBox.Show("Thông tin đăng nhập không đúng!", "Thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Lỗi hệ thống: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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