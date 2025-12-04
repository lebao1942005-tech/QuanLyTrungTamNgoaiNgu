using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DTO;
using BLL;
using System.Collections.ObjectModel;
using System.Windows;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using GUI.Utilities;

namespace GUI.ViewModels
{
    public partial class ClassManagementViewModel : ObservableObject
    {
        private readonly ClassBLL _classBLL = new ClassBLL();
        private readonly TeacherBLL _teacherBLL = new TeacherBLL();

        // --- PHÂN QUYỀN ---
        public bool IsAdmin => UserSession.IsAdmin;

        // --- DATA SOURCES ---
        [ObservableProperty] private ObservableCollection<ClassDTO> _classes;
        [ObservableProperty] private ObservableCollection<CourseDTO> _courses;

        // Danh sách tất cả giáo viên (Dùng để cache)
        [ObservableProperty] private ObservableCollection<TeacherDTO> _teachers;

        // [MỚI] Danh sách giáo viên phù hợp (Dùng để hiển thị trên ComboBox Thêm lớp)
        [ObservableProperty] private ObservableCollection<TeacherDTO> _availableTeachers;

        // --- SELECTED ITEM ---
        [ObservableProperty] private ClassDTO _selectedClass;

        partial void OnSelectedClassChanged(ClassDTO value)
        {
            if (InfoVM != null) InfoVM.SelectedClass = value;
            if (StudentListVM != null) StudentListVM.SelectedClass = value;
            if (GradingVM != null) GradingVM.SelectedClass = value;
            if (AttendanceVM != null) AttendanceVM.SelectedClass = value;
        }

        // --- TAB VIEW ---
        [ObservableProperty] private object _currentView;
        public ClassInfoViewModel InfoVM { get; } = new ClassInfoViewModel();
        public ClassStudentListViewModel StudentListVM { get; } = new ClassStudentListViewModel();
        public ClassAttendanceViewModel AttendanceVM { get; } = new ClassAttendanceViewModel();
        public ClassGradingViewModel GradingVM { get; } = new ClassGradingViewModel();

        // --- POPUP ADD CLASS ---
        [ObservableProperty] private bool _isAddPopupOpen;
        [ObservableProperty] private string _newClassName;

        [ObservableProperty] private CourseDTO _newSelectedCourse;

        // [CẬP NHẬT] Khi chọn khóa học -> Tính ngày kết thúc VÀ Lọc giáo viên
        partial void OnNewSelectedCourseChanged(CourseDTO value)
        {
            CalculateEndDatePreview();
            FilterTeachersForNewClass(); // <--- Gọi hàm lọc
        }

        [ObservableProperty] private TeacherDTO _newSelectedTeacher;
        [ObservableProperty] private string _newSchedule;
        [ObservableProperty] private int _newMaxStudents = 20;
        [ObservableProperty] private DateTime _newStartDate = DateTime.Now;
        partial void OnNewStartDateChanged(DateTime value) => CalculateEndDatePreview();
        [ObservableProperty] private DateTime _newEndDate = DateTime.Now.AddMonths(3);

        // --- POPUP DELETE & EDIT (Giữ nguyên) ---
        [ObservableProperty] private bool _isDeletePopupOpen;
        private ClassDTO _classToDelete;

        [ObservableProperty] private bool _isEditPopupOpen;
        [ObservableProperty] private string _editingClassName;
        [ObservableProperty] private CourseDTO _editingSelectedCourse;
        partial void OnEditingSelectedCourseChanged(CourseDTO value)
        {
            CalculateEditEndDate();
            FilterTeachersForEdit(); // <--- THÊM DÒNG NÀY
        }

        [ObservableProperty] private TeacherDTO _editingSelectedTeacher;
        [ObservableProperty] private string _editingSchedule;
        [ObservableProperty] private int _editingMaxStudents;
        [ObservableProperty] private DateTime _editingStartDate;
        partial void OnEditingStartDateChanged(DateTime value) => CalculateEditEndDate();
        [ObservableProperty] private DateTime? _editingEndDate;



        public List<string> PresetSchedules { get; } = new List<string>
        {
            "2-4-6 (17h30 - 19h00)",
            "2-4-6 (19h30 - 21h00)",
            "3-5-7 (17h30 - 19h00)",
            "3-5-7 (19h30 - 21h00)"
        };

