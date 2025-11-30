using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace GUI.ViewModels
{
    public partial class AdminMainWindowViewModel : ObservableObject
    {
        // Thuộc tính chứa ViewModel hiện tại
        [ObservableProperty]
        private object currentViewModel;

        public AdminMainWindowViewModel()
        {
            // Mặc định vào Dashboard hoặc Student tùy bạn
            CurrentViewModel = new StudentManagementViewModel();
        }

        // Command điều hướng
        [RelayCommand]
        private void Navigate(string pageKey)
        {
            if (string.IsNullOrWhiteSpace(pageKey))
                return;

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
