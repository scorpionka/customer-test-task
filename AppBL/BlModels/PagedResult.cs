﻿namespace AppBL.BlModels;

public sealed class PagedResult<TEntity>
{
    public IEnumerable<TEntity> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages =>
        PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 1;
}
