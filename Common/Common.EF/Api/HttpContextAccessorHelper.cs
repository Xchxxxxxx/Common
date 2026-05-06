using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Common.EF.Api
{
    internal static class HttpContextAccessorHelper
    {
        private static IHttpContextAccessor? _httpContextAccessor;

        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public static string? TraceId =>
            _httpContextAccessor?.HttpContext?.TraceIdentifier ??
            Activity.Current?.Id;
    }
}
