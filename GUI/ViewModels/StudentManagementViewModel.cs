using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DTO; // Nhớ using namespace chứa StudentDTO
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using Microsoft.Win32;

namespace GUI.ViewModels
{
    public partial class StudentManagementViewModel : ObservableObject
    {
        // Khởi tạo BLL
        private readonly StudentBLL _studentBLL = new StudentBLL();

        // ====== Danh sách hiển thị lên DataGrid ======
        [ObservableProperty]
        private ObservableCollection<StudentDTO> _students;


        // [MỚI 1] Danh sách gốc để lưu toàn bộ dữ liệu (Backup)
        private List<StudentDTO> _originalStudentsList;

        // [MỚI 2] Biến chứa từ khóa tìm kiếm
        [ObservableProperty]
        private string _searchText;

        // Hàm này tự động chạy khi bạn gõ chữ vào ô tìm kiếm
        partial void OnSearchTextChanged(string value)
        {
            FilterStudents();
        }


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
                        Status = row.Table.Columns.Contains("Status") ? Convert.ToInt32(row["Status"]) : 1 
                    };
                    Students.Add(student);
                }

                // [MỚI 3] Lưu bản sao dữ liệu vào danh sách gốc
                _originalStudentsList = Students.ToList();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}");
            }
        }



        // [MỚI 4] Hàm xử lý logic tìm kiếm
        private void FilterStudents()
        {
            // Nếu chưa có dữ liệu gốc thì không làm gì cả
            if (_originalStudentsList == null) return;

            // Nếu ô tìm kiếm trống -> Hiển thị lại toàn bộ danh sách gốc
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                Students = new ObservableCollection<StudentDTO>(_originalStudentsList);
            }
            else
            {
                // Lọc theo Tên, SĐT, Email (chữ thường không dấu)
                string keyword = SearchText.ToLower();

                var filteredList = _originalStudentsList.Where(s =>
                    (s.Name != null && s.Name.ToLower().Contains(keyword)) ||
                    (s.Phone != null && s.Phone.Contains(keyword)) ||
                    (s.Email != null && s.Email.ToLower().Contains(keyword)) ||
                    (s.StudentID.ToString().Contains(keyword)) // Tìm theo cả ID nếu muốn
                ).ToList();

                // Cập nhật lại danh sách hiển thị
                Students = new ObservableCollection<StudentDTO>(filteredList);
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
        /*
        [RelayCommand]
        private void RequestDeleteStudent(StudentDTO student)
        {
            if (student == null) return;

            _studentToDelete = student; // Lưu lại để tí nữa xóa
            IsDeleteStudentPopupVisible = true; // Hiện popup
        }
        */


        [RelayCommand]
        private void RequestDeleteStudent(StudentDTO student)
        {
            if (student == null) return;

            // [QUAN TRỌNG] Kiểm tra trạng thái học viên
            // Nếu Status == 2 (Đang học), chặn ngay lập tức
            if (student.Status == 2)
            {
                MessageBox.Show($"Không thể xóa học viên '{student.Name}' vì đang có lớp học (Enrollment).\n" +
                                "Vui lòng xóa thông tin đăng ký lớp học hoặc hủy lớp của học viên này trước.",
                                "Thao tác bị chặn",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return; // Dừng lại, không mở popup xóa
            }

            // Nếu Status == 1 (Không lớp), cho phép mở popup xác nhận
            _studentToDelete = student;
            IsDeleteStudentPopupVisible = true;
        }


        /*
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
        */


        [RelayCommand]
        private void ConfirmDeleteStudent()
        {
            if (_studentToDelete == null) return;

            // [An toàn] Kiểm tra lại lần nữa (Double check)
            if (_studentToDelete.Status == 2)
            {
                MessageBox.Show("Học viên đang có lớp học, không thể xóa.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                IsDeleteStudentPopupVisible = false;
                return;
            }

            string error = "";
            // Gọi BLL để xóa trong Database
            if (_studentBLL.DeleteStudent(_studentToDelete.StudentID, out error))
            {
                // Xóa thành công thì xóa luôn trên giao diện
                Students.Remove(_studentToDelete);

                // Cập nhật lại list gốc để tìm kiếm vẫn đúng
                if (_originalStudentsList != null) _originalStudentsList.Remove(_studentToDelete);

                IsDeleteStudentPopupVisible = false;
                _studentToDelete = null;
                MessageBox.Show("Đã xóa học viên thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"Xóa thất bại: {error}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                IsDeleteStudentPopupVisible = false; // Đóng popup kể cả khi lỗi để tránh kẹt
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



        [RelayCommand]
        private void ExportToPdf()
        {
            // 1. Mở hộp thoại chọn nơi lưu file
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                FileName = $"DanhSachHocVien_{DateTime.Now:ddMMyyyy_HHmm}.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // 2. Tạo tài liệu PDF
                    // PageSize.A4.Rotate() để xoay ngang khổ giấy nếu bảng rộng
                    Document doc = new Document(PageSize.A4, 20, 20, 20, 20);
                    PdfWriter.GetInstance(doc, new FileStream(saveFileDialog.FileName, FileMode.Create));

                    doc.Open();

                    // 3. Cấu hình Font chữ Tiếng Việt (Quan trọng)
                    // Tìm đường dẫn font Arial trong Windows
                    string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
                    BaseFont bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                    Font titleFont = new Font(bf, 18, Font.BOLD, BaseColor.BLUE);
                    Font headerFont = new Font(bf, 12, Font.BOLD, BaseColor.WHITE);
                    Font contentFont = new Font(bf, 11, Font.NORMAL, BaseColor.BLACK);

                    // 4. Thêm Tiêu đề
                    Paragraph title = new Paragraph("DANH SÁCH HỌC VIÊN", titleFont);
                    title.Alignment = Element.ALIGN_CENTER;
                    title.SpacingAfter = 20;
                    doc.Add(title);

                    // 5. Tạo Bảng (5 cột: STT, Tên, Ngày sinh, SĐT, Email)
                    PdfPTable table = new PdfPTable(5);
                    table.WidthPercentage = 100; // Chiều rộng 100%
                                                 // Set tỉ lệ độ rộng các cột (VD: STT nhỏ, Email rộng...)
                    table.SetWidths(new float[] { 10f, 30f, 15f, 20f, 25f });

                    // --- Header ---
                    AddCellToBody(table, "STT", headerFont, BaseColor.DARK_GRAY);
                    AddCellToBody(table, "Họ Tên", headerFont, BaseColor.DARK_GRAY);
                    AddCellToBody(table, "Ngày Sinh", headerFont, BaseColor.DARK_GRAY);
                    AddCellToBody(table, "Điện Thoại", headerFont, BaseColor.DARK_GRAY);
                    AddCellToBody(table, "Email", headerFont, BaseColor.DARK_GRAY);

                    // --- Dữ liệu ---
                    int stt = 1;
                    // Lưu ý: Export danh sách đang hiển thị (Students) chứ không phải danh sách gốc
                    foreach (var sv in Students)
                    {
                        AddCellToBody(table, stt++.ToString(), contentFont, BaseColor.WHITE);
                        AddCellToBody(table, sv.Name, contentFont, BaseColor.WHITE);
                        AddCellToBody(table, sv.Birthday?.ToString("dd/MM/yyyy"), contentFont, BaseColor.WHITE);
                        AddCellToBody(table, sv.Phone, contentFont, BaseColor.WHITE);
                        AddCellToBody(table, sv.Email, contentFont, BaseColor.WHITE);
                    }

                    doc.Add(table);
                    doc.Close();

                    MessageBox.Show("Xuất file PDF thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Tùy chọn: Mở file ngay sau khi lưu
                    // System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(saveFileDialog.FileName) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi xuất file: " + ex.Message);
                }
            }
        }

        // Hàm hỗ trợ thêm ô vào bảng cho gọn code
        private void AddCellToBody(PdfPTable table, string text, Font font, BaseColor bgColor)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text ?? "", font));
            cell.BackgroundColor = bgColor;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.Padding = 5;
            table.AddCell(cell);
        }


    }
}