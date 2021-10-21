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
    public class ContractTemplateController : ControllerBase
    {
        private readonly Context _context;
        public ContractTemplateController(Context context)
        {
            _context = context;
        }
        // GET: api/<ContractTemplateController>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _context.ContractTemplate.AsQueryable().AsNoTracking().Where(i => i.Status == 0).OrderBy(i=>i.Id).ToListAsync());
        }

        // GET api/<ContractTemplateController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            return Ok(await _context.ContractTemplate.AsQueryable().AsNoTracking().Where(i => i.Id == id).FirstOrDefaultAsync());
        }

        // POST api/<ContractTemplateController>
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] ContractTemplateRequest value)
        {
            var newObject = new ContractTemplate();
            newObject.ContractAmount = value.ContractAmount;
            newObject.ContractName = value.ContractName;
            newObject.ContractFile = value.ContractFile;
            newObject.ContractDetail = value.ContractDetail;
            newObject.ContractType = value.ContractType;
            newObject.Status = 0;
            _context.ContractTemplate.Add(newObject);
            await _context.SaveChangesAsync();

            return Ok(newObject);
        }

        // PUT api/<ContractTemplateController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAsync(int id, [FromBody] ContractTemplateRequest value)
        {
            var newObject = await _context.ContractTemplate.Where(i => i.Id == id).FirstOrDefaultAsync();

            if (newObject == null)
            {
                return NotFound(new { message = "找不到该项目" });
            }

            newObject.ContractAmount = value.ContractAmount;
            newObject.ContractName = value.ContractName;
            newObject.ContractFile = value.ContractFile;
            newObject.ContractDetail = value.ContractDetail;
            newObject.ContractType = value.ContractType;
            _context.ContractTemplate.Update(newObject);
            await _context.SaveChangesAsync();

            return Ok(newObject);
        }
        [HttpDelete("{id}")]

        public async Task<IActionResult> DeleteAsync(int id)
        {
            var newObject = await _context.ContractTemplate.Where(i => i.Id == id).FirstOrDefaultAsync();

            if (newObject == null)
            {
                return NotFound(new { message = "找不到该项目" });
            }

            newObject.Status = 1;

            _context.ContractTemplate.Update(newObject);
            await _context.SaveChangesAsync();

            return Ok(new { message = "删除成功"});
        }
    }
}
