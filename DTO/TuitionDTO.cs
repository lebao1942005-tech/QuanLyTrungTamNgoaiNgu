using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class TuitionDTO
    {
        public int TuitionID { get; set; }         // Khóa chính
        public int EnrollmentID { get; set; }      // 1-1 với Enrollment
        public decimal Amount { get; set; }        // Học phí (tự động từ trigger)
        public string Status { get; set; }         // Unpaid / Paid
        public DateTime? PaidAt { get; set; }      // Ngày thanh toán (nullable)
    }
}
