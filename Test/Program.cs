using System;
using BLL;
using DTO;

namespace ConsoleTestSQL
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== TEST KẾT NỐI SQL SERVER ===");

            // Nhập server
            Console.Write("Nhập Server (ví dụ: .\\SQLEXPRESS): ");
            string server = Console.ReadLine();

            // Nhập database
            Console.Write("Nhập Database: ");
            string database = Console.ReadLine();

            // Chọn chế độ đăng nhập
            Console.WriteLine("Chọn chế độ:");
            Console.WriteLine("1. Windows Authentication");
            Console.WriteLine("2. SQL Server Authentication");
            Console.Write("Lựa chọn: ");
            string mode = Console.ReadLine();

            SystemConfig cfg = new SystemConfig();
            cfg.ServerName = server;
            cfg.DatabaseName = database;

            if (mode == "1")
            {
                cfg.UseWindowsAuth = true;
            }
            else
            {
                cfg.UseWindowsAuth = false;

                Console.Write("User: ");
                cfg.UserName = Console.ReadLine();

                Console.Write("Password: ");
                cfg.Password = ReadPassword();
            }

            // Tạo BLL
            SystemConfigBLL bll = new SystemConfigBLL();
            string connStr = bll.BuildConnectionString(cfg);

            Console.WriteLine("\nConnection String:");
            Console.WriteLine(connStr);

            // Test connection
            Console.WriteLine("\nĐang kiểm tra kết nối...");
            bool ok = bll.TestConnection(connStr);

            if (ok)
                Console.WriteLine("🎉 KẾT NỐI THÀNH CÔNG!");
            else
                Console.WriteLine("❌ KẾT NỐI THẤT BẠI!");

            Console.WriteLine("\nNhấn phím bất kỳ để thoát...");
            Console.ReadKey();
        }

        // ==============================
        // Hàm nhập password không hiện ký tự
        // ==============================
        static string ReadPassword()
        {
            string pass = "";
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);

                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    pass += key.KeyChar;
                    Console.Write("*");
                }
                else if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                {
                    pass = pass.Substring(0, pass.Length - 1);
                    Console.Write("\b \b");
                }
            }
            while (key.Key != ConsoleKey.Enter);

            Console.WriteLine();
            return pass;
        }
    }
}
