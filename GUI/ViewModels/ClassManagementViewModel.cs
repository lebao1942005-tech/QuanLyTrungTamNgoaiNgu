using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace GUI.ViewModels
{
    public partial class ClassManagementViewModel : ObservableObject
    {
        // Thuộc tính lưu View hiện tại đang hiển thị
        [ObservableProperty]
        private object _currentView;

        // Khai báo các ViewModel con cho từng Tab (Giữ trạng thái)
        // Bạn có thể tách các class này ra file riêng nếu muốn
        public ClassInfoViewModel InfoVM { get; } = new ClassInfoViewModel();
        public ClassStudentListViewModel StudentListVM { get; } = new ClassStudentListViewModel();
        public ClassAttendanceViewModel AttendanceVM { get; } = new ClassAttendanceViewModel();
        public ClassGradingViewModel GradingVM { get; } = new ClassGradingViewModel();

        public ClassManagementViewModel()
        {
            // Mặc định hiển thị Tab Thông tin khi khởi tạo
            CurrentView = InfoVM;
        }

        // Command để chuyển tab
        [RelayCommand]
        private void SwitchTab(string tabName)
        {
            switch (tabName)
            {
                case "Info":
                    CurrentView = InfoVM;
                    break;
                case "Students":
                    CurrentView = StudentListVM;
                    break;
                case "Attendance":
                    CurrentView = AttendanceVM;
                    break;
                case "Grading":
                    CurrentView = GradingVM;
                    break;
            }
        }
    }

    // --- Các ViewModel con (Placeholder) ---
    // Trong thực tế, bạn nên tạo file riêng cho từng class này trong thư mục ViewModels
    
    public class ClassAttendanceViewModel : ObservableObject { }
    public class ClassGradingViewModel : ObservableObject { }
}