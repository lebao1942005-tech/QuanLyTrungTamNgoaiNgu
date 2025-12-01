using BLL;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DTO;
using System;
using System.Collections.Generic; // Cần cho List<DateTime>
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;

namespace GUI.ViewModels
{
   
    


    public partial class ClassAttendanceViewModel : ObservableObject
    {
        private readonly AttendanceBLL _bll = new AttendanceBLL();

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
                    string status = row["Status"] != DBNull.Value && row["Status"] != null
                ? row["Status"].ToString()
                : "";


                    list.Add(new AttendanceDisplayDTO
                    {
                        Index = index++,
                        EnrollmentID = Convert.ToInt32(row["EnrollmentID"]),
                        AttendanceID = row["AttendanceID"] != DBNull.Value ? Convert.ToInt32(row["AttendanceID"]) : (int?)null,
                        Status = row["Status"] != DBNull.Value ? row["Status"].ToString() : "",

                        StudentID = Convert.ToInt32(row["StudentID"]),
                        StudentCode = $"HV{DateTime.Now.Year % 100}{row["StudentID"]:0000}",
                        Name = row["Name"].ToString(),
                        //Status = status,
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
                // Gọi hàm BLL lấy danh sách ngày (Cần thêm hàm này vào BLL/DAL)
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
        [RelayCommand] private void StartEdit() => IsEditing = true;

        [RelayCommand]
        private void CancelEdit()
        {
            IsEditing = false;
            LoadData();
        }

        [RelayCommand]
        private void SaveAttendance()
        {
            try
            {
                int successCount = 0;

                foreach (var item in AttendanceList)
                {
                    var dto = new AttendanceDTO
                    {
                        AttendanceID = item.AttendanceID ?? 0, // DAL upsert sẽ kiểm tra tồn tại
                        StudentID = item.StudentID,
                        ClassID = SelectedClass.ClassID,
                        SessionDate = SelectedDate,
                        Status = item.Status,
                        Note = item.Note
                    };

                    if (_bll.Insert(dto)) // Insert trong DAL đã upsert
                    {
                        // Nếu lưu thành công, cập nhật AttendanceID mới vào item
                        if (item.AttendanceID == null || item.AttendanceID == 0)
                        {
                            item.AttendanceID = dto.AttendanceID;
                        }
                        successCount++;
                    }
                }

                MessageBox.Show("Đã lưu điểm danh thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                IsEditing = false;
                LoadData(); // Load lại dữ liệu sạch sẽ
                LoadHistory();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu dữ liệu: " + ex.Message);
            }
        }

    }
}