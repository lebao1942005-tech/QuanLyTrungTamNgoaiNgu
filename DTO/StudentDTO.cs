namespace DTO
{
    public class StudentDTO
    {
        public int StudentID { get; set; }
        public string Name { get; set; }
        public DateTime? Birthday { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        // [MỚI] Trạng thái học tập
        // 1: Không lớp, 2: Đang học
        public int Status { get; set; }
    }

}
