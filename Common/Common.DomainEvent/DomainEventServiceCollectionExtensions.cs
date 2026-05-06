using Common.DamainEvent;
using Common.EF.EF;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Common.DomainEvent
{
    public static class DomainEventServiceCollectionExtensions
    {
        /// <summary>
        /// 添加领域事件相关服务（包含 MediatR 注册）
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="assemblies">需要扫描的程序集（包含MediatR处理器/领域事件）</param>
        /// <returns>服务集合</returns>
        /// <exception cref="ArgumentNullException">参数为空时抛出</exception>
        public static IServiceCollection AddDomainEvents(this IServiceCollection services, params Assembly[] assemblies)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services), "服务集合不能为空");

            if (assemblies == null || assemblies.Length == 0)
                throw new ArgumentNullException(nameof(assemblies), "至少需要指定一个扫描的程序集");

            // 1. 注册 MediatR（核心依赖）
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblies(assemblies);
                // 可选：配置 MediatR 行为（如日志、验证等）
                // cfg.AddBehavior<...>();
            });

            // 2. 注册领域事件调度器（Scoped 生命周期，适配 Web 场景）
            services.TryAddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();

            _ = services;
            return services;
        }

        /// <summary>
        /// 重载：通过类型扫描程序集
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="markerTypes">标记类型（用于获取程序集）</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddDomainEvents(this IServiceCollection services, params Type[] markerTypes)
        {
            if (markerTypes == null || markerTypes.Length == 0)
                throw new ArgumentNullException(nameof(markerTypes), "至少需要指定一个标记类型");

            var assemblies = markerTypes.Select(t => t.Assembly).Distinct().ToArray();
            return services.AddDomainEvents(assemblies);
        }
    }
}
