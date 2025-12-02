using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using DTO;

namespace BLL
{
    public class LoginBLL
    {
        private readonly LoginDAL login = new LoginDAL();
        //Dang nhap
        /*    public bool Login(string username, string password, string role)
            {
                return login.Login(username, password, role);
            }
        */


        public UserDTO Login(string username, string password, string role)
        {
            return login.Login(username, password, role);
        }

        //Doi mat khau
        public bool ChangePassword(string username, string newPassword)
        {
            return login.ChangePassword(username, newPassword);
        }
        //OTP
        public static string GenerateOTP()
        {
            Random random = new Random();
            int otp = random.Next(100000, 999999); // Tạo mã OTP 6 chữ số
            return otp.ToString();
        }
        public bool SendOTP(string toEmail, string otp)
        {
            if(string.IsNullOrEmpty(toEmail) || !(login.CheckEmailExists(toEmail)))
            {
                return false;
            }
            try
            {
                string fromEmail = "pentanix79@gmail.com";     // email gửi
                string appPassword = "cjrm zdds dacn bgtn";       // app password 

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(fromEmail, "EngCenter App");
                mail.To.Add(toEmail);
                mail.Subject = "OTP Verification Code";
                mail.Body = $"Your verification code is: {otp}";
                mail.IsBodyHtml = false;

                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.Credentials = new NetworkCredential(fromEmail, appPassword);
                smtp.EnableSsl = true;

                smtp.Send(mail);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        string opt=GenerateOTP();
        //Dang nhap bang opt
        public bool LoginWithOTP(string toEmail, string inputOTP)
        {
            if (SendOTP(toEmail, opt))
            {
                if (inputOTP == opt)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
