using System;
using System.Collections.Generic;
using System.Text;

namespace Common.EF.Controllers.Attributes
{
    public enum RateLimitKeyType
    {
        ClientIp,
        UserId,
        ApiKey,
        Custom
    }
}
