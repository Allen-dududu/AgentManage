using System;

namespace AgentManage.Controllers
{
    public class ContractTemplateRequest
    {
        public Decimal ContractAmount { get; set; }
        public string ContractName { get; set; }
        public string ContractFile { get; set; }
        public string ContractDetail { get; set; }
        public int ContractType { get; set; }
    }
}