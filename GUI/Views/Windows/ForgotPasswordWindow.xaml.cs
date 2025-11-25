using GUI.ViewModels; // Nhớ using
using System.Windows;
using System.Windows.Input;

namespace GUI.Views.Windows
{
    public partial class ForgotPasswordWindow : Window
    {
        public ForgotPasswordWindow()
        {
            InitializeComponent();
            // Gắn ViewModel
            this.DataContext = new ForgotPasswordViewModel();
        }

        // Các hàm xử lý giao diện (Kéo thả, Phóng to, Thu nhỏ, Đóng) GIỮ NGUYÊN
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { if (e.ButtonState == MouseButtonState.Pressed) this.DragMove(); }
        private void MinimizeButton_Click(object sender, RoutedEventArgs e) { this.WindowState = WindowState.Minimized; }
        private void CloseButton_Click(object sender, RoutedEventArgs e) { this.Close(); }
        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized) this.WindowState = WindowState.Normal;
            else this.WindowState = WindowState.Maximized;
        }

        // XÓA các hàm SendOtpButton_Click và BackToLogin_Click cũ đi vì đã dùng Command trong ViewModel
    }
}