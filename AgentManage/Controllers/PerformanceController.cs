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

            var user = await _context.Employees.Where(i => i.Id == GetUserId()).FirstOrDefaultAsync();
            if (user == null)
            {
                return BadRequest(new { message = "当前账号不正确" });
            }
            var employees = _context.Employees;
            var result = new List<object>();
            if (user.Role == Role.Administrator)
            {
                var constract = from c in _context.Contracts
                                group c by c.EmployeeId into newGroup
                                select newGroup;

                foreach (var c in await constract.ToListAsync())
                {
                    var e = new
                    {
                        EmployeeId = c.Key,
                        EmployeeName = employees.Where(i => i.Id == c.Key).FirstOrDefault()?.Name,
                        Month = c.Where(i => i.DealTime >= DateTime.UtcNow.AddDays(-DateTime.UtcNow.Day)).Sum(i => i.DealAmount),
                        Year = c.Where(i => i.DealTime >= DateTime.UtcNow.AddDays(-DateTime.UtcNow.DayOfYear)).Sum(i => i.DealAmount),
                        All = c.Sum(i => i.DealAmount),
                    };
                    result.Add(e);
                }

            }
            else if (user.Role == Role.Manager)
            {
                var children = await employees.Where(i => i.Pid == user.Id).ToListAsync();
                var constract = from c in _context.Contracts.Where(x => children.Select(i => i.Id).Contains(x.EmployeeId))
                                group c by c.EmployeeId into newGroup
                                select newGroup;

                foreach (var c in await constract.ToListAsync())
                {
                    var e = new
                    {
                        EmployeeId = c.Key,
                        EmployeeName = employees.Where(i => i.Id == c.Key).FirstOrDefault()?.Name,
                        Month = c.Where(i => i.DealTime >= DateTime.UtcNow.AddDays(-DateTime.UtcNow.Day)).Sum(i => i.DealAmount),
                        Year = c.Where(i => i.DealTime >= DateTime.UtcNow.AddDays(-DateTime.UtcNow.DayOfYear)).Sum(i => i.DealAmount),
                        All = c.Sum(i => i.DealAmount),
                    };
                    result.Add(e);
                }
            }
            else
            {
                result.Add(new
                {
                    EmployeeId = user.Id,
                    EmployeeName = user.Name,
                    Month = _context.Contracts.Where(i => i.DealTime >= DateTime.UtcNow.AddDays(-DateTime.UtcNow.Day) && i.EmployeeId == user.Id).Sum(i => i.DealAmount),
                    Year = _context.Contracts.Where(i => i.DealTime >= DateTime.UtcNow.AddDays(-DateTime.UtcNow.DayOfYear) && i.EmployeeId == user.Id).Sum(i => i.DealAmount),
                    All = _context.Contracts.Where(i => i.EmployeeId == user.Id).Sum(i => i.DealAmount),
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
