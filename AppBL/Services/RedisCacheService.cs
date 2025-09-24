using AppBL.BlModels;
using AppBL.Services.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Text.Json;

namespace AppBL.Services;

public class RedisCacheService<TEntity>(IDistributedCache cache, IConnectionMultiplexer connection) : ICacheService<TEntity>
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private readonly string _registrySetName = $"{typeof(TEntity).Name.ToLowerInvariant()}_cache_keys";

    public async Task<PagedResult<TEntity>> GetAllAsync(Func<Task<PagedResult<TEntity>>> factory, string key)
    {
        var cached = await cache.GetStringAsync(key);
        if (!string.IsNullOrEmpty(cached))
        {
            var deserialized = JsonSerializer.Deserialize<PagedResult<TEntity>>(cached);
            if (deserialized != null)
            {
                return deserialized;
            }
        }

        var data = await factory();

        if (data.Items.Any())
        {
            await cache.SetStringAsync(key, JsonSerializer.Serialize(data),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheDuration });

            await connection.GetDatabase().SetAddAsync(_registrySetName, key);
        }

        return data;
    }

    public async Task<TEntity?> GetByIdAsync(Guid id, Func<Task<TEntity?>> factory)
    {
        var key = $"{typeof(TEntity).Name.ToLowerInvariant()}_byid_{id}";
        var cached = await cache.GetStringAsync(key);

        if (!string.IsNullOrEmpty(cached))
            return JsonSerializer.Deserialize<TEntity>(cached);

        var entity = await factory();

        if (entity != null)
        {
            await cache.SetStringAsync(key, JsonSerializer.Serialize(entity),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheDuration });

            await connection.GetDatabase().SetAddAsync(_registrySetName, key);
        }

        return entity;
    }

    public async Task InvalidateAsync(params string[] keys)
    {
        var db = connection.GetDatabase();
        foreach (var key in keys)
        {
            await cache.RemoveAsync(key);
            await db.SetRemoveAsync(_registrySetName, key);
        }
    }

    public async Task InvalidateByPrefixAsync(string prefix)
    {
        var db = connection.GetDatabase();
        var keys = await db.SetMembersAsync(_registrySetName);

        foreach (var redisValue in keys)
        {
            string? keyStr = (string?)redisValue;

            if (!string.IsNullOrEmpty(keyStr) && keyStr.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                await cache.RemoveAsync(keyStr);
                await db.SetRemoveAsync(_registrySetName, keyStr);
            }
        }
    }
}
