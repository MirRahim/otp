using Microsoft.EntityFrameworkCore;
using OtpSystem.Application.Interfaces;
using OtpSystem.Domain.Entities;
using OtpSystem.Domain.Enums;

namespace OtpSystem.Infrastructure.Persistence.Repositories;

public class OtpRepository : IOtpRepository
{
    private readonly AppDbContext _db;

    public OtpRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(OtpRecord record)
    {
        await _db.OtpRecords.AddAsync(record);
        await _db.SaveChangesAsync();
    }

    public async Task<OtpRecord?> GetPendingAsync(Guid tenantId, string phone)
    {
        return await _db.OtpRecords
            .Where(o => o.TenantId == tenantId
                     && o.Phone == phone
                     && o.Status == OtpStatus.Pending
                     && o.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(OtpRecord record)
    {
        _db.OtpRecords.Update(record);
        await _db.SaveChangesAsync();
    }
}