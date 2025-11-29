namespace DTO
{
    public class ClassDTO
    {
        public int ClassID { get; set; }
        public string ClassName { get; set; }
        public int CourseID { get; set; }
        public int TeacherID { get; set; }
        public string Schedule { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int MaxStudents { get; set; } = 20;
    }
}
