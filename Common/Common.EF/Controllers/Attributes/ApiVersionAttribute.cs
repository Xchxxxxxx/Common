using System;
using System.Collections.Generic;
using System.Text;

namespace Common.EF.Controllers.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiVersionAttribute : Attribute
    {
        public string Version { get; }
        public bool Deprecated { get; set; }

        public ApiVersionAttribute(string version)
        {
            Version = version;
        }
    }
}
