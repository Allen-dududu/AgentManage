using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.EF
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly string connectString = Environment.GetEnvironmentVariable("DapperConnect");
        public async Task<List<Customer>> GetCustomers()
        {
            using (IDbConnection db = new NpgsqlConnection(connectString))
            {
                var result = await db.QueryAsync<Customer>("Select * From Customer");
                return result.ToList();
            }
        }

        public async Task<List<CustomerDetail>> GetCustomersById(Guid customerId)
        {
            using IDbConnection db = new NpgsqlConnection(connectString);
            var result = await db.QueryAsync<CustomerDetail>(@"Select cu.""Id"" , cu.""CustomerId"" ,cu.""BusinessLicense"" , cu.""ContactDetail"" , cu.""Type"" ,cu.""CreateTime"" ,cu.""UpdateTime"" ,
cu.""IsOld"", cu.""Reviewing"", c.""DealTime"", c.""Id"" as ""contractId"", c.""DealAmount"", c.""ContractName"",c.""ContractType"", c.""DealDuration"", e.""Name"" as ""EmployeeName""
From ""AgentManage"".customer cu left join ""AgentManage"".contract c  on cu.""Id"" = c.""CustomerId2"" left join ""AgentManage"".employee e on cu.""EmployeeId"" = e.""Id"" where cu.""CustomerId"" = @customerId", new { customerId });
            return result.ToList();
        }
    }
}
