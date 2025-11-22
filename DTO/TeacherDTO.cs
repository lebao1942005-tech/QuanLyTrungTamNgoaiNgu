namespace DTO
{
    public class TeacherDTO
    {
        public int TeacherID { get; set; }
        public string Name { get; set; }
        public string Subject { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        // Khóa ngoại
        public int UserID { get; set; }
    }
}
