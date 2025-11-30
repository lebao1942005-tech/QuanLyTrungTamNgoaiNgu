using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class ExamResultDTO
    {
        public int ResultID { get; set; }
        public int EnrollmentID { get; set; }
        public decimal? Score { get; set; }
        public DateTime? GradingDate { get; set; }
        public string Note { get; set; }
    }
}
