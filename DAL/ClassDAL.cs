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
    public class ClassDAL
    {
        private readonly Database db = new Database();

        // Lấy tất cả lớp học
        public DataTable GetAllClasses()
        {
            string query = "SELECT * FROM Class";
            return db.ExecuteQuery(query);
        }

        // Thêm lớp học
        public bool InsertClass(ClassDTO c)
        {
            if (IsTeacherScheduleConflict(c))
                throw new Exception("Giáo viên đã có lớp khác trùng lịch!");

            string query = @"
                INSERT INTO Class (ClassName, CourseID, TeacherID, Schedule, StartDate, EndDate, MaxStudents)
                VALUES (@ClassName, @CourseID, @TeacherID, @Schedule, @StartDate, @EndDate, @MaxStudents)";

            SqlParameter[] parameters = {
                new SqlParameter("@ClassName", c.ClassName),
                new SqlParameter("@CourseID", c.CourseID),
                new SqlParameter("@TeacherID", c.TeacherID),
                new SqlParameter("@Schedule", (object)c.Schedule ?? DBNull.Value),
                new SqlParameter("@StartDate", (object)c.StartDate ?? DBNull.Value),
                new SqlParameter("@EndDate", (object)c.EndDate ?? DBNull.Value),
                new SqlParameter("@MaxStudents", c.MaxStudents)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        // Cập nhật lớp học
        public bool UpdateClass(ClassDTO c)
        {
            if (IsTeacherScheduleConflict(c))
                throw new Exception("Giáo viên đã có lớp khác trùng lịch!");

            string query = @"
                UPDATE Class
                SET ClassName = @ClassName,
                    CourseID = @CourseID,
                    TeacherID = @TeacherID,
                    Schedule = @Schedule,
                    StartDate = @StartDate,
                    EndDate = @EndDate,
                    MaxStudents = @MaxStudents
                WHERE ClassID = @ClassID";

            SqlParameter[] parameters = {
                new SqlParameter("@ClassID", c.ClassID),
                new SqlParameter("@ClassName", c.ClassName),
                new SqlParameter("@CourseID", c.CourseID),
                new SqlParameter("@TeacherID", c.TeacherID),
                new SqlParameter("@Schedule", (object)c.Schedule ?? DBNull.Value),
                new SqlParameter("@StartDate", (object)c.StartDate ?? DBNull.Value),
                new SqlParameter("@EndDate", (object)c.EndDate ?? DBNull.Value),
                new SqlParameter("@MaxStudents", c.MaxStudents)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        // Xóa lớp học
        public bool DeleteClass(int classID)
        {
            string query = "DELETE FROM Class WHERE ClassID = @ClassID";
            SqlParameter[] parameters = { new SqlParameter("@ClassID", classID) };
            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        // Kiểm tra trùng lịch của giáo viên
        public bool IsTeacherScheduleConflict(ClassDTO c)
        {
            string query = @"
                SELECT COUNT(*) 
                FROM Class 
                WHERE TeacherID = @TeacherID 
                  AND Schedule = @Schedule
                  AND ClassID <> @ClassID";

            SqlParameter[] parameters = {
                new SqlParameter("@TeacherID", c.TeacherID),
                new SqlParameter("@Schedule", (object)c.Schedule ?? DBNull.Value),
                new SqlParameter("@ClassID", c.ClassID) // loại trừ chính lớp đang update
            };

            int count = (int)db.ExecuteScalar(query, parameters);
            return count > 0;
        }

        // Lấy danh sách giáo viên đang bận trong lịch
        public DataTable GetTeachersBySchedule(string schedule)
        {
            string query = @"
                SELECT DISTINCT t.TeacherID, t.TeacherName
                FROM Teacher t
                INNER JOIN Class c ON t.TeacherID = c.TeacherID
                WHERE c.Schedule = @Schedule";

            SqlParameter[] parameters = { new SqlParameter("@Schedule", schedule) };
            return db.ExecuteQuery(query, parameters);
        }

        // Lấy danh sách giáo viên trống lịch
        public DataTable GetFreeTeachersBySchedule(string schedule)
        {
            string query = @"
                SELECT TeacherID, TeacherName
                FROM Teacher
                WHERE TeacherID NOT IN (
                    SELECT TeacherID FROM Class WHERE Schedule = @Schedule
                )";

            SqlParameter[] parameters = { new SqlParameter("@Schedule", schedule) };
            return db.ExecuteQuery(query, parameters);
        }
    }
}
