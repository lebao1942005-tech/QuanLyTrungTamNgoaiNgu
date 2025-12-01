using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DTO;
using DAL;
using System;
using System.Collections.Generic; // Cần thiết cho List<>
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Linq;

namespace GUI.ViewModels
{
    public partial class TuitionViewModel : ObservableObject
    {
        private readonly TuitionDAL _tuitionDAL = new TuitionDAL();
        private readonly ClassDAL _classDAL = new ClassDAL();

        // --- DANH SÁCH DỮ LIỆU ---
        [ObservableProperty] private ObservableCollection<ClassDTO> _classes;

        // Đây là danh sách hiển thị lên màn hình (đã lọc)
        [ObservableProperty] private ObservableCollection<TuitionDisplayDTO> _tuitionList;

        // Cache danh sách gốc (chưa lọc) để không phải gọi DB liên tục
        private List<TuitionDisplayDTO> _allTuitionCache;

        // --- SELECTION & FILTER ---
        [ObservableProperty] private ClassDTO _selectedClass;

        partial void OnSelectedClassChanged(ClassDTO value)
        {
            LoadTuitionData();
        }

        // Biến lưu vị trí chọn của ComboBox Trạng thái
        // 0: Tất cả, 1: Chưa đóng, 2: Đã hoàn thành
        [ObservableProperty]
        private int _selectedStatusIndex = 0;

        // Khi người dùng đổi ComboBox -> Tự động chạy hàm lọc lại
        partial void OnSelectedStatusIndexChanged(int value)
        {
            ApplyFilter();
        }

        // --- POPUP LOGIC ---
        [ObservableProperty] private bool _isPaymentPopupOpen;
        [ObservableProperty] private TuitionDisplayDTO _selectedTuition;

        public TuitionViewModel()
        {
            LoadClasses();
        }

        private void LoadClasses()
        {
            try
            {
                var dt = _classDAL.GetAllClasses();
                var tempList = new List<ClassDTO>();

                if (dt != null)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        tempList.Add(new ClassDTO
                        {
                            ClassID = Convert.ToInt32(row["ClassID"]),
                            ClassName = row["ClassName"].ToString(),
                        });
                    }
                }

                Classes = new ObservableCollection<ClassDTO>(tempList);
                if (Classes.Count > 0) SelectedClass = Classes[0];
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách lớp: " + ex.Message);
            }
        }

        public void LoadTuitionData()
        {
            if (SelectedClass == null) return;

            try
            {
                DataTable dt = _tuitionDAL.GetTuitionByClassID(SelectedClass.ClassID);

                // Dùng List thường để lưu tạm
                var list = new List<TuitionDisplayDTO>();
                int index = 1;

                if (dt != null)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        decimal tuitionAmount = row["Amount"] != DBNull.Value ? Convert.ToDecimal(row["Amount"]) : 0;
                        decimal courseFee = 0;
                        if (dt.Columns.Contains("BaseFee") && row["BaseFee"] != DBNull.Value)
                        {
                            courseFee = Convert.ToDecimal(row["BaseFee"]);
                        }

                        decimal finalAmount = tuitionAmount > 0 ? tuitionAmount : courseFee;

                        var rawDto = new TuitionDTO
                        {
                            TuitionID = row["TuitionID"] != DBNull.Value ? Convert.ToInt32(row["TuitionID"]) : 0,
                            EnrollmentID = Convert.ToInt32(row["EnrollmentID"]),
                            Amount = finalAmount,
                            Status = row["Status"] != DBNull.Value ? row["Status"].ToString() : "Unpaid",
                            PaidAt = row["PaidAt"] != DBNull.Value ? Convert.ToDateTime(row["PaidAt"]) : (DateTime?)null
                        };

                        var displayDto = new TuitionDisplayDTO(rawDto)
                        {
                            Index = index++,
                            StudentName = row["StudentName"].ToString(),
                            StudentCode = $"HV{row["StudentID"]:0000}"
                        };

                        list.Add(displayDto);
                    }
                }

                // 1. Lưu vào cache
                _allTuitionCache = list;

                // 2. Gọi hàm lọc để hiển thị đúng theo ComboBox đang chọn
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu học phí: " + ex.Message);
            }
        }

        // --- HÀM LỌC DỮ LIỆU ---
        private void ApplyFilter()
        {
            if (_allTuitionCache == null) return;

            IEnumerable<TuitionDisplayDTO> query = _allTuitionCache;

            switch (SelectedStatusIndex)
            {
                case 1: // Chưa đóng (Unpaid)
                    query = query.Where(x => !x.IsPaid);
                    break;
                case 2: // Đã hoàn thành (Paid)
                    query = query.Where(x => x.IsPaid);
                    break;
                    // case 0: Lấy tất cả (Mặc định)
            }

            // Cập nhật lại danh sách hiển thị trên UI
            // Lưu ý: Cần đánh lại số thứ tự (Index) cho đẹp nếu muốn
            var filteredList = query.ToList();
            int newIndex = 1;
            foreach (var item in filteredList) item.Index = newIndex++;

            TuitionList = new ObservableCollection<TuitionDisplayDTO>(filteredList);
        }

        // --- COMMANDS ---

        [RelayCommand]
        private void OpenPaymentDialog(TuitionDisplayDTO item)
        {
            SelectedTuition = item;
            IsPaymentPopupOpen = true;
        }

        [RelayCommand]
        private void ConfirmPayment()
        {
            if (SelectedTuition == null) return;

            try
            {
                var dtoToUpdate = SelectedTuition.OriginalData;

                dtoToUpdate.Status = "Paid";
                dtoToUpdate.PaidAt = DateTime.Now;

                bool success = false;

                if (dtoToUpdate.TuitionID == 0)
                {
                    if (_tuitionDAL.InsertTuition(dtoToUpdate)) success = true;
                }
                else
                {
                    if (_tuitionDAL.UpdateTuition(dtoToUpdate)) success = true;
                }

                if (success)
                {
                    SelectedTuition.Status = "Paid";
                    SelectedTuition.PaidAt = DateTime.Now;

                    MessageBox.Show($"Đã thu học phí của {SelectedTuition.StudentName} thành công!");
                    IsPaymentPopupOpen = false;

                    // Load lại dữ liệu để cập nhật cache và filter lại (dòng vừa đóng sẽ biến mất nếu đang lọc 'Chưa đóng')
                    LoadTuitionData();
                }
                else
                {
                    MessageBox.Show("Lỗi cập nhật CSDL. Vui lòng thử lại.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hệ thống: " + ex.Message);
            }
        }

        [RelayCommand]
        private void CancelPayment()
        {
            IsPaymentPopupOpen = false;
        }
    }
}