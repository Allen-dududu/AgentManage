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
        private readonly IContractRepository _contractRepository;
        public PerformanceController(Context context, IContractRepository contractRepository)
        {
            _context = context;
            _contractRepository = contractRepository;
        }

        // GET: api/<ReportController>
        [HttpGet("pageSize/{pageSize}/page/{page}/employeeName/{employeeName}")]
        public async Task<IActionResult> Get(int pageSize, int page, string employeeName)
        {

            var user = await _context.Employees.Where(i => i.Id == GetUserId()).AsQueryable().AsNoTracking().FirstOrDefaultAsync();
            if (user == null)
            {
                return BadRequest(new { message = "当前账号不正确" });
            }
            var employees = _context.Employees.AsQueryable().AsNoTracking();
            var constract = await _contractRepository.GetContracts();

            var result = new List<Perfomace>();
            if (user.Role == Role.Administrator)
            {

                var cons =  constract.GroupBy(i => i.EmployeeId).Select(x => x).ToList();
                foreach (var c in cons)
                {
                    var e = new Perfomace
                    {
                        EmployeeId = c.Key,
                        EmployeeName = employees.Where(i => i.Id == c.Key).FirstOrDefault()?.Name,
                        Month = decimal.Add(c.Where(i => i.DealTime >= DateTime.UtcNow.AddDays(-DateTime.UtcNow.Day) && i.ContractTemplateType == 1).Sum(i => i.DealAmount),
                            c.Where(i => i.DealTime >= DateTime.UtcNow.AddDays(-DateTime.UtcNow.Day)&& i.ContractTemplateType == 2).Sum(i => i.DealAmount) / new decimal(2)),
                        Year = decimal.Add(c.Where(i => i.DealTime >= DateTime.UtcNow.AddDays(-DateTime.UtcNow.DayOfYear) && i.ContractTemplateType == 1).Sum(i => i.DealAmount) 
                            , c.Where(i => i.DealTime >= DateTime.UtcNow.AddDays(-DateTime.UtcNow.DayOfYear) && i.ContractTemplateType == 2).Sum(i => i.DealAmount) / new decimal(2)),
                        All = decimal.Add(c.Where(i => i.ContractTemplateType == 1).Sum(i => i.DealAmount), c.Where(i => i.ContractTemplateType == 2).Sum(i => i.DealAmount) / new decimal(2)),
                    };
                    result.Add(e);
                }

            }
            else if (user.Role == Role.Manager)
            {
                var children = await employees.Where(i => i.Pid == user.Id).AsQueryable().AsNoTracking().ToListAsync();
                var constract2= constract.Where(x => children.Select(i => i.Id).Contains(x.EmployeeId)).ToList();
                var cons = constract2.GroupBy(i => i.EmployeeId).Select(x => x).ToList();

                foreach (var c in cons)
                {
                    var e = new Perfomace
                    {
                        EmployeeId = c.Key,
                        EmployeeName = employees.Where(i => i.Id == c.Key).FirstOrDefault()?.Name,
                        Month = decimal.Add(c.Where(i => i.DealTime >= DateTime.UtcNow.AddDays(-DateTime.UtcNow.Day) && i.ContractTemplateType == 1).Sum(i => i.DealAmount),
                            c.Where(i => i.DealTime >= DateTime.UtcNow.AddDays(-DateTime.UtcNow.Day) && i.ContractTemplateType == 2).Sum(i => i.DealAmount) / new decimal(2)),
                        Year = decimal.Add(c.Where(i => i.DealTime >= DateTime.UtcNow.AddDays(-DateTime.UtcNow.DayOfYear) && i.ContractTemplateType == 1).Sum(i => i.DealAmount)
                            , c.Where(i => i.DealTime >= DateTime.UtcNow.AddDays(-DateTime.UtcNow.DayOfYear) && i.ContractTemplateType == 2).Sum(i => i.DealAmount) / new decimal(2)),
                        All = decimal.Add(c.Where(i => i.ContractTemplateType == 1).Sum(i => i.DealAmount), c.Where(i => i.ContractTemplateType == 2).Sum(i => i.DealAmount) / new decimal(2)),
                    };
                    result.Add(e);
                }

                var employeeContract = constract.Where(i => i.EmployeeId == user.Id).ToList();
                result.Add(new Perfomace
                {
                    EmployeeId = user.Id,
                    EmployeeName = user.Name,
                    Month = decimal.Add(employeeContract.Where(i => i.DealTime >= DateTime.UtcNow.AddDays(-DateTime.UtcNow.Day) && i.ContractTemplateType == 1).Sum(i => i.DealAmount),
                    employeeContract.Where(i => i.DealTime >= DateTime.UtcNow.AddDays(-DateTime.UtcNow.Day) && i.ContractTemplateType == 2).Sum(i => i.DealAmount) / new decimal(2)),
                    Year = decimal.Add(employeeContract.Where(i => i.DealTime >= DateTime.UtcNow.AddDays(-DateTime.UtcNow.DayOfYear) && i.ContractTemplateType == 1).Sum(i => i.DealAmount),
                    employeeContract.Where(i => i.DealTime >= DateTime.UtcNow.AddDays(-DateTime.UtcNow.DayOfYear) && i.ContractTemplateType == 2).Sum(i => i.DealAmount) / new decimal(2)),
                    All = decimal.Add(employeeContract.Where(i => i.ContractTemplateType == 1).Sum(i => i.DealAmount),
                    employeeContract.Where(i => i.ContractTemplateType == 2).Sum(i => i.DealAmount) / new decimal(2)),
                });
            }
            else
            {
                var employeeContract = constract.Where(i => i.EmployeeId == user.Id).ToList();
                result.Add(new Perfomace
                {
                    EmployeeId = user.Id,
                    EmployeeName = user.Name,
                    Month = decimal.Add(employeeContract.Where(i => i.DealTime >= DateTime.UtcNow.AddDays(-DateTime.UtcNow.Day) && i.ContractTemplateType == 1).Sum(i => i.DealAmount),
                    employeeContract.Where(i => i.DealTime >= DateTime.UtcNow.AddDays(-DateTime.UtcNow.Day) && i.ContractTemplateType == 2).Sum(i => i.DealAmount) / new decimal(2)),
                    Year = decimal.Add(employeeContract.Where(i => i.DealTime >= DateTime.UtcNow.AddDays(-DateTime.UtcNow.DayOfYear) && i.ContractTemplateType == 1).Sum(i => i.DealAmount),
                    employeeContract.Where(i => i.DealTime >= DateTime.UtcNow.AddDays(-DateTime.UtcNow.DayOfYear) && i.ContractTemplateType == 2).Sum(i => i.DealAmount) / new decimal(2)),
                    All = decimal.Add(employeeContract.Where(i => i.ContractTemplateType == 1).Sum(i => i.DealAmount),
                    employeeContract.Where(i => i.ContractTemplateType == 2).Sum(i => i.DealAmount) / new decimal(2)),
                });
            }

            return Ok(new { data = result.Where(i=>i.EmployeeName.StartsWith(employeeName)).Skip(pageSize * page).Take(pageSize) ,
                count = result.Count
            });
        }
        private class Perfomace
        {
            public int EmployeeId { get; set; }
            public string EmployeeName { get; set; }
            public decimal Month { get; set; }
            public decimal Year { get; set; }
            public decimal All { get; set; }

        }
        private int GetUserId()
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == "userId").Value;
            return int.Parse(user);
        }
    }
}
