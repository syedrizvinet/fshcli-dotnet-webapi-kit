using DN.WebApi.Application.Common;
using DN.WebApi.Application.Identity.Users;
using DN.WebApi.Domain.Common.Contracts;
using DN.WebApi.Domain.Multitenancy;
using DN.WebApi.Infrastructure.Auditing;
using DN.WebApi.Infrastructure.Common;
using DN.WebApi.Infrastructure.Identity.Models;
using DN.WebApi.Infrastructure.Multitenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DN.WebApi.Infrastructure.Persistence.Contexts;

public abstract class BaseDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string, IdentityUserClaim<string>, IdentityUserRole<string>, IdentityUserLogin<string>, ApplicationRoleClaim, IdentityUserToken<string>>
{
    private readonly ICurrentTenant _currentTenant;
    protected readonly ICurrentUser _currentUser;
    private readonly ISerializerService _serializer;

    protected BaseDbContext(DbContextOptions options, ICurrentTenant currentTenant, ICurrentUser currentUser, ISerializerService serializer)
        : base(options)
    {
        _currentTenant = currentTenant;
        if (_currentTenant.TryGetKey(out string? key))
        {
            TenantKey = key;
        }

        _currentUser = currentUser;
        _serializer = serializer;
    }

    public DbSet<Trail> AuditTrails => Set<Trail>();
    public string? TenantKey { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        modelBuilder.ApplyIdentityConfiguration();
        modelBuilder.AppendGlobalQueryFilter<IMustHaveTenant>(b => b.Tenant == TenantKey)
                    .AppendGlobalQueryFilter<ISoftDelete>(s => s.DeletedOn == null)
                    .AppendGlobalQueryFilter<IIdentityTenant>(b => b.Tenant == TenantKey);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableSensitiveDataLogging();
        string tenantConnectionString = _currentTenant.ConnectionString;
        switch (_currentTenant.DbProvider.ToLowerInvariant())
        {
            case DbProviderKeys.Npgsql:
                optionsBuilder.UseNpgsql(tenantConnectionString);
                break;
            case DbProviderKeys.SqlServer:
                optionsBuilder.UseSqlServer(tenantConnectionString);
                break;
            case DbProviderKeys.MySql:
                optionsBuilder.UseMySql(tenantConnectionString, ServerVersion.AutoDetect(tenantConnectionString));
                break;
            case DbProviderKeys.Oracle:
                optionsBuilder.UseOracle(tenantConnectionString);
                break;
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        foreach (var entry in ChangeTracker.Entries<IMustHaveTenant>().ToList())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                case EntityState.Modified:
                    if (entry.Entity.Tenant == null)
                    {
                        entry.Entity.Tenant = TenantKey;
                    }

                    break;
            }
        }

        var currentUserId = _currentUser.GetUserId();
        var auditEntries = OnBeforeSaveChanges(currentUserId);
        int result = await base.SaveChangesAsync(cancellationToken);
        await OnAfterSaveChangesAsync(auditEntries, cancellationToken);
        return result;
    }

    private List<AuditTrail> OnBeforeSaveChanges(in Guid userId)
    {
        ChangeTracker.DetectChanges();
        var trailEntries = new List<AuditTrail>();
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>()
            .Where(e => e.State is EntityState.Added or EntityState.Deleted or EntityState.Modified)
            .ToList())
        {
            var trailEntry = new AuditTrail(entry, _serializer)
            {
                TableName = entry.Entity.GetType().Name,
                UserId = userId
            };
            trailEntries.Add(trailEntry);
            foreach (var property in entry.Properties)
            {
                if (property.IsTemporary)
                {
                    trailEntry.TemporaryProperties.Add(property);
                    continue;
                }

                string propertyName = property.Metadata.Name;
                if (property.Metadata.IsPrimaryKey())
                {
                    trailEntry.KeyValues[propertyName] = property.CurrentValue;
                    continue;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        trailEntry.TrailType = TrailType.Create;
                        trailEntry.NewValues[propertyName] = property.CurrentValue;
                        break;

                    case EntityState.Deleted:
                        trailEntry.TrailType = TrailType.Delete;
                        trailEntry.OldValues[propertyName] = property.OriginalValue;
                        break;

                    case EntityState.Modified:
                        if (property.IsModified && entry.Entity is ISoftDelete && property.OriginalValue == null && property.CurrentValue != null)
                        {
                            trailEntry.ChangedColumns.Add(propertyName);
                            trailEntry.TrailType = TrailType.Delete;
                            trailEntry.OldValues[propertyName] = property.OriginalValue;
                            trailEntry.NewValues[propertyName] = property.CurrentValue;
                        }
                        else if (property.IsModified && property.OriginalValue?.Equals(property.CurrentValue) == false)
                        {
                            trailEntry.ChangedColumns.Add(propertyName);
                            trailEntry.TrailType = TrailType.Update;
                            trailEntry.OldValues[propertyName] = property.OriginalValue;
                            trailEntry.NewValues[propertyName] = property.CurrentValue;
                        }

                        break;
                }
            }
        }

        foreach (var auditEntry in trailEntries.Where(_ => !_.HasTemporaryProperties))
        {
            AuditTrails.Add(auditEntry.ToAuditTrail());
        }

        return trailEntries.Where(_ => _.HasTemporaryProperties).ToList();
    }

    private Task OnAfterSaveChangesAsync(List<AuditTrail> trailEntries, in CancellationToken cancellationToken = new())
    {
        if (trailEntries == null || trailEntries.Count == 0)
            return Task.CompletedTask;

        foreach (var entry in trailEntries)
        {
            foreach (var prop in entry.TemporaryProperties)
            {
                if (prop.Metadata.IsPrimaryKey())
                {
                    entry.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
                }
                else
                {
                    entry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                }
            }

            AuditTrails.Add(entry.ToAuditTrail());
        }

        return SaveChangesAsync(cancellationToken);
    }
}