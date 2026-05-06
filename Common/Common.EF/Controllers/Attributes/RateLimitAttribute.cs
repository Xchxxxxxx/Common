using System;
using System.Collections.Generic;
using System.Text;

namespace Common.EF.Controllers.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RateLimitAttribute : Attribute
    {
        /// <summary>时间窗口（秒）</summary>
        public int WindowSeconds { get; set; } = 60;

        /// <summary>最大请求数</summary>
        public int MaxRequests { get; set; } = 100;

        /// <summary>限流键类型</summary>
        public RateLimitKeyType KeyType { get; set; } = RateLimitKeyType.ClientIp;

        public RateLimitAttribute(int maxRequests, int windowSeconds = 60)
        {
            MaxRequests = maxRequests;
            WindowSeconds = windowSeconds;
        }
    }
}
