using BLL;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DTO;
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
                // Khi giá trị điểm thay đổi, tự động cập nhật Ngày chấm là DateTime.Now
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

        [ObservableProperty]
        private ClassDTO _selectedClass;

        partial void OnSelectedClassChanged(ClassDTO value)
        {
            LoadData();
            // Reset chế độ sửa khi chuyển lớp
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
                        StudentCode = $"HV{DateTime.Now.Year % 100}{row["StudentID"]:0000}",
                        Name = row["Name"].ToString(),
                        ResultID = row["ResultID"] != DBNull.Value ? Convert.ToInt32(row["ResultID"]) : 0,
                        Note = row["Note"].ToString(),

                        // LƯU Ý QUAN TRỌNG: Thứ tự gán Score trước GradingDate là cần thiết để:
                        // 1. Setter Score chạy -> Set GradingDate = Now (tạm)
                        // 2. Sau đó gán GradingDate từ DB -> Ghi đè lại đúng ngày cũ từ DB
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
            int successCount = 0;
            try
            {
                foreach (var item in GradingList)
                {
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