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

        public async Task<List<ContractAll>> GetContractByCustomerId(Guid customerId)
        {
            using IDbConnection db = new NpgsqlConnection(connectString);
            var result = await db.QueryAsync<ContractAll>(@"SELECT c.""Id"", c.""DealTime"", c.""DealAmount"", c.""Remark"", 
c.""ContractName"", c.""ContractFile"",  c.""ContractFile"",c.""DealDuration"", c.""AfterSale"", c.""EmployeeId"", c.""CustomerId"", c.""CustomerId2"",
ct.""ContractType"" as ContractTemplateType, ct.""ContractAmount"" as ContractTemplateAmount, ct.""ContractName"" as ContractTemplateName, ct.""ContractFile"" as ContractTemplateFile, ct.""ContractDetail"" as ContractTemplateDetail,
e.""Name"" as EmployeeName 
FROM ""AgentManage"".contract c left join ""AgentManage"".employee e on c.""EmployeeId"" = e.""Id"" 
left join ""AgentManage"".contracttemplate ct on c.""ContractTemplateId"" = ct.""Id"" where c.""CustomerId"" = @customerId ", new { customerId });
            return result.ToList();
        }

        public async Task<ContractAll> GetContractById(int id)
        {
            using IDbConnection db = new NpgsqlConnection(connectString);
            var result = await db.QueryFirstOrDefaultAsync<ContractAll>(@"SELECT c.""Id"", c.""DealTime"", c.""DealAmount"", c.""Remark"", 
c.""ContractName"", c.""ContractFile"",  c.""ContractFile"",c.""DealDuration"", c.""AfterSale"", c.""EmployeeId"", c.""CustomerId"", c.""CustomerId2"",
ct.""ContractType"" as ContractTemplateType, ct.""ContractAmount"" as ContractTemplateAmount, ct.""ContractName"" as ContractTemplateName, ct.""ContractFile"" as ContractTemplateFile, ct.""ContractDetail"" as ContractTemplateDetail,
e.""Name"" as EmployeeName 
FROM ""AgentManage"".contract c left join ""AgentManage"".employee e on c.""EmployeeId"" = e.""Id"" 
left join ""AgentManage"".contracttemplate ct on c.""ContractTemplateId"" = ct.""Id"" where c.""Id"" = @id", new { id });
            return result;
        }

        public async Task<List<ContractAll>> GetContracts()
        {
            using IDbConnection db = new NpgsqlConnection(connectString);
            var result = await db.QueryAsync<ContractAll>(@"SELECT c.""Id"", c.""DealTime"", c.""DealAmount"", c.""Remark"", 
c.""ContractName"", c.""ContractFile"",  c.""ContractFile"",c.""DealDuration"", c.""AfterSale"", c.""EmployeeId"", c.""CustomerId"", c.""CustomerId2"",
ct.""ContractType"" as ContractTemplateType, ct.""ContractAmount"" as ContractTemplateAmount, ct.""ContractName"" as ContractTemplateName, ct.""ContractFile"" as ContractTemplateFile, ct.""ContractDetail"" as ContractTemplateDetail,
e.""Name"" as EmployeeName
FROM ""AgentManage"".contract c left join ""AgentManage"".employee e on c.""EmployeeId"" = e.""Id"" 
left join ""AgentManage"".contracttemplate ct on c.""ContractTemplateId"" = ct.""Id"" ");
            return result.ToList();
        }
    }
}
