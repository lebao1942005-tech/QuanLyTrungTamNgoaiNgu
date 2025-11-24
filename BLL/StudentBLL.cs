using DTO;
using System;
using System.Data;

public class StudentBLL
{
    private readonly StudentDAL dal = new StudentDAL();

    // ✔ Lấy tất cả sinh viên (trả về DataTable để đổ vào DataGridView)
    public DataTable GetAllStudents()
    {
        return dal.GetAllStudents();
    }

    // ✔ Lấy sinh viên theo ID
    public StudentDTO GetStudentById(int id)
    {
        return dal.GetStudentById(id);
    }

    // ✔ Thêm sinh viên
    public bool AddStudent(StudentDTO st, out string error)
    {
        error = "";

        // Validate ở BLL nếu cần
        if (string.IsNullOrWhiteSpace(st.Name))
        {
            error = "Tên sinh viên không được bỏ trống.";
            return false;
        }

        return dal.InsertStudent(st);
    }

    // ✔ Cập nhật sinh viên
    public bool UpdateStudent(StudentDTO st, out string error)
    {
        error = "";

        if (st.StudentID <= 0)
        {
            error = "ID sinh viên không hợp lệ.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(st.Name))
        {
            error = "Tên sinh viên không được bỏ trống.";
            return false;
        }

        return dal.UpdateStudent(st);
    }

    // ✔ Xóa sinh viên
    public bool DeleteStudent(int id, out string error)
    {
        error = "";

        if (id <= 0)
        {
            error = "ID sinh viên không hợp lệ.";
            return false;
        }

        return dal.DeleteStudent(id);
    }
}
