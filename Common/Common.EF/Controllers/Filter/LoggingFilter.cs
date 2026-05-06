using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Common.EF.Controllers.Filter
{
    /// <summary>
    /// 请求日志过滤器
    /// </summary>
    public class LoggingFilter : IAsyncActionFilter
    {
        private readonly Logger<LoggingFilter> _logger;

        public LoggingFilter(Logger<LoggingFilter> logger)
        {
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var stopwatch = Stopwatch.StartNew();
            var request = context.HttpContext.Request;

            // 记录请求信息
            var requestInfo = new
            {
                Method = request.Method,
                Path = request.Path,
                QueryString = request.QueryString.ToString(),
                User = context.HttpContext.User.Identity?.Name,
                Ip = context.HttpContext.Connection.RemoteIpAddress?.ToString(),
                TraceId = context.HttpContext.TraceIdentifier
            };

            _logger.LogInformation("请求开始: {@RequestInfo}", requestInfo);

            // 执行Action
            var resultContext = await next();

            stopwatch.Stop();

            // 记录响应信息
            var responseInfo = new
            {
                StatusCode = resultContext.HttpContext.Response.StatusCode,
                Duration = stopwatch.ElapsedMilliseconds,
                TraceId = context.HttpContext.TraceIdentifier
            };

            _logger.LogInformation("请求结束: {@ResponseInfo}", responseInfo);
        }
    }
}
