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
    public class CourseBLL
    {
        private readonly CourseDAL dal = new CourseDAL();

     /*   public DataTable GetAllCourses()
        {
            return dal.GetAllCourses();
        }
     */




        public List<CourseDTO> GetAllCourses()
        {
            return dal.GetAllCourses();
        }

        public bool InsertCourse(CourseDTO c)
        {
            return dal.InsertCourse(c);
        }

        public bool UpdateCourse(CourseDTO c)
        {
            return dal.UpdateCourse(c);
        }

        public bool DeleteCourse(int courseID)
        {
            return dal.DeleteCourse(courseID);
        }
    }
}
