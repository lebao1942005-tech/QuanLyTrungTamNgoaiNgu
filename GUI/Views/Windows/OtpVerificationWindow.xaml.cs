using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GUI.Views.Windows
{
    public partial class OtpVerificationWindow : Window
    {
        public OtpVerificationWindow()
        {
            InitializeComponent();
            SetupOtpBoxLogic();
        }

        // Nếu bạn dùng constructor nhận email
        public OtpVerificationWindow(string email) : this()
        {
            var viewModel = new GUI.ViewModels.OtpVerificationViewModel();
            viewModel.Initialize(email);
            this.DataContext = viewModel;
        }

        private void SetupOtpBoxLogic()
        {
            // Logic nhảy sang ô tiếp theo khi nhập 1 ký tự
            OtpBox1.TextChanged += (s, e) => MoveFocus(OtpBox1, OtpBox2);
            OtpBox2.TextChanged += (s, e) => MoveFocus(OtpBox2, OtpBox3);
            OtpBox3.TextChanged += (s, e) => MoveFocus(OtpBox3, OtpBox4);
            OtpBox4.TextChanged += (s, e) => MoveFocus(OtpBox4, OtpBox5);
            OtpBox5.TextChanged += (s, e) => MoveFocus(OtpBox5, OtpBox6);

            // Logic quay lại ô trước khi nhấn Backspace (Tùy chọn nâng cao, ở đây xử lý đơn giản)
        }

        private void MoveFocus(TextBox current, TextBox next)
        {
            if (current.Text.Length == 1)
            {
                next.Focus();
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => this.Close();
        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;
        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal) this.WindowState = WindowState.Maximized;
            else this.WindowState = WindowState.Normal;
        }
    }
}