using AppBL.BlModels;

namespace AppBL.Services.Interfaces;

public interface ICacheService<TEntity>
{
    Task<PagedResult<TEntity>> GetAllAsync(Func<Task<PagedResult<TEntity>>> factory, string key);
    Task<TEntity?> GetByIdAsync(Guid id, Func<Task<TEntity?>> factory);
    Task InvalidateAsync(params string[] keys);
    Task InvalidateByPrefixAsync(string prefix);
}
