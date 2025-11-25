using BLL;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GUI.Views.Windows;
using System.Windows;
using System.Windows.Controls;

namespace GUI.ViewModels
{
    public partial class ResetPasswordViewModel : ObservableObject
    {
        private readonly LoginBLL _loginBLL = new LoginBLL();

        // Email của tài khoản cần đổi pass (nhận từ màn hình OTP)
        [ObservableProperty]
        private string _targetEmail;

        public ResetPasswordViewModel() { }

        // Command nhận tham số là PasswordBox (vì PasswordBox không binding trực tiếp được)
        [RelayCommand]
        private void ConfirmResetPassword(object parameter)
        {
            var passwordBox = parameter as PasswordBox;
            string newPassword = passwordBox?.Password;

            // 1. Validate dữ liệu
            if (string.IsNullOrEmpty(TargetEmail))
            {
                MessageBox.Show("Lỗi hệ thống: Không tìm thấy email tài khoản.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(newPassword))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu mới!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newPassword.Length < 6)
            {
                MessageBox.Show("Mật khẩu phải có ít nhất 6 ký tự để đảm bảo an toàn.", "Cảnh báo bảo mật", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Gọi BLL để đổi mật khẩu
            // Giả định Username chính là Email. Nếu Username khác Email, bạn cần thêm bước lấy Username từ Email.
            bool isSuccess = _loginBLL.ChangePassword(TargetEmail, newPassword);

            if (isSuccess)
            {
                MessageBox.Show("Đổi mật khẩu thành công! Vui lòng đăng nhập lại bằng mật khẩu mới.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                // --- LOGIC CHUYỂN TRANG LOGIN ---
                var loginWindow = new Login();
                loginWindow.Show();

                // Đóng cửa sổ này, có thể mở lại LoginWindow nếu cần
                CloseCurrentWindow();
            }
            else
            {
                MessageBox.Show("Đổi mật khẩu thất bại. Có thể tài khoản không tồn tại hoặc lỗi hệ thống.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void BackToLogin()
        {
            // Quay lại Login nếu người dùng hủy bỏ
            CloseCurrentWindow();
        }

        private void CloseCurrentWindow()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.Close();
                    break;
                }
            }
        }
    }
}