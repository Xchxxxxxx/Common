using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Common.EF.Controllers.Filter
{
    /// <summary>
    /// 性能监控过滤器
    /// </summary>
    public class PerformanceFilter : IAsyncActionFilter
    {
        private readonly ILogger<PerformanceFilter> _logger;
        private readonly int _thresholdMs;

        public PerformanceFilter(ILogger<PerformanceFilter> logger, int thresholdMs = 1000)
        {
            _logger = logger;
            _thresholdMs = thresholdMs;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var stopwatch = Stopwatch.StartNew();
            var actionName = $"{context.Controller.GetType().Name}.{context.ActionDescriptor.RouteValues["action"]}";

            var resultContext = await next();

            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            if (elapsedMs > _thresholdMs)
            {
                _logger.LogWarning("慢请求警告: {ActionName} 耗时 {ElapsedMs}ms", actionName, elapsedMs);
            }

            // 添加响应头
            context.HttpContext.Response.Headers.Add("X-Response-Time-ms", elapsedMs.ToString());
        }
    }
}
