using System.Windows;
using System.Windows.Input;
using GUI.ViewModels;

namespace GUI.Views.Windows
{
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
            // Kết nối View với ViewModel
            this.DataContext = new LoginViewModel();
        }

        // Các logic điều khiển cửa sổ (Window Chrome) giữ nguyên ở code-behind
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        // --- LOGIC GỌI VIEWMODEL ---

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel viewModel)
            {
                // Truyền PasswordBox vào Command để ViewModel xử lý
                viewModel.LoginCommand.Execute(PasswordBox);
            }
        }

        private void ForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel viewModel)
            {
                viewModel.ForgotPasswordCommand.Execute(null);
            }
        }
    }
}