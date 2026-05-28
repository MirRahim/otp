using OtpSystem.Application.Interfaces;
using StackExchange.Redis;

namespace OtpSystem.Infrastructure.Cache;

public class RedisCacheService : ICacheService
{
    private readonly IDatabase _db;

    public RedisCacheService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task SetAsync(string key, string value, TimeSpan ttl)
        => await _db.StringSetAsync(key, value, ttl);

    public async Task<string?> GetAsync(string key)
    {
        var value = await _db.StringGetAsync(key);
        return value.IsNullOrEmpty ? null : value.ToString();
    }

    public async Task DeleteAsync(string key)
        => await _db.KeyDeleteAsync(key);

    public async Task<long> IncrementAsync(string key, TimeSpan? expiry = null)
    {
        var count = await _db.StringIncrementAsync(key);
        if (count == 1 && expiry.HasValue)
            await _db.KeyExpireAsync(key, expiry);
        return count;
    }
}