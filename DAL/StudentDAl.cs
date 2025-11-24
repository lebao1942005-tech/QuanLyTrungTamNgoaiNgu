using DTO;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

public class StudentDAL
{
    private readonly Database db = new Database();

    // ✔ Lấy tất cả sinh viên
   public DataTable GetAllStudents()
    {
        string sql = "SELECT * FROM Student";
        return db.ExecuteQuery(sql);
    }

    // ✔ Lấy 1 sinh viên theo ID
    public StudentDTO GetStudentById(int id)
    {
        string sql = "SELECT * FROM Student WHERE StudentID = @ID";

        SqlParameter[] parameters =
        {
            new SqlParameter("@ID", id)
        };

        DataTable dt = db.ExecuteQuery(sql, parameters);

        if (dt.Rows.Count == 0)
            return null;

        DataRow row = dt.Rows[0];

        return new StudentDTO
        {
            StudentID = Convert.ToInt32(row["StudentID"]),
            Name = row["Name"].ToString(),
            Birthday = row["Birthday"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(row["Birthday"]),
            Phone = row["Phone"]?.ToString(),
            Email = row["Email"]?.ToString()
        };
    }

    // ✔ Thêm sinh viên (không truyền StudentID vì IDENTITY tự tăng)
    public bool InsertStudent(StudentDTO st)
    {
        string sql = @"
            INSERT INTO Student (Name, Birthday, Phone, Email)
            VALUES (@Name, @Birthday, @Phone, @Email)";

        SqlParameter[] parameters =
        {
            new SqlParameter("@Name", st.Name),
            new SqlParameter("@Birthday", (object)st.Birthday ?? DBNull.Value),
            new SqlParameter("@Phone", (object)st.Phone ?? DBNull.Value),
            new SqlParameter("@Email", (object)st.Email ?? DBNull.Value)
        };

        return db.ExecuteNonQuery(sql, parameters) > 0;
    }

    // ✔ Cập nhật sinh viên
    public bool UpdateStudent(StudentDTO st)
    {
        string sql = @"
            UPDATE Student
            SET Name = @Name,
                Birthday = @Birthday,
                Phone = @Phone,
                Email = @Email
            WHERE StudentID = @ID";

        SqlParameter[] parameters =
        {
            new SqlParameter("@ID", st.StudentID),
            new SqlParameter("@Name", st.Name),
            new SqlParameter("@Birthday", (object)st.Birthday ?? DBNull.Value),
            new SqlParameter("@Phone", (object)st.Phone ?? DBNull.Value),
            new SqlParameter("@Email", (object)st.Email ?? DBNull.Value)
        };

        return db.ExecuteNonQuery(sql, parameters) > 0;
    }

    // ✔ Xóa sinh viên
    public bool DeleteStudent(int id)
    {
        string sql = "DELETE FROM Student WHERE StudentID = @ID";

        SqlParameter[] parameters =
        {
            new SqlParameter("@ID", id)
        };

        return db.ExecuteNonQuery(sql, parameters) > 0;
    }
}
