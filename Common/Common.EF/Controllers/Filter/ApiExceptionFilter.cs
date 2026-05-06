using Common.EF.Api;
using Common.EF.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Common.EF.Controllers.Filter
{
    internal class ApiExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<ApiExceptionFilter> _logger;
        private readonly IWebHostEnvironment _env;

        public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public void OnException(ExceptionContext context)
        {
            var exception = context.Exception;
            var traceId = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;

            _logger.LogError(exception, "请求异常 TraceId: {TraceId}", traceId);

            ApiResponse<object> response;

            switch (exception)
            {
                case NotFoundException notFound:
                    response = ApiResponse<object>.NotFound(notFound.Message);
                    break;

                case ValidationException validation:
                    // 获取所有验证错误
                    var errors = validation.Errors?
                        .SelectMany(kvp => kvp.Value.Select(v => $"{kvp.Key}: {v}"))
                        .ToList() ?? new List<string>();

                    // 如果没有具体错误，使用异常消息
                    if (errors.Count == 0)
                    {
                        errors.Add(validation.Message);
                    }

                    response = ApiResponse<object>.ValidationError(errors);
                    break;

                case BusinessException business:
                    response = ApiResponse<object>.Error(business.Message, 400);
                    break;

                case UnauthorizedAccessException:
                    response = ApiResponse<object>.Unauthorized(exception.Message);
                    break;

                default:
                    var errorMessage = _env.IsDevelopment() ? exception.Message : "服务器内部错误";
                    response = ApiResponse<object>.Error(errorMessage, 500);
                    break;
            }

            context.Result = new ObjectResult(response)
            {
                StatusCode = response.StatusCode
            };

            context.ExceptionHandled = true;
        }
    }
}