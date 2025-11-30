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
    public class EnrollmentDAL
    {
        private readonly Database db = new Database();

        // Lấy tất cả Enrollment
        public DataTable GetAllEnrollments()
        {
            string query = "SELECT * FROM Enrollment";
            return db.ExecuteQuery(query);
        }

        // Thêm Enrollment
        public bool InsertEnrollment(EnrollmentDTO e)
        {
            string query = @"
                INSERT INTO Enrollment (StudentID, ClassID, EnrollDate, Status)
                VALUES (@StudentID, @ClassID, @EnrollDate, @Status)";

            SqlParameter[] parameters =
            {
                new SqlParameter("@StudentID", e.StudentID),
                new SqlParameter("@ClassID", e.ClassID),
                new SqlParameter("@EnrollDate", e.EnrollDate),
                new SqlParameter("@Status", e.Status)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        // Cập nhật Enrollment
        public bool UpdateEnrollment(EnrollmentDTO e)
        {
            string query = @"
                UPDATE Enrollment
                SET StudentID = @StudentID,
                    ClassID = @ClassID,
                    EnrollDate = @EnrollDate,
                    Status = @Status
                WHERE EnrollmentID = @EnrollmentID";

            SqlParameter[] parameters =
            {
                new SqlParameter("@EnrollmentID", e.EnrollmentID),
                new SqlParameter("@StudentID", e.StudentID),
                new SqlParameter("@ClassID", e.ClassID),
                new SqlParameter("@EnrollDate", e.EnrollDate),
                new SqlParameter("@Status", e.Status)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        // Xóa Enrollment
        public bool DeleteEnrollment(int enrollmentID)
        {
            string query = "DELETE FROM Enrollment WHERE EnrollmentID = @EnrollmentID";
            SqlParameter[] parameters =
            {
                new SqlParameter("@EnrollmentID", enrollmentID)
            };
            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        // Kiểm tra xem StudentID + ClassID đã tồn tại chưa
        public bool Exists(int studentID, int classID)
        {
            string query = @"
                SELECT COUNT(*) 
                FROM Enrollment
                WHERE StudentID = @StudentID AND ClassID = @ClassID";

            SqlParameter[] parameters =
            {
                new SqlParameter("@StudentID", studentID),
                new SqlParameter("@ClassID", classID)
            };

            object result = db.ExecuteScalar(query, parameters);
            return Convert.ToInt32(result) > 0;
        }
    }
}
