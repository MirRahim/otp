using OtpSystem.Domain.Entities;

namespace OtpSystem.Application.Interfaces;

public interface ITenantRepository
{
    Task<Tenant?> GetByApiKeyAsync(string hashedApiKey);
    Task AddAsync(Tenant tenant);
}