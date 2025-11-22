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
    public class UserDAL
    {
        private readonly Database db = new Database();

        // Lấy tất cả user
        public DataTable GetAllUsers()
        {
            string sql = "SELECT * FROM Users";
            return db.ExecuteQuery(sql);
        }

        // Lấy user theo ID
        public UserDTO GetUserById(int id)
        {
            string sql = "SELECT * FROM Users WHERE UserID = @ID";

            SqlParameter[] parameters =
            {
            new SqlParameter("@ID", id)
        };

            DataTable dt = db.ExecuteQuery(sql, parameters);

            if (dt.Rows.Count == 0)
                return null;

            DataRow row = dt.Rows[0];

            return new UserDTO
            {
                UserID = Convert.ToInt32(row["UserID"]),
                Username = row["Username"].ToString(),
                PasswordHash = row["PasswordHash"].ToString(),
                Role = row["Role"].ToString(),
                CreatedAt = Convert.ToDateTime(row["CreatedAt"])
            };
        }

        // Thêm user
        public bool InsertUser(UserDTO user)
        {
            string sql = @"
            INSERT INTO Users (Username, PasswordHash, Role)
            VALUES (@Username, @PasswordHash, @Role)";

            SqlParameter[] parameters =
            {
            new SqlParameter("@Username", user.Username),
            new SqlParameter("@PasswordHash", user.PasswordHash),
            new SqlParameter("@Role", user.Role)
        };

            return db.ExecuteNonQuery(sql, parameters) > 0;
        }

        // Cập nhật user
        public bool UpdateUser(UserDTO user)
        {
            string sql = @"
            UPDATE Users
            SET Username = @Username,
                PasswordHash = @PasswordHash,
                Role = @Role
            WHERE UserID = @UserID";

            SqlParameter[] parameters =
            {
            new SqlParameter("@UserID", user.UserID),
            new SqlParameter("@Username", user.Username),
            new SqlParameter("@PasswordHash", user.PasswordHash),
            new SqlParameter("@Role", user.Role)
        };

            return db.ExecuteNonQuery(sql, parameters) > 0;
        }

        // Xóa user
        public bool DeleteUser(int id)
        {
            string sql = "DELETE FROM Users WHERE UserID = @ID";

            SqlParameter[] parameters =
            {
            new SqlParameter("@ID", id)
        };

            return db.ExecuteNonQuery(sql, parameters) > 0;
        }
        public int InsertUserReturnID(UserDTO user)
        {
            string sql = @"
        INSERT INTO Users (Username, PasswordHash, Role)
        VALUES (@Username, @PasswordHash, @Role);
        SELECT CAST(SCOPE_IDENTITY() AS INT);";

            SqlParameter[] parameters =
            {
        new SqlParameter("@Username", user.Username),
        new SqlParameter("@PasswordHash", user.PasswordHash),
        new SqlParameter("@Role", user.Role)
    };

            // ExecuteScalar sẽ trả về giá trị SCOPE_IDENTITY
            object result = db.ExecuteScalar(sql, parameters);

            // Chuyển sang int, nếu null trả về 0
            return result != null ? Convert.ToInt32(result) : 0;
        }

    }
}
