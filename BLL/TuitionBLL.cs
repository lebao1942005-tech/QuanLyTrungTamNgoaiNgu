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
    public class TuitionBLL
    {
        private readonly TuitionDAL dal = new TuitionDAL();

        // Lấy toàn bộ Tuition
        public DataTable GetAllTuition()
        {
            return dal.GetAllTuition();
        }

        // Lấy theo EnrollmentID
        public DataTable GetTuitionByEnrollment(int enrollmentID)
        {
            return dal.GetTuitionByEnrollmentID(enrollmentID);
        }

        // Thêm Tuition
        public string InsertTuition(TuitionDTO t)
        {
            if (dal.Exists(t.EnrollmentID))
                return "Học phí của Enrollment này đã tồn tại!";

            if (dal.InsertTuition(t))
                return "Thêm học phí thành công!";

            return "Thêm học phí thất bại!";
        }

        // Cập nhật trạng thái thanh toán
        public string UpdateTuition(TuitionDTO t)
        {
            if (dal.UpdateTuition(t))
                return "Cập nhật học phí thành công!";

            return "Cập nhật thất bại!";
        }

        // Xóa
        public string DeleteTuition(int id)
        {
            if (dal.DeleteTuition(id))
                return "Xóa thành công!";

            return "Xóa thất bại!";
        }
    }
}
