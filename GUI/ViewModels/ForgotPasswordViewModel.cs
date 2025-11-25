using BLL;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GUI.Views.Windows;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace GUI.ViewModels
{
    public partial class ForgotPasswordViewModel : ObservableObject
    {
        private readonly LoginBLL _loginBLL = new LoginBLL();

        [ObservableProperty]
        private string _email;

        public ForgotPasswordViewModel()
        {
        }

        [RelayCommand]
        private void SendCode()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                MessageBox.Show("Vui lòng nhập địa chỉ email!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Email không đúng định dạng.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string otpCode = LoginBLL.GenerateOTP();
            bool isSent = _loginBLL.SendOTP(Email, otpCode);

            if (isSent)
            {
                MessageBox.Show($"Mã OTP đã được gửi đến {Email}.\nVui lòng kiểm tra hộp thư.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                // --- SỬA ĐOẠN NÀY ---

                // 1. Tạo cửa sổ
                var otpWindow = new OtpVerificationWindow();

                // 2. Tạo ViewModel cho cửa sổ đó
                var otpVM = new OtpVerificationViewModel();

                // 3. Nạp dữ liệu (Email và mã OTP vừa gửi) vào ViewModel
                otpVM.SetOtpData(Email, otpCode);

                // 4. Gán ViewModel vào DataContext của cửa sổ
                otpWindow.DataContext = otpVM;

                // 5. Hiển thị
                otpWindow.Show();

                CloseCurrentWindow();
            }
            else
            {
                MessageBox.Show("Gửi thất bại! Email không tồn tại trong hệ thống hoặc lỗi mạng.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void BackToLogin()
        {
            var loginWindow = new Login();
            loginWindow.Show();
            CloseCurrentWindow();
        }

        private void CloseCurrentWindow()
        {
            var window = Application.Current.Windows
                                    .OfType<Window>()
                                    .FirstOrDefault(w => w.DataContext == this);

            window?.Close();
        }
    }
}