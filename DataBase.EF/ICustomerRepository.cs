using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.EF
{
    public interface ICustomerRepository
    {
        public Task<List<CustomerInfo>> GetCustomers(string role, int employeeId);

        public Task<List<CustomerDetail>> GetCustomersById(Guid customerId);
    }
}
