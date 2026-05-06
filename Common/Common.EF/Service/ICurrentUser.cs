using System;
using System.Collections.Generic;
using System.Text;

namespace Common.EF.Service
{
    public interface ICurrentUser
    {
        string? UserId { get; }
        string? UserName { get; }
        string? Email { get; }
        IEnumerable<string> Roles { get; }
        bool IsAuthenticated { get; }
        bool IsInRole(string role);
        T? GetClaimValue<T>(string claimType);
    }

}
