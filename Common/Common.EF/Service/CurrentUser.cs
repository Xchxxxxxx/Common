using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.EF.Service
{
    public class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? UserId => GetClaimValue<string>("sub") ?? GetClaimValue<string>("nameid");
        public string? UserName => GetClaimValue<string>("unique_name") ?? GetClaimValue<string>("name");
        public string? Email => GetClaimValue<string>("email");
        public IEnumerable<string> Roles => GetClaimValues("role");
        public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

        public bool IsInRole(string role)
        {
            return Roles.Contains(role);
        }

        public T? GetClaimValue<T>(string claimType)
        {
            var claim = _httpContextAccessor.HttpContext?.User.FindFirst(claimType);
            if (claim == null) return default;

            try
            {
                return (T)Convert.ChangeType(claim.Value, typeof(T));
            }
            catch
            {
                return default;
            }
        }

        private IEnumerable<string> GetClaimValues(string claimType)
        {
            return _httpContextAccessor.HttpContext?.User.FindAll(claimType).Select(c => c.Value) ?? Array.Empty<string>();
        }
    }
}
