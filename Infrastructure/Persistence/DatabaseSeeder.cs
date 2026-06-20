using OtpSystem.Domain.Entities;
using OtpSystem.API.Middlewares;

namespace OtpSystem.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext db, ILogger logger)
    {
        if (db.Tenants.Any())
        {
            logger.LogInformation("Database already seeded, skipping");
            return;
        }

        // raw key — only shown once at startup, store it somewhere safe
        var rawApiKey = "test-tenant-api-key-12345";
        var hashedKey = TenantResolutionMiddleware.HashApiKey(rawApiKey);

        var tenant = Tenant.Create(
            name: "Test Tenant",
            hashedApiKey: hashedKey,
            webhookUrl: "https://webhook.site/your-unique-id"  // use webhook.site for testing
        );

        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();

        logger.LogInformation("═══════════════════════════════════════");
        logger.LogInformation("Tenant seeded successfully");
        logger.LogInformation("Tenant ID : {Id}", tenant.Id);
        logger.LogInformation("API Key   : {Key}", rawApiKey);   // raw key shown once
        logger.LogInformation("═══════════════════════════════════════");
    }
}