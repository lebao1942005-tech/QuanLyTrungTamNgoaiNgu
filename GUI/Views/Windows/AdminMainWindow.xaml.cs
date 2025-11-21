using GUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GUI.Views.Windows
{
    /// <summary>
    /// Interaction logic for AdminMainWindow.xaml
    /// </summary>
    public partial class AdminMainWindow : Window
    {
        public AdminMainWindow()
        {
            InitializeComponent();
            DataContext = new AdminMainWindowViewModel();
        }


        private void HeaderView_ToggleSidebarClick(object sender, RoutedEventArgs e)
        {
            if (FullSidebar.Visibility == Visibility.Visible)
            {
                FullSidebar.Visibility = Visibility.Collapsed;
                SmallSidebar.Visibility = Visibility.Visible;
                SidebarColumn.Width = new GridLength(70);
            }
            else
            {
                FullSidebar.Visibility = Visibility.Visible;
                SmallSidebar.Visibility = Visibility.Collapsed;
                SidebarColumn.Width = new GridLength(240);
            }
        }

    }
}
