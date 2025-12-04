using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GUI.Views.Pages.ClassManagement
{
    /// <summary>
    /// Interaction logic for ClassGradingView.xaml
    /// </summary>
    public partial class ClassGradingView : UserControl
    {
        public ClassGradingView()
        {
            InitializeComponent();
        }


        private void ScoreValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            // Chỉ cho phép số (0-9) và dấu chấm (.)
            // Dấu ^ ở đầu [] có nghĩa là phủ định (NOT)
            // Tức là: Nếu ký tự KHÔNG PHẢI là 0-9 hoặc ., thì chặn lại (Handled = true)
            Regex regex = new Regex("[^0-9.]+");
            e.Handled = regex.IsMatch(e.Text);
        }

    }
}
