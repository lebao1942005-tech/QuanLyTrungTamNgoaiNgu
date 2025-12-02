using BLL;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DTO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;

namespace GUI.ViewModels
{
    // DTO phụ dùng để hiển thị lên DataGrid (Join giữa Enrollment và Student)
    public class ClassStudentDisplayDTO
    {
        public int EnrollmentID { get; set; }
        public int StudentID { get; set; }
        public string StudentCode { get; set; } // Mã HV (VD: HV23001)
        public string Name { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Phone { get; set; }
        public string Status { get; set; } // Active/Cancelled
    }

    public partial class ClassStudentListViewModel : ObservableObject
    {
        private readonly EnrollmentBLL _enrollmentBLL = new EnrollmentBLL();

        // Biến lưu lớp đang được chọn (được truyền từ ClassManagementViewModel sang)
        [ObservableProperty]
        private ClassDTO _selectedClass;

        // Khi SelectedClass thay đổi -> Tự động load lại danh sách học viên
        partial void OnSelectedClassChanged(ClassDTO value)
        {
            LoadData();
        }

        // Danh sách hiển thị lên DataGrid
        [ObservableProperty]
        private ObservableCollection<ClassStudentDisplayDTO> _classStudents;

        public ClassStudentListViewModel()
        {
            ClassStudents = new ObservableCollection<ClassStudentDisplayDTO>();
        }

        public void LoadData()
        {
            if (SelectedClass == null)
            {
                if (ClassStudents != null) ClassStudents.Clear();
                return;
            }

            try
            {
                // 1. Lấy tất cả danh sách đăng ký
                var dtEnrollments = _enrollmentBLL.GetAllEnrollments();

                // 2. Lấy tất cả danh sách học viên (để map tên, ngày sinh...)
                var dtStudents = _enrollmentBLL.GetAllStu();

                // 3. Tạo Dictionary cho Student để tra cứu nhanh theo ID
                var studentDict = new Dictionary<int, DataRow>();
                foreach (DataRow row in dtStudents.Rows)
                {
                    int id = Convert.ToInt32(row["StudentID"]);
                    if (!studentDict.ContainsKey(id)) studentDict.Add(id, row);
                }

                // 4. Lọc và Join dữ liệu
                var list = new List<ClassStudentDisplayDTO>();

                foreach (DataRow row in dtEnrollments.Rows)
                {
                    int classId = Convert.ToInt32(row["ClassID"]);

                    // CHỈ LẤY HỌC VIÊN CỦA LỚP ĐANG CHỌN
                    if (classId == SelectedClass.ClassID)
                    {
                        int studentId = Convert.ToInt32(row["StudentID"]);

                        if (studentDict.ContainsKey(studentId))
                        {
                            var stuRow = studentDict[studentId];

                            // Xử lý an toàn cho ngày sinh (phòng trường hợp null)
                            DateTime dob = DateTime.Now;
                            if (stuRow.Table.Columns.Contains("DateOfBirth") && stuRow["DateOfBirth"] != DBNull.Value)
                            {
                                dob = Convert.ToDateTime(stuRow["DateOfBirth"]);
                            }

                            list.Add(new ClassStudentDisplayDTO
                            {
                                EnrollmentID = Convert.ToInt32(row["EnrollmentID"]),
                                StudentID = studentId,
                                StudentCode = studentId.ToString(), // Tạo mã giả lập
                                Name = stuRow["Name"].ToString(),
                                Phone = stuRow["Phone"].ToString(),
                                DateOfBirth = dob,
                                Status = row["Status"].ToString()
                            });
                        }
                    }
                }

                // Cập nhật lên UI
                ClassStudents = new ObservableCollection<ClassStudentDisplayDTO>(list);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách học viên: " + ex.Message);
            }
        }

        // Chức năng Xóa học viên khỏi lớp
        [RelayCommand]
        private void RemoveStudent(ClassStudentDisplayDTO student)
        {
            if (student == null) return;

            var result = MessageBox.Show($"Bạn có chắc muốn xóa học viên {student.Name} khỏi lớp này?",
                "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    string msg = _enrollmentBLL.DeleteEnrollment(student.EnrollmentID);
                    MessageBox.Show(msg);
                    LoadData(); // Load lại danh sách sau khi xóa
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi xóa: " + ex.Message);
                }
            }
        }

        // Chức năng Thêm học viên (Mở Popup - Chưa có View nên để tạm)
        [RelayCommand]
        private void AddStudent()
        {
            MessageBox.Show("Chức năng thêm học viên vào lớp đang được phát triển.");
            // Logic: Mở popup danh sách học viên chưa đăng ký lớp này -> Chọn -> Insert Enrollment
        }
    }
}