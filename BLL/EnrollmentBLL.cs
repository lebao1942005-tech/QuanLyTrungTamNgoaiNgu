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
    public class EnrollmentBLL
    {
        private readonly EnrollmentDAL dal = new EnrollmentDAL();
        private readonly StudentDAL student = new StudentDAL();
        private readonly ClassDAL classdal =new ClassDAL();

        // Lấy tất cả
        public DataTable GetAllEnrollments()
        {
            return dal.GetAllEnrollments();
        }
        //cbb student
        public DataTable GetAllStu()
        {
            return student.GetAllStudents();
        }
        //cbb class
        public DataTable GetAllClass()
        {
            return classdal.GetAllClasses();
        }
        // Thêm mới
        public string InsertEnrollment(EnrollmentDTO e)
        {
            // Kiểm tra trùng
            if (dal.Exists(e.StudentID, e.ClassID))
                return "Sinh viên này đã đăng ký lớp này rồi!";

            if (dal.InsertEnrollment(e))
                return "Thêm đăng ký thành công!";

            return "Thêm thất bại!";
        }

        // Cập nhật
        public string UpdateEnrollment(EnrollmentDTO e)
        {
            if (dal.UpdateEnrollment(e))
                return "Cập nhật thành công!";

            return "Cập nhật thất bại!";
        }

        // Xóa
        public string DeleteEnrollment(int id)
        {
            if (dal.DeleteEnrollment(id))
                return "Xóa thành công!";

            return "Xóa thất bại!";
        }
    }
}
