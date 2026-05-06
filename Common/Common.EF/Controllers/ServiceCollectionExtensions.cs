using Common.EF.Controllers.Filter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using Common.EF.Controllers;
using Common.EF.Api;

namespace Common.EF.xxx
{
    /// <summary>
    /// 服务集合扩展
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加控制器帮助类
        /// </summary>
        public static IServiceCollection AddControllerHelpers(this IServiceCollection services, Action<ControllerHelperOptions>? configure = null)
        {
            var option = new ControllerHelperOptions();
            configure?.Invoke(option);

            // 添加过滤器
            services.AddScoped<ApiExceptionFilter>();
            services.AddScoped<ValidationFilter>();
            services.AddScoped<LoggingFilter>();
            services.AddScoped<PerformanceFilter>();

            // 配置MVC
            services.AddControllers(options =>
            {
                if (option.UseGlobalExceptionFilter)
                {
                    options.Filters.Add<ApiExceptionFilter>();
                }
                if (option.UseValidationFilter)
                {
                    options.Filters.Add<ValidationFilter>();
                }
                if (option.UseLoggingFilter)
                {
                    options.Filters.Add<LoggingFilter>();
                }
                if (option.UsePerformanceFilter)
                {
                    options.Filters.Add<PerformanceFilter>();
                }
            })
            .ConfigureApiBehaviorOptions(apiOptions =>
            {
                // 禁用默认模型验证响应
                apiOptions.SuppressModelStateInvalidFilter = !option.UseModelStateValidation;
                apiOptions.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .SelectMany(x => x.Value!.Errors.Select(e => $"{x.Key}: {e.ErrorMessage}"))
                        .ToList();

                    var response = ApiResponse<object>.ValidationError(errors);
                    return new BadRequestObjectResult(response);
                };
            });

            return services;
        }

        /// <summary>
        /// 添加全局中间件
        /// </summary>
        //public static IApplicationBuilder UseControllerMiddlewares(this IApplicationBuilder app)
        //{
        //    app.UseMiddleware<ExceptionHandlingMiddleware>();
        //    app.UseMiddleware<RequestTimingMiddleware>();

        //    return app;
        //}
    }
}
