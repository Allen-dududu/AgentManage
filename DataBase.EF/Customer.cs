using System;
using System.Collections.Generic;
using System.Text;

namespace DataBase.EF
{
    public class Customer
    {
        public int Id { get; set; }
        public Guid CustomerId { get; set; }

        public string BusinessLicense { get; set; }

        public string ContactDetail { get; set; }
        public string Type { get; set; }
        public int EmployeeId { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public bool IsOld { get; set; }
        public string Informant { get; set; }
        public bool Reviewing { get; set; }
        public string FollowUp { get; set; }
        public bool Discard { get; set; }
        public int Version { get; set; }

        public ICollection<Contract> Contracts { get; set; }

        public virtual string test()
        {
            return "";
        }
    }
}
