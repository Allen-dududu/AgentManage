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
            //ע��Swagger������������һ���Ͷ��Swagger �ĵ�
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
            });

            // �������������Ϣ
            services.AddAuthentication(options =>
            {
                // ����Ĭ��ʹ��jwt��֤��ʽ
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                var t = Configuration["Issure"];
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    // ��֤������
                    ValidateAudience = true,
                    // ��֤������
                    ValidateIssuer = true,
                    // ��֤����ʱ��
                    ValidateLifetime = true,
                    // ��֤��Կ
                    ValidateIssuerSigningKey = true,
                    // ������Issure
                    ValidIssuer = Configuration["Issure"],
                    // ������Audience
                    ValidAudience = Configuration["Audience"],
                    // ��������token����Կ
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
            //�����м����������Swagger��ΪJSON�ս��
            app.UseSwagger();
            //�����м�������swagger-ui��ָ��Swagger JSON�ս��
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
