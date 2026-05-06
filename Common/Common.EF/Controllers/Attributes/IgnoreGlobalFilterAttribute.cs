using System;
using System.Collections.Generic;
using System.Text;

namespace Common.EF.Controllers.Attributes
{
    /// <summary>
    /// 忽略全局过滤器特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class IgnoreGlobalFilterAttribute : Attribute
    {
        public string[] FilterNames { get; }

        public IgnoreGlobalFilterAttribute(params string[] filterNames)
        {
            FilterNames = filterNames;
        }
    }
}
