using AppBL.BlModels;

namespace AppBL.Services.Interfaces;

public interface ICacheService<TEntity>
{
    Task<PagedResult<TEntity>> GetAllAsync(Func<Task<PagedResult<TEntity>>> valueFactory, string cacheKey);
    Task<TEntity?> GetByIdAsync(Guid id, Func<Task<TEntity?>> valueFactory);
    Task InvalidateAsync(params string[] cacheKeys);
    Task InvalidateByPrefixAsync(string cacheKeyPrefix);
}
