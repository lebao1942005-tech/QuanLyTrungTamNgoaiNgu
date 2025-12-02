using System.Windows;
using GUI.ViewModels;

namespace GUI.Views.Windows
{
    public partial class AdminMainWindow : Window
    {
        public AdminMainWindow()
        {
            InitializeComponent();
            this.DataContext = new AdminMainWindowViewModel();
        }

        // XÓA HÀM NÀY NẾU CÒN TỒN TẠI
        // private void HeaderView_ToggleSidebarClick(...) { ... }
    }
}