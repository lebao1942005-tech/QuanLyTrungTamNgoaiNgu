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
    public class ExamResultDAL
    {
        private readonly Database db = new Database();

        public DataTable GetAll()
        {
            string q = "SELECT * FROM ExamResult";
            return db.ExecuteQuery(q);
        }

        public bool Insert(ExamResultDTO r)
        {
            string q = @"
                INSERT INTO ExamResult (EnrollmentID, Score, GradingDate, Note)
                VALUES (@EnrollmentID, @Score, @GradingDate, @Note)";

            SqlParameter[] p =
            {
                new("@EnrollmentID", r.EnrollmentID),
                new("@Score", (object?)r.Score ?? DBNull.Value),
                new("@GradingDate", (object?)r.GradingDate ?? DBNull.Value),
                new("@Note", (object?)r.Note ?? DBNull.Value)
            };

            return db.ExecuteNonQuery(q, p) > 0;
        }

        public bool Update(ExamResultDTO r)
        {
            string q = @"
                UPDATE ExamResult
                SET Score=@Score, GradingDate=@GradingDate, Note=@Note
                WHERE ResultID=@ResultID";

            SqlParameter[] p =
            {
                new("@ResultID", r.ResultID),
                new("@Score", (object?)r.Score ?? DBNull.Value),
                new("@GradingDate", (object?)r.GradingDate ?? DBNull.Value),
                new("@Note", (object?)r.Note ?? DBNull.Value)
            };

            return db.ExecuteNonQuery(q, p) > 0;
        }

        public bool Delete(int id)
        {
            string q = "DELETE FROM ExamResult WHERE ResultID=@ID";
            SqlParameter[] p = { new("@ID", id) };
            return db.ExecuteNonQuery(q, p) > 0;
        }
        public DataTable GetStudentByIdEnrollment(int id)
        {
            string q = @"
            SELECT 
                st.StudentID,
                st.FullName,
                st.Gender,
                st.DateOfBirth,
                c.ClassName,
                ex.Score,
                ex.GradingDate,
                ex.Note
            FROM Student st
            JOIN Enrollment en ON en.StudentID = st.StudentID
            JOIN Class c ON c.ClassID = en.ClassID
            JOIN ExamResult ex ON ex.EnrollmentID = en.EnrollmentID
            WHERE en.EnrollmentID = @ID";

            SqlParameter[] p =
            {
                new SqlParameter("@ID", id)
            };

            return db.ExecuteQuery(q, p);

        }


        public DataTable GetGradingListByClass(int classId)
        {
            // Kỹ thuật LEFT JOIN: Lấy tất cả học viên trong lớp (Enrollment + Student)
            // Kết hợp với bảng ExamResult. Nếu chưa chấm điểm thì các cột Score, Note sẽ là NULL.
            string query = @"
                SELECT 
                    e.EnrollmentID,
                    s.StudentID,
                    s.Name,
                    er.ResultID,
                    er.Score,
                    er.GradingDate,
                    er.Note
                FROM Enrollment e
                JOIN Student s ON e.StudentID = s.StudentID
                LEFT JOIN ExamResult er ON e.EnrollmentID = er.EnrollmentID
                WHERE e.ClassID = @ClassID AND e.Status = 'Active'";

            SqlParameter[] parameters = { new SqlParameter("@ClassID", classId) };
            return db.ExecuteQuery(query, parameters);
        }

    }
}
