using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
namespace Common.Auth
{
    public static class JwtExtensions
    {
        public static IServiceCollection AddCustomJwtAuth(this IServiceCollection services, IConfiguration configuration)
        {
            // 读取JWT配置项（统一从appsettings读取）
            var jwtConfig = configuration.GetSection("JwtConfig");
            var secretKey = jwtConfig["SecretKey"];
            var issuer = jwtConfig["Issuer"];
            var audience = jwtConfig["Audience"];
            var expiresMinutes = int.Parse(jwtConfig["ExpiresMinutes"] ?? "30");

            // 校验配置项
            if (string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
            {
                throw new ArgumentNullException("JwtConfig 配置项不完整，请检查appsettings.json");
            }

            // 配置JWT认证
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            // 是否验证发行人
                            ValidateIssuer = true,
                            ValidIssuer = issuer,
                            // 是否验证受众
                            ValidateAudience = true,
                            ValidAudience = audience,
                            // 是否验证密钥
                            ValidateIssuerSigningKey = true,
                            // 密钥（需与签发时一致）
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                            // 是否验证过期时间
                            ValidateLifetime = true,
                            // 允许的时钟偏差（避免时间同步问题导致验证失败）
                            ClockSkew = TimeSpan.FromSeconds(30)
                        };

                        // 可选：配置JWT事件（如令牌验证失败处理）
                        options.Events = new JwtBearerEvents
                        {
                            OnAuthenticationFailed = context =>
                            {
                                // 令牌过期处理
                                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                                {
                                    context.Response.Headers.Append("Token-Expired", "true");
                                }
                                return Task.CompletedTask;
                            },
                            OnMessageReceived = context =>
                            {
                                // 从QueryString中读取Token（前端连接时传 ?access_token=xxx）
                                var accessToken = context.Request.Query["access_token"];
                                if (!string.IsNullOrEmpty(accessToken) &&
                                    context.HttpContext.Request.Path.StartsWithSegments("/SGJHub")) // 你的SignalR Hub路径
                                {
                                    context.Token = accessToken;
                                }
                                return Task.CompletedTask;
                            }
                        };
                        
                    });
            return services;
        }

        /// <summary>
        /// 签发JWT令牌（统一令牌生成逻辑）
        /// </summary>
        /// <param name="configuration">配置对象</param>
        /// <param name="claims">自定义声明（如用户ID、角色）</param>
        /// <returns>JWT令牌</returns>
        public static string GenerateJwtToken(IConfiguration configuration, IEnumerable<System.Security.Claims.Claim> claims)
        {
            var jwtConfig = configuration.GetSection("JwtConfig");
            var secretKey = jwtConfig["SecretKey"];
            var issuer = jwtConfig["Issuer"];
            var audience = jwtConfig["Audience"];
            var expiresMinutes = int.Parse(jwtConfig["ExpiresMinutes"] ?? "30");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 创建令牌
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(expiresMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
