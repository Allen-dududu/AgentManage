using AgentManage.Model;
using DataBase.EF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AgentManage.Controllers
{
    [Route("api/[controller]")]
    //[Authorize]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly Context _context;
        public UserController(Context context)
        {
            _context = context;
        }
        // GET: api/<UserController>

        [HttpGet]
        public IEnumerable<Employee> Get()
        {
            return _context.Employees.ToList();
        }

        // GET api/<UserController>/5
        [HttpGet("{id}")]
        public Employee Get(int id)
        {
            return _context.Employees.Where(i => i.Id == id).FirstOrDefault();
        }

        // POST api/<UserController>
        [HttpPost]
        public IActionResult Post([FromBody] User value)
        {
            var exist = _context.Employees.Where(i => i.Phone == value.Phone).FirstOrDefault();
            if (exist != null)
            {
                return BadRequest(new { message = "账号已存在" });
            }
            var employee = new Employee()
            {
                Name = value.Name,
                PassWord = value.PassWord,
                Role = value.Role,
                Phone = value.Phone,
                Pid = value.Pid,
                Status = 0
            };
            _context.Employees.Add(employee);
            _context.SaveChanges();
            return Ok();
        }

        // PUT api/<UserController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] User value)
        {
            var e = _context.Employees.Where(i => i.Id == id).FirstOrDefault();
            if (e != null)
            {
                e.Name = value.Name;
                e.PassWord = value.Name;
                e.Role = value.Role;
                e.Phone = value.Phone;
                e.Pid = value.Pid;
                e.Status = value.Status;
                _context.SaveChanges();
            }
        }

        // DELETE api/<UserController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {

            var e = _context.Employees.Where(i => i.Id == id).FirstOrDefault();
            if (e != null)
            {
                e.Status = 1;
                _context.SaveChanges();
            }
        }
    }
}
