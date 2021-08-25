using DN.WebApi.Application.Settings;
using DN.WebApi.Domain.Constants;
using DN.WebApi.Infrastructure.Identity.Models;
using DN.WebApi.Infrastructure.Utilties;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace DN.WebApi.Infrastructure.Persistence.Extensions
{
    public static class TenantDatabaseExtensions
    {
        private static readonly ILogger _logger = Log.ForContext(typeof(TenantDatabaseExtensions));
        public static IServiceCollection PrepareTenantDatabases<T>(this IServiceCollection services, IConfiguration config) where T : ApplicationDbContext
        {
            services.Configure<TenantSettings>(config.GetSection(nameof(TenantSettings)));
            var options = services.GetOptions<TenantSettings>(nameof(TenantSettings));
            var defaultConnectionString = options.Defaults?.ConnectionString;
            var defaultDbProvider = options.Defaults?.DBProvider;
            var tenants = options.Tenants;
            foreach (var tenant in tenants)
            {
                string connectionString;
                if (string.IsNullOrEmpty(tenant.ConnectionString))
                {
                    connectionString = defaultConnectionString;
                }
                else
                {
                    connectionString = tenant.ConnectionString;
                }
                if (defaultDbProvider.ToLower() == "postgresql")
                {
                    services.AddDbContext<T>(m => m.UseNpgsql(e => e.MigrationsAssembly(typeof(T).Assembly.FullName)));
                    services.MigrateAndSeedIdentityData<T>(connectionString, tenant.TID, options);
                    services.AddHangfire(x => x.UsePostgreSqlStorage(defaultConnectionString));
                }
            }
            return services;
        }
        private static IServiceCollection MigrateAndSeedIdentityData<T>(this IServiceCollection services, string connectionString, string tenantId, TenantSettings options) where T : ApplicationDbContext
        {
            var tenant = options.Tenants.Where(a => a.TID == tenantId).FirstOrDefault();
            _logger.Information($"{tenant.Name} : Initializing Database....");
            using var scope = services.BuildServiceProvider().CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<T>();
            dbContext.Database.SetConnectionString(connectionString);
            dbContext.Database.Migrate();
             _logger.Information($"{tenant.Name} : Migrations complete....");
            SeedRoles(tenantId, tenant, dbContext);
            SeedTenantAdmins(tenantId, tenant, scope, dbContext);
            return services;
        }
        #region Seeding
        private static void SeedTenantAdmins<T>(string tenantId, Tenant tenant, IServiceScope scope, T dbContext) where T : ApplicationDbContext
        {
            var adminUserName = $"{tenant.Name}.admin";
            var superUser = new ExtendedUser
            {
                FirstName = tenant.Name,
                LastName = "admin",
                Email = tenant.AdminEmail,
                UserName = adminUserName,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                NormalizedEmail = tenant.AdminEmail.ToUpper(),
                NormalizedUserName = adminUserName.ToUpper(),
                IsActive = true,
                TenantId = tenantId
            };
            if (!dbContext.Users.IgnoreQueryFilters().Any(u => u.Email == tenant.AdminEmail))
            {
                var password = new PasswordHasher<ExtendedUser>();
                var hashed = password.HashPassword(superUser, UserConstants.DefaultPassword);
                superUser.PasswordHash = hashed;
                var userStore = new UserStore<ExtendedUser>(dbContext);
                userStore.CreateAsync(superUser).Wait();
                _logger.Information($"{tenant.Name} : Seeding Admin User {tenant.AdminEmail}....");
                AssignRoles(scope.ServiceProvider, superUser.Email, RoleConstants.Admin).Wait();
            }
        }

        private static void SeedRoles<T>(string tenantId, Tenant tenant, T dbContext) where T : ApplicationDbContext
        {
            foreach (string roleName in typeof(RoleConstants).GetAllPublicConstantValues<string>())
            {
                var roleStore = new RoleStore<ExtendedRole>(dbContext);
                if (!dbContext.Roles.IgnoreQueryFilters().Any(r => r.Name == roleName))
                {
                    var role = new ExtendedRole(roleName, tenantId, $"Admin Role for {tenant.Name} Tenant");
                    roleStore.CreateAsync(role).Wait();
                    _logger.Information($"{tenant.Name} : Seeding Admin Role....");
                }
            }
        }

        public static async Task<IdentityResult> AssignRoles(IServiceProvider services, string email, string role)
        {
            UserManager<ExtendedUser> _userManager = services.GetService<UserManager<ExtendedUser>>();
            var user = await _userManager.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Email.Equals(email));
            var result = await _userManager.AddToRoleAsync(user, role);
            return result;
        }
        #endregion
        public static T GetOptions<T>(this IServiceCollection services, string sectionName) where T : new()
        {
            using var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var section = configuration.GetSection(sectionName);
            var options = new T();
            section.Bind(options);

            return options;
        }
    }
}