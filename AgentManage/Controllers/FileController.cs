using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        private readonly string path = Environment.GetEnvironmentVariable("FilePath");
        [HttpPost]
        public IActionResult Upload(IFormFile file,string type,string typeId)
        {
            var fullPath = Path.Combine(path, type, typeId);
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

            return Ok(new { filePath = Path.Combine(type, typeId, file.FileName) });
        }
    }
}
