using System;
using DTO;
using BLL;

class Program
{
    static void Main(string[] args)
    {
        StudentBLL bll = new StudentBLL();

        Console.WriteLine("=== TEST CRUD STUDENT ===");

        // ---------------------------
        // 1. TEST THÊM SINH VIÊN
        // ---------------------------
        StudentDTO newStudent = new StudentDTO
        {
            Name = "Nguyen Van A",
            Birthday = new DateTime(2005, 5, 20),
            Phone = "0123456789",
            Email = "a@student.com"
        };

        if (bll.AddStudent(newStudent, out string err1))
            Console.WriteLine("✓ Thêm sinh viên thành công!");
        else
            Console.WriteLine("X Thêm thất bại: " + err1);


        // ---------------------------
        // 2. TEST LẤY DANH SÁCH
        // ---------------------------
        Console.WriteLine("\n=== DANH SÁCH SINH VIÊN ===");
        var table = bll.GetAllStudents();
        foreach (System.Data.DataRow row in table.Rows)
        {
            Console.WriteLine($"ID: {row["StudentID"]}, Name: {row["Name"]}, Phone: {row["Phone"]}");
        }


        // ---------------------------
        // 3. TEST LẤY MỘT SINH VIÊN
        // ---------------------------
        Console.WriteLine("\nNhập ID để xem chi tiết:");
        int id = int.Parse(Console.ReadLine());

        var st = bll.GetStudentById(id);
        if (st != null)
        {
            Console.WriteLine($"ID: {st.StudentID}");
            Console.WriteLine($"Name: {st.Name}");
            Console.WriteLine($"Phone: {st.Phone}");
        }
        else
        {
            Console.WriteLine("Không tìm thấy sinh viên!");
        }


        // ---------------------------
        // 4. TEST CẬP NHẬT
        // ---------------------------
        Console.WriteLine("\n=== TEST SỬA ===");
        st.Name = "Tên mới cập nhật";
        if (bll.UpdateStudent(st, out string err2))
            Console.WriteLine("✓ Cập nhật thành công!");
        else
            Console.WriteLine("X Cập nhật thất bại: " + err2);


        // ---------------------------
        // 5. TEST XÓA
        // ---------------------------
        Console.WriteLine("\nNhập ID để xóa:");
        int deleteId = int.Parse(Console.ReadLine());

        if (bll.DeleteStudent(deleteId, out string err3))
            Console.WriteLine("✓ Xóa thành công!");
        else
            Console.WriteLine("X Xóa thất bại: " + err3);

        Console.WriteLine("\n--- KẾT THÚC ---");
        Console.ReadKey();
    }
}
