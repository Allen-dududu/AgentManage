using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgentManage.Model
{
    public class User
    {
        public string Name { get; set; }
        public int Phone { get; set; }
        public string PassWord { get; set; }
        public string Role { get; set; }
        public int Status { get; set; }
        public int Pid { get; set; }
    }
}
