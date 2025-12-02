using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class SystemConfig
    {
        public string ServerName { get; set; }
        public string DatabaseName { get; set; }
        public bool UseWindowsAuth { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
