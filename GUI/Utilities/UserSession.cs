using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI.Utilities // Namespace sẽ là GUI.Utilities
{
    public static class UserSession
    {
        // Vai trò: "Admin" hoặc "Teacher"
        public static string Role { get; set; }

        // Nếu là Giáo viên thì lưu ID vào đây để lọc lớp, Admin thì null
        public static int? CurrentTeacherID { get; set; }

        // Lưu tên hiển thị (VD: "Nguyễn Văn A") để hiện lên góc màn hình cho đẹp
        public static string CurrentUserName { get; set; }

        // Helper check nhanh
        public static bool IsAdmin => Role == "Admin";

        // Hàm đăng xuất (xóa dữ liệu phiên)
        public static void ClearSession()
        {
            Role = null;
            CurrentTeacherID = null;
            CurrentUserName = null;
        }
    }
}