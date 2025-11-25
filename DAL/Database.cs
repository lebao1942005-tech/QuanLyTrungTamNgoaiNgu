using Microsoft.Data.SqlClient;
using System;
using System.Data;

public class Database
{
    private readonly string connectionString =
        "Data Source=LAPTOP-JH9IJG9F\\SQLEXPRESS;Initial Catalog=EducationDB;Integrated Security=True;Trust Server Certificate=True";

    // ✔ Hàm SELECT, trả về DataTable
    public DataTable ExecuteQuery(string query, SqlParameter[] parameters = null)
    {
        using (SqlConnection conn = new SqlConnection(connectionString))
        using (SqlCommand cmd = new SqlCommand(query, conn))
        {
            if (parameters != null)
                cmd.Parameters.AddRange(parameters);

            DataTable dt = new DataTable();
            conn.Open();

            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                dt.Load(reader);
            }

            return dt;
        }
    }

    // ✔ Hàm INSERT / UPDATE / DELETE, trả về số dòng bị ảnh hưởng
    public int ExecuteNonQuery(string query, SqlParameter[] parameters = null)
    {
        using (SqlConnection conn = new SqlConnection(connectionString))
        using (SqlCommand cmd = new SqlCommand(query, conn))
        {
            if (parameters != null)
                cmd.Parameters.AddRange(parameters);

            conn.Open();
            return cmd.ExecuteNonQuery();
        }
    }

    // ✔ Hàm lấy 1 giá trị (VD: COUNT, MAX, MIN…)
    public object ExecuteScalar(string query, SqlParameter[] parameters = null)
    {
        using (SqlConnection conn = new SqlConnection(connectionString))
        using (SqlCommand cmd = new SqlCommand(query, conn))
        {
            if (parameters != null)
                cmd.Parameters.AddRange(parameters);

            conn.Open();
            return cmd.ExecuteScalar();
        }
    }
}
