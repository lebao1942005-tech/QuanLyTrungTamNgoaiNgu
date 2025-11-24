using System;
using BLL;
using DTO;
using System.Data;

class Program
{
    static void Main(string[] args)
    {
        TeacherBLL teacherBLL = new TeacherBLL();
        string error;

        Console.WriteLine("=== TEST TEACHER BLL ===");

        // 1. Thêm giáo viên
        TeacherDTO newTeacher = new TeacherDTO
        {
            Name = "Nguyen Van D",
            Subject = "Math",
            Phone = "0123456789",
            Email = "teacherD@example.com",
            UserID = 1 // Chỉ cần >0, sẽ tạo User trong BLL
        };

        if (teacherBLL.AddTeacher(newTeacher, out error))
            Console.WriteLine("✓ Thêm giáo viên thành công!");
        else
            Console.WriteLine("X Thêm thất bại: " + error);

        // 2. Lấy danh sách giáo viên
        Console.WriteLine("\n--- Danh sách giáo viên ---");
        DataTable dt = teacherBLL.GetAllTeachers();
        foreach (DataRow row in dt.Rows)
        {
            Console.WriteLine($"{row["TeacherID"]} | {row["Name"]} | {row["Subject"]} | {row["Email"]}");
        }

        // 3. Lấy 1 giáo viên theo ID
        Console.WriteLine("\nNhập ID giáo viên để xem chi tiết:");
        int id = int.Parse(Console.ReadLine());
        var teacher = teacherBLL.GetTeacherById(id);
        if (teacher != null)
        {
            Console.WriteLine($"ID: {teacher.TeacherID}");
            Console.WriteLine($"Name: {teacher.Name}");
            Console.WriteLine($"Email: {teacher.Email}");
            Console.WriteLine($"UserID: {teacher.UserID}");
        }
        else
        {
            Console.WriteLine("Không tìm thấy giáo viên!");
        }

        // 4. Cập nhật giáo viên
        Console.WriteLine("\n=== Cập nhật giáo viên ===");
        teacher.Name += " (Updated)";
        if (teacherBLL.UpdateTeacher(teacher, out error))
            Console.WriteLine("✓ Cập nhật thành công!");
        else
            Console.WriteLine("X Cập nhật thất bại: " + error);

        // 5. Xóa giáo viên
        Console.WriteLine("\nNhập ID giáo viên để xóa (tài khoản User cũng sẽ bị xóa):");
        int deleteId = int.Parse(Console.ReadLine());

        if (teacherBLL.DeleteTeacher(deleteId, out error))
            Console.WriteLine("✓ Xóa giáo viên và tài khoản thành công!");
        else
            Console.WriteLine("X Xóa thất bại: " + error);

        Console.WriteLine("\n--- KẾT THÚC TEST ---");
        Console.ReadKey();
    }
}
