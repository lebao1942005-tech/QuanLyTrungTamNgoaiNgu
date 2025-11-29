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
    public class CourseDAL
    {
        private readonly Database db = new Database();

        // Lấy tất cả khóa học
        public DataTable GetAllCourses()
        {
            string query = "SELECT * FROM Course";
            return db.ExecuteQuery(query);
        }

        // Thêm khóa học
        public bool InsertCourse(CourseDTO c)
        {
            string query = @"
                INSERT INTO Course (CourseName, Certificate, BaseFee)
                VALUES (@CourseName, @Certificate, @BaseFee)";

            SqlParameter[] parameters = {
                new SqlParameter("@CourseName", c.CourseName),
                new SqlParameter("@Certificate", (object)c.Certificate ?? DBNull.Value),
                new SqlParameter("@BaseFee", c.BaseFee)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        // Cập nhật khóa học
        public bool UpdateCourse(CourseDTO c)
        {
            string query = @"
                UPDATE Course
                SET CourseName = @CourseName,
                    Certificate = @Certificate,
                    BaseFee = @BaseFee
                WHERE CourseID = @CourseID";

            SqlParameter[] parameters = {
                new SqlParameter("@CourseID", c.CourseID),
                new SqlParameter("@CourseName", c.CourseName),
                new SqlParameter("@Certificate", (object)c.Certificate ?? DBNull.Value),
                new SqlParameter("@BaseFee", c.BaseFee)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        // Xóa khóa học
        public bool DeleteCourse(int courseID)
        {
            string query = "DELETE FROM Course WHERE CourseID = @CourseID";
            SqlParameter[] parameters = { new SqlParameter("@CourseID", courseID) };
            return db.ExecuteNonQuery(query, parameters) > 0;
        }
    }
}
