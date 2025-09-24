namespace AppDAL.DalModels;

public sealed record PagedResult<TEntity>
{
    public IEnumerable<TEntity> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
