using AppBL.BlModels;
using AppBL.Services.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Text.Json;

namespace AppBL.Services;

public class RedisCacheService<TEntity>(IDistributedCache distributedCache, IConnectionMultiplexer redisConnection) : ICacheService<TEntity>
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly string _registrySetName = $"{typeof(TEntity).Name.ToLowerInvariant()}_cache_keys";

    public async Task<PagedResult<TEntity>> GetAllAsync(Func<Task<PagedResult<TEntity>>> valueFactory, string cacheKey, CancellationToken cancellationToken = default)
    {
        var cachedJson = await distributedCache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedJson))
        {
            var cachedResult = JsonSerializer.Deserialize<PagedResult<TEntity>>(cachedJson, JsonOptions);
            if (cachedResult != null)
            {
                return cachedResult;
            }
        }

        var result = await valueFactory();

        if (result.Items.Any())
        {
            await distributedCache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result, JsonOptions), new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheDuration }, cancellationToken);
            await redisConnection.GetDatabase().SetAddAsync(_registrySetName, cacheKey);
        }

        return result;
    }

    public async Task<TEntity?> GetByIdAsync(Guid id, Func<Task<TEntity?>> valueFactory, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{typeof(TEntity).Name.ToLowerInvariant()}_byid_{id}";
        var cachedJson = await distributedCache.GetStringAsync(cacheKey, cancellationToken);

        if (!string.IsNullOrEmpty(cachedJson))
            return JsonSerializer.Deserialize<TEntity>(cachedJson, JsonOptions);

        var entity = await valueFactory();

        if (entity != null)
        {
            await distributedCache.SetStringAsync(cacheKey, JsonSerializer.Serialize(entity, JsonOptions), new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheDuration }, cancellationToken);
            await redisConnection.GetDatabase().SetAddAsync(_registrySetName, cacheKey);
        }

        return entity;
    }

    public async Task InvalidateAsync(CancellationToken cancellationToken = default, params string[] cacheKeys)
    {
        var db = redisConnection.GetDatabase();
        foreach (var cacheKey in cacheKeys)
        {
            await distributedCache.RemoveAsync(cacheKey, cancellationToken);
            await db.SetRemoveAsync(_registrySetName, cacheKey);
        }
    }

    public async Task InvalidateByPrefixAsync(string cacheKeyPrefix, CancellationToken cancellationToken = default)
    {
        var db = redisConnection.GetDatabase();
        var keys = await db.SetMembersAsync(_registrySetName);

        foreach (var redisValue in keys)
        {
            string? cachedKey = (string?)redisValue;

            if (!string.IsNullOrEmpty(cachedKey) && cachedKey.StartsWith(cacheKeyPrefix, StringComparison.OrdinalIgnoreCase))
            {
                await distributedCache.RemoveAsync(cachedKey, cancellationToken);
                await db.SetRemoveAsync(_registrySetName, cachedKey);
            }
        }
    }
}
