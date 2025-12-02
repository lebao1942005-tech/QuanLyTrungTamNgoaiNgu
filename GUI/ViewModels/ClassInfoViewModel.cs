using BLL;
using CommunityToolkit.Mvvm.ComponentModel;
using DTO;

namespace GUI.ViewModels
{
    public partial class ClassInfoViewModel : ObservableObject
    {
        private readonly ClassBLL _classBLL = new ClassBLL(); // Gọi BLL

        // Thuộc tính hiển thị số lượng học viên hiện tại
        [ObservableProperty]
        private int _currentStudentCount = 0;

        [ObservableProperty]
        private ClassDTO _selectedClass;

        // Hàm này tự động chạy khi SelectedClass thay đổi (nhờ CommunityToolkit)
        partial void OnSelectedClassChanged(ClassDTO value)
        {
            if (value != null)
            {
                LoadStudentCount();
            }
        }

        public void LoadStudentCount()
        {
            if (SelectedClass != null)
            {
                // Gọi xuống BLL để lấy số lượng mới nhất
                CurrentStudentCount = _classBLL.GetStudentCount(SelectedClass.ClassID);
            }
        }

        public ClassInfoViewModel()
        {
        }
    }
}