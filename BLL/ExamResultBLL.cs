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
    public class ExamResultBLL
    {
        private readonly ExamResultDAL dal = new ExamResultDAL();
        
        public DataTable GetStudentByIdEnrollment(int studentId) => dal.GetStudentByIdEnrollment(studentId);
        public DataTable GetAll() => dal.GetAll();
        public bool Insert(ExamResultDTO r) => dal.Insert(r);
        public bool Update(ExamResultDTO r) => dal.Update(r);
        public bool Delete(int id) => dal.Delete(id);

        public DataTable GetGradingListByClass(int classId) => dal.GetGradingListByClass(classId);
    }
}
