using BLL;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GUI.Views.Windows;
using System.Windows;
using System.Linq;

namespace GUI.ViewModels
{
    public partial class OtpVerificationViewModel : ObservableObject
    {
        private readonly LoginBLL _loginBLL = new LoginBLL();
        private string _currentOtp;

        [ObservableProperty] private string _targetEmail;

        // 6 ô nhập liệu OTP
        [ObservableProperty] private string _otp1;
        [ObservableProperty] private string _otp2;
        [ObservableProperty] private string _otp3;
        [ObservableProperty] private string _otp4;
        [ObservableProperty] private string _otp5;
        [ObservableProperty] private string _otp6;

        public OtpVerificationViewModel() { }

        // Hàm này dùng khi người dùng bấm nút "Gửi lại"
        public void Initialize(string email)
        {
            TargetEmail = email;
            SendOtpToEmail();
        }

        // [MỚI] Hàm này dùng để nhận OTP từ màn hình Quên mật khẩu (không gửi lại mail)
        public void SetOtpData(string email, string sentOtp)
        {
            TargetEmail = email;
            _currentOtp = sentOtp;
        }

        private void SendOtpToEmail()
        {
            _currentOtp = LoginBLL.GenerateOTP();
            bool isSent = _loginBLL.SendOTP(TargetEmail, _currentOtp);

            if (isSent)
                MessageBox.Show($"Mã OTP mới đã được gửi đến {TargetEmail}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show("Gửi mã thất bại. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        [RelayCommand]
        private void VerifyOtp()
        {
            string inputOtp = $"{Otp1}{Otp2}{Otp3}{Otp4}{Otp5}{Otp6}";

            if (inputOtp.Length < 6)
            {
                MessageBox.Show("Vui lòng nhập đầy đủ 6 số xác thực!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (inputOtp == _currentOtp)
            {
                MessageBox.Show("Xác thực thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                // Mở màn hình Đặt lại mật khẩu
                var resetWindow = new ResetPasswordWindow();
                // Truyền email sang màn hình Reset để biết đổi pass cho ai
                if (resetWindow.DataContext is ResetPasswordViewModel vm)
                {
                    vm.TargetEmail = TargetEmail;
                }
                resetWindow.Show();

                CloseCurrentWindow();
            }
            else
            {
                MessageBox.Show("Mã OTP không chính xác.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void ResendOtp()
        {
            Otp1 = Otp2 = Otp3 = Otp4 = Otp5 = Otp6 = "";
            SendOtpToEmail();
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
            var window = Application.Current.Windows.OfType<Window>()
                                .FirstOrDefault(w => w.DataContext == this);
            window?.Close();
        }
    }
}