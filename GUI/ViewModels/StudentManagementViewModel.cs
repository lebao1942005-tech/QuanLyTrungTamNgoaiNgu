using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DTO; // Nhớ using namespace chứa StudentDTO
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Text.RegularExpressions;

namespace GUI.ViewModels
{
    public partial class StudentManagementViewModel : ObservableObject
    {
        // Khởi tạo BLL
        private readonly StudentBLL _studentBLL = new StudentBLL();

        // ====== Danh sách hiển thị lên DataGrid ======
        [ObservableProperty]
        private ObservableCollection<StudentDTO> _students;

        // ====== Thuộc tính dùng cho popup thêm học viên ======
        [ObservableProperty] private bool _isAddStudentPopupVisible;

        // Các trường nhập liệu
        [ObservableProperty] private string _newStudentName;
        [ObservableProperty] private string _newStudentPhone;
        [ObservableProperty] private string _newStudentEmail;
        [ObservableProperty] private DateTime? _newStudentBirthday; // Đổi sang DateTime? cho đúng kiểu



        [ObservableProperty] private bool _isDeleteStudentPopupVisible;
        private StudentDTO _studentToDelete;




        [ObservableProperty] private bool _isEditStudentPopupVisible;
        [ObservableProperty] private int _editStudentId; // Để hiển thị ID Read-only
        [ObservableProperty] private string _editStudentName;
        [ObservableProperty] private string _editStudentPhone;
        [ObservableProperty] private string _editStudentEmail;
        [ObservableProperty] private DateTime? _editStudentBirthday;

        public StudentManagementViewModel()
        {
            Students = new ObservableCollection<StudentDTO>();
            LoadStudents();
        }

        // ====== Logic tải dữ liệu từ BLL ======
        private void LoadStudents()
        {
            try
            {
                DataTable dt = _studentBLL.GetAllStudents();
                Students.Clear();

                foreach (DataRow row in dt.Rows)
                {
                    // Map từ DataRow sang DTO thủ công vì BLL trả về DataTable
                    var student = new StudentDTO
                    {
                        StudentID = Convert.ToInt32(row["StudentID"]),
                        Name = row["Name"].ToString(),
                        // Kiểm tra null khi map dữ liệu
                        Birthday = row["Birthday"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(row["Birthday"]),
                        Phone = row["Phone"] == DBNull.Value ? "" : row["Phone"].ToString(),
                        Email = row["Email"] == DBNull.Value ? "" : row["Email"].ToString(),

                        // Lưu ý: Nếu database của bạn chưa có cột Status, bạn cần thêm vào hoặc giả lập
                        // Status = row.Table.Columns.Contains("Status") ? Convert.ToInt32(row["Status"]) : 1 
                    };
                    Students.Add(student);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}");
            }
        }




        private bool IsValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            // \p{L} là đại diện cho chữ cái Unicode (bao gồm tiếng Việt có dấu)
            string pattern = @"^[\p{L}\s]+$";
            return Regex.IsMatch(name, pattern);
        }

        // 2. Validate SĐT: Chỉ cho phép số, độ dài từ 10-11 số
        private bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;
            // Chỉ chứa số (0-9) và độ dài từ 10 đến 11 ký tự
            string pattern = @"^[0-9]{10,11}$";
            return Regex.IsMatch(phone, pattern);
        }