        /*
        private void FilterTeachersForEdit()
        {
            // Lưu lại giáo viên đang được chọn (để lát nữa gán lại nếu họ vẫn nằm trong danh sách phù hợp)
            var currentTeacherId = EditingSelectedTeacher?.TeacherID;

            if (EditingSelectedCourse == null || Teachers == null)
            {
                AvailableTeachers.Clear();
                return;
            }

            // Lọc: Chỉ lấy giáo viên có môn dạy == Tên khóa học đang sửa
            var filtered = Teachers.Where(t => t.Subject == EditingSelectedCourse.CourseName).ToList();

            // Cập nhật danh sách hiển thị
            AvailableTeachers = new ObservableCollection<TeacherDTO>(filtered);

            // Kiểm tra xem giáo viên cũ có nằm trong danh sách mới lọc không
            var stillValidTeacher = AvailableTeachers.FirstOrDefault(t => t.TeacherID == currentTeacherId);

            if (stillValidTeacher != null)
            {
                // Nếu có, giữ nguyên lựa chọn
                EditingSelectedTeacher = stillValidTeacher;
            }
            else
            {
                // Nếu giáo viên cũ không dạy môn này nữa (hoặc đổi khóa học khác), reset lựa chọn
                EditingSelectedTeacher = null;
            }
        }
        */



        private void FilterTeachersForEdit()
        {
            var currentTeacherId = EditingSelectedTeacher?.TeacherID;

            if (EditingSelectedCourse == null || Teachers == null)
            {
                AvailableTeachers.Clear();
                return;
            }

            // [SỬA ĐOẠN NÀY TƯƠNG TỰ]
            string targetSubject = EditingSelectedCourse.CourseName.Trim();

            var filtered = Teachers.Where(t =>
            {
                if (string.IsNullOrEmpty(t.Subject)) return false;

                var teacherSubjects = t.Subject.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                               .Select(s => s.Trim());

                return teacherSubjects.Contains(targetSubject, StringComparer.OrdinalIgnoreCase);
            }).ToList();

            // Cập nhật danh sách hiển thị
            AvailableTeachers = new ObservableCollection<TeacherDTO>(filtered);

            // Kiểm tra xem giáo viên cũ có nằm trong danh sách mới lọc không
            var stillValidTeacher = AvailableTeachers.FirstOrDefault(t => t.TeacherID == currentTeacherId);

            if (stillValidTeacher != null)
            {
                EditingSelectedTeacher = stillValidTeacher;
            }
            else
            {
                EditingSelectedTeacher = null;
            }
        }



        public ClassManagementViewModel()
        {
            CurrentView = InfoVM;
            AvailableTeachers = new ObservableCollection<TeacherDTO>(); // Khởi tạo list lọc
            Application.Current.Dispatcher.InvokeAsync(LoadData);
        }

        private void LoadData()
        {
            try
            {
                var allClassList = _classBLL.GetAllClasses();

                // Logic phân quyền load danh sách
                if (UserSession.IsAdmin)
                {
                    Classes = new ObservableCollection<ClassDTO>(allClassList);
                }
                else
                {
                    var teacherClasses = allClassList.Where(c => c.TeacherID == UserSession.CurrentTeacherID).ToList();
                    Classes = new ObservableCollection<ClassDTO>(teacherClasses);
                }

                var courseList = _classBLL.GetAllCourses();
                Courses = new ObservableCollection<CourseDTO>(courseList);

                var dtTeachers = _teacherBLL.GetAllTeachers();
                Teachers = new ObservableCollection<TeacherDTO>();
                foreach (DataRow row in dtTeachers.Rows)
                {
                    // [QUAN TRỌNG] Cần lấy thêm cột Subject để lọc
                    Teachers.Add(new TeacherDTO
                    {
                        TeacherID = Convert.ToInt32(row["TeacherID"]),
                        Name = row["Name"].ToString(),
                        Subject = row["Subject"] != DBNull.Value ? row["Subject"].ToString() : ""
                    });
                }

                if (Classes.Count > 0 && SelectedClass == null)
                    SelectedClass = Classes[0];
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message); }
        }

