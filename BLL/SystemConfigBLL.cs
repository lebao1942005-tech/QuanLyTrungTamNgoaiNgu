using DTO;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public class SystemConfigBLL
    {
        public string BuildConnectionString(SystemConfig cfg)
        {
            if (cfg.UseWindowsAuth)
            {
                return $"Data Source={cfg.ServerName};Initial Catalog={cfg.DatabaseName};Integrated Security=True;Trust Server Certificate=True;";
            }
            else
            {
                return $"Data Source={cfg.ServerName};Initial Catalog={cfg.DatabaseName};User ID={cfg.UserName};Password={cfg.Password};Trust Server Certificate=True;";
            }
        }

        public bool TestConnection(string connectionString)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
