using System.Windows;
using System.Windows.Input;

namespace GUI.Views.Windows
{
    public partial class ResetPasswordWindow : Window
    {
        public ResetPasswordWindow()
        {
            InitializeComponent();
            // Khởi tạo ViewModel nếu chưa có (để đảm bảo Binding Command hoạt động)
            if (this.DataContext == null)
            {
                this.DataContext = new GUI.ViewModels.ResetPasswordViewModel();
            }
        }

        // Cho phép kéo thả cửa sổ
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // --- THÊM 2 HÀM NÀY ĐỂ SỬA LỖI ---

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
            {
                this.WindowState = WindowState.Maximized;
            }
            else
            {
                this.WindowState = WindowState.Normal;
            }
        }
    }
}