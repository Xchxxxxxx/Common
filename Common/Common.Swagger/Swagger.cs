using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using System.Reflection;


namespace Common.Swagger
{
    public static class Swagger
    {
        public static IServiceCollection AddCustomSwagger(this IServiceCollection services, string title, string version = "v1")
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(version, new OpenApiInfo { Title = title, Version = version });
               
                // 启用注解
                options.EnableAnnotations();
            });
            return services;
        }

        public static IApplicationBuilder UseCustomSwagger(this IApplicationBuilder app, string version = "v1") 
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint($"/swagger/{version}/swagger.json", version);
                options.RoutePrefix = string.Empty; // 直接根路径访问 Swagger
            });
            return app;
        }
    }
}
