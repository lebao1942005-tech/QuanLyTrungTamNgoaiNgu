using BLL;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DTO;
using GUI.Utilities; // Cần namespace này để dùng UserSession
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;

namespace GUI.ViewModels
{
    // DTO phụ dùng riêng cho việc hiển thị và nhập liệu trên lưới
    public class GradingDisplayDTO : ObservableObject
    {
        public int Index { get; set; }
        public int EnrollmentID { get; set; }
        public int ResultID { get; set; }
        public int StudentID { get; set; }
        public string StudentCode { get; set; }
        public string Name { get; set; }

        private decimal? _score;
        public decimal? Score
        {
            get => _score;
            set
            {
                // Validate logic: Nếu nhỏ hơn 0 thì gán bằng 0, lớn hơn 10 thì gán bằng 10
                if (value < 0) value = 0;
                if (value > 10) value = 10;

                if (SetProperty(ref _score, value))
                {
                    GradingDate = DateTime.Now;
                }
            }
        }

        private DateTime? _gradingDate;
        public DateTime? GradingDate
        {
            get => _gradingDate;
            set => SetProperty(ref _gradingDate, value);
        }

        private string _note;
        public string Note
        {
            get => _note;
            set => SetProperty(ref _note, value);
        }
    }

    public partial class ClassGradingViewModel : ObservableObject
    {
        private readonly ExamResultBLL _examBLL = new ExamResultBLL();

        // --- 1. PROPERTY PHÂN QUYỀN (MỚI) ---
        // Property này giúp View có thể binding để ẩn hiện nút nếu muốn
        public bool IsTeacher => UserSession.Role == "Teacher";
        // Hoặc dùng: public bool IsTeacher => UserSession.IsTeacher; (nếu bên UserSession đã có helper này)

        [ObservableProperty]
        private ClassDTO _selectedClass;

        partial void OnSelectedClassChanged(ClassDTO value)
        {
            LoadData();
            // Reset chế độ sửa khi chuyển lớp để tránh lỗi logic
            CancelEdit();
        }

        [ObservableProperty]
        private ObservableCollection<GradingDisplayDTO> _gradingList;

        [ObservableProperty]
        private decimal _classAverageScore;

        // --- TRẠNG THÁI EDIT MODE ---
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsReadOnlyMode))] // Tự động báo thay đổi cho IsReadOnlyMode
        private bool _isEditing;

        // Property phụ để Binding vào IsReadOnly của TextBox (Ngược lại với IsEditing)
        public bool IsReadOnlyMode => !IsEditing;

        public ClassGradingViewModel()
        {
            GradingList = new ObservableCollection<GradingDisplayDTO>();
            IsEditing = false; // Mặc định là chế độ Xem
        }

        public void LoadData()
        {
            if (SelectedClass == null) return;

            try
            {
                var dt = _examBLL.GetGradingListByClass(SelectedClass.ClassID);
                var list = new ObservableCollection<GradingDisplayDTO>();

                int index = 1;
                decimal totalScore = 0;
                int countScore = 0;

                foreach (DataRow row in dt.Rows)
                {
                    var item = new GradingDisplayDTO
                    {
                        Index = index++,
                        EnrollmentID = Convert.ToInt32(row["EnrollmentID"]),
                        StudentID = Convert.ToInt32(row["StudentID"]),
                        StudentCode = row["StudentID"].ToString(),
                        Name = row["Name"].ToString(),
                        ResultID = row["ResultID"] != DBNull.Value ? Convert.ToInt32(row["ResultID"]) : 0,
                        Note = row["Note"].ToString(),

                        // LƯU Ý: Gán Score trước GradingDate
                        Score = row["Score"] != DBNull.Value ? Convert.ToDecimal(row["Score"]) : null,
                        GradingDate = row["GradingDate"] != DBNull.Value ? Convert.ToDateTime(row["GradingDate"]) : DateTime.Now
                    };

                    list.Add(item);

                    if (item.Score.HasValue)
                    {
                        totalScore += item.Score.Value;
                        countScore++;
                    }
                }

                GradingList = list;
                ClassAverageScore = countScore > 0 ? totalScore / countScore : 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải bảng điểm: " + ex.Message);
            }
        }




        // --- CÁC COMMAND XỬ LÝ ---

        [RelayCommand]
        private void StartEdit()
        {
            // --- 2. KIỂM TRA QUYỀN (MỚI) ---
            // Nếu không phải Giáo viên, chặn ngay lập tức
            if (!IsTeacher)
            {
                MessageBox.Show("Chỉ giáo viên phụ trách mới có quyền chấm điểm/sửa điểm.",
                                "Hạn chế quyền truy cập",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsEditing = true;
        }

        [RelayCommand]
        private void CancelEdit()
        {
            IsEditing = false;
            LoadData(); // Load lại dữ liệu gốc (hủy các thay đổi chưa lưu)
        }

        [RelayCommand]
        private void SaveGrading()
        {
            // --- 3. BẢO MẬT 2 LỚP (MỚI) ---
            // Đề phòng trường hợp hacker bypass nút bấm, check lại lần nữa
            if (!IsTeacher)
            {
                MessageBox.Show("Bạn không có quyền lưu dữ liệu này.");
                return;
            }

            int successCount = 0;
            try
            {
                foreach (var item in GradingList)
                {
                    // Chỉ lưu những dòng có điểm hoặc có ghi chú (hoặc logic tùy bạn)
                    if (item.Score != null || !string.IsNullOrEmpty(item.Note))
                    {
                        var dto = new ExamResultDTO
                        {
                            ResultID = item.ResultID,
                            EnrollmentID = item.EnrollmentID,
                            Score = item.Score,
                            GradingDate = item.GradingDate,
                            Note = item.Note
                        };

                        bool result = false;
                        if (item.ResultID == 0) result = _examBLL.Insert(dto);
                        else result = _examBLL.Update(dto);

                        if (result) successCount++;
                    }
                }

                MessageBox.Show($"Đã lưu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                // Sau khi lưu thành công:
                IsEditing = false; // Tắt chế độ sửa
                LoadData();        // Load lại dữ liệu mới nhất
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu điểm: " + ex.Message);
            }
        }
    }
}