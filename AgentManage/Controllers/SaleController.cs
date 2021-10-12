using DataBase.EF;
using Microsoft.AspNetCore.Mvc;
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
        public IActionResult Get()
        {
            var user = _context.Employees.Where(i => i.Id == GetUserId()).FirstOrDefault();

            if(user != null)
            {
                if(user.Role == Role.Administrator)
                {
                    return Ok(_context.Customer.Where(i => i.IsOld == false).OrderBy(i => i.UpdateTime).ToList());
                }
                else if(user.Role == Role.Manager)
                {
                    var customers = _context.Customer.Where(i => i.IsOld == false);
                    var children = _context.Employees.Where(i => i.Pid == user.Id).Select(i => i.Id).ToList();

                    return Ok(customers.Where(i => children.Contains(i.EmployeeId)).OrderBy(i => i.UpdateTime).ToList());
                }
                else
                {
                    return Ok(_context.Customer.Where(i => i.IsOld == false && i.EmployeeId == user.Id).OrderBy(i => i.UpdateTime).ToList());
                }
            }
            return BadRequest(new { message = "当前用户没找到" });
        }

        // GET api/<SaleController>/5
        [HttpGet("Customer/{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<SaleController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<SaleController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<SaleController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        private int GetUserId()
        {
            var user = User.Claims.FirstOrDefault(c => c.Type == "userId").Value;
            return int.Parse(user);
        }
    }
}
