using Common.EF.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;

namespace Common.EF.Controllers
{

    /// <summary>
    /// API控制器基类
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public abstract class BaseApiController : ControllerBase
    {
        private ILogger? _logger;

        protected ILogger Logger => _logger ??= HttpContext.RequestServices.GetRequiredService<ILogger<BaseApiController>>();

        /// <summary>
        /// 当前用户ID
        /// </summary>
        protected virtual string? CurrentUserId =>
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
            User.FindFirst("sub")?.Value;

        /// <summary>
        /// 当前用户名
        /// </summary>
        protected virtual string? CurrentUserName =>
            User.Identity?.Name ??
            User.FindFirst(ClaimTypes.Name)?.Value;

        /// <summary>
        /// 当前用户角色
        /// </summary>
        protected virtual IEnumerable<string> CurrentRoles =>
            User.FindAll(ClaimTypes.Role).Select(c => c.Value);

        /// <summary>
        /// 是否已认证
        /// </summary>
        protected virtual bool IsAuthenticated => User.Identity?.IsAuthenticated ?? false;

        /// <summary>
        /// 获取客户端IP
        /// </summary>
        protected virtual string? ClientIp =>
            HttpContext.Connection.RemoteIpAddress?.ToString();

        /// <summary>
        /// 获取请求追踪ID
        /// </summary>
        protected virtual string TraceId =>
            HttpContext.TraceIdentifier ?? Activity.Current?.Id ?? Guid.NewGuid().ToString();

        /// <summary>
        /// 统一返回ApiResponse
        /// </summary>
        protected virtual ActionResult<ApiResponse<T>> Ok<T>(T data, string message = "操作成功")
        {
            return base.Ok(ApiResponse<T>.Success(data, message));
        }

        /// <summary>
        /// 无数据成功响应
        /// </summary>
        protected virtual ActionResult<ApiResponse<object>> Ok(string message = "操作成功")
        {
            return base.Ok(ApiResponse<object>.Success(null, message));
        }

        /// <summary>
        /// 创建成功响应
        /// </summary>
        protected virtual ActionResult<ApiResponse<T>> Created<T>(string actionName, object routeValues, T data, string message = "创建成功")
        {
            var response = ApiResponse<T>.Success(data, message, 201);
            return CreatedAtAction(actionName, routeValues, response);
        }

        /// <summary>
        /// 创建成功响应（无路由）
        /// </summary>
        protected virtual ActionResult<ApiResponse<T>> Created<T>(T data, string message = "创建成功")
        {
            var response = ApiResponse<T>.Success(data, message, 201);
            return StatusCode(201, response);
        }

        /// <summary>
        /// 错误响应
        /// </summary>
        protected virtual ActionResult<ApiResponse<T>> Error<T>(string message, int statusCode = 400, List<string>? errors = null)
        {
            return StatusCode(statusCode, ApiResponse<T>.Error(message, statusCode, errors));
        }

        /// <summary>
        /// 未找到响应
        /// </summary>
        protected virtual ActionResult<ApiResponse<T>> NotFound<T>(string message = "资源不存在")
        {
            return NotFound(ApiResponse<T>.NotFound(message));
        }

        /// <summary>
        /// 验证失败响应
        /// </summary>
        protected virtual ActionResult<ApiResponse<T>> BadRequest<T>(List<string> errors)
        {
            return BadRequest(ApiResponse<T>.ValidationError(errors));
        }

        /// <summary>
        /// 无权限响应
        /// </summary>
        protected virtual ActionResult<ApiResponse<T>> Forbidden<T>(string message = "无权限访问")
        {
            return StatusCode(403, ApiResponse<T>.Forbidden(message));
        }

        /// <summary>
        /// 未授权响应
        /// </summary>
        protected virtual ActionResult<ApiResponse<T>> Unauthorized<T>(string message = "请先登录")
        {
            return StatusCode(401, ApiResponse<T>.Unauthorized(message));
        }

        /// <summary>
        /// 处理ApiResponse结果
        /// </summary>
        protected virtual ActionResult ToResponse<T>(ApiResponse<T> response)
        {
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// 处理服务层返回的ApiResponse
        /// </summary>
        protected virtual async Task<ActionResult> ExecuteAsync<T>(Func<Task<ApiResponse<T>>> action)
        {
            try
            {
                var result = await action();
                return ToResponse(result);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "请求处理异常");
                return StatusCode(500, ApiResponse<object>.Error($"系统异常: {ex.Message}", 500));
            }
        }
    }
}
