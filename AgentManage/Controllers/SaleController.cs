using AgentManage.Model;
using DataBase.EF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AgentManage.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class SaleController : ControllerBase
    {
        private readonly Context _context;
        public SaleController(Context context)
        {
            _context = context;
        }

        // GET: api/<SaleController>
        [HttpGet("Customer")]
        public async Task<IActionResult> GetAsync()
        {
            var user = _context.Employees.Where(i => i.Id == GetUserId()).FirstOrDefault();
            var customers = _context.Customer.Where(i => i.IsOld == false);
            if (user != null)
            {
                if (user.Role == Role.Administrator)
                {
                }
                else if (user.Role == Role.Manager)
                {
                    var children = _context.Employees.Where(i => i.Pid == user.Id).Select(i => i.Id).ToList();
                    customers = customers.Where(i => children.Contains(i.EmployeeId));
                }
                else
                {
                    customers = customers.Where( i => i.EmployeeId == user.Id);
                }
                var result = new List<Customer>();
                var customersA = customers.Where(i => i.Type == CustomerType.A && i.UpdateTime.AddDays(10) > DateTime.Now);

                var customersB = customers.Where(i => i.Type == CustomerType.B && i.UpdateTime.AddDays(10) > DateTime.Now);

                var customersC = customers.Where(i => i.Type == CustomerType.C && i.UpdateTime.AddDays(3) > DateTime.Now);

                result.AddRange(customersA);
                result.AddRange(customersB);
                result.AddRange(customersC);
                return Ok(result.OrderByDescending(i => i.UpdateTime));

            }
            return BadRequest(new { message = "当前用户没找到" });
        }

        [HttpGet("Customer/Open")]
        public async Task<IActionResult> GetOpenAsync()
        {
            var openCustomer = new List<Customer>();
            var customers = await _context.Customer.Where(i => i.IsOld == false && i.Type != CustomerType.D).ToListAsync();

            var customersA = customers.Where(i => i.Type == CustomerType.A && i.UpdateTime.AddDays(10) <= DateTime.Now);

            var customersB = customers.Where(i => i.Type == CustomerType.B && i.UpdateTime.AddDays(10) <= DateTime.Now);

            var customersC = customers.Where(i => i.Type == CustomerType.C && i.UpdateTime.AddDays(3) <= DateTime.Now);

            openCustomer.AddRange(customersA);
            openCustomer.AddRange(customersB);
            openCustomer.AddRange(customersC);

            return Ok(openCustomer.OrderByDescending(i => i.UpdateTime));
        }
        [HttpPost("Customer/Open")]
        public async Task<IActionResult> AssignOpenAsync(Open value)
        {
            if (value.CustomerType != CustomerType.A && value.CustomerType != CustomerType.B && value.CustomerType != CustomerType.C)
            {
                return  BadRequest(new { message = "客户类型不正确." });
            }
            if (!CustomerAssignCheckNumber(value.CustomerType))
            {
                return BadRequest(new { message = "客户数量超出限制" });
            }

            var user = await _context.Employees.Where(i => i.Id == GetUserId()).FirstOrDefaultAsync();
            var customer = await _context.Customer.Where(i => i.CustomerId == value.CustomerId).OrderByDescending(i => i.Id).FirstOrDefaultAsync();

            customer.IsOld = true;
            customer.UpdateTime = DateTime.Now;
            _context.Customer.Update(customer);
            _context.SaveChanges();
            _context.Entry(customer).State = EntityState.Detached;


            var newCustomer = new Customer();
            newCustomer.CustomerId = customer.CustomerId;
            newCustomer.BusinessLicense = customer.BusinessLicense;
            newCustomer.ContactDetail = customer.ContactDetail;
            newCustomer.CreateTime = DateTime.Now;
            newCustomer.UpdateTime = DateTime.Now;
            newCustomer.IsOld = false;
            newCustomer.Type = value.CustomerType;
            newCustomer.EmployeeId = user.Id;
            _context.Customer.Add(newCustomer);
            _context.SaveChanges();

            return Ok();

        }
        // GET api/<SaleController>/5
        [HttpGet("Customer/{customerId}")]
        public async Task<IActionResult> GetAsync(Guid customerId)
        {
            var customer =  _context.Customer.Where(i => i.CustomerId == customerId);
            if (!customer.Any())
            {
                return NotFound(new { message = "当客户没找到" });
            }
            if (!await ChackAuthAsync(customerId))
            {
                return Unauthorized(new { message = "没有权限访问此客户" });
            }

            var contracts =  _context.Contracts;
            var users=  _context.Employees;
            var result = new List<object>();
            foreach(var  i in customer)
            {
                i.Contracts = await contracts.Where(c => c.CustomerId == customerId && c.DealTime >= i.CreateTime && c.DealTime <= i.UpdateTime).ToListAsync();
                var employee = await users.Where(e => e.Id == i.EmployeeId).FirstOrDefaultAsync();

                result.Add(new
                {
                    customer = i,
                    employeeName = employee?.Name,
                });
            }

            return Ok(result);
        }

        // POST api/<SaleController>
        [HttpPost("Customer")]
        public async Task<IActionResult> PostAsync([FromBody] CustomerRequest value)
        {
            var existcustomer = await _context.Customer.FirstOrDefaultAsync(i => i.BusinessLicense == value.BusinessLicense);
            if (existcustomer != null)
            {
                return Conflict(new { message = "客户已存在" });
            }
            if (!CustomerAssignCheckNumber(value.Type))
            {
                return BadRequest(new { message = "客户数量超出限制" });
            }
            var customer = new Customer();
            customer.CustomerId = Guid.NewGuid();
            customer.IsOld = false;
            customer.Reviewing = false;
            customer.Type = value.Type;
            customer.CreateTime = DateTime.Now;
            customer.UpdateTime = DateTime.Now;
            customer.BusinessLicense = value.BusinessLicense;
            customer.ContactDetail = value.ContactDetail;
            customer.EmployeeId = GetUserId();

            await _context.Customer.AddAsync(customer);
            _context.SaveChanges();

            return Ok(await _context.Customer.FirstOrDefaultAsync(i => i.CustomerId == customer.CustomerId));
        }

        // PUT api/<SaleController>/5
        [HttpPut("Customer/{customerId}")]
        public async Task<IActionResult> PutAsync(Guid customerId, [FromBody] CustomerRequest value)
        {
            var customer = await _context.Customer.FirstOrDefaultAsync(i => i.IsOld == false && i.CustomerId == customerId);
            if (customer == null)
            {
                return NotFound(new { message = "当客户没找到" });
            }
            if (!await ChackAuthAsync(customerId))
            {
                return Unauthorized(new { message = "没有权限访问此客户" });
            }
            var customers = _context.Customer.Where(i => i.IsOld == false && i.EmployeeId == GetUserId());
            if (value.Type == CustomerType.A)
            {
                if (customers.Where(i => i.Type == CustomerType.A).Count() >= 10)
                {
                    return BadRequest(new { message = "客户数量超出限制" });
                }
            }
            else if (value.Type == CustomerType.B)
            {
                if (customers.Where(i => i.Type == CustomerType.A).Count() >= 20)
                {
                    return BadRequest(new { message = "客户数量超出限制" });
                }
            }


            customer.IsOld = false;
            customer.Reviewing = false;
            customer.Type = value.Type;
            customer.BusinessLicense = value.BusinessLicense;
            customer.ContactDetail = value.ContactDetail;

            await _context.Customer.AddAsync(customer);
            _context.SaveChanges();

            return Ok(await _context.Customer.FirstOrDefaultAsync(i => i.BusinessLicense == value.BusinessLicense));
        }

        // DELETE api/<SaleController>/5
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteAsync(int id)
        //{
        //    var customer = await _context.Customer.FirstOrDefaultAsync(i => i.Id == id);
        //    if (customer == null)
        //    {
        //        return NotFound(new { message = "当客户没找到" });
        //    }
        //    if (!await ChackAuthAsync(id))
        //    {
        //        return Unauthorized("没有权限访问此客户");
        //    }
        //    customer.

        //}

        [HttpPost("Customer/{customerId}/Review")]
        public async Task<IActionResult> PostAsync(Guid customerId)
        {
            var customer = await _context.Customer.FirstOrDefaultAsync(i => i.CustomerId == customerId && i.IsOld == false);
            if (customer == null)
            {
                return NotFound(new { message = "当客户没找到" });
            }
            if (!await ChackAuthAsync(customerId))
            {
                return Unauthorized(new { message = "没有权限访问此客户" });
            }

            customer.Reviewing = true;
            customer.UpdateTime = DateTime.Now;
            _context.Customer.Update(customer);
            _context.SaveChanges();
            return Ok(await _context.Customer.FirstOrDefaultAsync(i => i.Id == customer.Id));
        }

        [HttpGet("Customer/Review")]
        public async Task<IActionResult> GetReviewAsync()
        {
            var user = _context.Employees.Where(i => i.Id == GetUserId()).FirstOrDefault();
            var customers = _context.Customer.Where(i => i.IsOld == false);
            if (user == null)
            {
                return BadRequest(new { message = "当前用户没找到" });
            }

            if (user.Role == Role.Administrator)
            {
            }
            else if (user.Role == Role.Manager)
            {
                var children = _context.Employees.Where(i => i.Pid == user.Id).Select(i => i.Id).ToList();
                customers = customers.Where(i => children.Contains(i.EmployeeId));
            }
            else
            {
                customers = customers.Where(i => i.EmployeeId == user.Id);
            }

            return Ok(await customers.Where(i => i.Reviewing == true).ToListAsync());
        }


        [HttpPost("Customer/{customerId}/Contract")]
        public async Task<IActionResult> PostAsync(Guid customerId, [FromBody] ContractRequest value)
        {
            var customer = await _context.Customer.FirstOrDefaultAsync(i => i.CustomerId == customerId && i.IsOld == false);
            if (customer == null)
            {
                return NotFound(new { message = "当客户没找到" });
            }
            if (!await ChackAuthAsync(customerId))
            {
                return Unauthorized(new { message = "没有权限访问此客户" });
            }
            if (!customer.Reviewing)
            {
                return NotFound(new { message = "客户没有被提出申请" });
            }

            var contract = new Contract();
            contract.ContractName = value.ContractName;
            contract.CustomerId = customerId;
            contract.DealAmount = value.DealAmount;
            contract.ContractFile = value.ContractFile;
            contract.DealDuration = value.DealDuration;
            contract.EmployeeId = GetUserId();
            contract.Remark = value.Remark;
            contract.AfterSale = value.AfterSale;
            contract.DealTime = DateTime.Now;


            await _context.Contracts.AddAsync(contract);

            customer.Reviewing = false;
            customer.Type = CustomerType.D;
            customer.UpdateTime = DateTime.Now;
             _context.Customer.Update(customer);

            _context.SaveChanges();
            return Ok();
        }

        private int GetUserId()
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == "userId").Value;
            return int.Parse(user);
        }

        private async ValueTask<bool> ChackAuthAsync(Guid customerId)
        {
            var user = _context.Employees.Where(i => i.Id == GetUserId()).FirstOrDefault();
            var customers = _context.Customer.Where(i => i.IsOld == false && i.CustomerId == customerId);

            if (user.Role == Role.Administrator)
            {
                return true;
            }
            else if (user.Role == Role.Manager)
            {
                var children = _context.Employees.Where(i => i.Pid == user.Id).Select(i => i.Id).ToList();

                var t = await customers.Where(i => children.Contains(i.EmployeeId)).ToListAsync();
                return t.Any();
            }
            else
            {
                var c = customers.Where(i => i.EmployeeId == user.Id).FirstOrDefaultAsync();
                return c != null;
            }

        }

        private bool CustomerAssignCheckNumber(string customerType)
        {
            var user = _context.Employees.Where(i => i.Id == GetUserId()).FirstOrDefault();
            var customers = _context.Customer.Where(i => i.IsOld == false && i.EmployeeId == user.Id);
            if (customerType == CustomerType.A)
            {
                if(customers.Where(i => i.Type == CustomerType.A && i.UpdateTime.AddDays(10) > DateTime.Now).Count() >= 10)
                {
                    return false;
                }
            }
            else if(customerType == CustomerType.B)
            {
                if (customers.Where(i => i.Type == CustomerType.B && i.UpdateTime.AddDays(10) > DateTime.Now).Count() >= 20)
                {
                    return false;
                }
            }


            return true;
            
        }
    }
}
