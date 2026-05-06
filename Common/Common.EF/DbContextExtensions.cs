using Common.EF.EF;
using Common.EF.Service;
using Common.uniwork;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Data;

namespace Common.EF
{
    public static class DbContextExtensions
    {
        /// <summary>
        /// 注册自定义DbContext（通用方法，支持任意DbContext子类）
        /// 已从SQL Server改为MySQL连接
        /// </summary>
        /// <typeparam name="TDbContext">你的DbContext类型（如OrderDbContext、ProductDbContext）</typeparam>
        /// <param name="services">服务集合</param>
        /// <param name="configuration">配置对象</param>
        /// <param name="connectionStringKey">appsettings中连接字符串的键（默认：DefaultConnection）</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddCustomDbContext<TDbContext>(this IServiceCollection services,
            IConfiguration configuration,
            string connectionStringKey = "DefaultConnection")
            where TDbContext : DbContext
        {
            // 读取数据库连接字符串
            var connectionString = configuration.GetConnectionString(connectionStringKey);
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(connectionStringKey, "数据库连接字符串未配置，请检查appsettings.json");

            // 注册DbContext（默认Scoped生命周期，EF Core推荐）
            services.AddDbContext<TDbContext>(options =>
            {
                // ========== 注释掉SQL Server连接配置 ==========
                // options.UseSqlServer(connectionString, sqlOptions =>
                // {
                //     // 配置连接池、命令超时等（可选，根据业务需求调整）
                //     sqlOptions.MaxBatchSize(100); // 连接池最大连接数
                //     sqlOptions.CommandTimeout(30); // 命令超时时间（秒）
                //     // 启用迁移程序集（若使用数据库迁移，指定迁移所在程序集）
                //     sqlOptions.MigrationsAssembly(typeof(TDbContext).Assembly.FullName);
                // });

                // ========== MySQL连接配置 ==========
                options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 30)), // 适配MySQL 8.0+
                    mysqlOptions =>
                    {
                        // MySQL连接配置（对应原SQL Server的配置项）
                        mysqlOptions.MaxBatchSize(100); // 批量操作最大条数
                        mysqlOptions.CommandTimeout(30); // 命令超时时间（秒）
                        mysqlOptions.MigrationsAssembly(typeof(TDbContext).Assembly.FullName); // 迁移程序集

                        // MySQL特有配置（可选）
                        //mysqlOptions.CharSetBehavior(CharSetBehavior.Never); // 字符集行为
                        //mysqlOptions.UseNewtonsoftJson(); // 支持JSON字段（如需）
                    });

                // 开发环境启用详细日志（可选）
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    options.EnableSensitiveDataLogging(); // 显示敏感数据
                    options.EnableDetailedErrors(); // 显示详细错误信息
                }
            });
            services.AddScoped<IUnitOfWork, UnitOfWork<TDbContext>>();
            services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
            return services;
        }

        /// <summary>
        /// 注册Identity+自定义DbContext（集成IdentityDbContext）
        /// 已从SQL Server改为MySQL连接
        /// </summary>
        /// <typeparam name="TUser">用户实体（继承IdentityUser）</typeparam>
        /// <typeparam name="TRole">角色实体（继承IdentityRole）</typeparam>
        /// <typeparam name="TDbContext">集成IdentityDbContext的上下文</typeparam>
        public static IServiceCollection AddCustomIdentityWithDbContext<TUser, TRole, TDbContext>(
            this IServiceCollection services,
            IConfiguration configuration,
            string connectionStringKey = "DefaultConnection")
            where TUser : IdentityUser<int>
            where TRole : IdentityRole<int>
            where TDbContext : IdentityDbContext<TUser, TRole, int>
        {
            // 1. 注册 Identity DbContext (开启这部分)
            var connectionString = configuration.GetConnectionString(connectionStringKey);
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(connectionStringKey, "Identity数据库连接字符串未配置");

            services.AddDbContext<TDbContext>(options =>
            {
                // 使用 MySQL 连接配置
                options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 30)),
                    mysqlOptions =>
                    {
                        mysqlOptions.MaxBatchSize(100);
                        mysqlOptions.CommandTimeout(30);
                        mysqlOptions.MigrationsAssembly(typeof(TDbContext).Assembly.FullName);
                    });

                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }
            });

            // 2. 注册你的 UnitOfWork 和 Repository (确保它们能正确解析到上面注册的 TDbContext)
            services.AddScoped<IUnitOfWork, UnitOfWork<TDbContext>>();
            services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
            services.AddScoped<ICurrentUser, CurrentUser>();

            // 3. 注册完整的 Identity 服务 (使用 AddIdentity)
            services.AddIdentity<TUser, TRole>(options =>
            {
                // 密码配置
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
                // 其他配置...
            })
            .AddEntityFrameworkStores<TDbContext>()
            .AddDefaultTokenProviders();

            // 4. 配置 Cookie 认证 (可选，但推荐)
            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromHours(12);
                options.LoginPath = "/api/auth/login";
                options.LogoutPath = "/api/auth/logout";
                options.AccessDeniedPath = "/api/auth/access-denied";
                options.SlidingExpiration = true;
            });

            return services;
        }
    }
}