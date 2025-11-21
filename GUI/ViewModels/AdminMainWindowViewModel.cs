using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace GUI.ViewModels
{
    public partial class AdminMainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private object currentViewModel;

        public AdminMainWindowViewModel()
        {
            // Load Student page mặc định
            CurrentViewModel = new StudentManagementViewModel();
        }
    }
}
