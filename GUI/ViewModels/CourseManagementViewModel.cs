using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DTO;
using BLL;
using System.Collections.ObjectModel;
using System.Windows;
using System;

namespace GUI.ViewModels
{
    public partial class CourseManagementViewModel : ObservableObject
    {
        private readonly CourseBLL _courseBLL = new CourseBLL();

        // 1. DANH SÁCH KHÓA HỌC (Hiển thị lên DataGrid)
        [ObservableProperty]
        private ObservableCollection<CourseDTO> _courses;

        // 2. BIẾN ĐIỀU KHIỂN POPUP (True = Hiện, False = Ẩn)
        [ObservableProperty]
        private bool _isAddPopupOpen;

        // 3. CÁC BIẾN NHẬP LIỆU (Binding vào các TextBox trong Popup Add)
        [ObservableProperty] private string _newCourseName;
        [ObservableProperty] private int _newDurationMonths;
        [ObservableProperty] private decimal _newBaseFee;


        [ObservableProperty] private bool _isDeletePopupOpen;
        private CourseDTO _courseToDelete;


        [ObservableProperty] private bool _isEditPopupOpen;

        // Các biến này Binding với TextBox trong EditCourseView
        [ObservableProperty] private int _editingCourseId; // ID để biết đang sửa ai
        [ObservableProperty] private string _editingCourseName;
        [ObservableProperty] private int _editingDurationMonths;
        [ObservableProperty] private decimal _editingBaseFee;

        public CourseManagementViewModel()
        {
            LoadData();
        }

        // Hàm tải dữ liệu
        private void LoadData()
        {
            var list = _courseBLL.GetAllCourses();
            Courses = new ObservableCollection<CourseDTO>(list);
        }

        // --- COMMAND: MỞ POPUP ---
        [RelayCommand]
        private void OpenAddDialog()
        {
            // Reset form về trắng tinh
            NewCourseName = "";
            NewDurationMonths = 3;   // Mặc định 3 tháng
            NewBaseFee = 0;          // Mặc định 0đ

            // Mở popup
            IsAddPopupOpen = true;
        }

        // --- COMMAND: LƯU KHÓA HỌC ---
        [RelayCommand]
        private void SaveNewCourse()
        {
            // 1. Validate (Kiểm tra dữ liệu)
            if (string.IsNullOrWhiteSpace(NewCourseName))
            {
                MessageBox.Show("Vui lòng nhập tên khóa học!", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (NewDurationMonths <= 0)
            {
                MessageBox.Show("Thời lượng học phải lớn hơn 0.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (NewBaseFee < 0)
            {
                MessageBox.Show("Học phí không được âm.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Tạo đối tượng DTO
            var newCourse = new CourseDTO
            {
                CourseName = NewCourseName.Trim(),
                DurationMonths = NewDurationMonths,
                BaseFee = NewBaseFee
            };

            // 3. Gọi BLL để lưu
            try
            {
                if (_courseBLL.InsertCourse(newCourse))
                {
                    MessageBox.Show("Thêm khóa học thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    IsAddPopupOpen = false; // Đóng popup
                    LoadData();             // Tải lại danh sách để thấy khóa học mới
                }
                else
                {
                    MessageBox.Show("Thêm thất bại. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi hệ thống: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- COMMAND: HỦY BỎ ---
        [RelayCommand]
        private void CancelAdd()
        {
            IsAddPopupOpen = false;
        }



        [RelayCommand]
        private void OpenDeleteDialog(CourseDTO course)
        {
            if (course == null) return;

            _courseToDelete = course; // Lưu lại đối tượng cần xóa
            IsDeletePopupOpen = true; // Hiển thị popup
        }

        // 2. Xác nhận xóa (Gọi BLL)
        [RelayCommand]
        private void ConfirmDeleteCourse()
        {
            if (_courseToDelete == null) return;

            try
            {
                if (_courseBLL.DeleteCourse(_courseToDelete.CourseID))
                {
                    // Xóa thành công
                    IsDeletePopupOpen = false;
                    Courses.Remove(_courseToDelete); // Cập nhật giao diện ngay lập tức
                    MessageBox.Show("Đã xóa khóa học thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Không thể xóa khóa học này (có thể do đang có lớp học sử dụng).", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    IsDeletePopupOpen = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hệ thống: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                IsDeletePopupOpen = false;
            }
        }

        // 3. Hủy xóa
        [RelayCommand]
        private void CancelDeleteCourse()
        {
            IsDeletePopupOpen = false;
            _courseToDelete = null;
        }


        [RelayCommand]
        private void OpenEditDialog(CourseDTO course)
        {
            if (course == null) return;

            // Copy dữ liệu từ dòng được chọn vào các biến Editing
            EditingCourseId = course.CourseID;
            EditingCourseName = course.CourseName;
            EditingDurationMonths = course.DurationMonths;
            EditingBaseFee = course.BaseFee;

            IsEditPopupOpen = true; // Hiện popup sửa
        }

        // 2. Lưu thay đổi
        [RelayCommand]
        private void SaveEditCourse()
        {
            // Validate
            if (string.IsNullOrWhiteSpace(EditingCourseName))
            {
                MessageBox.Show("Tên khóa học không được để trống.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (EditingDurationMonths <= 0)
            {
                MessageBox.Show("Thời lượng học phải lớn hơn 0.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Tạo DTO cập nhật
            var updatedCourse = new CourseDTO
            {
                CourseID = EditingCourseId,
                CourseName = EditingCourseName.Trim(),
                DurationMonths = EditingDurationMonths,
                BaseFee = EditingBaseFee
            };

            // Gọi BLL
            try
            {
                if (_courseBLL.UpdateCourse(updatedCourse))
                {
                    MessageBox.Show("Cập nhật khóa học thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    IsEditPopupOpen = false;
                    LoadData(); // Load lại để thấy thay đổi trên lưới
                }
                else
                {
                    MessageBox.Show("Cập nhật thất bại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hệ thống: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 3. Hủy sửa
        [RelayCommand]
        private void CancelEditCourse()
        {
            IsEditPopupOpen = false;
        }

    }
}