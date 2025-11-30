using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class EnrollmentDTO
    {
        public int EnrollmentID { get; set; }      // Khóa chính
        public int StudentID { get; set; }         // Khóa ngoại -> Student
        public int ClassID { get; set; }           // Khóa ngoại -> Class
        public DateTime EnrollDate { get; set; }   // Ngày đăng ký
        public string Status { get; set; }         // Trạng thái (Active / Cancelled / Completed)
    }
}
