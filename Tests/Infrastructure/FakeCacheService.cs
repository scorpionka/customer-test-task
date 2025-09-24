using AppBL.BlModels;
using AppBL.Services.Interfaces;
using AppBL.Utilities;

namespace Tests.Infrastructure;

public class FakeCacheService<TEntity> : ICacheService<TEntity>
{
    private readonly Dictionary<string, object> _store = [];
    private readonly Dictionary<string, int> _callCounts = [];

    public Task<PagedResult<TEntity>> GetAllAsync(Func<Task<PagedResult<TEntity>>> valueFactory, string cacheKey, string group, CancellationToken cancellationToken = default)
    {
        if (_store.TryGetValue(cacheKey, out var v) && v is PagedResult<TEntity> r)
            return Task.FromResult(r);
        return Add(cacheKey, valueFactory);
    }

    public Task<TEntity?> GetByIdAsync(Guid id, Func<Task<TEntity?>> valueFactory, CancellationToken cancellationToken = default)
    {
        var key = CacheKeyUtil.Id<TEntity>(id);
        if (_store.TryGetValue(key, out var v) && v is TEntity e)
            return Task.FromResult<TEntity?>(e);
        return AddEntity(key, valueFactory);
    }

    public Task InvalidateAsync(CancellationToken cancellationToken = default, params string[] cacheKeys)
    {
        foreach (var k in cacheKeys) _store.Remove(k);
        return Task.CompletedTask;
    }

    public Task InvalidateByPrefixAsync(string cacheKeyPrefix, CancellationToken cancellationToken = default)
    {
        var keys = _store.Keys.Where(k => k.StartsWith(cacheKeyPrefix, StringComparison.OrdinalIgnoreCase)).ToList();
        foreach (var k in keys) _store.Remove(k);
        return Task.CompletedTask;
    }

    public bool HasKey(string key) => _store.ContainsKey(key);

    public int GetCallCount(string key) => _callCounts.GetValueOrDefault(key, 0);

    private async Task<PagedResult<TEntity>> Add(string key, Func<Task<PagedResult<TEntity>>> factory)
    {
        var val = await factory();
        _store[key] = val;
        _callCounts[key] = _callCounts.GetValueOrDefault(key, 0) + 1;
        return val;
    }

    private async Task<TEntity?> AddEntity(string key, Func<Task<TEntity?>> factory)
    {
        var val = await factory();
        if (val != null)
        {
            _store[key] = val;
            _callCounts[key] = _callCounts.GetValueOrDefault(key, 0) + 1;
        }
        return val;
    }
}
