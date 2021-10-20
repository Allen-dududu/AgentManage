using AgentManage.Model;
using DataBase.EF;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AgentManage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly Context _context;
        private readonly IConfiguration _configuration;
        public AuthController(Context context, IConfiguration config)
        {
            _context = context;
            _configuration = config;
        }

        [HttpPost("login")]
        public IActionResult Logon([FromForm] Login logon)
        {
            if (!string.IsNullOrEmpty(logon.Phone) && !string.IsNullOrEmpty(logon.PassWord))
            {
                var user = _context.Employees.Where(i => i.Phone == logon.Phone && i.PassWord == MD5Encrypt.GetMD5Password(logon.PassWord)).FirstOrDefault();
                if (user != null)
                {
                    // token中的claims用于储存自定义信息，如登录之后的用户id等
                    var claims = new[]
                    {
                         new Claim("userId", user.Id.ToString()),
                    };
                    // 获取SecurityKey
                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("SecurityKey")));
                    var token = new JwtSecurityToken(
                        issuer: Environment.GetEnvironmentVariable("Issure"),                    // 发布者
                        audience: Environment.GetEnvironmentVariable("Audience"),                // 接收者
                        notBefore: DateTime.UtcNow,                                                          // token签发时间
                        expires: DateTime.UtcNow.AddDays(30),                                             // token过期时间
                        claims: claims,                                                                   // 该token内存储的自定义字段信息
                        signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)    // 用于签发token的秘钥算法
                    );

                    var data = new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(token),
                        name = user.Name,
                        role = user.Role,
                        phone = user.Phone,
                    };
                    // 返回成功信息，写出token
                    return Ok(new
                    {
                        message = "登录成功",
                        data = data
                    });
                }

            }
            // 返回错误请求信息
            return BadRequest(new { message = "登录失败，用户名或密码错误" });

        }
    }
}
