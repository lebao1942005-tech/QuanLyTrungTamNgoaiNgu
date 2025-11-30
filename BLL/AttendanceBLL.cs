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
    public class AttendanceBLL
    {
        private readonly AttendanceDAL dal = new AttendanceDAL();

        public DataTable GetByClass(int classID) => dal.GetByClass(classID);

        public bool Insert(AttendanceDTO a) => dal.Insert(a);
        public bool Update(AttendanceDTO a) => dal.Update(a);
        public bool Delete(int id) => dal.Delete(id);
    }
}
