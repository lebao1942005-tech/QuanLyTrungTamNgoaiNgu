using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class CourseDTO
    {
        public int CourseID { get; set; }          // Khóa chính
        public string CourseName { get; set; }     // Tên khóa học
        public int DurationMonths { get; set; }    // Số tháng học
        public decimal BaseFee { get; set; }       // Học phí cơ bản

    }
}
