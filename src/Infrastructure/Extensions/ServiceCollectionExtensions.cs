using System.Reflection;
using DN.WebApi.Infrastructure.Identity.Permissions;
using DN.WebApi.Infrastructure.Localizer;
using DN.WebApi.Infrastructure.Persistence;
using DN.WebApi.Infrastructure.Persistence.Extensions;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace DN.WebApi.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddLocalization();
            services.AddServices(config);
            services.AddDistributedMemoryCache();
            services.AddSettings(config);
            services.AddPermissions(config);
            services.AddIdentity(config);
            services.PrepareTenantDatabases<ApplicationDbContext>(config);
            services.AddHangfireServer();
            services.AddRouting(options => options.LowercaseUrls = true);
            services.AddMiddlewares(config);
            services.AddSwaggerDocumentation();
            services.AddCorsPolicy();
            services.AddApiVersioning(config =>
           {
               config.DefaultApiVersion = new ApiVersion(1, 0);
               config.AssumeDefaultVersionWhenUnspecified = true;
               config.ReportApiVersions = true;
           });
            services.AddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>();
            return services;
        }

        public static IServiceCollection AddPermissions(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>()
                .AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
            return services;
        }
    }
}