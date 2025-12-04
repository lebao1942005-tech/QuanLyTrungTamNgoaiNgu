using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GUI.Utilities;      // Để dùng UserSession
using GUI.Views.Windows;  // Để mở LoginWindow
using System.Windows;

namespace GUI.ViewModels
{
    public partial class AdminMainWindowViewModel : ObservableObject
    {
        // Thuộc tính chứa ViewModel hiện tại (View đang hiển thị giữa màn hình)
        [ObservableProperty]
        private object _currentViewModel;

        // --- 1. HEADER & SIDEBAR LOGIC (Thêm mới) ---

        // Lấy tên hiển thị từ Session để HeaderView binding vào
        public string CurrentUserName => UserSession.CurrentUsername;

        // Quản lý trạng thái mở/đóng của Sidebar (cho nút Hamburger ở Header)
        [ObservableProperty]
        private bool _isSidebarOpen = true;

        [RelayCommand]
        private void ToggleSidebar()
        {
            IsSidebarOpen = !IsSidebarOpen;
        }

        // --- 2. QUẢN LÝ TRẠNG THÁI TRANG (Navigation) ---

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsDashboardPage))]
        [NotifyPropertyChangedFor(nameof(IsStudentPage))]
        [NotifyPropertyChangedFor(nameof(IsTeacherPage))]
        [NotifyPropertyChangedFor(nameof(IsClassPage))]
        [NotifyPropertyChangedFor(nameof(IsRegistrationPage))]
        [NotifyPropertyChangedFor(nameof(IsCoursePage))]
        [NotifyPropertyChangedFor(nameof(IsTuitionPage))]
        private string _currentPage;

        // Các biến Boolean để Binding vào RadioButton (IsChecked)
        // [MỚI] Thuộc tính này dùng để binding với nút Dashboard trên Sidebar
        public bool IsDashboardPage { get => CurrentPage == "Dashboard"; set { if (value) Navigate("Dashboard"); } }
        public bool IsStudentPage { get => CurrentPage == "StudentManagement"; set { if (value) Navigate("StudentManagement"); } }
        public bool IsTeacherPage { get => CurrentPage == "TeacherManagement"; set { if (value) Navigate("TeacherManagement"); } }
        public bool IsClassPage { get => CurrentPage == "ClassManagement"; set { if (value) Navigate("ClassManagement"); } }
        public bool IsRegistrationPage { get => CurrentPage == "RegistrationManagement"; set { if (value) Navigate("RegistrationManagement"); } }
        public bool IsCoursePage { get => CurrentPage == "CourseManagement"; set { if (value) Navigate("CourseManagement"); } }
        public bool IsTuitionPage { get => CurrentPage == "TuitionManagement"; set { if (value) Navigate("TuitionManagement"); } }

        public AdminMainWindowViewModel()
        {
            // Mặc định vào trang Học viên khi mở app
            // Navigate("StudentManagement");
            Navigate("Dashboard");
        }

        // --- 3. HÀM ĐIỀU HƯỚNG ---
        [RelayCommand]
        private void Navigate(string pageKey)
        {
            if (string.IsNullOrWhiteSpace(pageKey)) return;
            if (CurrentPage == pageKey) return; // Tránh load lại nếu đang ở trang đó

            CurrentPage = pageKey;

            switch (pageKey)
            {
                case "Dashboard":
                    CurrentViewModel = new DashboardViewModel();
                    break;

                case "StudentManagement":
                    CurrentViewModel = new StudentManagementViewModel();
                    break;

                case "TeacherManagement":
                    CurrentViewModel = new TeacherManagementViewModel();
                    break;

                case "ClassManagement":
                    CurrentViewModel = new ClassManagementViewModel();
                    break;

                case "RegistrationManagement":
                    CurrentViewModel = new RegistrationManagementViewModel();
                    break;

                case "CourseManagement":
                    CurrentViewModel = new CourseManagementViewModel();
                    break;

                case "TuitionManagement":
                    CurrentViewModel = new TuitionViewModel();
                    break;
            }
        }

        // --- 4. HÀM ĐĂNG XUẤT (Cập nhật) ---
        // Nhận tham số là Window để đóng cửa sổ hiện tại
        [RelayCommand]
        private void Logout(Window currentWindow)
        {
            if (MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                // 1. Xóa thông tin phiên làm việc
                UserSession.ClearSession();

                // 2. Mở lại màn hình đăng nhập
                var loginScreen = new Login();
                loginScreen.Show();

                // 3. Đóng cửa sổ Admin hiện tại
                currentWindow?.Close();
            }
        }
    }
}