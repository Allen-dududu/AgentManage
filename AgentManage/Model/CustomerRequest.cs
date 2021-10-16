using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AgentManage.Model
{
    public class CustomerRequest
    {

        public string BusinessLicense { get; set; }
        
        public string ContactDetail { get; set; }


        [CustomerType]
        public string Type { get; set; }
        //[Required]

        //public int EmployeeId { get; set; }

        //public string Informant { get; set; }
    }
}
