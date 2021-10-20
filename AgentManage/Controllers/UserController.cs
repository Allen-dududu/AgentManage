using AgentManage.Model;
using DataBase.EF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AgentManage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly Context _context;
        public UserController(Context context)
        {
            _context = context;
        }
        // GET: api/<UserController>
        [Authorize]

        [HttpGet("pageSize/{pageSize}/page/{page}")]
        public async Task<IActionResult> GetAsync(int pageSize,int page)
        {
            return Ok(await _context.Employees.Where(i => i.Status == 0).Select(i => new { i.Id,i.Name,i.Phone,i.Pid,i.Role,i.Status})
                .Skip(pageSize * page).Take(pageSize).ToListAsync());
        }

        // GET api/<UserController>/5
        [Authorize]
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            return Ok(_context.Employees.Where(i => i.Id == id).FirstOrDefault());
        }

        // POST api/<UserController>

        [HttpPost]
        public IActionResult Post([FromBody] User value)
        {
            var exist = _context.Employees.Where(i => (i.Phone == value.Phone && i.Status == 0)).FirstOrDefault();
            if (exist != null)
            {
                return BadRequest(new { message = "账号已存在" });
            }
            if(_context.Employees.Where(i => i.Name == value.Name && i.Status == 0).Any())
            {
                return BadRequest(new { message = "姓名已存在" });
            }
            if (Regex.IsMatch(value.PassWord, @"[\u4e00-\u9fa5]")) 
            {
                return BadRequest(new { message = "密码不容许有中文" });
            };
            var employee = new Employee();
            employee.Status = 0;
            employee.Name = value.Name;

            employee.PassWord = MD5Encrypt.GetMD5Password(value.PassWord);

            employee.Phone = value.Phone;
            employee.Pid = value.Pid;

            // 总监
            if (value.Pid == 0)
            {
                employee.Role = Role.Administrator;
            }
            else
            {
                var father = _context.Employees.Where(i => i.Id == value.Pid && i.Status == 0).FirstOrDefault();
                if (father == null)
                {
                    return BadRequest(new { message = "领导不存在" });
                }
                else
                {
                    if (father.Role == Role.Administrator)
                    {
                        employee.Role = Role.Manager;
                    }
                    else
                    {
                        employee.Role = Role.Agent;
                    }
                }
            }


            _context.Employees.Add(employee);
            _context.SaveChanges();
            return Ok(_context.Employees.Where(i => i.Phone == value.Phone).FirstOrDefault());
        }

        // PUT api/<UserController>/5
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAsync(int id, [FromBody] User value)
        {
            var e = await _context.Employees.Where(i => i.Id == id).FirstOrDefaultAsync();
            if (e != null)
            {
                e.Name = value.Name;
                e.PassWord = value.PassWord;
                e.Phone = value.Phone;
                e.Pid = value.Pid;
                e.Status = value.Status;
                // 总监
                if (value.Pid == 0)
                {
                    e.Role = Role.Administrator;
                }
                else
                {
                    var father = _context.Employees.Where(i => i.Id == value.Pid && i.Status == 0).FirstOrDefault();
                    if (father == null)
                    {
                        return BadRequest(new { message = "领导不存在" });
                    }
                    else
                    {
                        if (father.Role == Role.Administrator)
                        {
                            e.Role = Role.Manager;
                        }
                        else
                        {
                            e.Role = Role.Agent;
                        }
                    }
                }
                _context.SaveChanges();
                return Ok(_context.Employees.Where(i => i.Id == id).FirstOrDefault());
            }
            else
            {
                return NotFound(new { message = "未找到此用户" });
            }
        }

        // DELETE api/<UserController>/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {

            var e = await _context.Employees.Where(i => i.Id == id).FirstOrDefaultAsync();
            if (e != null)
            {
                e.Status = 1;
                _context.SaveChanges();
                return Ok(new { message = "删除成功" });

            }
            else
            {
                return NotFound(new { message = "未找到此用户" });

            }

        }
        [HttpGet("rolesTree")]
        [Authorize]
        public async Task<IActionResult> RolesTreeAsync()
        {
            var users = await _context.Employees.AsQueryable().AsNoTracking().Where(i => i.Status == 0).ToListAsync();
            var admin = users.Where(i => i.Pid == 0).Select(i => new Node { Id = i.Id, Title = i.Name, Role = i.Role }).ToList();

            for (int i = 0; i < admin.Count(); i++)
            {
                BuildRoleTree(users, admin[i]);
            }
            return Ok(admin);

        }
        private class Node
        {
            public string Title { get; set; }

            public int Id { get; set; }

            public string Role { get; set; }

            public bool Expand { get => true; }

            public List<Node> Children { get; set; }
        }
        private void BuildRoleTree(IEnumerable<Employee> users, Node head)
        {
            var children = users.Where(i => i.Pid == head.Id).ToList();
            if (children != null)
            {
                var c = children.Select(i => new Node { Id = i.Id, Title = i.Name, Role = i.Role });
                head.Children = c.ToList();
                foreach(var child in head.Children)
                {
                    var children2 = users.Where(i => i.Pid == child.Id).ToList();
                    child.Children = children2.Select(i => new Node { Id = i.Id, Title = i.Name, Role = i.Role }).ToList();
                }
            }
            else
            {
                return;
            }
        }
    }
}
