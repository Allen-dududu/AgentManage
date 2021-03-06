using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.EF
{
    public interface IContractRepository
    {
        public Task<List<ContractAll>> GetContracts();

        public Task<ContractAll> GetContractById(int id);

        public Task<List<ContractAll>> GetContractByCustomerId(Guid CustomerId);

    }
}
