using System.Windows;
using System.Windows.Input;

namespace GUI.Views.Windows
{
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // hoặc để trống
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized; // hoặc để trống
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: xử lý đăng nhập sau
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove(); // cho phép kéo cửa sổ
        }
    }
}
