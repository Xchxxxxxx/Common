using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Common.EF.Api
{
    public class ApiResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? TraceId { get; set; }
        public List<string> Errors { get; set; } = new();
    }
    public class ApiResponse<T> : ApiResponse
    {
        public T? Data { get; set; }

        public static ApiResponse<T> Success(T data, string message = "操作成功", int statusCode = 200)
        {
            return new ApiResponse<T>
            {
                IsSuccess = true,
                Data = data,
                Message = message,
                StatusCode = statusCode,
                TraceId = Activity.Current?.Id ?? HttpContextAccessorHelper.TraceId
            };
        }

        public static ApiResponse<T> Error(string message, int statusCode = 400, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                IsSuccess = false,
                Message = message,
                StatusCode = statusCode,
                Errors = errors ?? new List<string>(),
                TraceId = Activity.Current?.Id ?? HttpContextAccessorHelper.TraceId
            };
        }

        public static ApiResponse<T> NotFound(string message = "资源不存在")
        {
            return Error(message, 404);
        }

        public static ApiResponse<T> Unauthorized(string message = "未授权访问")
        {
            return Error(message, 401);
        }

        public static ApiResponse<T> Forbidden(string message = "禁止访问")
        {
            return Error(message, 403);
        }

        public static ApiResponse<T> ValidationError(List<string> errors)
        {
            return Error("验证失败", 400, errors);
        }
    }
}
