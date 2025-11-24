using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class LoginDAL
    {
        Database dt = new Database();
        //Dang nhap
        public bool Login(string username, string password, string role)
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
    }
}
