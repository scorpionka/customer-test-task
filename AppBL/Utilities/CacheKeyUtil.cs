namespace AppBL.Utilities;

public static class CacheKeyUtil
{
    public static string Prefix<TEntity>() => typeof(TEntity).Name.ToLowerInvariant();
    public static string All<TEntity>() => $"{Prefix<TEntity>()}:list:all";
    public static string Page<TEntity>(int page, int pageSize) => $"{Prefix<TEntity>()}:page:{page}:{pageSize}";
    public static string Id<TEntity>(Guid id) => $"{Prefix<TEntity>()}:id:{id}";
    public static string GroupAll<TEntity>() => $"{Prefix<TEntity>()}:list";
    public static string GroupPage<TEntity>() => $"{Prefix<TEntity>()}:page";
    public static string GroupId<TEntity>() => $"{Prefix<TEntity>()}:id";
}
