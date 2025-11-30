using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace GUI.ViewModels
{
    public partial class RegistrationManagementViewModel : ObservableObject
    {
        // 1. Khởi tạo các ViewModel con
        public NewRegistrationViewModel NewRegVM { get; } = new NewRegistrationViewModel();

        // GIỮ NGUYÊN: Dùng ContractListViewModel
        public ContractListViewModel ContractListVM { get; } = new ContractListViewModel();

        [ObservableProperty]
        private object _currentView;

        public RegistrationManagementViewModel()
        {
            // Mặc định vào tab Đăng ký mới
            CurrentView = NewRegVM;
        }

        [RelayCommand]
        private void SwitchTab(string viewName)
        {
            if (viewName == "NewRegistration")
            {
                CurrentView = NewRegVM;
            }
            // GIỮ NGUYÊN: Kiểm tra chuỗi "ContractList"
            else if (viewName == "ContractList")
            {
                CurrentView = ContractListVM;
            }
        }
    }
}