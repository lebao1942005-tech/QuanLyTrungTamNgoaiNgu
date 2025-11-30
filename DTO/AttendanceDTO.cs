using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class AttendanceDTO
    {
        public int AttendanceID { get; set; }
        public int StudentID { get; set; }
        public int ClassID { get; set; }
        public DateTime SessionDate { get; set; }
        public string Status { get; set; }
        public string Note { get; set; }
    }
}
