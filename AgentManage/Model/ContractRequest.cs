using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgentManage.Model
{
    public class ContractRequest
    {
        public Decimal DealAmount { get; set; }
        public string Remark { get; set; }
        public string ContractName { get; set; }
        public int ContractTemplateId { get; set; }
        public string MoneyProof { get; set; }

        public string ContractFile { get; set; }

        public int DealDuration { get; set; }
        public string AfterSale { get; set; }
    }
}
