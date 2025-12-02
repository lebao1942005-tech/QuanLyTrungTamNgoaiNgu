using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GUI.Utilities;
using GUI.Views.Windows;
using System.Windows;

namespace GUI.ViewModels
{
    public partial class TeacherMainWindowViewModel : ObservableObject
    {
        // View đang hiển thị ở phần nội dung chính
        [ObservableProperty]
        private object _currentViewModel;

        // --- 1. HEADER DATA (Thêm cái này để Header hiện tên) ---
        public string CurrentUserName => UserSession.CurrentUsername;

        // Lưu tên trang hiện tại để xử lý logic menu
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsClassPage))]
        private string _currentPage;

        // --- 2. QUẢN LÝ TRẠNG THÁI SIDEBAR ---
        [ObservableProperty]
        private bool _isSidebarOpen = true;

        // Command cho nút 3 gạch ở Header
        [RelayCommand]
        private void ToggleSidebar()
        {
            IsSidebarOpen = !IsSidebarOpen;
        }

        // --- BINDING CHO MENU ---
        public bool IsClassPage
        {
            get => CurrentPage == "ClassManagement";
            set { if (value) Navigate("ClassManagement"); }
        }

        public TeacherMainWindowViewModel()
        {
            Navigate("ClassManagement");
        }

        // --- HÀM CHUYỂN TRANG ---
        [RelayCommand]
        private void Navigate(string pageKey)
        {
            if (CurrentPage == pageKey) return;

            CurrentPage = pageKey;

            switch (pageKey)
            {
                case "ClassManagement":
                    CurrentViewModel = new ClassManagementViewModel();
                    break;
            }
        }

        // --- HÀM ĐĂNG XUẤT ---
        [RelayCommand]
        private void Logout(Window currentWindow)
        {
            if (MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                // 1. Xóa thông tin phiên làm việc
                UserSession.ClearSession();

                // 2. Mở lại màn hình đăng nhập (Sửa Login -> LoginWindow)
                var loginScreen = new Login();
                loginScreen.Show();

                // 3. Đóng cửa sổ hiện tại
                currentWindow?.Close();
            }
        }
    }
}