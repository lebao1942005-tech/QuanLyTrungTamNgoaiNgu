using DTO;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class TuitionDAL
    {
        private readonly Database db = new Database();

        // Lấy tất cả các Tuition
        public DataTable GetAllTuition()
        {
            string query = "SELECT * FROM Tuition";
            return db.ExecuteQuery(query);
        }

        // Lấy Tuition theo EnrollmentID
        public DataTable GetTuitionByEnrollmentID(int enrollmentID)
        {
            string query = "SELECT * FROM Tuition WHERE EnrollmentID = @EnrollmentID";

            SqlParameter[] parameters =
            {
                new SqlParameter("@EnrollmentID", enrollmentID)
            };

            return db.ExecuteQuery(query, parameters);
        }

        // Kiểm tra xem EnrollmentID đã có trong Tuition chưa (UNIQUE)
        public bool Exists(int enrollmentID)
        {
            string query = "SELECT COUNT(*) FROM Tuition WHERE EnrollmentID = @EnrollmentID";

            SqlParameter[] parameters =
            {
                new SqlParameter("@EnrollmentID", enrollmentID)
            };

            object result = db.ExecuteScalar(query, parameters);
            return Convert.ToInt32(result) > 0;
        }

        // Thêm mới Tuition — Amount sẽ được Trigger tự động set
        public bool InsertTuition(TuitionDTO t)
        {
            string query = @"
                INSERT INTO Tuition (EnrollmentID, Amount, Status, PaidAt)
                VALUES (@EnrollmentID, NULL, @Status, @PaidAt)";

            SqlParameter[] parameters =
            {
                new SqlParameter("@EnrollmentID", t.EnrollmentID),
                new SqlParameter("@Status", (object)t.Status ?? DBNull.Value),
                new SqlParameter("@PaidAt", (object)t.PaidAt ?? DBNull.Value)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        // Update trạng thái học phí
        public bool UpdateTuition(TuitionDTO t)
        {
            string query = @"
                UPDATE Tuition
                SET Status = @Status,
                    PaidAt = @PaidAt
                WHERE TuitionID = @TuitionID";

            SqlParameter[] parameters =
            {
                new SqlParameter("@TuitionID", t.TuitionID),
                new SqlParameter("@Status", t.Status),
                new SqlParameter("@PaidAt", (object)t.PaidAt ?? DBNull.Value)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        // Xóa
        public bool DeleteTuition(int tuitionID)
        {
            string query = "DELETE FROM Tuition WHERE TuitionID = @TuitionID";

            SqlParameter[] parameters =
            {
                new SqlParameter("@TuitionID", tuitionID)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }


        public DataTable GetTuitionByClassID(int classId)
        {
            // CẬP NHẬT: Join thêm Class và Course để lấy BaseFee
            string query = @"
        SELECT 
            t.TuitionID,
            e.EnrollmentID, 
            t.Amount,       -- Số tiền thực tế trong bảng Tuition (có thể null)
            co.BaseFee,     -- Học phí gốc từ khóa học
            t.Status,
            t.PaidAt,
            s.StudentID,
            s.Name AS StudentName,
            s.Phone         -- Lấy thêm nếu cần
        FROM Enrollment e
        JOIN Student s ON e.StudentID = s.StudentID
        LEFT JOIN Tuition t ON e.EnrollmentID = t.EnrollmentID
        JOIN Class c ON e.ClassID = c.ClassID       -- Join bảng Lớp
        JOIN Course co ON c.CourseID = co.CourseID  -- Join bảng Khóa học để lấy tiền
        WHERE e.ClassID = @ClassID";

            SqlParameter[] parameters =
            {
        new SqlParameter("@ClassID", classId)
    };

            return db.ExecuteQuery(query, parameters);
        }

    }
}
