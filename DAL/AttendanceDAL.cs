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

        public DataTable GetByClass(int classID)
        {
            string q = "SELECT * FROM Attendance WHERE ClassID=@CID";
            return db.ExecuteQuery(q, new[] { new SqlParameter("@CID", classID) });
        }

        public bool Insert(AttendanceDTO a)
        {
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
    }
}
