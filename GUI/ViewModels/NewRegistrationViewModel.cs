using BLL;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DAL;
using DTO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;

namespace GUI.ViewModels
{
    public partial class NewRegistrationViewModel : ObservableObject
    {
        private readonly EnrollmentBLL _bll = new EnrollmentBLL();

        // Danh sách gốc (dùng để cache dữ liệu cho tìm kiếm)
        private List<StudentDTO> _allStudents = new List<StudentDTO>();
        private List<ClassDTO> _allClasses = new List<ClassDTO>();

        // --- DANH SÁCH HIỂN THỊ (BINDING) ---
        [ObservableProperty]
        private ObservableCollection<StudentDTO> _students;

        [ObservableProperty]
        private ObservableCollection<ClassDTO> _classes;

        // --- SELECTION ---
        [ObservableProperty]
        private StudentDTO _selectedStudent;

        [ObservableProperty]
        private ClassDTO _selectedClass;

        // --- TÌM KIẾM ---
        [ObservableProperty]
        private string _studentSearchText;

        // Tự động filter khi text thay đổi
        partial void OnStudentSearchTextChanged(string value) => FilterStudents();

        [ObservableProperty]
        private string _classSearchText;

        partial void OnClassSearchTextChanged(string value) => FilterClasses();


        public NewRegistrationViewModel()
        {
            LoadData();
        }

        public void LoadData()
        {
            try
            {
                // 1. Load Học Viên
                var dtStu = _bll.GetAllStu();
                _allStudents.Clear();
                foreach (DataRow row in dtStu.Rows)
                {
                    _allStudents.Add(new StudentDTO
                    {
                        StudentID = Convert.ToInt32(row["StudentID"]),
                        Name = row["Name"].ToString(),
                        Phone = row["Phone"].ToString(),
                        Email = row["Email"].ToString()
                        // Map thêm các trường khác nếu cần
                    });
                }
                FilterStudents(); // Hiển thị ra UI

                // 2. Load Lớp Học
                var dtClass = _bll.GetAllClass();
                _allClasses.Clear();
                foreach (DataRow row in dtClass.Rows)
                {
                    _allClasses.Add(new ClassDTO
                    {
                        ClassID = Convert.ToInt32(row["ClassID"]),
                        ClassName = row["ClassName"].ToString(),
                        Schedule = row["Schedule"].ToString(),
                        MaxStudents = Convert.ToInt32(row["MaxStudents"])
                        // Map thêm CourseID, TeacherID nếu cần
                    });
                }
                FilterClasses(); // Hiển thị ra UI
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        private void FilterStudents()
        {
            if (string.IsNullOrWhiteSpace(StudentSearchText))
            {
                Students = new ObservableCollection<StudentDTO>(_allStudents);
            }
            else
            {
                var keyword = StudentSearchText.ToLower();
                var filtered = _allStudents.Where(s =>
                    s.Name.ToLower().Contains(keyword) ||
                    s.Phone.Contains(keyword) ||
                    s.Email.ToLower().Contains(keyword) ||
                    s.StudentID.ToString().Contains(keyword)
                );
                Students = new ObservableCollection<StudentDTO>(filtered);
            }
        }

        private void FilterClasses()
        {
            if (string.IsNullOrWhiteSpace(ClassSearchText))
            {
                Classes = new ObservableCollection<ClassDTO>(_allClasses);
            }
            else
            {
                var keyword = ClassSearchText.ToLower();
                var filtered = _allClasses.Where(c =>
                    c.ClassName.ToLower().Contains(keyword) ||
                    c.Schedule.ToLower().Contains(keyword)
                );
                Classes = new ObservableCollection<ClassDTO>(filtered);
            }
        }

        // --- COMMAND ĐĂNG KÝ ---
        [RelayCommand]
        private void Register()
        {
            // Validate
            if (SelectedStudent == null)
            {
                MessageBox.Show("Vui lòng chọn học viên!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (SelectedClass == null)
            {
                MessageBox.Show("Vui lòng chọn lớp học!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Tạo DTO
            var enrollment = new EnrollmentDTO
            {
                StudentID = SelectedStudent.StudentID,
                ClassID = SelectedClass.ClassID,
                EnrollDate = DateTime.Now,
                Status = "Active" // Mặc định trạng thái Active
            };

            // Gọi BLL
            try
            {
                string result = _bll.InsertEnrollment(enrollment);
                MessageBox.Show(result, "Thông báo");

                // Nếu thành công thì có thể clear selection hoặc chuyển tab
                if (result.Contains("thành công"))
                {
                    SelectedClass = null;
                    SelectedStudent = null;
                    // Reset search text nếu muốn
                    StudentSearchText = "";
                    ClassSearchText = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hệ thống: " + ex.Message);
            }
        }
    }
}