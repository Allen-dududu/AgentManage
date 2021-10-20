using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgentManage.Model
{
    public class Open
    {
        public string CustomerType { get; set; }
        public Guid CustomerId { get; set; }

        public int Version { get; set; }
    }
}
