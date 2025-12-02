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
    public class LoginDAL
    {
        Database dt = new Database();
        //Dang nhap
        /*    public bool Login(string username, string password, string role)
            {
                string query = @"
            SELECT COUNT(*) 
            FROM Users 
            WHERE Username = @username 
              AND PasswordHash = @password 
              AND Role = @role";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@username", username),
                    new SqlParameter("@password", password),
                    new SqlParameter("@role", role)
                };

                object result = dt.ExecuteScalar(query, parameters);
                int count = result == null ? 0 : Convert.ToInt32(result);

                return count > 0;
            }
        */


        public UserDTO Login(string username, string password, string role)
        {
            // SỬA: Dùng LEFT JOIN để lấy TeacherID từ bảng Teachers nếu Email trùng khớp
            // Giả định: u.Username lưu Email, và t.Email cũng lưu Email giống nhau
            string query = @"
        SELECT 
            u.UserID, 
            u.Username, 
            u.Role, 
            t.TeacherID
        FROM Users u
        LEFT JOIN Teacher t ON u.Username = t.Email
        WHERE u.Username = @username 
          AND u.PasswordHash = @password 
          AND u.Role = @role";

            SqlParameter[] parameters = new SqlParameter[]
            {
        new SqlParameter("@username", username),
        new SqlParameter("@password", password),
        new SqlParameter("@role", role)
            };

            DataTable data = dt.ExecuteQuery(query, parameters);

            if (data.Rows.Count > 0)
            {
                DataRow row = data.Rows[0];
                return new UserDTO
                {
                    UserID = Convert.ToInt32(row["UserID"]),
                    Username = row["Username"].ToString(),
                    Role = row["Role"].ToString(),

                    // Code này giữ nguyên, nhưng nhờ câu query mới, nó sẽ lấy được ID thật
                    TeacherID = row["TeacherID"] != DBNull.Value ? Convert.ToInt32(row["TeacherID"]) : (int?)null
                };
            }

            return null; // Đăng nhập thất bại
        }


        //Doi mat khau
        public bool ChangePassword(string username, string newPassword)
        {
            string updateQuery = @"
        UPDATE Users 
        SET PasswordHash = @newPassword
        WHERE Username = @username";

            SqlParameter[] updateParams = new SqlParameter[]
            {
                new SqlParameter("@username", username),
                new SqlParameter("@newPassword", newPassword)
            };

            int affected = dt.ExecuteNonQuery(updateQuery, updateParams);

            return affected > 0; // Đổi mật khẩu thành công
        }
        //check mail đăng nhập
        /*
        public bool CheckEmailExists(string email)
        {
            string query ="SELECT COUNT(*) FROM Users WHERE Email = @Email";
            SqlParameter[] parameters =
            {
                new SqlParameter("@Email", email)
            };
            object result = dt.ExecuteScalar(query, parameters);
            int count = result == null ? 0 : Convert.ToInt32(result);
            return count > 0;
        }
        */


        public bool CheckEmailExists(string email)
        {
            string query = "SELECT COUNT(*) FROM Users WHERE Username = @Email";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Email", email)
            };

            object result = dt.ExecuteScalar(query, parameters);
            int count = result == null ? 0 : Convert.ToInt32(result);
            return count > 0;
        }


    }
}
