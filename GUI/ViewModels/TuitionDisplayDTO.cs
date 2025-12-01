using CommunityToolkit.Mvvm.ComponentModel;
using DTO;
using System;

namespace GUI.ViewModels
{
    // Class này dùng để hiển thị lên DataGrid, hỗ trợ thông báo thay đổi (MVVM)
    public class TuitionDisplayDTO : ObservableObject
    {
        // Giữ đối tượng gốc để khi Save có thể lấy ID
        public TuitionDTO OriginalData { get; private set; }

        public TuitionDisplayDTO(TuitionDTO original)
        {
            OriginalData = original;
        }

        public int Index { get; set; } // Số thứ tự
        public string StudentCode { get; set; }
        public string StudentName { get; set; }

        // --- Các thuộc tính ánh xạ từ DTO gốc (có raise event để UI tự cập nhật) ---

        public decimal Amount
        {
            get => OriginalData.Amount;
            set
            {
                if (OriginalData.Amount != value)
                {
                    OriginalData.Amount = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime? PaidAt
        {
            get => OriginalData.PaidAt;
            set
            {
                if (OriginalData.PaidAt != value)
                {
                    OriginalData.PaidAt = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Status
        {
            get => OriginalData.Status;
            set
            {
                if (OriginalData.Status != value)
                {
                    OriginalData.Status = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsPaid)); // Cập nhật luôn cờ IsPaid
                }
            }
        }

        // Thuộc tính phụ trợ cho DataTrigger trong XAML
        public bool IsPaid => Status == "Paid";
    }
}