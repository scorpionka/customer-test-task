using AppBL.BlModels;

namespace AppBL.Services.Interfaces;

public interface ICacheService<TEntity>
{
    Task<PagedResult<TEntity>> GetAllAsync(Func<Task<PagedResult<TEntity>>> valueFactory, string cacheKey, string group, CancellationToken cancellationToken = default);
    Task<TEntity?> GetByIdAsync(Guid id, Func<Task<TEntity?>> valueFactory, CancellationToken cancellationToken = default);
    Task InvalidateAsync(CancellationToken cancellationToken = default, params string[] cacheKeys);
    Task InvalidateByPrefixAsync(string cacheKeyPrefix, CancellationToken cancellationToken = default);
}