        // 3. Validate Email: Đơn giản là có @ và đúng định dạng cơ bản
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            // Pattern kiểm tra email cơ bản
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern);
        }



        // ====== Mở Popup ======
        [RelayCommand]
        private void AddStudent()
        {
            // Reset form
            NewStudentName = string.Empty;
            NewStudentPhone = string.Empty;
            NewStudentEmail = string.Empty;
            NewStudentBirthday = DateTime.Now; // Default date

            IsAddStudentPopupVisible = true;
        }

        // ====== Lưu học viên mới ======
        [RelayCommand]
        private void SaveNewStudent()
        {
            // --- BẮT ĐẦU VALIDATE ---

            // 1. Kiểm tra Tên
            if (!IsValidName(NewStudentName))
            {
                MessageBox.Show("Tên học viên không hợp lệ!\nTên không được chứa số hoặc ký tự đặc biệt.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // Dừng lại, không lưu
            }

            // 2. Kiểm tra Số điện thoại
            if (!IsValidPhone(NewStudentPhone))
            {
                MessageBox.Show("Số điện thoại không hợp lệ!\nSĐT chỉ được chứa số và phải từ 10-11 ký tự.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 3. Kiểm tra Email
            if (!IsValidEmail(NewStudentEmail))
            {
                MessageBox.Show("Email không đúng định dạng (phải có @ và tên miền).", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 4. Kiểm tra Ngày sinh (Tuỳ chọn: ví dụ phải trên 6 tuổi)
            if (NewStudentBirthday == null || NewStudentBirthday > DateTime.Now.AddYears(-6))
            {
                MessageBox.Show("Ngày sinh không hợp lệ (Học viên phải trên 6 tuổi).", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // --- KẾT THÚC VALIDATE ---


            // Nếu mọi thứ OK thì mới thực hiện logic lưu xuống Database
            var newStudent = new StudentDTO
            {
                Name = NewStudentName.Trim(), // Xóa khoảng trắng thừa 2 đầu
                Phone = NewStudentPhone.Trim(),
                Email = NewStudentEmail.Trim(),
                Birthday = NewStudentBirthday
            };

            string error = "";
            bool result = _studentBLL.AddStudent(newStudent, out error);

            if (result)
            {
                MessageBox.Show("Thêm học viên thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadStudents();
                IsAddStudentPopupVisible = false;
            }
            else
            {
                MessageBox.Show($"Thêm thất bại: {error}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ====== Đóng Popup ======
        [RelayCommand]
        private void CancelAddStudent()
        {
            IsAddStudentPopupVisible = false;
        }

        // ====== Xóa học viên ======
        [RelayCommand]
        private void RequestDeleteStudent(StudentDTO student)
        {
            if (student == null) return;

            _studentToDelete = student; // Lưu lại để tí nữa xóa
            IsDeleteStudentPopupVisible = true; // Hiện popup
        }


        [RelayCommand]
        private void ConfirmDeleteStudent()
        {
            if (_studentToDelete == null) return;

            string error = "";
            // Gọi BLL để xóa trong Database
            if (_studentBLL.DeleteStudent(_studentToDelete.StudentID, out error))
            {
                // Xóa thành công thì xóa luôn trên giao diện (ko cần load lại DB)
                Students.Remove(_studentToDelete);
                IsDeleteStudentPopupVisible = false;
                _studentToDelete = null;
                MessageBox.Show("Đã xóa học viên thành công!");
            }
            else
            {
                MessageBox.Show($"Xóa thất bại: {error}");
            }
        }

        // 3. Command này gắn ở nút "Hủy bỏ" và nút "X" trong Popup Xóa
        [RelayCommand]
        private void CancelDeleteStudent()
        {
            IsDeleteStudentPopupVisible = false;
            _studentToDelete = null;
        }



        [RelayCommand]
        private void EditStudent(StudentDTO student)
        {
            if (student == null) return;

            // Đổ dữ liệu từ dòng được chọn lên form Edit
            EditStudentId = student.StudentID;
            EditStudentName = student.Name;
            EditStudentPhone = student.Phone;
            EditStudentEmail = student.Email;
            EditStudentBirthday = student.Birthday;

            IsEditStudentPopupVisible = true;
        }

        // 2. Command Hủy sửa
        [RelayCommand]
        private void CancelEditStudent()
        {
            IsEditStudentPopupVisible = false;
        }

        // 3. Command Lưu thay đổi
        [RelayCommand]
        private void ConfirmEditStudent()
        {
            // --- VALIDATE (Tái sử dụng hàm validate đã viết) ---
            if (!IsValidName(EditStudentName))
            {
                MessageBox.Show("Tên học viên không hợp lệ!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!IsValidPhone(EditStudentPhone))
            {
                MessageBox.Show("Số điện thoại không hợp lệ!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!IsValidEmail(EditStudentEmail))
            {
                MessageBox.Show("Email không đúng định dạng.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // --------------------------------------------------

            // Tạo đối tượng DTO cập nhật
            var updateStudent = new StudentDTO
            {
                StudentID = EditStudentId, // Quan trọng: Phải có ID để biết sửa ai
                Name = EditStudentName.Trim(),
                Phone = EditStudentPhone.Trim(),
                Email = EditStudentEmail.Trim(),
                Birthday = EditStudentBirthday
            };

            string error = "";
            if (_studentBLL.UpdateStudent(updateStudent, out error))
            {
                MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                IsEditStudentPopupVisible = false;
                LoadStudents(); // Tải lại danh sách để thấy thay đổi
            }
            else
            {
                MessageBox.Show($"Cập nhật thất bại: {error}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}