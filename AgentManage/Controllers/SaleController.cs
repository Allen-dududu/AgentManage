using AgentManage.Model;
using DataBase.EF;
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

            if (user != null)
            {
                if (user.Role == Role.Administrator)
                {
                    return Ok(await _context.Customer.Where(i => i.IsOld == false).OrderBy(i => i.UpdateTime).ToListAsync());
                }
                else if (user.Role == Role.Manager)
                {
                    var customers = _context.Customer.Where(i => i.IsOld == false);
                    var children = _context.Employees.Where(i => i.Pid == user.Id).Select(i => i.Id).ToList();

                    return Ok(await customers.Where(i => children.Contains(i.EmployeeId)).OrderBy(i => i.UpdateTime).ToListAsync());
                }
                else
                {
                    return Ok(await _context.Customer.Where(i => i.IsOld == false && i.EmployeeId == user.Id).OrderBy(i => i.UpdateTime).ToListAsync());
                }
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

            return Ok(openCustomer);
        }
        [HttpPost("Customer/Open")]
        public async Task<IActionResult> AssignOpenAsync(int customerId)
        {
            var user = await _context.Employees.Where(i => i.Id == GetUserId()).FirstOrDefaultAsync();
            var customer = await _context.Customer.Where(i => i.Id == customerId).FirstOrDefaultAsync();

            customer.IsOld = true;

            var newCustomer = new Customer();
            newCustomer = customer;

        }
        // GET api/<SaleController>/5
        [HttpGet("Customer/{id}")]
        public async Task<IActionResult> GetAsync(int id)
        {
            var user = _context.Employees.Where(i => i.Id == GetUserId()).FirstOrDefault();
            if (user != null)
            {
                var customers = _context.Customer.Where(i => i.IsOld == false);
                var children = _context.Employees.Where(i => i.Pid == user.Id).Select(i => i.Id).ToList();

                return Ok(await customers.Where(i => i.EmployeeId == user.Id).FirstOrDefaultAsync());

            }
            return BadRequest(new { message = "当前用户没找到" });
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

            var customer = new Customer();
            customer.IsOld = false;
            customer.Reviewing = false;
            customer.Type = value.Type;
            customer.CreateTime = DateTime.Now;
            customer.UpdateTime = DateTime.Now;
            customer.BusinessLicense = value.BusinessLicense;
            customer.ConnectDetail = value.ConnectDetail;

            await _context.Customer.AddAsync(customer);
            _context.SaveChanges();

            return Ok(await _context.Customer.FirstOrDefaultAsync(i => i.BusinessLicense == value.BusinessLicense));
        }

        // PUT api/<SaleController>/5
        [HttpPut("Customer/{id}")]
        public async Task<IActionResult> PutAsync(int id, [FromBody] CustomerRequest value)
        {
            var customer = await _context.Customer.FirstOrDefaultAsync(i => i.Id == id);
            if (customer == null)
            {
                return NotFound(new { message = "当客户没找到" });
            }
            if (!await ChackAuthAsync(id))
            {
                return Unauthorized("没有权限访问此客户");
            }
            customer.IsOld = false;
            customer.Reviewing = false;
            customer.Type = value.Type;
            customer.BusinessLicense = value.BusinessLicense;
            customer.ConnectDetail = value.ConnectDetail;

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

        [HttpPost("Customer/{id}/Review")]
        public async Task<IActionResult> PostAsync(int id)
        {
            var customer = await _context.Customer.FirstOrDefaultAsync(i => i.Id == id);
            if (customer == null)
            {
                return NotFound(new { message = "当客户没找到" });
            }
            if (!await ChackAuthAsync(id))
            {
                return Unauthorized("没有权限访问此客户");
            }

            customer.Reviewing = true;

            await _context.Customer.AddAsync(customer);
            _context.SaveChanges();
            return Ok(await _context.Customer.FirstOrDefaultAsync(i => i.Id == id));
        }


        [HttpGet("Customer/{id}/Contract")]
        public async Task<IActionResult> PostAsync(int id, [FromBody] ContractRequest value)
        {
            var customer = await _context.Customer.FirstOrDefaultAsync(i => i.Id == id);
            if (customer == null)
            {
                return NotFound(new { message = "当客户没找到" });
            }
            if (!await ChackAuthAsync(id))
            {
                return Unauthorized("没有权限访问此客户");
            }
            var contract = new Contract();
            contract.ContractName = value.ContractName;
            contract.CustomerId = id;
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

            _context.SaveChanges();
            return Ok();
        }

        private int GetUserId()
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == "userId").Value;
            return int.Parse(user);
        }

        private async ValueTask<bool> ChackAuthAsync(int customerId)
        {
            var user = _context.Employees.Where(i => i.Id == GetUserId()).FirstOrDefault();
            var customers = _context.Customer.Where(i => i.IsOld == false && i.Id == customerId);

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
    }
}
