using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AgentManage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly string path;

        public IConfiguration Configuration { get; }
        public FileController(IConfiguration configuration)
        {
            Configuration = configuration;
            path = Configuration["FilePath"];
        }
        [HttpPost]
        public IActionResult Upload(IFormFile file, string type, string typeName)
        {
            if (file == null)
            {
                return BadRequest(new { message = "文件为空" });
            }

            var fullPath = Path.Combine(path, type, typeName);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
            // Extract file name from whatever was posted by browser
            var fileName = Path.Combine(fullPath, file.FileName);

            // If file with same name exists delete it
            if (System.IO.File.Exists(fileName))
            {
                return BadRequest(new { message = "同名文件已存在，请更改文件名" });
            }

            // Create new local file and copy contents of uploaded file
            using (var localFile = System.IO.File.OpenWrite(fileName))
            using (var uploadedFile = file.OpenReadStream())
            {
                uploadedFile.CopyTo(localFile);
            }

            return Ok(new { filePath = Path.Combine(type, typeName, file.FileName) });
        }

        [HttpDelete]
        public IActionResult Delete(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return BadRequest(new { message = "文件名不能为空" });
            }

            var fullPath = Path.Combine(path, fileName);


            // If file with same name exists delete it
            if (System.IO.File.Exists(fileName))
            {
                System.IO.File.Delete(fullPath);
            }

            return Ok(new { message = "删除成功" });
        }
    }
}
