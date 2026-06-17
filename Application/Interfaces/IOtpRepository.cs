using OtpSystem.Domain.Entities;
using OtpSystem.Domain.Enums;

namespace OtpSystem.Application.Interfaces;

public interface IOtpRepository
{
    Task AddAsync(OtpRecord record);
    Task<OtpRecord?> GetPendingAsync(Guid tenantId, string phone);
    Task UpdateAsync(OtpRecord record);
}