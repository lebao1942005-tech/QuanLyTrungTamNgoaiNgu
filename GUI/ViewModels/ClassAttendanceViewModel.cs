using BLL;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DTO;
using GUI.Utilities; // [QUAN TRỌNG] Cần namespace này để dùng UserSession
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;

namespace GUI.ViewModels
{
    public partial class ClassAttendanceViewModel : ObservableObject
    {
        private readonly AttendanceBLL _bll = new AttendanceBLL();

        // --- 1. PROPERTY PHÂN QUYỀN (MỚI) ---
        // Property này giúp View có thể binding để ẩn/hiện nút "Điểm danh"
        // Admin chỉ xem -> Nút sẽ bị ẩn hoặc disable
        public bool IsTeacher => UserSession.Role == "Teacher";

        [ObservableProperty] private ClassDTO _selectedClass;

        partial void OnSelectedClassChanged(ClassDTO value)
        {
            LoadData();
            LoadHistory(); // Tải danh sách ngày đã điểm danh khi chọn lớp
            CancelEdit();
        }

        [ObservableProperty] private DateTime _selectedDate = DateTime.Now;
        partial void OnSelectedDateChanged(DateTime value) => LoadData();

        [ObservableProperty] private ObservableCollection<AttendanceDisplayDTO> _attendanceList;

        // --- LỊCH SỬ ĐIỂM DANH ---
        [ObservableProperty]
        private ObservableCollection<DateTime> _attendanceDates;

        // --- THỐNG KÊ ---
        [ObservableProperty] private int _countPresent;
        [ObservableProperty] private int _countAbsent;
        [ObservableProperty] private int _countExcused;
        [ObservableProperty] private int _countLate;

        // --- CHẾ ĐỘ CHỈNH SỬA ---
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsReadOnlyMode))]
        private bool _isEditing;
        public bool IsReadOnlyMode => !IsEditing;

        public ClassAttendanceViewModel()
        {
            AttendanceList = new ObservableCollection<AttendanceDisplayDTO>();
            AttendanceDates = new ObservableCollection<DateTime>(); // Khởi tạo
            IsEditing = false;
        }

        public void LoadData()
        {
            if (SelectedClass == null) return;

            try
            {
                var dt = _bll.GetAttendanceByClassAndDate(SelectedClass.ClassID, SelectedDate);
                var list = new ObservableCollection<AttendanceDisplayDTO>();
                int index = 1;

                foreach (DataRow row in dt.Rows)
                {
                    // Lấy status an toàn
                    string status = row["Status"] != DBNull.Value ? row["Status"].ToString() : "";

                    list.Add(new AttendanceDisplayDTO
                    {
                        Index = index++,
                        EnrollmentID = Convert.ToInt32(row["EnrollmentID"]),
                        AttendanceID = row["AttendanceID"] != DBNull.Value ? Convert.ToInt32(row["AttendanceID"]) : (int?)null,
                        Status = status,

                        StudentID = Convert.ToInt32(row["StudentID"]),
                        StudentCode = row["StudentID"].ToString(),
                        Name = row["Name"].ToString(),
                        Note = row["Note"] != DBNull.Value ? row["Note"].ToString() : "",

                        ParentVM = this
                    });
                }
                AttendanceList = list;
                CalculateStats();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message); }
        }

        // Tải danh sách các ngày đã điểm danh từ DB
        public void LoadHistory()
        {
            if (SelectedClass == null) return;
            try
            {
                // Gọi hàm BLL lấy danh sách ngày
                var dates = _bll.GetHistoryDates(SelectedClass.ClassID);
                AttendanceDates = new ObservableCollection<DateTime>(dates);
            }
            catch { /* Bỏ qua lỗi nếu chưa có hàm BLL */ }
        }

        public void CalculateStats()
        {
            if (AttendanceList == null) return;
            CountPresent = AttendanceList.Count(x => x.Status == "Present");
            CountAbsent = AttendanceList.Count(x => x.Status == "Absent");
            CountExcused = AttendanceList.Count(x => x.Status == "Excused");
            CountLate = AttendanceList.Count(x => x.Status == "Late");
        }

        // --- COMMANDS ---

        [RelayCommand]
        private void StartEdit()
        {
            // --- 2. KIỂM TRA QUYỀN (MỚI) ---
            // Chỉ giáo viên mới được phép bắt đầu điểm danh
            if (!IsTeacher)
            {
                MessageBox.Show("Chỉ giáo viên phụ trách mới có quyền thực hiện điểm danh.",
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
            LoadData();
        }

        [RelayCommand]
        private void SaveAttendance()
        {
            // --- 3. BẢO MẬT 2 LỚP (MỚI) ---
            if (!IsTeacher)
            {
                MessageBox.Show("Bạn không có quyền lưu dữ liệu này.");
                return;
            }

            try
            {
                int successCount = 0;

                foreach (var item in AttendanceList)
                {
                    var dto = new AttendanceDTO
                    {
                        AttendanceID = item.AttendanceID ?? 0,
                        StudentID = item.StudentID,
                        ClassID = SelectedClass.ClassID,
                        SessionDate = SelectedDate,
                        Status = item.Status,
                        Note = item.Note
                    };

                    if (_bll.Insert(dto))
                    {
                        // Nếu lưu thành công, cập nhật AttendanceID mới vào item để UI đồng bộ
                        if (item.AttendanceID == null || item.AttendanceID == 0)
                        {
                            item.AttendanceID = dto.AttendanceID;
                        }
                        successCount++;
                    }
                }

                MessageBox.Show("Đã lưu điểm danh thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                IsEditing = false;

                LoadData();    // Load lại dữ liệu sạch sẽ
                LoadHistory(); // Cập nhật lại lịch sử ngày điểm danh
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu dữ liệu: " + ex.Message);
            }
        }
    }
}