using System;
using System.Collections.Generic;
using System.Text;

namespace DataBase.EF
{
    public class ContractAll
    {
        public int Id { get; set; }
        public DateTime DealTime { get; set; }
        /// <summary>
        /// 公司名
        /// </summary>
        public string BusinessLicense { get; set; }
        public Decimal DealAmount { get; set; }
        public string Remark { get; set; }
        public string ContractName { get; set; }
        public int ContractTemplateType { get; set; }

        public string ContractFile { get; set; }
        public string MoneyProof { get; set; }
        public int DealDuration { get; set; }
        public string AfterSale { get; set; }
        public int EmployeeId { get; set; }
        public Guid CustomerId { get; set; }
        public int CustomerId2 { get; set; }

        public string EmployeeName { get; set; }

        public int ContractTemplateId { get; set; }
        public Decimal ContractTemplateAmount { get; set; }
        public string ContractTemplateName { get; set; }
        public string ContractTemplateFile { get; set; }
        public string ContractTemplateDetail { get; set; }
    }
}
