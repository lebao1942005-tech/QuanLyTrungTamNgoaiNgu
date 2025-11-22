using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;

namespace GUI.ViewModels
{
    public partial class StudentManagementViewModel : ObservableObject
    {
       // private readonly StudentService _service;
       /*
        [ObservableProperty]
        private ObservableCollection<Student> students;
       */
        // ====== Thuộc tính dùng cho popup thêm học viên ======
        [ObservableProperty] private bool _isAddStudentPopupVisible;
        [ObservableProperty] private string _newStudentName;
        [ObservableProperty] private string _newStudentPhone;
        [ObservableProperty] private string _newStudentEmail;
        [ObservableProperty] private string _newStudentAddress;
        [ObservableProperty] private string _newStudentNotes;

        public StudentManagementViewModel()
        {
          //  _service = new StudentService();
           // Students = new ObservableCollection<Student>(_service.GetAllStudents());
        }

        private void LoadStudents()
        {
           // Students = new ObservableCollection<Student>(_service.GetAllStudents());
        }

        // ====== Command mở popup thêm học viên ======
        [RelayCommand]
        private void AddStudent()
        {
            NewStudentName = string.Empty;
            NewStudentPhone = string.Empty;
            NewStudentEmail = string.Empty;
            NewStudentAddress = string.Empty;
            NewStudentNotes = string.Empty;

            IsAddStudentPopupVisible = true;
        }

        // ====== Command lưu học viên mới ======
        /*
        [RelayCommand]
        private void SaveNewStudent()
        {
            if (string.IsNullOrWhiteSpace(NewStudentName))
            {
                MessageBox.Show("Vui lòng nhập tên học viên.");
                return;
            }

            try
            {
                bool result = _service.AddStudent(
                    NewStudentName,
                    NewStudentPhone,
                    NewStudentEmail,
                    NewStudentAddress,
                    NewStudentNotes
                );

                if (result)
                {
                    MessageBox.Show("Thêm học viên thành công!");
                    LoadStudents();
                    IsAddStudentPopupVisible = false;
                }
                else
                {
                    MessageBox.Show("Thêm học viên thất bại!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}");
            }
        }
        */

        // ====== Command đóng popup ======
        [RelayCommand]
        private void CancelAddStudent()
        {
            IsAddStudentPopupVisible = false;
        }

        // ====== Command xem chi tiết ======
        /*
        [RelayCommand]
        private void ViewStudent(Student student)
        {
            if (student == null) return;
            MessageBox.Show($"Xem chi tiết học viên: {student.Name}");
        }
        */

        // ====== Command chỉnh sửa ======
        /*
        [RelayCommand]
        private void EditStudent(Student student)
        {
            if (student == null) return;
            MessageBox.Show($"Chỉnh sửa học viên: {student.Name}");
        }
        */

        // ====== Command xóa ======
        /*
        [RelayCommand]
        private void DeleteStudent(Student student)
        {
            if (student == null) return;

            var result = MessageBox.Show(
                $"Bạn có chắc muốn xóa học viên {student.Name}?",
                "Xác nhận",
                MessageBoxButton.YesNo
            );

            if (result == MessageBoxResult.Yes)
            {
                if (_service.DeleteStudent(student.Id))
                {
                    Students.Remove(student);
                    MessageBox.Show("Xóa thành công!");
                }
                else
                {
                    MessageBox.Show("Xóa thất bại!");
                }
            }
        }
        */
    }
}
