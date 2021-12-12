using System.Reflection;
using DN.WebApi.Application.Common.Interfaces;
using DN.WebApi.Domain.Catalog;
using DN.WebApi.Domain.Multitenancy;
using DN.WebApi.Infrastructure.Persistence.Contexts;
using Microsoft.Extensions.Logging;

namespace DN.WebApi.Infrastructure.Seeding;

public class BrandSeeder : IDatabaseSeeder
{
    private readonly ISerializerService _serializerService;
    private readonly ApplicationDbContext _db;
    private readonly ILogger<BrandSeeder> _logger;

    public BrandSeeder(ISerializerService serializerService, ILogger<BrandSeeder> logger, ApplicationDbContext db)
    {
        _serializerService = serializerService;
        _logger = logger;
        _db = db;
    }

    public void Initialize(Tenant tenant)
    {
        Task.Run(async () =>
        {
            string? path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (!_db.Brands.Any() && !string.IsNullOrEmpty(_db.Tenant))
            {
                _logger.LogInformation("Started to Seed Brands.");

                // Here you can use your own logic to populate the database.
                // As an example, I am using a JSON file to populate the database.
                string brandData = await File.ReadAllTextAsync(path + "/Seeding/brands.json");
                var brands = _serializerService.Deserialize<List<Brand>>(brandData);

                if (brands != null)
                {
                    foreach (var brand in brands)
                    {
                        brand.Tenant = tenant.Key;
                        await _db.Brands.AddAsync(brand);
                    }
                }

                await _db.SaveChangesAsync();
                _logger.LogInformation("Seeded Brands.");
            }
        }).GetAwaiter().GetResult();
    }
}