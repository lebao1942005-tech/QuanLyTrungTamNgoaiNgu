using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace GUI.ViewModels // Hoặc GUI.Models tùy cấu trúc bạn muốn
{
    // Class này dùng để hiển thị lên lưới (Wrapper cho DTO gốc)
    public class AttendanceDisplayDTO : ObservableObject
    {
        public int Index { get; set; }
        public int EnrollmentID { get; set; }
        public int StudentID { get; set; }
        public int? AttendanceID { get; set; } // Cho phép null nếu chưa có điểm danh
        public string StudentCode { get; set; }
        public string Name { get; set; }

        private string _note;
        public string Note
        {
            get => _note;
            set => SetProperty(ref _note, value);
        }

        private string _status;
        public string Status
        {
            get => _status;
            set
            {
                if (value == null) value = "";
                if (SetProperty(ref _status, value))
                {
                    // Thông báo thay đổi cho các property bool để RadioButton tự cập nhật
                    OnPropertyChanged(nameof(IsPresent));
                    OnPropertyChanged(nameof(IsAbsent));
                    OnPropertyChanged(nameof(IsExcused));
                    OnPropertyChanged(nameof(IsLate));

                    // Tính toán lại thống kê ngay lập tức khi trạng thái đổi
                    ParentVM?.CalculateStats();
                }
            }
        }

        // Các Property bool này bind vào RadioButton
        public bool IsPresent { get => Status == "Present"; set { if (value) Status = "Present"; } }
        public bool IsAbsent { get => Status == "Absent"; set { if (value) Status = "Absent"; } }
        public bool IsExcused { get => Status == "Excused"; set { if (value) Status = "Excused"; } }
        public bool IsLate { get => Status == "Late"; set { if (value) Status = "Late"; } }

        // Tham chiếu ngược lại ViewModel cha để gọi hàm tính toán (CalculateStats)
        public ClassAttendanceViewModel ParentVM { get; set; }
    }
}