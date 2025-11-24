using DAL;
using DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public class UserBLL
    {
        private readonly UserDAL dal = new UserDAL();

        public DataTable GetAllUsers()
        {
            return dal.GetAllUsers();
        }

        public UserDTO GetUserById(int id)
        {
            return dal.GetUserById(id);
        }

        public bool InsertUser(UserDTO user)
        {
            if (string.IsNullOrWhiteSpace(user.Username))
                return false;

            if (string.IsNullOrWhiteSpace(user.PasswordHash))
                return false;

            if (user.Role != "Admin" && user.Role != "Teacher")
                return false;

            return dal.InsertUser(user);
        }

        public bool UpdateUser(UserDTO user)
        {
            if (user.UserID <= 0)
                return false;

            return dal.UpdateUser(user);
        }

        public bool DeleteUser(int id)
        {
            return dal.DeleteUser(id);
        }
    }
}
