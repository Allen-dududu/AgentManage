using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.EF
{
    public interface ICustomerRepository
    {
        public Task<List<Customer>> GetCustomers();

        public Task<List<CustomerDetail>> GetCustomersById(Guid customerId);
    }
}
