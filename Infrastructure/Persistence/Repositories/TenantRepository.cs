using Microsoft.EntityFrameworkCore;
using OtpSystem.Application.Interfaces;
using OtpSystem.Domain.Entities;

namespace OtpSystem.Infrastructure.Persistence.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly AppDbContext _db;

    public TenantRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Tenant?> GetByApiKeyAsync(string hashedApiKey)
    {
        return await _db.Tenants
            .FirstOrDefaultAsync(t => t.ApiKey == hashedApiKey && t.IsActive);
    }

    public async Task AddAsync(Tenant tenant)
    {
        await _db.Tenants.AddAsync(tenant);
        await _db.SaveChangesAsync();
    }
}