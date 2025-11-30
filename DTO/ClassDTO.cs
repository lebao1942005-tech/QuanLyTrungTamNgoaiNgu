using System;

namespace DTO
{
    public class ClassDTO
    {
        // --- CÁC TRƯỜNG GỐC (Map bảng Class) ---
        public int ClassID { get; set; }
        public string ClassName { get; set; }
        public int CourseID { get; set; }
        public int TeacherID { get; set; }
        public string Schedule { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int MaxStudents { get; set; } = 20;

        // --- CÁC TRƯỜNG BỔ SUNG (Lấy từ JOIN) ---
        public string TeacherName { get; set; } // Lấy từ bảng Teacher
        public string CourseName { get; set; }  // Lấy từ bảng Course
        public decimal TuitionFee { get; set; } // Map từ Course.BaseFee

        // --- THUỘC TÍNH HỖ TRỢ HIỂN THỊ TRÊN XAML ---

        // 1. Hiển thị giá tiền: "5,000,000 đ"
        public string FeeDisplay => string.Format("{0:N0} đ", TuitionFee);

        // 2. Hiển thị thời gian: "01/10/2025 - 30/12/2025"
        public string DurationDisplay
        {
            get
            {
                if (StartDate == null || EndDate == null) return "Chưa xếp lịch";
                return $"{StartDate.Value:dd/MM/yyyy} - {EndDate.Value:dd/MM/yyyy}";
            }
        }

        // 3. Sĩ số giả định (sau này bạn query COUNT từ bảng Enrollment vào đây)
        public int CurrentStudents { get; set; } = 0;
    }
}