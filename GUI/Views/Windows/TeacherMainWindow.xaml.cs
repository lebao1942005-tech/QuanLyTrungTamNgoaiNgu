using System.Windows;
using GUI.ViewModels;

namespace GUI.Views.Windows
{
    public partial class TeacherMainWindow : Window
    {
        public TeacherMainWindow()
        {
            InitializeComponent();
            // Gán ViewModel làm DataContext cho Window này
            this.DataContext = new TeacherMainWindowViewModel();
        }
    }
}