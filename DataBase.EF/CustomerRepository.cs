using Dapper;
using Microsoft.Extensions.Configuration;
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
        public IConfiguration Configuration { get; }
        private readonly string connectString;

        public CustomerRepository(IConfiguration config)
        {
            Configuration = config;
            connectString = Configuration["DapperConnect"];
        }
        public async Task<List<CustomerInfo>> GetCustomers(string role, int employeeId)
        {
            using IDbConnection db = new NpgsqlConnection(connectString);
            List<CustomerInfo> result = new List<CustomerInfo>();
            if (role == Role.Administrator)
            {
                var data = await db.QueryAsync<CustomerInfo>(@"Select 
c.""Id"", c.""CustomerId"", c.""BusinessLicense"", c.""ContactDetail"", c.""Type"", c.""EmployeeId"",
c.""CreateTime"", c.""UpdateTime"", c.""IsOld"", c.""Informant"", c.""Reviewing"",c.""FollowUp"",c.""Discard"",c.""Version"",
 e.""Name"" as ""EmployeeName"" 
From ""AgentManage"".Customer c left join  ""AgentManage"".employee e on c.""EmployeeId"" = e.""Id"" 
where c.""IsOld"" = false
");
                if (data != null) result.AddRange(data);
            }
            else if (role == Role.Manager)
            {
                var data1 = await db.QueryAsync<CustomerInfo>(@"Select 
c.""Id"", c.""CustomerId"", c.""BusinessLicense"", c.""ContactDetail"", c.""Type"", c.""EmployeeId"",
c.""CreateTime"", c.""UpdateTime"", c.""IsOld"", c.""Informant"", c.""Reviewing"",c.""FollowUp"",c.""Discard"",c.""Version"",
 e.""Name"" as ""EmployeeName"" 
From ""AgentManage"".Customer c left join  ""AgentManage"".employee e on c.""EmployeeId"" = e.""Id"" 
where c.""EmployeeId"" in (SELECT ""Id""  FROM ""AgentManage"".employee  WHERE ""Pid""  = @employeeId) and  c.""IsOld"" = false ", new { employeeId });
                var data2 = await db.QueryAsync<CustomerInfo>(@"Select 
c.""Id"", c.""CustomerId"", c.""BusinessLicense"", c.""ContactDetail"", c.""Type"", c.""EmployeeId"",
c.""CreateTime"", c.""UpdateTime"", c.""IsOld"", c.""Informant"", c.""Reviewing"",c.""FollowUp"",c.""Discard"",c.""Version"",
 e.""Name"" as ""EmployeeName"" 
From ""AgentManage"".Customer c left join  ""AgentManage"".employee e on c.""EmployeeId"" = e.""Id"" 
where c.""EmployeeId"" = @employeeId and c.""IsOld"" = false", new { employeeId });
                if (data1 != null) result.AddRange(data1);
                if (data2 != null) result.AddRange(data2);

            }
            else
            {
                var data = await db.QueryAsync<CustomerInfo>(@"Select 
c.""Id"", c.""CustomerId"", c.""BusinessLicense"", c.""ContactDetail"", c.""Type"", c.""EmployeeId"",
c.""CreateTime"", c.""UpdateTime"", c.""IsOld"", c.""Informant"", c.""Reviewing"",c.""FollowUp"",c.""Discard"",c.""Version"",
 e.""Name"" as ""EmployeeName"" 
From ""AgentManage"".Customer c left join  ""AgentManage"".employee e on c.""EmployeeId"" = e.""Id"" 
where c.""EmployeeId"" = @employeeId and c.""IsOld"" = false", new { employeeId });
                if (data != null) result.AddRange(data);

            }
            return result;
        }

        public async Task<List<CustomerDetail>> GetCustomersById(Guid customerId)
        {
            using IDbConnection db = new NpgsqlConnection(connectString);
            var result = await db.QueryAsync<CustomerDetail>(@"Select cu.""Id"" , cu.""CustomerId"" ,cu.""BusinessLicense"" , cu.""ContactDetail"" , cu.""Type"" ,cu.""CreateTime"" ,cu.""UpdateTime"" ,cu.""FollowUp"",cu.""Discard"",cu.""Version"",
cu.""IsOld"", cu.""Reviewing"", e.""Name"" as ""EmployeeName""
From ""AgentManage"".customer cu left join ""AgentManage"".employee e on cu.""EmployeeId"" = e.""Id"" 
where cu.""CustomerId"" = @customerId ", new { customerId });
            return result.ToList();
        }
    }
}