        // [MỚI] Hàm lọc giáo viên theo môn học
        /*
        private void FilterTeachersForNewClass()
        {
            NewSelectedTeacher = null; // Reset lựa chọn cũ

            if (NewSelectedCourse == null || Teachers == null)
            {
                AvailableTeachers.Clear();
                return;
            }

            // Logic lọc: Môn dạy của GV == Tên khóa học
            var filtered = Teachers.Where(t => t.Subject == NewSelectedCourse.CourseName).ToList();

            AvailableTeachers = new ObservableCollection<TeacherDTO>(filtered);

            // UX: Nếu chỉ có 1 người dạy môn này, tự chọn luôn
            if (AvailableTeachers.Count == 1)
            {
                NewSelectedTeacher = AvailableTeachers[0];
            }
        }
        */


        private void FilterTeachersForNewClass()
        {
            NewSelectedTeacher = null;

            if (NewSelectedCourse == null || Teachers == null)
            {
                AvailableTeachers.Clear();
                return;
            }

            // [SỬA ĐOẠN NÀY]
            // Logic cũ: t.Subject == CourseName (Sai nếu GV dạy nhiều môn)
            // Logic mới: Cắt chuỗi theo dấu phẩy, rồi tìm xem có chứa CourseName không

            string targetSubject = NewSelectedCourse.CourseName.Trim();

            var filtered = Teachers.Where(t =>
            {
                if (string.IsNullOrEmpty(t.Subject)) return false;

                // 1. Tách chuỗi "IELTS, TOEIC" -> mảng ["IELTS", " TOEIC"]
                var teacherSubjects = t.Subject.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                               .Select(s => s.Trim()); // Xóa khoảng trắng thừa

                // 2. Kiểm tra xem trong mảng có chứa môn cần tìm không (So sánh không phân biệt hoa thường)
                return teacherSubjects.Contains(targetSubject, StringComparer.OrdinalIgnoreCase);
            }).ToList();

            AvailableTeachers = new ObservableCollection<TeacherDTO>(filtered);

            if (AvailableTeachers.Count == 1)
            {
                NewSelectedTeacher = AvailableTeachers[0];
            }
        }


        private void CalculateEndDatePreview()
        {
            if (NewSelectedCourse != null)
                NewEndDate = NewStartDate.AddMonths(NewSelectedCourse.DurationMonths);
        }

        [RelayCommand]
        private void SwitchTab(string tabName)
        {
            switch (tabName)
            {
                case "Info": CurrentView = InfoVM; break;
                case "Students": CurrentView = StudentListVM; break;
                case "Attendance": CurrentView = AttendanceVM; break;
                case "Grading": CurrentView = GradingVM; break;
            }
        }

        // ==========================================================
        //  LOGIC THÊM LỚP HỌC
        // ==========================================================

        [RelayCommand]
        private void OpenAddDialog()
        {
            if (!IsAdmin)
            {
                MessageBox.Show("Bạn không có quyền thực hiện chức năng này.");
                return;
            }

            NewClassName = "";
            NewSelectedCourse = null;
            NewSelectedTeacher = null;

            // Xóa danh sách lọc cũ để người dùng buộc phải chọn Khóa học trước
            AvailableTeachers.Clear();

            NewSchedule = "";
            NewMaxStudents = 20;
            NewStartDate = DateTime.Now;
            CalculateEndDatePreview();
            IsAddPopupOpen = true;
        }

