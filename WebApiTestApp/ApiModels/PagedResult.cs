namespace WebApiTestApp.ApiModels;

public sealed record PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalPages =>
        PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 1;

    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}
