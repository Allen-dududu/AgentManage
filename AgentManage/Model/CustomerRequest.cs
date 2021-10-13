using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AgentManage.Model
{
    public class CustomerRequest
    {
        [Required]
        public string BusinessLicense { get; set; }
        [Required]
        public string ConnectDetail { get; set; }

        [Required]
        [CustomerType]
        public string Type { get; set; }
        [Required]

        public int EmployeeId { get; set; }

        public string Informant { get; set; }
    }
}
