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
        private readonly ICustomerRepository _customerRepository;
        public SaleController(Context context, ICustomerRepository customerRepository)
        {
            _context = context;
            _customerRepository = customerRepository;
        }

        // GET: api/<SaleController>
        [HttpGet("Customer/pageSize/{pageSize}/page/{page}")]
        public async Task<IActionResult> GetAsync(bool isD, int pageSize, int page)
        {
            var user = _context.Employees.Where(i => i.Id == GetUserId()).AsQueryable().AsNoTracking().FirstOrDefault();
            var result = new List<CustomerInfo>();
            if (user != null)
            {
                var customers = await _customerRepository.GetCustomers(user.Role, user.Id);
                //customers = customers.Where(i => i.IsOld == false).ToList();
                if (isD)
                {
                    result.AddRange(customers.Where(i => i.Type == CustomerType.D).ToList());
                }
                else
                {
                    result.AddRange(customers.Where(i => i.Type == CustomerType.A && i.UpdateTime.AddDays(10) > DateTime.UtcNow && i.Discard == false).ToList());

                    result.AddRange(customers.Where(i => i.Type == CustomerType.B && i.UpdateTime.AddDays(10) > DateTime.UtcNow && i.Discard == false).ToList());

                    result.AddRange(customers.Where(i => i.Type == CustomerType.C && i.UpdateTime.AddDays(3) > DateTime.UtcNow && i.Discard == false).ToList());

                }
                return Ok(new { data = result.OrderByDescending(i => i.UpdateTime).Skip(pageSize * page).Take(pageSize),
                    pages = (result.Count / pageSize)+1
                });

            }
            return BadRequest(new { message = "当前用户没找到" });
        }

        [HttpGet("Customer/Open/pageSize/{pageSize}/page/{page}")]
        public async Task<IActionResult> GetOpenAsync(int pageSize, int page)
        {
            var customers = await _customerRepository.GetCustomers(Role.Administrator, 0);
            //customers = customers.Where(i => i.IsOld == false).ToList();
            var result = new List<CustomerInfo>();

            result.AddRange(customers.Where(i => i.Type == CustomerType.A && i.UpdateTime.AddDays(10) <= DateTime.UtcNow).ToList());

            result.AddRange(customers.Where(i => i.Type == CustomerType.B && i.UpdateTime.AddDays(10) <= DateTime.UtcNow).ToList());

            result.AddRange(customers.Where(i => i.Type == CustomerType.C && i.UpdateTime.AddDays(3) <= DateTime.UtcNow).ToList());
            result.AddRange(customers.Where(i => i.Discard == true && !result.Any(r => r.Id == i.Id)).ToList());


            return Ok(result.OrderByDescending(i => i.UpdateTime).Skip(pageSize * page).Take(pageSize));
        }
        [HttpPost("Customer/{customerId}/Discard")]
        public async Task<IActionResult> PostDiscard(Guid customerId)
        {
            var customer = await _context.Customer.Where(i => i.CustomerId == customerId && i.IsOld == false).FirstOrDefaultAsync();
            if(customer == null)
            {
                return BadRequest(new { message = "客户不正确." });
            }

            customer.Discard = true;
            _context.Update(customer);
            _context.SaveChanges();

            return Ok(customer);
        }
        [HttpPost("Customer/Open")]
        public async Task<IActionResult> AssignOpenAsync(Open value)
        {
            if (value.CustomerType != CustomerType.A && value.CustomerType != CustomerType.B && value.CustomerType != CustomerType.C)
            {
                return BadRequest(new { message = "客户类型不正确." });
            }
            var customer = await _context.Customer.Where(i => i.CustomerId == value.CustomerId && i.IsOld == false).AsQueryable().AsNoTracking().FirstOrDefaultAsync();

            if (customer.Version != value.Version)
            {
                return BadRequest(new { message = "客户数据已更改，请刷新" });
            }

            if (!CustomerAssignCheckNumber(value.CustomerType))
            {
                return BadRequest(new { message = "客户数量超出限制" });
            }

            var user = await _context.Employees.Where(i => i.Id == GetUserId()).AsQueryable().AsNoTracking().FirstOrDefaultAsync();

            customer.IsOld = true;
            customer.UpdateTime = DateTime.UtcNow;
            customer.Version = customer.Version + 1;
            _context.Customer.Update(customer);
            _context.SaveChanges();
            _context.Entry(customer).State = EntityState.Detached;


            var newCustomer = new Customer();
            newCustomer.CustomerId = customer.CustomerId;
            newCustomer.BusinessLicense = customer.BusinessLicense;
            newCustomer.ContactDetail = customer.ContactDetail;
            newCustomer.CreateTime = DateTime.UtcNow;
            newCustomer.UpdateTime = DateTime.UtcNow;
            newCustomer.IsOld = false;
            newCustomer.Type = value.CustomerType;
            newCustomer.EmployeeId = user.Id;
            newCustomer.FollowUp = customer.FollowUp;
            newCustomer.Version = 1;
            _context.Customer.Add(newCustomer);
            _context.SaveChanges();

            return Ok();

        }
        // GET api/<SaleController>/5
        [HttpGet("Customer/{customerId}")]
        public async Task<IActionResult> GetAsync(Guid customerId)
        {
            var customer = _context.Customer.Where(i => i.CustomerId == customerId).AsQueryable().AsNoTracking().ToList();
            if (!customer.Any())
            {
                return NotFound(new { message = "当客户没找到" });
            }
            if (!await ChackAuthAsync(customerId))
            {
                return Unauthorized(new { message = "没有权限访问此客户" });
            }

            var result = await _customerRepository.GetCustomersById(customerId);

            return Ok(result);
        }

        // POST api/<SaleController>
        [HttpPost("Customer")]
        public async Task<IActionResult> PostAsync([FromBody] CustomerRequest value)
        {
            if (string.IsNullOrWhiteSpace(value.BusinessLicense) || string.IsNullOrWhiteSpace(value.ContactDetail))
            {
                return BadRequest("参数不能为空");
            }
            var existcustomer = await _context.Customer.AsQueryable().AsNoTracking().FirstOrDefaultAsync(i => i.BusinessLicense == value.BusinessLicense);
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
            customer.CreateTime = DateTime.UtcNow;
            customer.UpdateTime = DateTime.UtcNow;
            customer.BusinessLicense = value.BusinessLicense;
            customer.ContactDetail = value.ContactDetail;
            customer.Version = 1;
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
            if(customer.Version != value.Version)
            {
                return BadRequest(new { message = "客户数据已更改，请刷新" });
            }
            var customers = _context.Customer.AsQueryable().AsNoTracking().Where(i => i.IsOld == false && i.EmployeeId == GetUserId());
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


            customer.Type = value.Type;
            customer.BusinessLicense = value.BusinessLicense;
            customer.ContactDetail = value.ContactDetail;
            customer.FollowUp = value.FollowUp;
            customer.Version = customer.Version + 1;

            _context.Customer.Update(customer);
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
            customer.UpdateTime = DateTime.UtcNow;
            _context.Customer.Update(customer);
            _context.SaveChanges();
            return Ok(await _context.Customer.FirstOrDefaultAsync(i => i.Id == customer.Id));
        }

        [HttpGet("Customer/Review")]
        public async Task<IActionResult> GetReviewAsync()
        {
            var user = _context.Employees.AsQueryable().AsNoTracking().Where(i => i.Id == GetUserId()).FirstOrDefault();
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
            contract.CustomerId = customer.CustomerId;
            contract.DealAmount = value.DealAmount;
            contract.ContractFile = value.ContractFile;
            contract.ContractType = value.ContractType;
            contract.DealDuration = value.DealDuration;
            contract.EmployeeId = GetUserId();
            contract.Remark = value.Remark;
            contract.AfterSale = value.AfterSale;
            contract.DealTime = DateTime.UtcNow;
            contract.CustomerId2 = customer.Id;


            await _context.Contracts.AddAsync(contract);

            customer.Reviewing = false;
            customer.Type = CustomerType.D;
            customer.UpdateTime = DateTime.UtcNow;
            _context.Customer.Update(customer);

            _context.SaveChanges();
            return Ok();
        }

        [HttpGet("Customer/Contract/{contractId}")]
        public async Task<IActionResult> GetContractAsync(int contractId)
        {
            var contract = await _context.Contracts.FirstOrDefaultAsync(i => i.Id == contractId);
            if (contract == null)
            {
                return NotFound(new { message = "合同没找到" });
            }

            return Ok(contract);
        }

        private int GetUserId()
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == "userId").Value;
            return int.Parse(user);
        }

        private async ValueTask<bool> ChackAuthAsync(Guid customerId)
        {
            var user = _context.Employees.Where(i => i.Id == GetUserId()).AsQueryable().AsNoTracking().FirstOrDefault();
            var customers = _context.Customer.AsQueryable().AsNoTracking().Where(i => i.IsOld == false && i.CustomerId == customerId);

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
                var c = await customers.Where(i => i.EmployeeId == user.Id).FirstOrDefaultAsync();
                return c != null;
            }

        }

        private bool CustomerAssignCheckNumber(string customerType)
        {
            var user = _context.Employees.Where(i => i.Id == GetUserId()).AsQueryable().AsNoTracking().FirstOrDefault();
            var customers = _context.Customer.AsQueryable().AsNoTracking().Where(i => i.IsOld == false && i.EmployeeId == user.Id && i.Discard == false);
            if (customerType == CustomerType.A)
            {
                if (customers.Where(i => i.Type == CustomerType.A && i.UpdateTime.AddDays(10) > DateTime.UtcNow).Count() >= 10)
                {
                    return false;
                }
            }
            else if (customerType == CustomerType.B)
            {
                if (customers.Where(i => i.Type == CustomerType.B && i.UpdateTime.AddDays(10) > DateTime.UtcNow).Count() >= 20)
                {
                    return false;
                }
            }


            return true;

        }
    }
}
