using AppDAL.DalModels;

namespace AppDAL.Repositories.Interfaces;

public interface IRepository<TEntity>
    where TEntity : class
{
    Task<PagedResult<TEntity>> GetAllAsync(int? page = null, int? pageSize = null, CancellationToken cancellationToken = default);
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<TEntity?> UpdateAsync(Guid id, TEntity updated, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}