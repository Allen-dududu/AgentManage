using System;
using System.Collections.Generic;
using System.Text;

namespace DataBase.EF
{
    public class Contract
    {
        public int Id { get; set; }
        public DateTime DealTime { get; set; }
        public Decimal DealAmount { get; set; }
        public string Remark { get; set; }
        public string ContractName { get; set; }
        public int ContractTemplateId { get; set; }
        public string MoneyProof { get; set; }

        public string ContractFile { get; set; }
        public int DealDuration { get; set; }
        public string AfterSale { get; set; }
        public int EmployeeId { get; set; }
        public Guid CustomerId { get; set; }
        public int CustomerId2 { get; set; }



    }
}
