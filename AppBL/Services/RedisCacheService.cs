using AppBL.BlModels;
using AppBL.Configuration;
using AppBL.Services.Interfaces;
using AppBL.Utilities;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;

namespace AppBL.Services;

public class RedisCacheService<TEntity>(
    IDistributedCache distributedCache,
    IConnectionMultiplexer redisConnection,
    IOptions<RedisCacheOptions> options,
    ILogger<RedisCacheService<TEntity>> logger,
    JsonSerializerOptions jsonSerializerOptions) : ICacheService<TEntity>
{
    private readonly TimeSpan _cacheDuration = TimeSpan.FromSeconds(options.Value.CacheDurationSeconds);
    private readonly JsonSerializerOptions _json = jsonSerializerOptions;

    private static string RegistrySet(string group) => $"{typeof(TEntity).Name.ToLowerInvariant()}:{group}:keys";

    public async Task<PagedResult<TEntity>> GetAllAsync(Func<Task<PagedResult<TEntity>>> valueFactory, string cacheKey, string group, CancellationToken cancellationToken = default)
    {
        var cachedJson = await distributedCache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedJson))
        {
            try
            {
                var cachedResult = JsonSerializer.Deserialize<PagedResult<TEntity>>(cachedJson, _json);
                if (cachedResult != null)
                {
                    logger.LogDebug("Cache hit {CacheKey}", cacheKey);
                    return cachedResult;
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to deserialize cache for {CacheKey}", cacheKey);
            }
        }
        logger.LogDebug("Cache miss {CacheKey}", cacheKey);
        var result = await valueFactory();
        if (result.Items.Any())
        {
            await distributedCache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result, _json), new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = _cacheDuration }, cancellationToken);
            await redisConnection.GetDatabase().SetAddAsync(RegistrySet(group), cacheKey);
        }
        return result;
    }

    public async Task<TEntity?> GetByIdAsync(Guid id, Func<Task<TEntity?>> valueFactory, CancellationToken cancellationToken = default)
    {
        var cacheKey = CacheKeyUtil.Id<TEntity>(id);
        var cachedJson = await distributedCache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedJson))
        {
            try
            {
                var entity = JsonSerializer.Deserialize<TEntity>(cachedJson, _json);
                if (entity != null)
                {
                    logger.LogDebug("Cache hit {CacheKey}", cacheKey);
                    return entity;
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to deserialize cache for {CacheKey}", cacheKey);
            }
        }
        logger.LogDebug("Cache miss {CacheKey}", cacheKey);
        var value = await valueFactory();
        if (value != null)
        {
            await distributedCache.SetStringAsync(cacheKey, JsonSerializer.Serialize(value, _json), new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = _cacheDuration }, cancellationToken);
            await redisConnection.GetDatabase().SetAddAsync(RegistrySet(CacheKeyUtil.GroupId<TEntity>()), cacheKey);
        }
        return value;
    }

    public async Task InvalidateAsync(CancellationToken cancellationToken = default, params string[] cacheKeys)
    {
        var db = redisConnection.GetDatabase();
        foreach (var cacheKey in cacheKeys)
        {
            await distributedCache.RemoveAsync(cacheKey, cancellationToken);
            await db.SetRemoveAsync(RegistrySet(CacheKeyUtil.GroupAll<TEntity>()), cacheKey);
            await db.SetRemoveAsync(RegistrySet(CacheKeyUtil.GroupPage<TEntity>()), cacheKey);
            await db.SetRemoveAsync(RegistrySet(CacheKeyUtil.GroupId<TEntity>()), cacheKey);
        }
    }

    public async Task InvalidateByPrefixAsync(string cacheKeyPrefix, CancellationToken cancellationToken = default)
    {
        var db = redisConnection.GetDatabase();
        var groups = new[] { CacheKeyUtil.GroupAll<TEntity>(), CacheKeyUtil.GroupPage<TEntity>(), CacheKeyUtil.GroupId<TEntity>() };
        foreach (var group in groups)
        {
            var members = await db.SetMembersAsync(RegistrySet(group));
            foreach (var m in members)
            {
                var key = (string?)m;
                if (!string.IsNullOrEmpty(key) && key.StartsWith(cacheKeyPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    await distributedCache.RemoveAsync(key, cancellationToken);
                    await db.SetRemoveAsync(RegistrySet(group), key);
                }
            }
        }
    }
}
