using System;
using System.Collections.Generic;
using System.Text;

namespace DataBase.EF
{
    public class ContractTemplate
	{
		public int Id { get; set; }
		public Decimal ContractAmount { get; set; }
		public string ContractName { get; set; }
		public string ContractFile { get; set; }
		public string ContractDetail { get; set; }
		public int ContractType { get; set; }

		public int Status { get; set; }
    }
}
