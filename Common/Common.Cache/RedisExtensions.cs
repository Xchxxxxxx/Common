using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Common.Cache
{
    public static class RedisExtensions
    {
        /// <summary>
        /// 注册自定义 Redis 客户端（适配带参数的原生连接字符串）
        /// </summary>
        public static IServiceCollection AddCustomRedisClient(this IServiceCollection services,
            IConfiguration configuration, string configName = "RedisConfig")
        {
            var redisConfig = configuration.GetSection(configName);

            // 1. 读取基础配置（兼容配置文件中的字符串转数值）
            var defaultDatabase = int.TryParse(redisConfig["DefaultDatabase"], out int db) ? db : 0;
            var connectTimeout = int.TryParse(redisConfig["ConnectTimeout"], out int ct) ? ct : 5000;
            var syncTimeout = int.TryParse(redisConfig["SyncTimeout"], out int st) ? st : 3000;
            var connectRetry = 3; // 配置文件中connectRetry默认3，也可从连接字符串解析

            // 2. 优先读取环境变量 REDIS_URL（Railway部署），否则读配置文件
            string connectionString = Environment.GetEnvironmentVariable("REDIS_URL");
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = redisConfig["ConnectionString"];
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new ArgumentNullException($"{configName}:ConnectionString", "Redis 连接字符串未配置");
                }
            }

            // 3. 核心：使用StackExchange.Redis原生解析方法（完美兼容带参数的连接字符串）
            ConfigurationOptions configOptions = ConfigurationOptions.Parse(connectionString);
            // 覆盖/补充配置（配置文件优先级 > 连接字符串）
            configOptions.DefaultDatabase = defaultDatabase;
            configOptions.ConnectTimeout = connectTimeout;
            configOptions.SyncTimeout = syncTimeout;
            configOptions.AsyncTimeout = connectTimeout; // 异步超时和连接超时保持一致
            configOptions.ConnectRetry = connectRetry;
            configOptions.ReconnectRetryPolicy = new ExponentialRetry(1000);
            configOptions.AbortOnConnectFail = false;
            configOptions.ResolveDns = true;
            configOptions.KeepAlive = 60;

            Console.WriteLine($"【Redis配置】地址: {string.Join(",", configOptions.EndPoints)}, 数据库: {configOptions.DefaultDatabase}");

            // 4. 注册Redis连接池（单例）
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var multiplexer = ConnectionMultiplexer.Connect(configOptions);

                // 验证连接
                try
                {
                    var redisDb = multiplexer.GetDatabase();
                    var pingResult = redisDb.Execute("PING");
                    Console.WriteLine($"【Redis连接成功】PING响应: {pingResult}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"【Redis连接失败】{ex.Message}");
                    throw; // 连接失败时终止启动，避免静默错误
                }

                // 监控连接状态
                multiplexer.ConnectionFailed += (s, e) =>
                    Console.WriteLine($"【Redis连接异常】{e.Exception.Message}");
                multiplexer.ConnectionRestored += (s, e) =>
                    Console.WriteLine("【Redis连接恢复】已重新建立连接");
                multiplexer.ErrorMessage += (s, e) =>
                    Console.WriteLine($"【Redis错误】{e.Message}");

                return multiplexer;
            });

            // 5. 注册IDatabase（作用域）
            services.AddScoped<IDatabase>(sp =>
            {
                var multiplexer = sp.GetRequiredService<IConnectionMultiplexer>();
                return multiplexer.GetDatabase(defaultDatabase);
            });

            return services;
        }

        /// <summary>
        /// 注册分布式Redis缓存（适配你的配置格式）
        /// </summary>
        public static IServiceCollection AddCustomRedisCache(this IServiceCollection services,
            IConfiguration configuration, string configName = "RedisConfig")
        {
            var redisConfig = configuration.GetSection(configName);
            var instanceName = redisConfig["InstanceName"] ?? "Ecommerce:";
            var connectionString = redisConfig["ConnectionString"];

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException($"{configName}:ConnectionString", "Redis 连接字符串未配置");
            }

            services.AddStackExchangeRedisCache(options =>
            {
                options.InstanceName = instanceName;
                // 直接使用配置文件中的原生连接字符串，无需解析
                options.Configuration = connectionString;
                // 补充配置项
                options.ConfigurationOptions = new ConfigurationOptions
                {
                    AbortOnConnectFail = false,
                    ConnectTimeout = int.TryParse(redisConfig["ConnectTimeout"], out int ct) ? ct : 5000,
                    SyncTimeout = int.TryParse(redisConfig["SyncTimeout"], out int st) ? st : 3000,
                    ConnectRetry = 3
                };
            });

            return services;
        }

        /// <summary>
        /// 一键注册Redis缓存服务
        /// </summary>
        public static IServiceCollection AddCustomRedisCacheService(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddCustomRedisClient(configuration);
            // 注意：请确保IRedisCacheService和RedisCacheService已正确定义
            services.AddScoped<IRedisCacheService, RedisCacheService>();
            return services;
        }

        /// <summary>
        /// 获取Redis服务器实例
        /// </summary>
        public static IServer GetRedisServer(this IConnectionMultiplexer multiplexer)
        {
            var endPoints = multiplexer.GetEndPoints();
            return endPoints.Any() ? multiplexer.GetServer(endPoints.First()) : null;
        }
    }
}