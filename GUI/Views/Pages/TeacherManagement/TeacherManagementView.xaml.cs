using GUI.ViewModels;
using System.Windows.Controls;

namespace GUI.Views.Pages.TeacherManagement
{
    public partial class TeacherManagementView : UserControl
    {
        public TeacherManagementView()
        {
            InitializeComponent();
            // Kết nối View với ViewModel
            this.DataContext = new TeacherManagementViewModel();
        }
    }
}