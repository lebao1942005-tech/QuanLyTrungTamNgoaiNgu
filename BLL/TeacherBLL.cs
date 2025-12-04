using DAL;
using DTO;
using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace BLL;
public class TeacherBLL
{
    private readonly TeacherDAL dal = new TeacherDAL();
    private readonly UserDAL userDal = new UserDAL();

    // Get all
    public DataTable GetAllTeachers()
    {
        return dal.GetAllTeachers();
    }

    //Get by ID
    public TeacherDTO GetTeacherById(int id)
    {
        return dal.GetTeacherById(id);
    }

    // Add
    public bool AddTeacher(TeacherDTO t, out string error)
    {
        error = "";

        if (string.IsNullOrWhiteSpace(t.Name))
        {
            error = "Tên giáo viên không được bỏ trống.";
            return false;
        }

        // === NGUYÊN NHÂN GÂY LỖI LÀ ĐÂY ===
        // Bạn hãy xóa hoặc comment đoạn này đi:
        /* if (t.UserID <= 0)
        {
            error = "UserID không hợp lệ.";
            return false;
        }
        */
        // ==================================

        try
        {
            // Tạo user cho giáo viên
            
            UserDTO user = new UserDTO
            {
                Username = t.Email,
                PasswordHash = "123", 
                Role = "Teacher"
            };
            int usID = userDal.InsertUserReturnID(user);
            t.UserID = usID;
            return dal.InsertTeacher(t);
        }
        catch (SqlException ex)
        {
            if (ex.Number == 2627) // UNIQUE constraint
                error = "UserID này đã được dùng cho giáo viên khác.";

            else
                error = ex.Message;

            return false;
        }
    }

    //Update
    public bool UpdateTeacher(TeacherDTO t, out string error)
    {
        error = "";

        if (t.TeacherID <= 0)
        {
            error = "TeacherID không hợp lệ.";
            return false;
        }
        if (string.IsNullOrWhiteSpace(t.Name))
        {
            error = "Tên giáo viên không được bỏ trống.";
            return false;
        }

        try
        {
            return dal.UpdateTeacher(t);
        }
        catch (SqlException ex)
        {
            if (ex.Number == 2627)
                error = "UserID này đã được dùng cho giáo viên khác.";
            else
                error = ex.Message;

            return false;
        }
    }

    //Delete
    public bool DeleteTeacher(int id, out string error)
    {
        error = "";

        if (id <= 0)
        {
            error = "ID giáo viên không hợp lệ.";
            return false;
        }

        // Lấy Teacher
        TeacherDTO t = dal.GetTeacherById(id);
        if (t == null)
        {
            error = "Không tìm thấy giáo viên.";
            return false;
        }

        // 1. Xóa Teacher trước
        bool teacherDeleted = dal.DeleteTeacher(id);
        if (!teacherDeleted)
        {
            error = "Xóa giáo viên thất bại.";
            return false;
        }

        // 2. Xóa User liên quan
        bool userDeleted = userDal.DeleteUser(t.UserID);
        if (!userDeleted)
            error = "Xóa tài khoản user thất bại.";

        return true;
    }




    public bool HasClasses(int id)
    {
        return dal.HasClasses(id);
    }

}
