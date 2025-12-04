using BLL;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DTO;
using System.Collections.ObjectModel;
using System.Windows;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using Microsoft.Win32;
using System.Linq; // Cần thiết để xử lý danh sách

namespace GUI.ViewModels
{
    public partial class TeacherManagementViewModel : ObservableObject
    {
        // 1. Khai báo các BLL cần thiết
        private readonly TeacherBLL _teacherBLL = new TeacherBLL();
        private readonly CourseBLL _courseBLL = new CourseBLL(); // Dùng để lấy danh sách tên khóa học

        // 2. Danh sách hiển thị
        [ObservableProperty]
        private ObservableCollection<TeacherDTO> _teachers;


        // [MỚI 1] Danh sách gốc để lưu toàn bộ dữ liệu (Backup cho việc tìm kiếm)
        private List<TeacherDTO> _originalTeacherList;

        // [MỚI 2] Biến chứa từ khóa tìm kiếm
        [ObservableProperty]
        private string _searchText;

        // [MỚI 3] Hàm này tự động chạy khi bạn gõ chữ vào ô tìm kiếm
        partial void OnSearchTextChanged(string value)
        {
            FilterTeachers();
        }



        // [MỚI] Danh sách môn học (lấy từ tên khóa học) để binding vào ComboBox
        [ObservableProperty]
        private ObservableCollection<string> _subjectList;


        [ObservableProperty]
        private ObservableCollection<SelectableItem> _subjectSelectionList;

        // ====== POPUP VISIBILITY ======
        [ObservableProperty] private bool _isAddTeacherPopupVisible;
        [ObservableProperty] private bool _isDeleteTeacherPopupVisible;
        [ObservableProperty] private bool _isEditTeacherPopupVisible;

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
        [ObservableProperty] private string _editingTeacherEmail; // Thường là ReadOnly
        [ObservableProperty] private string _editingTeacherSubject;

        public TeacherManagementViewModel()
        {
            Teachers = new ObservableCollection<TeacherDTO>();
            //SubjectList = new ObservableCollection<string>(); // Khởi tạo list rỗng
            SubjectSelectionList = new ObservableCollection<SelectableItem>();
            LoadTeachers();
            LoadSubjects(); // Gọi hàm load môn học ngay khi khởi tạo
        }

        // --- HÀM LOAD DỮ LIỆU ---

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

