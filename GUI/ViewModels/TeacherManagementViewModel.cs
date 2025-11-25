using BLL;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DTO;
using System.Collections.ObjectModel;
using System.Windows;
using System.Text.RegularExpressions;

namespace GUI.ViewModels
{
    public partial class TeacherManagementViewModel : ObservableObject
    {
        private readonly TeacherBLL _teacherBLL = new TeacherBLL();

        [ObservableProperty]
        private ObservableCollection<TeacherDTO> _teachers;

        // ====== POPUP VISIBILITY ======
        [ObservableProperty] private bool _isAddTeacherPopupVisible;
        [ObservableProperty] private bool _isDeleteTeacherPopupVisible;
        [ObservableProperty] private bool _isEditTeacherPopupVisible; // Mới thêm

        private TeacherDTO _teacherToDelete; // Lưu tạm giáo viên đang chọn xóa
        private TeacherDTO _teacherToEdit;   // Lưu tạm giáo viên đang chọn sửa

        // ====== CÁC TRƯỜNG NHẬP LIỆU (THÊM MỚI) ======
        [ObservableProperty] private string _newTeacherName;
        [ObservableProperty] private string _newTeacherPhone;
        [ObservableProperty] private string _newTeacherEmail;
        [ObservableProperty] private string _newTeacherSubject;

        // ====== CÁC TRƯỜNG NHẬP LIỆU (CHỈNH SỬA) ======
        [ObservableProperty] private string _editingTeacherName;
        [ObservableProperty] private string _editingTeacherPhone;
        [ObservableProperty] private string _editingTeacherEmail; // Thường là ReadOnly vì là UserID
        [ObservableProperty] private string _editingTeacherSubject;

        public TeacherManagementViewModel()
        {
            Teachers = new ObservableCollection<TeacherDTO>();
            LoadTeachers();
        }

        private void LoadTeachers()
        {
            try
            {
                var dt = _teacherBLL.GetAllTeachers();
                Teachers.Clear();
                foreach (System.Data.DataRow row in dt.Rows)
                {
                    Teachers.Add(new TeacherDTO
                    {
                        TeacherID = (int)row["TeacherID"],
                        Name = row["Name"].ToString(),
                        Subject = row["Subject"]?.ToString(),
                        Phone = row["Phone"]?.ToString(),
                        Email = row["Email"]?.ToString(),
                        UserID = (int)row["UserID"]
                    });
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        // ====== HÀM VALIDATE HỖ TRỢ ======
        private bool IsValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            return Regex.IsMatch(name, @"^[\p{L}\s]+$");
        }

        // ==========================================
        // COMMANDS: THÊM GIÁO VIÊN
        // ==========================================

        [RelayCommand]
        private void OpenAddTeacherPopup()
        {
            NewTeacherName = "";
            NewTeacherPhone = "";
            NewTeacherEmail = "";
            NewTeacherSubject = "";
            IsAddTeacherPopupVisible = true;
        }

        [RelayCommand]
        private void CancelAddTeacher()
        {
            IsAddTeacherPopupVisible = false;
        }

        [RelayCommand]
        private void SaveNewTeacher()
        {
            if (!IsValidName(NewTeacherName))
            {
                MessageBox.Show("Tên giáo viên không hợp lệ!\nTên không được chứa số hoặc ký tự đặc biệt.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Regex.IsMatch(NewTeacherPhone ?? "", @"^\d{10,11}$"))
            {
                MessageBox.Show("Số điện thoại không hợp lệ (phải là 10-11 số).", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Regex.IsMatch(NewTeacherEmail ?? "", @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Email không đúng định dạng.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newTeacher = new TeacherDTO
            {
                Name = NewTeacherName.Trim(),
                Phone = NewTeacherPhone.Trim(),
                Email = NewTeacherEmail.Trim(),
                Subject = NewTeacherSubject?.Trim() ?? "Chưa phân công"
            };

            string error = "";
            if (_teacherBLL.AddTeacher(newTeacher, out error))
            {
                MessageBox.Show("Thêm giáo viên thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                IsAddTeacherPopupVisible = false;
                LoadTeachers();
            }
            else
            {
                MessageBox.Show($"Thêm thất bại: {error}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ==========================================
        // COMMANDS: XÓA GIÁO VIÊN
        // ==========================================

        [RelayCommand]
        private void RequestDeleteTeacher(TeacherDTO teacher)
        {
            if (teacher == null) return;
            _teacherToDelete = teacher;
            IsDeleteTeacherPopupVisible = true;
        }

        [RelayCommand]
        private void ConfirmDeleteTeacher()
        {
            if (_teacherToDelete == null) return;

            string error = "";
            if (_teacherBLL.DeleteTeacher(_teacherToDelete.TeacherID, out error))
            {
                MessageBox.Show("Đã xóa giáo viên thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                Teachers.Remove(_teacherToDelete);
                IsDeleteTeacherPopupVisible = false;
                _teacherToDelete = null;
            }
            else
            {
                MessageBox.Show($"Xóa thất bại: {error}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void CancelDeleteTeacher()
        {
            IsDeleteTeacherPopupVisible = false;
            _teacherToDelete = null;
        }

        // ==========================================
        // COMMANDS: SỬA GIÁO VIÊN (MỚI THÊM)
        // ==========================================

        [RelayCommand]
        private void EditTeacher(TeacherDTO teacher)
        {
            if (teacher == null) return;

            _teacherToEdit = teacher;

            // Đổ dữ liệu hiện tại vào form sửa
            EditingTeacherName = teacher.Name;
            EditingTeacherPhone = teacher.Phone;
            EditingTeacherEmail = teacher.Email; // ReadOnly trên View
            EditingTeacherSubject = teacher.Subject;

            IsEditTeacherPopupVisible = true;
        }

        [RelayCommand]
        private void CancelEditTeacher()
        {
            IsEditTeacherPopupVisible = false;
            _teacherToEdit = null;
        }

        [RelayCommand]
        private void SaveEditTeacher()
        {
            if (_teacherToEdit == null) return;

            // 1. Validate
            if (!IsValidName(EditingTeacherName))
            {
                MessageBox.Show("Tên giáo viên không hợp lệ!", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Regex.IsMatch(EditingTeacherPhone ?? "", @"^\d{10,11}$"))
            {
                MessageBox.Show("Số điện thoại không hợp lệ!", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Cập nhật vào object tạm (Không cần check email vì email là UserID/LoginName thường không cho sửa)
            _teacherToEdit.Name = EditingTeacherName.Trim();
            _teacherToEdit.Phone = EditingTeacherPhone.Trim();
            _teacherToEdit.Subject = EditingTeacherSubject?.Trim();

            // 3. Gọi BLL Update
            string error = "";
            // Giả định hàm UpdateTeacher tồn tại trong BLL
            if (_teacherBLL.UpdateTeacher(_teacherToEdit, out error))
            {
                MessageBox.Show("Cập nhật thông tin thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                IsEditTeacherPopupVisible = false;
                LoadTeachers(); // Tải lại để cập nhật hiển thị
                _teacherToEdit = null;
            }
            else
            {
                MessageBox.Show($"Cập nhật thất bại: {error}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}