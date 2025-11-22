using DTO;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace DAL;
public class TeacherDAL
{
    private readonly Database db = new Database();

    //  Lấy tất cả giáo viên
    public DataTable GetAllTeachers()
    {
        string sql = "SELECT * FROM Teacher";
        return db.ExecuteQuery(sql);
    }

    //Lấy giáo viên theo ID
    public TeacherDTO GetTeacherById(int id)
    {
        string sql = "SELECT * FROM Teacher WHERE TeacherID = @ID";

        SqlParameter[] parameters =
        {
            new SqlParameter("@ID", id)
        };

        DataTable dt = db.ExecuteQuery(sql, parameters);

        if (dt.Rows.Count == 0)
            return null;

        DataRow row = dt.Rows[0];

        return new TeacherDTO
        {
            TeacherID = Convert.ToInt32(row["TeacherID"]),
            Name = row["Name"].ToString(),
            Subject = row["Subject"]?.ToString(),
            Phone = row["Phone"]?.ToString(),
            Email = row["Email"]?.ToString(),
            UserID = Convert.ToInt32(row["UserID"])
        };
    }

    // Thêm giáo viên (TeacherID tự tăng)
    public bool InsertTeacher(TeacherDTO t)
    {
        string sql = @"
            INSERT INTO Teacher (Name, Subject, Phone, Email, UserID)
            VALUES (@Name, @Subject, @Phone, @Email, @UserID)";

        SqlParameter[] parameters =
        {
            new SqlParameter("@Name", t.Name),
            new SqlParameter("@Subject", (object)t.Subject ?? DBNull.Value),
            new SqlParameter("@Phone", (object)t.Phone ?? DBNull.Value),
            new SqlParameter("@Email", (object)t.Email ?? DBNull.Value),
            new SqlParameter("@UserID", t.UserID)
        };

        return db.ExecuteNonQuery(sql, parameters) > 0;
    }

    //Cập nhật giáo viên
    public bool UpdateTeacher(TeacherDTO t)
    {
        string sql = @"
            UPDATE Teacher
            SET Name = @Name,
                Subject = @Subject,
                Phone = @Phone,
                Email = @Email,
                UserID = @UserID
            WHERE TeacherID = @ID";

        SqlParameter[] parameters =
        {
            new SqlParameter("@ID", t.TeacherID),
            new SqlParameter("@Name", t.Name),
            new SqlParameter("@Subject", (object)t.Subject ?? DBNull.Value),
            new SqlParameter("@Phone", (object)t.Phone ?? DBNull.Value),
            new SqlParameter("@Email", (object)t.Email ?? DBNull.Value),
            new SqlParameter("@UserID", t.UserID)
        };

        return db.ExecuteNonQuery(sql, parameters) > 0;
    }

    //Xóa giáo viên
    public bool DeleteTeacher(int id)
    {
        string sql = "DELETE FROM Teacher WHERE TeacherID = @ID";

        SqlParameter[] parameters =
        {
            new SqlParameter("@ID", id)
        };

        return db.ExecuteNonQuery(sql, parameters) > 0;
    }
}