                // [MỚI 4] Lưu bản sao dữ liệu vào danh sách gốc ngay sau khi load xong
                _originalTeacherList = Teachers.ToList();

            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách giáo viên: " + ex.Message);
            }
        }

        // [MỚI] Hàm load danh sách môn học từ CourseBLL
    /*    private void LoadSubjects()
        {
            try
            {
                SubjectList.Clear();

                // Gọi sang CourseBLL để lấy list tên khóa học
                // Lưu ý: Bạn cần chắc chắn đã thêm hàm GetCourseNames() vào CourseBLL như hướng dẫn trước
                var courseNames = _courseBLL.GetCourseNames();

                foreach (var name in courseNames)
                {
                    SubjectList.Add(name);
                }
            }
            catch
            {
                // Fallback: Nếu lỗi kết nối hoặc chưa có khóa học nào, thêm vài môn mặc định
                SubjectList.Add("IELTS");
                SubjectList.Add("TOEIC");
                SubjectList.Add("Tiếng Anh Giao Tiếp");
            }
        }
        */




        private void LoadSubjects()
        {
            try
            {
                SubjectSelectionList.Clear();
                var courseNames = _courseBLL.GetCourseNames(); // Lấy danh sách tên từ DB

                // Loại bỏ trùng lặp tên môn (nếu có nhiều khóa cùng môn)
                var distinctSubjects = courseNames.Distinct().ToList();

                foreach (var name in distinctSubjects)
                {
                    // Mặc định là chưa chọn (false)
                    SubjectSelectionList.Add(new SelectableItem(name, false));
                }
            }
            catch
            {
                // Fallback nếu lỗi
                SubjectSelectionList.Add(new SelectableItem("IELTS"));
                SubjectSelectionList.Add(new SelectableItem("TOEIC"));
            }
        }




        // [MỚI 5] Hàm xử lý logic tìm kiếm
        private void FilterTeachers()
        {
            // Nếu chưa có dữ liệu gốc thì không làm gì cả
            if (_originalTeacherList == null) return;

            // Nếu ô tìm kiếm trống -> Hiển thị lại toàn bộ danh sách gốc
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                Teachers = new ObservableCollection<TeacherDTO>(_originalTeacherList);
            }
            else
            {
                // Lọc theo Tên, SĐT, Email hoặc Môn dạy (chữ thường không dấu)
                string keyword = SearchText.ToLower();

                var filteredList = _originalTeacherList.Where(t =>
                    (t.Name != null && t.Name.ToLower().Contains(keyword)) ||
                    (t.Phone != null && t.Phone.Contains(keyword)) ||
                    (t.Email != null && t.Email.ToLower().Contains(keyword)) ||
                    (t.Subject != null && t.Subject.ToLower().Contains(keyword))
                ).ToList();

                // Cập nhật lại danh sách hiển thị
                Teachers = new ObservableCollection<TeacherDTO>(filteredList);
            }
        }



        // ====== HÀM VALIDATE HỖ TRỢ ======
        private bool IsValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            // Regex cho phép chữ cái (bao gồm tiếng Việt) và khoảng trắng
            return Regex.IsMatch(name, @"^[\p{L}\s]+$");
        }

        // ==========================================
        // COMMANDS: THÊM GIÁO VIÊN
        // ==========================================

        /*
        [RelayCommand]
        private void OpenAddTeacherPopup()
        {
            NewTeacherName = "";
            NewTeacherPhone = "";
            NewTeacherEmail = "";
            NewTeacherSubject = "";

            // Reload lại danh sách môn học để cập nhật những khóa học mới nhất
            LoadSubjects();

            IsAddTeacherPopupVisible = true;
        }
        */



        [RelayCommand]
        private void OpenAddTeacherPopup()
        {
            NewTeacherName = "";
            NewTeacherPhone = "";
            NewTeacherEmail = "";

            // Reset lại các lựa chọn môn học về false
            foreach (var item in SubjectSelectionList) item.IsSelected = false;

            IsAddTeacherPopupVisible = true;
        }


        [RelayCommand]
        private void CancelAddTeacher()
        {
            IsAddTeacherPopupVisible = false;
        }


        /*
        [RelayCommand]
        private void SaveNewTeacher()
        {
            // 1. Validate
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

            // 2. Tạo DTO
            var newTeacher = new TeacherDTO
            {
                Name = NewTeacherName.Trim(),
                Phone = NewTeacherPhone.Trim(),
                Email = NewTeacherEmail.Trim(),
                Subject = NewTeacherSubject?.Trim() ?? "Chưa phân công"
            };

            // 3. Gọi BLL
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
        */


        [RelayCommand]
        private void SaveNewTeacher()
        {
            // 1. Validate
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

            // 2. Tạo DTO
            // 1. Lấy danh sách các môn được tick chọn
            var selectedSubjects = SubjectSelectionList
                                    .Where(x => x.IsSelected)
                                    .Select(x => x.Name)
                                    .ToList();

            // 2. Nối thành chuỗi: "IELTS, TOEIC"
            string subjectString = selectedSubjects.Count > 0
                                   ? string.Join(", ", selectedSubjects)
                                   : "Chưa phân công";

            var newTeacher = new TeacherDTO
            {
                Name = NewTeacherName.Trim(),
                Phone = NewTeacherPhone.Trim(),
                Email = NewTeacherEmail.Trim(),
                Subject = subjectString // Lưu chuỗi đã nối xuống DB
            };

            // 3. Gọi BLL
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
        // COMMANDS: SỬA GIÁO VIÊN
        // ==========================================
        /*
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

            // Load lại danh sách môn cho chắc chắn
            LoadSubjects();

            IsEditTeacherPopupVisible = true;
        }
        */



        // [CẬP NHẬT] Hàm mở Popup Sửa (Quan trọng: Phải tick lại đúng môn cũ)
        [RelayCommand]
        private void EditTeacher(TeacherDTO teacher)
        {
            if (teacher == null) return;
            _teacherToEdit = teacher;

            EditingTeacherName = teacher.Name;
            EditingTeacherPhone = teacher.Phone;
            EditingTeacherEmail = teacher.Email;

            // Reset list trước
            foreach (var item in SubjectSelectionList) item.IsSelected = false;

            // Tách chuỗi Subject cũ: "IELTS, TOEIC" -> ["IELTS", "TOEIC"]
            if (!string.IsNullOrEmpty(teacher.Subject))
            {
                var currentSubjects = teacher.Subject.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var subjectName in currentSubjects)
                {
                    // Tìm item trong list và tick vào
                    var item = SubjectSelectionList.FirstOrDefault(x => x.Name == subjectName);
                    if (item != null) item.IsSelected = true;
                }
            }

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

            // 2. Cập nhật vào object tạm
            var selectedSubjects = SubjectSelectionList
                                .Where(x => x.IsSelected)
                                .Select(x => x.Name)
                                .ToList();

            string subjectString = selectedSubjects.Count > 0
                                   ? string.Join(", ", selectedSubjects)
                                   : "Chưa phân công";

            _teacherToEdit.Name = EditingTeacherName.Trim();
            _teacherToEdit.Phone = EditingTeacherPhone.Trim();
            _teacherToEdit.Subject = subjectString; // Cập nhật chuỗi môn

            // 3. Gọi BLL Update
            string error = "";
            if (_teacherBLL.UpdateTeacher(_teacherToEdit, out error))
            {
                MessageBox.Show("Cập nhật thông tin thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                IsEditTeacherPopupVisible = false;
                LoadTeachers(); // Tải lại để cập nhật hiển thị trên lưới
                _teacherToEdit = null;
            }
            else
            {
                MessageBox.Show($"Cập nhật thất bại: {error}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }




        [RelayCommand]
        private void ExportToPdf()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                FileName = $"DanhSachGiaoVien_{DateTime.Now:ddMMyyyy_HHmm}.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // 1. Tạo tài liệu
                    Document doc = new Document(PageSize.A4, 20, 20, 20, 20);
                    PdfWriter.GetInstance(doc, new FileStream(saveFileDialog.FileName, FileMode.Create));
                    doc.Open();

                    // 2. Font chữ (Arial)
                    string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
                    BaseFont bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                    Font titleFont = new Font(bf, 18, Font.BOLD, BaseColor.BLUE);
                    Font headerFont = new Font(bf, 12, Font.BOLD, BaseColor.WHITE);
                    Font contentFont = new Font(bf, 11, Font.NORMAL, BaseColor.BLACK);

                    // 3. Tiêu đề
                    Paragraph title = new Paragraph("DANH SÁCH GIÁO VIÊN", titleFont);
                    title.Alignment = Element.ALIGN_CENTER;
                    title.SpacingAfter = 20;
                    doc.Add(title);

                    // 4. Tạo bảng (5 cột: STT, Tên, Bộ môn, SĐT, Email)
                    PdfPTable table = new PdfPTable(5);
                    table.WidthPercentage = 100;
                    // Chỉnh tỉ lệ cột cho phù hợp với giáo viên
                    table.SetWidths(new float[] { 8f, 25f, 20f, 17f, 30f });

                    // Header
                    AddCellToBody(table, "STT", headerFont, BaseColor.DARK_GRAY);
                    AddCellToBody(table, "Họ Tên", headerFont, BaseColor.DARK_GRAY);
                    AddCellToBody(table, "Bộ Môn", headerFont, BaseColor.DARK_GRAY); // Khác với SV
                    AddCellToBody(table, "Điện Thoại", headerFont, BaseColor.DARK_GRAY);
                    AddCellToBody(table, "Email", headerFont, BaseColor.DARK_GRAY);

                    // Dữ liệu
                    int stt = 1;
                    foreach (var gv in Teachers) // Duyệt danh sách giáo viên
                    {
                        AddCellToBody(table, stt++.ToString(), contentFont, BaseColor.WHITE);
                        AddCellToBody(table, gv.Name, contentFont, BaseColor.WHITE);
                        AddCellToBody(table, gv.Subject, contentFont, BaseColor.WHITE); // Cột Subject
                        AddCellToBody(table, gv.Phone, contentFont, BaseColor.WHITE);
                        AddCellToBody(table, gv.Email, contentFont, BaseColor.WHITE);
                    }

                    doc.Add(table);
                    doc.Close();

                    MessageBox.Show("Xuất file PDF thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi xuất file: " + ex.Message);
                }
            }
        }

        // Hàm hỗ trợ thêm ô (Copy y chang từ Student qua)
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