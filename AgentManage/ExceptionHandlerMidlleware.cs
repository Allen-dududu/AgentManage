using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AgentManage
{
    public class ExceptionHandlerMidlleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlerMidlleware> _logger;

        public ExceptionHandlerMidlleware(RequestDelegate next, ILogger<ExceptionHandlerMidlleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                _logger.LogError(ex, ex.Message);
                _logger.LogError($"[REQUEST FAILED]:  {GetHost(context)} {GetPath(context)}, {GetQuery(context)}");
                var response = context.Response;
                response.ContentType = "application/json";
                response.StatusCode = 500;
                await response.WriteAsync(JsonConvert.SerializeObject(new { message = "程序内部错误"})).ConfigureAwait(false);
            }
        }

        private static string GetHost(HttpContext context)
        {
            try
            {
                return context.Request.Host.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string GetPath(HttpContext context)
        {
            try
            {
                return context.Request.Path.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string GetQuery(HttpContext context)
        {
            try
            {
                return context.Request.QueryString.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            if (ex is UnauthorizedAccessException)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync(JsonConvert.SerializeObject(ex.Message), Encoding.UTF8);
        }
    }
}
