using GUI.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

// SỬA: Dùng namespace đúng chính tả (Management)
// Mặc dù thư mục là Managemnet nhưng ta có thể đặt namespace khác cho đúng nghĩa.
namespace GUI.Views.Pages.TuitionManagement
{
    public partial class TuitionManagementView : UserControl
    {
        public TuitionManagementView()
        {
            InitializeComponent();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is TuitionViewModel viewModel)
            {
                viewModel.IsPaymentPopupOpen = false;
            }
        }
    }
}