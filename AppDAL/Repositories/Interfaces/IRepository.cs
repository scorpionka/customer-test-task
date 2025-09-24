using AppDAL.DalModels;

namespace AppDAL.Repositories.Interfaces;

public interface IRepository<TEntity>
    where TEntity : class
{
    Task<PagedResult<TEntity>> GetAllAsync(int? page = null, int? pageSize = null);
    Task<TEntity?> GetByIdAsync(Guid id);
    Task<TEntity> AddAsync(TEntity entity);
    Task<TEntity?> UpdateAsync(Guid id, TEntity updated);
    Task<bool> DeleteAsync(Guid id);
}