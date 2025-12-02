using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI.Utilities
{
    public static class UserSession
    {
        // Vai trò: "Admin" hoặc "Teacher"
        public static string Role { get; set; }

        // Nếu là Giáo viên thì lưu ID vào đây để lọc lớp, Admin thì null
        public static int? CurrentTeacherID { get; set; }

        // Lưu tên hiển thị (VD: "Nguyễn Văn A")
        // [QUAN TRỌNG] Đặt tên là CurrentUsername để khớp với LoginViewModel
        public static string CurrentUsername { get; set; }

        // Helper check nhanh
        public static bool IsAdmin => Role == "Admin";
        public static bool IsTeacher => Role == "Teacher"; // Thêm helper này để check quyền giáo viên dễ hơn

        // Hàm đăng xuất (xóa dữ liệu phiên)
        public static void ClearSession()
        {
            Role = null;
            CurrentTeacherID = null;
            CurrentUsername = null;
        }
    }
}