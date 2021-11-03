using DataBase.EF;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.OpenApi.Models;

namespace AgentManage
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Console.WriteLine(Configuration["DapperConnect"]);
            Console.WriteLine("test");

            services.AddControllers().AddNewtonsoftJson();

            services.AddControllers();
            services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
            services.AddDbContext<Context>(options =>
             options.UseNpgsql(Configuration["Context"]));
            // Documentation can be accessed with http://localhost:5002/swagger/ui/index.html
            //注册Swagger生成器，定义一个和多个Swagger 文档
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
            });

            // 添加下面配置信息
            services.AddAuthentication(options =>
            {
                // 设置默认使用jwt验证方式
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                var t = Configuration["Issure"];
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    // 验证接收者
                    ValidateAudience = true,
                    // 验证发布者
                    ValidateIssuer = true,
                    // 验证过期时间
                    ValidateLifetime = true,
                    // 验证秘钥
                    ValidateIssuerSigningKey = true,
                    // 读配置Issure
                    ValidIssuer = Configuration["Issure"],
                    // 读配置Audience
                    ValidAudience = Configuration["Audience"],
                    // 设置生成token的秘钥
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecurityKey"]))
                };
            });
            services.AddSingleton<ICustomerRepository, CustomerRepository>();
            services.AddSingleton<IContractRepository, ContractRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            app.UseMiddleware(typeof(ExceptionHandlerMidlleware));

            if (env.IsDevelopment())
            {
                //app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            //启用中间件服务生成Swagger作为JSON终结点
            app.UseSwagger();
            //启用中间件服务对swagger-ui，指定Swagger JSON终结点
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });
            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });


        }
    }
}
