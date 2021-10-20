using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.EF
{
    public class ContractRepository : IContractRepository
    {
        private readonly string connectString = Environment.GetEnvironmentVariable("DapperConnect");
        public async Task<List<ContractAll>> GetContracts()
        {
            using IDbConnection db = new NpgsqlConnection(connectString);
            var result = await db.QueryAsync<ContractAll>(@"SELECT Id, DealTime, DealAmount, Remark, ContractName, ContractFile, DealDuration, AfterSale, EmployeeId, CustomerId, CustomerId2, ContractType, e.Name as EmployeeName
FROM ""AgentManage"".contract c left join ""AgentManage"".employee e on c.""EmployeeId"" = e.""Id"" ");
            return result.ToList();
        }
    }
}
