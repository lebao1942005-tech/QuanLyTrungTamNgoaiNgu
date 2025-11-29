using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class CourseDTO
    {
        public int CourseID { get; set; }
        public string CourseName { get; set; }
        public string Certificate { get; set; }
        public decimal BaseFee { get; set; }
    }
}
