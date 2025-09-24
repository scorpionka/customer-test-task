using ApiPagedResult = WebApiTestApp.ApiModels.PagedResult<WebApiTestApp.ApiModels.Product>;
using ApiProduct = WebApiTestApp.ApiModels.Product;
using BlPagedResult = AppBL.BlModels.PagedResult<AppBL.BlModels.Product>;
using BlProduct = AppBL.BlModels.Product;

namespace WebApiTestApp.Mappers;

public static class ProductMapper
{
    public static ApiProduct MapToApiProduct(this BlProduct blProduct)
        => new()
        {
            Id = blProduct.Id,
            Name = blProduct.Name,
            Description = blProduct.Description,
            Price = blProduct.Price,
            Category = blProduct.Category
        };

    public static BlProduct MapToBlProduct(this ApiProduct apiProduct)
    => new()
    {
        Id = apiProduct.Id,
        Name = apiProduct.Name,
        Description = apiProduct.Description,
        Price = apiProduct.Price,
        Category = apiProduct.Category
    };

    public static ApiPagedResult MapToApiPagedResult(this BlPagedResult blPaged)
        => new()
        {
            Items = blPaged.Items.Select(x => x.MapToApiProduct()),
            TotalCount = blPaged.TotalCount,
            Page = blPaged.Page,
            PageSize = blPaged.PageSize
        };
}
