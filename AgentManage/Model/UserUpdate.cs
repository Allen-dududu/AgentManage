using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AgentManage.Model
{
    public class UserUpdate
    {
        public string Name { get; set; }
        [Phone]
        public string Phone { get; set; }
        public string Role { get; set; }
        public int Status { get; set; }
        public int Pid { get; set; }
    }
}
