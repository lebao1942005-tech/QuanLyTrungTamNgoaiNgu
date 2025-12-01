using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace GUI.ViewModels
{
    public partial class AdminMainWindowViewModel : ObservableObject
    {
        // Thuộc tính chứa ViewModel hiện tại (View đang hiển thị giữa màn hình)
        [ObservableProperty]
        private object _currentViewModel;

        // --- QUẢN LÝ TRẠNG THÁI SIDEBAR ---

        // 1. Biến lưu "Key" của trang đang đứng (Ví dụ: "StudentManagement")
        // Khi biến này thay đổi, nó tự động báo cho các biến bool bên dưới cập nhật theo
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsStudentPage))]
        [NotifyPropertyChangedFor(nameof(IsTeacherPage))]
        [NotifyPropertyChangedFor(nameof(IsClassPage))]
        [NotifyPropertyChangedFor(nameof(IsRegistrationPage))]
        [NotifyPropertyChangedFor(nameof(IsCoursePage))]
        [NotifyPropertyChangedFor(nameof(IsTuitionPage))]
        private string _currentPage;

        // 2. Các biến Boolean để Binding vào RadioButton (IsChecked)
        // Logic: Nếu CurrentPage đúng là trang này -> Trả về True -> RadioButton sáng đèn
        public bool IsStudentPage
        {
            get => CurrentPage == "StudentManagement";
            set { if (value) Navigate("StudentManagement"); }
        }

        public bool IsTeacherPage
        {
            get => CurrentPage == "TeacherManagement";
            set { if (value) Navigate("TeacherManagement"); }
        }

        public bool IsClassPage
        {
            get => CurrentPage == "ClassManagement";
            set { if (value) Navigate("ClassManagement"); }
        }

        public bool IsRegistrationPage
        {
            get => CurrentPage == "RegistrationManagement";
            set { if (value) Navigate("RegistrationManagement"); }
        }

        public bool IsCoursePage
        {
            get => CurrentPage == "CourseManagement";
            set { if (value) Navigate("CourseManagement"); }
        }

        public bool IsTuitionPage
        {
            get => CurrentPage == "TuitionManagement";
            set { if (value) Navigate("TuitionManagement"); }
        }

        public AdminMainWindowViewModel()
        {
            // Mặc định vào trang Học viên
            // Gọi hàm Navigate để nó set cả CurrentViewModel lẫn CurrentPage
            Navigate("StudentManagement");
        }

        // Command điều hướng
        [RelayCommand]
        private void Navigate(string pageKey)
        {
            if (string.IsNullOrWhiteSpace(pageKey)) return;

            // Cập nhật trạng thái trang hiện tại (để Sidebar đồng bộ)
            if (CurrentPage != pageKey)
            {
                CurrentPage = pageKey;
            }

            // Cập nhật View tương ứng
            switch (pageKey)
            {
                case "Overview":
                    MessageBox.Show("Trang Dashboard đang phát triển và sẽ có trong phiên bản sau.");
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

        // Command Logout nếu có
        [RelayCommand]
        private void Logout()
        {
            System.Windows.MessageBox.Show("Đăng xuất thành công!");
            // Tùy bạn mở LoginWindow,...
        }
    }
}