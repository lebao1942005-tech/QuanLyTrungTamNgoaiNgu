using Microsoft.Data.SqlClient;
using System;

namespace DAL
{
    public class DashboardDAL
    {
        private readonly Database db = new Database();

        // 1. Tổng số học viên (Đếm active enrollment hoặc count student)
        public int GetTotalStudents()
        {
            // Đếm tổng số học viên đã đăng ký (Status = Active)
            string query = "SELECT COUNT(DISTINCT StudentID) FROM Enrollment WHERE Status = 'Active'";
            object result = db.ExecuteScalar(query);
            return result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }

        // 2. Tổng doanh thu (Đã thu)
        public decimal GetTotalRevenue()
        {
            // Join các bảng để lấy học phí gốc từ Course, chỉ tính những dòng Tuition có Status = 'Paid'
            string query = @"
                SELECT SUM(co.BaseFee)
                FROM Tuition t
                JOIN Enrollment e ON t.EnrollmentID = e.EnrollmentID
                JOIN Class c ON e.ClassID = c.ClassID
                JOIN Course co ON c.CourseID = co.CourseID
                WHERE t.Status = 'Paid'";

            object result = db.ExecuteScalar(query);
            return result != DBNull.Value ? Convert.ToDecimal(result) : 0;
        }

        // 3. Học phí chưa thu (Nợ)
        public decimal GetTotalDebt()
        {
            // Tương tự, nhưng tính Status = 'Unpaid' hoặc NULL
            string query = @"
                SELECT SUM(co.BaseFee)
                FROM Tuition t
                JOIN Enrollment e ON t.EnrollmentID = e.EnrollmentID
                JOIN Class c ON e.ClassID = c.ClassID
                JOIN Course co ON c.CourseID = co.CourseID
                WHERE t.Status IS NULL OR t.Status = 'Unpaid'";

            object result = db.ExecuteScalar(query);
            return result != DBNull.Value ? Convert.ToDecimal(result) : 0;
        }

        // 4. Tỉ lệ chuyên cần (%)
        public double GetAttendanceRate()
        {
            // Công thức: (Số buổi đi học / Tổng số buổi đã điểm danh) * 100
            string query = @"
                SELECT 
                    CAST(SUM(CASE WHEN Status = 'Present' THEN 1 ELSE 0 END) AS FLOAT) 
                    / NULLIF(COUNT(*), 0) * 100
                FROM Attendance";

            object result = db.ExecuteScalar(query);
            return result != DBNull.Value ? Convert.ToDouble(result) : 0;
        }


        public Dictionary<string, decimal> GetRevenueLast6Months()
        {
            var data = new Dictionary<string, decimal>();

            // Query lấy doanh thu group by Tháng/Năm
            // FORMAT(t.PaidAt, 'MM/yyyy') có thể khác tùy phiên bản SQL, dùng MONTH/YEAR an toàn hơn
            string query = @"
        SELECT 
            FORMAT(t.PaidAt, 'MM/yyyy') as Month,
            SUM(co.BaseFee) as Total
        FROM Tuition t
        JOIN Enrollment e ON t.EnrollmentID = e.EnrollmentID
        JOIN Class c ON e.ClassID = c.ClassID
        JOIN Course co ON c.CourseID = co.CourseID
        WHERE t.Status = 'Paid' 
          AND t.PaidAt >= DATEADD(MONTH, -6, GETDATE())
        GROUP BY FORMAT(t.PaidAt, 'MM/yyyy'), YEAR(t.PaidAt), MONTH(t.PaidAt)
        ORDER BY YEAR(t.PaidAt), MONTH(t.PaidAt)";

            var dt = db.ExecuteQuery(query);

            foreach (System.Data.DataRow row in dt.Rows)
            {
                string month = row["Month"].ToString();
                decimal total = Convert.ToDecimal(row["Total"]);
                data[month] = total;
            }

            return data;
        }


    }
}