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
    
    public class ClassBLL
    {
        private readonly ClassDAL dal = new ClassDAL();
        private readonly CourseDAL courseDal = new CourseDAL();

     /*   public DataTable GetAllClasses()
        {
            return dal.GetAllClasses();
           
        }
     */

        public List<ClassDTO> GetAllClasses()
        {
            return dal.GetAllClassesDetailed();
        }
        public DataTable GetStudentsByClassID(int classID)
        {
            return dal.GetStudentsByClassID(classID);
        }

        public bool InsertClass(ClassDTO c)
        {
            return dal.InsertClass(c);
        }

        public bool UpdateClass(ClassDTO c)
        {
            return dal.UpdateClass(c);
        }

        public bool DeleteClass(int classID)
        {
            return dal.DeleteClass(classID);
        }

        public DataTable GetTeachersBusyInSchedule(string schedule)
        {
            return dal.GetTeachersBySchedule(schedule);
        }

        public DataTable GetTeachersFreeInSchedule(string schedule)
        {
            return dal.GetFreeTeachersBySchedule(schedule);
        }
        /*    public DataTable GetAllCourses()
            {
                return courseDal.GetAllCourses();
            }
        */

        public List<CourseDTO> GetAllCourses()
        {
            return courseDal.GetAllCourses();
        }




        public int GetStudentCount(int classID)
        {
            return dal.CountStudentsInClass(classID);
        }

    }
    
}
