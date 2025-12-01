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

        // --- DATA SOURCES ---
        [ObservableProperty] private ObservableCollection<ClassDTO> _classes;
        [ObservableProperty] private ObservableCollection<CourseDTO> _courses;
        [ObservableProperty] private ObservableCollection<TeacherDTO> _teachers;


        // --- SELECTED ITEM ---
        [ObservableProperty]
        private ClassDTO _selectedClass;

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
        // Logic tính ngày kết thúc trên UI để người dùng xem trước (Preview)
        partial void OnNewSelectedCourseChanged(CourseDTO value) => CalculateEndDatePreview();

        [ObservableProperty] private TeacherDTO _newSelectedTeacher;
        [ObservableProperty] private string _newSchedule;
        [ObservableProperty] private int _newMaxStudents = 20;

        [ObservableProperty] private DateTime _newStartDate = DateTime.Now;
        partial void OnNewStartDateChanged(DateTime value) => CalculateEndDatePreview();

        // Biến này chỉ dùng để hiển thị cho người dùng xem trước, không gửi xuống DB
        [ObservableProperty] private DateTime _newEndDate = DateTime.Now.AddMonths(3);

        // --- POPUP DELETE CLASS ---
        [ObservableProperty] private bool _isDeletePopupOpen;
        private ClassDTO _classToDelete;



        [ObservableProperty] private bool _isEditPopupOpen;

        [ObservableProperty] private string _editingClassName;

        // Khi thay đổi Khóa học -> Tự tính lại ngày kết thúc
        [ObservableProperty] private CourseDTO _editingSelectedCourse;
        partial void OnEditingSelectedCourseChanged(CourseDTO value) => CalculateEditEndDate();

        [ObservableProperty] private TeacherDTO _editingSelectedTeacher;
        [ObservableProperty] private string _editingSchedule;
        [ObservableProperty] private int _editingMaxStudents;

        // Khi thay đổi Ngày bắt đầu -> Tự tính lại ngày kết thúc
        [ObservableProperty] private DateTime _editingStartDate;
        partial void OnEditingStartDateChanged(DateTime value) => CalculateEditEndDate();

        [ObservableProperty] private DateTime? _editingEndDate;

        public ClassManagementViewModel()
        {
            CurrentView = InfoVM;
            Application.Current.Dispatcher.InvokeAsync(LoadData);
        }

        private void LoadData()
        {
            try
            {
                var classList = _classBLL.GetAllClasses();
                Classes = new ObservableCollection<ClassDTO>(classList);

                var courseList = _classBLL.GetAllCourses();
                Courses = new ObservableCollection<CourseDTO>(courseList);

                var dtTeachers = _teacherBLL.GetAllTeachers();
                Teachers = new ObservableCollection<TeacherDTO>();
                foreach (DataRow row in dtTeachers.Rows)
                {
                    Teachers.Add(new TeacherDTO
                    {
                        TeacherID = Convert.ToInt32(row["TeacherID"]),
                        Name = row["Name"].ToString()
                    });
                }

                if (Classes.Count > 0 && SelectedClass == null)
                    SelectedClass = Classes[0];
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message); }
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
            NewClassName = "";
            NewSelectedCourse = null;
            NewSelectedTeacher = null;
            NewSchedule = "";
            NewMaxStudents = 20;
            NewStartDate = DateTime.Now;
            CalculateEndDatePreview();
            IsAddPopupOpen = true;
        }

        [RelayCommand]
        private void SaveNewClass()
        {
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
                // QUAN TRỌNG: Gửi null để Trigger SQL tự tính toán chính xác
                EndDate = null
            };

            try
            {
                if (_classBLL.InsertClass(newClass))
                {
                    MessageBox.Show("Tạo lớp học thành công!");
                    IsAddPopupOpen = false;
                    LoadData(); // Load lại để lấy EndDate chuẩn từ SQL
                }
                else MessageBox.Show("Tạo lớp thất bại (Có thể do trùng lịch giáo viên).");
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        [RelayCommand] private void CancelAdd() => IsAddPopupOpen = false;

        // ==========================================================
        //  LOGIC XÓA LỚP HỌC
        // ==========================================================

        [RelayCommand]
        private void OpenDeleteDialog(ClassDTO classInfo)
        {
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
            if (_classToDelete == null) return;

            try
            {
                if (_classBLL.DeleteClass(_classToDelete.ClassID))
                {
                    MessageBox.Show("Đã xóa lớp học thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    Classes.Remove(_classToDelete);
                    if (SelectedClass == _classToDelete) SelectedClass = Classes.FirstOrDefault();
                    IsDeletePopupOpen = false;
                    _classToDelete = null;
                }
                else
                {
                    MessageBox.Show("Xóa thất bại. Lớp học có thể đang có dữ liệu liên quan.", "Lỗi");
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


        private void CalculateEditEndDate()
        {
            if (EditingSelectedCourse != null)
            {
                // Logic: Ngày bắt đầu + Thời lượng khóa học (tháng)
                EditingEndDate = EditingStartDate.AddMonths(EditingSelectedCourse.DurationMonths);
            }
        }

        // --- SỬA LẠI HÀM OPEN EDIT DIALOG (Fix lỗi CS0266) ---
        [RelayCommand]
        private void OpenEditDialog()
        {
            if (SelectedClass == null)
            {
                MessageBox.Show("Vui lòng chọn lớp cần sửa.");
                return;
            }

            // 1. Copy dữ liệu cơ bản
            EditingClassName = SelectedClass.ClassName;
            EditingSchedule = SelectedClass.Schedule;
            EditingMaxStudents = SelectedClass.MaxStudents;

            // 2. FIX LỖI CS0266 Ở ĐÂY:
            // Nếu SelectedClass.StartDate là null thì lấy ngày hiện tại (hoặc xử lý tùy ý)
            EditingStartDate = SelectedClass.StartDate ?? DateTime.Now;

            // 3. Gán ComboBox (Lưu ý: Việc gán này sẽ kích hoạt OnEditingSelectedCourseChanged -> Tự tính EndDate)
            EditingSelectedCourse = Courses.FirstOrDefault(c => c.CourseID == SelectedClass.CourseID);
            EditingSelectedTeacher = Teachers.FirstOrDefault(t => t.TeacherID == SelectedClass.TeacherID);

            // 4. Cập nhật lại EndDate một lần nữa cho chắc chắn đúng logic tính toán
            CalculateEditEndDate();

            IsEditPopupOpen = true;
        }

        // 3. Command Lưu Thay Đổi
        [RelayCommand]
        private void SaveEditClass()
        {
            // Validate dữ liệu
            if (string.IsNullOrWhiteSpace(EditingClassName))
            {
                MessageBox.Show("Tên lớp không được để trống.");
                return;
            }
            if (EditingSelectedCourse == null || EditingSelectedTeacher == null)
            {
                MessageBox.Show("Vui lòng chọn khóa học và giáo viên.");
                return;
            }

            // Tạo đối tượng DTO cập nhật
            // Lưu ý: Giữ nguyên ClassID của lớp đang chọn
            var updatedClass = new ClassDTO
            {
                ClassID = SelectedClass.ClassID,
                ClassName = EditingClassName,
                CourseID = EditingSelectedCourse.CourseID,
                TeacherID = EditingSelectedTeacher.TeacherID,
                Schedule = EditingSchedule,
                MaxStudents = EditingMaxStudents,
                StartDate = EditingStartDate,
                EndDate = EditingEndDate // Hoặc để null nếu muốn SQL tự tính lại
            };

            try
            {
                // Gọi BLL để update (Giả sử bạn đã có hàm UpdateClass trong ClassBLL)
                if (_classBLL.UpdateClass(updatedClass))
                {
                    MessageBox.Show("Cập nhật lớp học thành công!");
                    IsEditPopupOpen = false;
                    LoadData(); // Load lại danh sách để cập nhật giao diện
                }
                else
                {
                    MessageBox.Show("Cập nhật thất bại. (Có thể trùng lịch hoặc lỗi CSDL)");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hệ thống: " + ex.Message);
            }
        }

        // 4. Command Hủy Bỏ
        [RelayCommand]
        private void CancelEdit()
        {
            IsEditPopupOpen = false;
        }


    }
}