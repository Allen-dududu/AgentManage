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
    public class PerformanceController : ControllerBase
    {
        private readonly Context _context;
        public PerformanceController(Context context)
        {
            _context = context;
        }

        // GET: api/<ReportController>
        [HttpGet]
        public async Task<IActionResult> Get()
        {

            var user = await _context.Employees.Where(i => i.Id == GetUserId()).AsQueryable().AsNoTracking().FirstOrDefaultAsync();
            if (user == null)
            {
                return BadRequest(new { message = "当前账号不正确" });
            }
            var employees = _context.Employees.AsQueryable().AsNoTracking();
            var result = new List<object>();
            if (user.Role == Role.Administrator)
            {
                var constract = await _context.Contracts.AsQueryable().AsNoTracking().ToListAsync();

                var cons =  constract.GroupBy(i => i.EmployeeId).Select(x => x).ToList();
                foreach (var c in cons)
                {
                    var e = new
                    {
                        EmployeeId = c.Key,
                        EmployeeName = employees.Where(i => i.Id == c.Key).FirstOrDefault()?.Name,
                        Month = decimal.Add(c.Where(i => i.DealTime >= DateTime.UtcNow.AddDays(-DateTime.UtcNow.Day) && i.ContractType == 1).Sum(i => i.DealAmount),
                            c.Where(i => i.DealTime >= DateTime.UtcNow.AddDays(-DateTime.UtcNow.Day)&& i.ContractType == 2).Sum(i => i.DealAmount) / new decimal(2)),
                        Year = decimal.Add(c.Where(i => i.DealTime >= DateTime.UtcNow.AddDays(-DateTime.UtcNow.DayOfYear) && i.ContractType == 1).Sum(i => i.DealAmount) 
                            , c.Where(i => i.DealTime >= DateTime.UtcNow.AddDays(-DateTime.UtcNow.DayOfYear) && i.ContractType == 2).Sum(i => i.DealAmount) / new decimal(2)),
                        All = decimal.Add(c.Where(i => i.ContractType == 1).Sum(i => i.DealAmount), c.Where(i => i.ContractType == 2).Sum(i => i.DealAmount) / new decimal(2)),
                    };
                    result.Add(e);
                }

            }
            else if (user.Role == Role.Manager)
            {
                var children = await employees.Where(i => i.Pid == user.Id).AsQueryable().AsNoTracking().ToListAsync();
                var constract = await _context.Contracts.AsQueryable().AsNoTracking().Where(x => children.Select(i => i.Id).Contains(x.EmployeeId)).ToListAsync();
                var cons = constract.GroupBy(i => i.EmployeeId).Select(x => x).ToList();

                foreach (var c in cons)
                {
                    var e = new
                    {
                        EmployeeId = c.Key,
                        EmployeeName = employees.Where(i => i.Id == c.Key).FirstOrDefault()?.Name,
                        Month = decimal.Add(c.Where(i => i.DealTime >= DateTime.UtcNow.AddDays(-DateTime.UtcNow.Day) && i.ContractType == 1).Sum(i => i.DealAmount),
                            c.Where(i => i.DealTime >= DateTime.UtcNow.AddDays(-DateTime.UtcNow.Day) && i.ContractType == 2).Sum(i => i.DealAmount) / new decimal(2)),
                        Year = decimal.Add(c.Where(i => i.DealTime >= DateTime.UtcNow.AddDays(-DateTime.UtcNow.DayOfYear) && i.ContractType == 1).Sum(i => i.DealAmount)
                            , c.Where(i => i.DealTime >= DateTime.UtcNow.AddDays(-DateTime.UtcNow.DayOfYear) && i.ContractType == 2).Sum(i => i.DealAmount) / new decimal(2)),
                        All = decimal.Add(c.Where(i => i.ContractType == 1).Sum(i => i.DealAmount), c.Where(i => i.ContractType == 2).Sum(i => i.DealAmount) / new decimal(2)),
                    };
                    result.Add(e);
                }
            }
            else
            {
                var employeeContract = _context.Contracts.AsQueryable().AsNoTracking().Where(i => i.EmployeeId == user.Id);
                result.Add(new
                {
                    EmployeeId = user.Id,
                    EmployeeName = user.Name,
                    Month = decimal.Add(employeeContract.Where(i => i.DealTime >= DateTime.UtcNow.AddDays(-DateTime.UtcNow.Day) && i.ContractType == 1).Sum(i => i.DealAmount),
                    employeeContract.Where(i => i.DealTime >= DateTime.UtcNow.AddDays(-DateTime.UtcNow.Day) && i.ContractType == 2).Sum(i => i.DealAmount) / new decimal(2)),
                    Year = decimal.Add(employeeContract.Where(i => i.DealTime >= DateTime.UtcNow.AddDays(-DateTime.UtcNow.DayOfYear) && i.ContractType == 1).Sum(i => i.DealAmount),
                    employeeContract.Where(i => i.DealTime >= DateTime.UtcNow.AddDays(-DateTime.UtcNow.DayOfYear) && i.ContractType == 2).Sum(i => i.DealAmount) / new decimal(2)),
                    All = decimal.Add(employeeContract.Where(i => i.ContractType == 1).Sum(i => i.DealAmount),
                    employeeContract.Where(i => i.ContractType == 2).Sum(i => i.DealAmount) / new decimal(2)),
                });
            }

            return Ok(result);
        }

        private int GetUserId()
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == "userId").Value;
            return int.Parse(user);
        }
    }
}
