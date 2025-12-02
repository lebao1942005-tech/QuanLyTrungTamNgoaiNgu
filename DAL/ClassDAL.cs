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
        //Lấy học sinh theo lớp học
        public DataTable GetStudentsByClassID(int classID)
        {
            string query = @"
                SELECT s.StudentID, s.Name, s.Birthday, s.Phone, s.Email
                FROM Student s
                INNER JOIN Enrollment e ON s.StudentID = e.StudentID
                WHERE e.ClassID = @ClassID";
            SqlParameter[] parameters = { new SqlParameter("@ClassID", classID) };
            return db.ExecuteQuery(query, parameters);
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
                new SqlParameter("@ClassID", c.ClassID)
            };

            int count = (int)db.ExecuteScalar(query, parameters);
            return count > 0;
        }

        public DataTable GetTeachersBySchedule(string schedule)
        {
            // SỬA: Đổi t.TeacherName thành t.Name
            string query = @"
                SELECT DISTINCT t.TeacherID, t.Name as TeacherName
                FROM Teacher t
                INNER JOIN Class c ON t.TeacherID = c.TeacherID
                WHERE c.Schedule = @Schedule";

            SqlParameter[] parameters = { new SqlParameter("@Schedule", schedule) };
            return db.ExecuteQuery(query, parameters);
        }

        public DataTable GetFreeTeachersBySchedule(string schedule)
        {
            // SỬA: Đổi TeacherName thành Name
            string query = @"
                SELECT TeacherID, Name as TeacherName
                FROM Teacher
                WHERE TeacherID NOT IN (
                    SELECT TeacherID FROM Class WHERE Schedule = @Schedule
                )";

            SqlParameter[] parameters = { new SqlParameter("@Schedule", schedule) };
            return db.ExecuteQuery(query, parameters);
        }

        public List<ClassDTO> GetAllClassesDetailed()
        {
            // JOIN 3 bảng: Class, Teacher, Course
            // SỬA LỖI TẠI ĐÂY: Thay "t.TeacherName" thành "t.Name AS TeacherName"
            string query = @"
                SELECT 
                    c.ClassID, c.ClassName, c.Schedule, c.StartDate, c.EndDate, c.MaxStudents, 
                    c.TeacherID, c.CourseID,
                    t.Name AS TeacherName,  
                    co.CourseName, co.BaseFee
                FROM Class c
                LEFT JOIN Teacher t ON c.TeacherID = t.TeacherID
                LEFT JOIN Course co ON c.CourseID = co.CourseID";

            DataTable dt = db.ExecuteQuery(query);
            List<ClassDTO> list = new List<ClassDTO>();

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new ClassDTO
                {
                    ClassID = Convert.ToInt32(row["ClassID"]),
                    ClassName = row["ClassName"].ToString(),
                    Schedule = row["Schedule"].ToString(),
                    StartDate = row["StartDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["StartDate"]),
                    EndDate = row["EndDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["EndDate"]),
                    MaxStudents = Convert.ToInt32(row["MaxStudents"]),

                    // Các ID để dùng khi cần update
                    TeacherID = Convert.ToInt32(row["TeacherID"]),
                    CourseID = Convert.ToInt32(row["CourseID"]),

                    // Dữ liệu JOIN (Bây giờ row["TeacherName"] đã có dữ liệu nhờ AS TeacherName)
                    TeacherName = row["TeacherName"].ToString(),
                    CourseName = row["CourseName"].ToString(),
                    TuitionFee = row["BaseFee"] == DBNull.Value ? 0 : Convert.ToDecimal(row["BaseFee"])
                });
            }
            return list;
        }



        public int CountStudentsInClass(int classID)
        {
            // Truy vấn đếm số dòng trong bảng Enrollment (hoặc bảng đăng ký tương ứng) có ClassID trùng khớp
            string query = "SELECT COUNT(*) FROM Enrollment WHERE ClassID = @ClassID";

            SqlParameter[] parameters = {
                new SqlParameter("@ClassID", classID)
            };

            // Thực thi truy vấn và trả về số lượng (ExecuteScalar trả về object, cần ép kiểu)
            object result = db.ExecuteScalar(query, parameters);
            return result != null ? Convert.ToInt32(result) : 0;
        }


    }
}