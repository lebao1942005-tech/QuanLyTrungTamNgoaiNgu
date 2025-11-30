using CommunityToolkit.Mvvm.ComponentModel;
using DTO;

namespace GUI.ViewModels
{
    public partial class ClassInfoViewModel : ObservableObject
    {
        // Thuộc tính này sẽ nhận dữ liệu khi người dùng chọn dòng bên danh sách
        [ObservableProperty]
        private ClassDTO _selectedClass;

        public ClassInfoViewModel()
        {
            // Không cần khởi tạo dữ liệu giả ở đây nữa
        }
    }
}