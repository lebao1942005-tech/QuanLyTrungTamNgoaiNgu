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
    public class AttendanceDAL
    {
        private readonly Database db = new Database();

        public DataTable GetAttendanceByClassAndDate(int classId, DateTime date)
        {
            // Lấy dữ liệu điểm danh. Dùng LEFT JOIN để những ai chưa điểm danh sẽ có Status là NULL
            string query = @"
                SELECT 
                    e.EnrollmentID,
                    s.StudentID,
                    s.Name,
                    a.AttendanceID,
                    a.Status,
                    a.Note
                FROM Enrollment e
                JOIN Student s ON e.StudentID = s.StudentID
                LEFT JOIN Attendance a ON e.StudentID = a.StudentID 
                                       AND e.ClassID = a.ClassID 
                                       AND CAST(a.SessionDate AS DATE) = CAST(@Date AS DATE)
                WHERE e.ClassID = @ClassID AND e.Status = 'Active'";

            SqlParameter[] p =
            {
                new SqlParameter("@ClassID", classId),
                new SqlParameter("@Date", date.Date) // Quan trọng: Chỉ lấy phần ngày
            };

            return db.ExecuteQuery(query, p);
        }

        public DataTable GetByClass(int classID)
        {
            string q = "SELECT * FROM Attendance WHERE ClassID=@CID";
            return db.ExecuteQuery(q, new[] { new SqlParameter("@CID", classID) });
        }

        // SỬA LỖI QUAN TRỌNG: Thay thế Insert thường bằng Upsert (Kiểm tra tồn tại trước khi thêm)
        public bool Insert(AttendanceDTO a)
        {
            // 1. Kiểm tra xem đã có bản ghi điểm danh cho Học viên này, Lớp này, vào Ngày này chưa?
            string checkQuery = @"SELECT AttendanceID FROM Attendance 
                                  WHERE StudentID = @StudentID 
                                  AND ClassID = @ClassID 
                                  AND CAST(SessionDate AS DATE) = CAST(@SessionDate AS DATE)";

            SqlParameter[] checkParams = {
                new("@StudentID", a.StudentID),
                new("@ClassID", a.ClassID),
                new("@SessionDate", a.SessionDate.Date)
            };

            object result = db.ExecuteScalar(checkQuery, checkParams);

            // 2. Nếu đã có, chuyển sang gọi hàm Update thay vì Insert
            if (result != null && int.TryParse(result.ToString(), out int existingID))
            {
                a.AttendanceID = existingID;
                return Update(a);
            }

            // 3. Nếu chưa có, thực hiện Insert mới
            string q = @"
                INSERT INTO Attendance (StudentID, ClassID, SessionDate, Status, Note)
                VALUES (@StudentID, @ClassID, @SessionDate, @Status, @Note)";

            SqlParameter[] p =
            {
                new("@StudentID", a.StudentID),
                new("@ClassID", a.ClassID),
                new("@SessionDate", a.SessionDate),
                new("@Status", a.Status),
                new("@Note", (object?)a.Note ?? DBNull.Value)
            };

            return db.ExecuteNonQuery(q, p) > 0;
        }

        public bool Update(AttendanceDTO a)
        {
            string q = @"
                UPDATE Attendance
                SET Status=@Status, Note=@Note
                WHERE AttendanceID=@ID";

            SqlParameter[] p =
            {
                new("@ID", a.AttendanceID),
                new("@Status", a.Status),
                new("@Note", (object?)a.Note ?? DBNull.Value)
            };

            return db.ExecuteNonQuery(q, p) > 0;
        }

        public bool Delete(int id)
        {
            string q = "DELETE FROM Attendance WHERE AttendanceID=@ID";
            SqlParameter[] p = { new("@ID", id) };
            return db.ExecuteNonQuery(q, p) > 0;
        }

        public List<DateTime> GetHistoryDates(int classId)
        {
            List<DateTime> dates = new List<DateTime>();
            string query = "SELECT DISTINCT CAST(SessionDate AS DATE) as SessionDate FROM Attendance WHERE ClassID = @CID ORDER BY SessionDate DESC";

            DataTable dt = db.ExecuteQuery(query, new SqlParameter[] {
                new SqlParameter("@CID", classId)
            });

            foreach (DataRow r in dt.Rows)
            {
                if (r["SessionDate"] != DBNull.Value)
                    dates.Add(Convert.ToDateTime(r["SessionDate"]));
            }
            return dates;
        }
    }
}