using System;
using System.Collections.Generic;
using System.Text;

namespace DataBase.EF
{
    public class ContractAll
    {
        public int Id { get; set; }
        public DateTime DealTime { get; set; }
        public Decimal DealAmount { get; set; }
        public string Remark { get; set; }
        public string ContractName { get; set; }
        public int ContractType { get; set; }

        public string ContractFile { get; set; }
        public int DealDuration { get; set; }
        public string AfterSale { get; set; }
        public int EmployeeId { get; set; }
        public Guid CustomerId { get; set; }
        public int CustomerId2 { get; set; }
    }
}