        [RelayCommand]
        private void SaveNewClass()
        {
            if (!IsAdmin) return;

            if (string.IsNullOrWhiteSpace(NewClassName)) { MessageBox.Show("Vui lòng nhập tên lớp học."); return; }
            if (NewSelectedCourse == null || NewSelectedTeacher == null) { MessageBox.Show("Vui lòng chọn khóa học và giáo viên."); return; }

            var newClass = new ClassDTO
            {
                ClassName = NewClassName,
                CourseID = NewSelectedCourse.CourseID,
                TeacherID = NewSelectedTeacher.TeacherID,
                Schedule = NewSchedule,
                MaxStudents = NewMaxStudents,
                StartDate = NewStartDate,
                EndDate = null
            };

            try
            {
                if (_classBLL.InsertClass(newClass))
                {
                    MessageBox.Show("Tạo lớp học thành công!");
                    IsAddPopupOpen = false;
                    LoadData();
                }
                else MessageBox.Show("Tạo lớp thất bại (Có thể do trùng lịch giáo viên).");
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        [RelayCommand] private void CancelAdd() => IsAddPopupOpen = false;

        // ==========================================================
        //  LOGIC XÓA LỚP HỌC (Giữ nguyên)
        // ==========================================================

        [RelayCommand]
        private void OpenDeleteDialog(ClassDTO classInfo)
        {
            if (!IsAdmin) { MessageBox.Show("Bạn không có quyền xóa lớp."); return; }

            if (classInfo == null)
            {
                if (SelectedClass != null) classInfo = SelectedClass;
                else { MessageBox.Show("Vui lòng chọn lớp cần xóa."); return; }
            }

            _classToDelete = classInfo;
            IsDeletePopupOpen = true;
        }

        [RelayCommand]
        private void ConfirmDeleteClass()
        {
            if (!IsAdmin || _classToDelete == null) return;
            try
            {
                if (_classBLL.DeleteClass(_classToDelete.ClassID))
                {
                    MessageBox.Show("Đã xóa lớp học thành công!");
                    Classes.Remove(_classToDelete);
                    if (SelectedClass == _classToDelete) SelectedClass = Classes.FirstOrDefault();
                    IsDeletePopupOpen = false;
                    _classToDelete = null;
                }
                else
                {
                    MessageBox.Show("Xóa thất bại. Lớp học có thể đang có dữ liệu liên quan.");
                    IsDeletePopupOpen = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hệ thống: " + ex.Message);
                IsDeletePopupOpen = false;
            }
        }

        [RelayCommand]
        private void CancelDelete()
        {
            IsDeletePopupOpen = false;
            _classToDelete = null;
        }

        // ==========================================================
        //  LOGIC SỬA LỚP HỌC
        // ==========================================================

        private void CalculateEditEndDate()
        {
            if (EditingSelectedCourse != null)
                EditingEndDate = EditingStartDate.AddMonths(EditingSelectedCourse.DurationMonths);
        }

        [RelayCommand]
        private void OpenEditDialog()
        {
            if (!IsAdmin) { MessageBox.Show("Bạn không có quyền sửa lớp."); return; }
            if (SelectedClass == null) { MessageBox.Show("Vui lòng chọn lớp cần sửa."); return; }

            EditingClassName = SelectedClass.ClassName;
            EditingSchedule = SelectedClass.Schedule;
            EditingMaxStudents = SelectedClass.MaxStudents;
            EditingStartDate = SelectedClass.StartDate ?? DateTime.Now;

            // 1. Gán khóa học
            EditingSelectedCourse = Courses.FirstOrDefault(c => c.CourseID == SelectedClass.CourseID);

            // 2. Gán giáo viên tạm thời (để lấy ID)
            EditingSelectedTeacher = Teachers.FirstOrDefault(t => t.TeacherID == SelectedClass.TeacherID);

            // 3. [QUAN TRỌNG] Gọi hàm lọc để nạp dữ liệu vào AvailableTeachers
            FilterTeachersForEdit();

            CalculateEditEndDate();
            IsEditPopupOpen = true;
        }

        [RelayCommand]
        private void SaveEditClass()
        {
            if (!IsAdmin) return;
            if (string.IsNullOrWhiteSpace(EditingClassName)) { MessageBox.Show("Tên lớp không được để trống."); return; }
            if (EditingSelectedCourse == null || EditingSelectedTeacher == null) { MessageBox.Show("Vui lòng chọn khóa học và giáo viên."); return; }

            var updatedClass = new ClassDTO
            {
                ClassID = SelectedClass.ClassID,
                ClassName = EditingClassName,
                CourseID = EditingSelectedCourse.CourseID,
                TeacherID = EditingSelectedTeacher.TeacherID,
                Schedule = EditingSchedule,
                MaxStudents = EditingMaxStudents,
                StartDate = EditingStartDate,
                EndDate = EditingEndDate
            };

            try
            {
                if (_classBLL.UpdateClass(updatedClass))
                {
                    MessageBox.Show("Cập nhật lớp học thành công!");
                    IsEditPopupOpen = false;
                    LoadData();
                }
                else MessageBox.Show("Cập nhật thất bại. (Có thể trùng lịch hoặc lỗi CSDL)");
            }
            catch (Exception ex) { MessageBox.Show("Lỗi hệ thống: " + ex.Message); }
        }

        [RelayCommand] private void CancelEdit() => IsEditPopupOpen = false;
    }
}