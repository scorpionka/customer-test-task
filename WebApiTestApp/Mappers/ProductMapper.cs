using Mapster;
using ApiPagedResult = WebApiTestApp.ApiModels.PagedResult<WebApiTestApp.ApiModels.Product>;
using ApiProduct = WebApiTestApp.ApiModels.Product;
using BlPagedResult = AppBL.BlModels.PagedResult<AppBL.BlModels.Product>;
using BlProduct = AppBL.BlModels.Product;

namespace WebApiTestApp.Mappers;

public static class ProductMapper
{
    public static ApiProduct MapToApiProduct(this BlProduct blProduct)
        => blProduct.Adapt<ApiProduct>();

    public static BlProduct MapToBlProduct(this ApiProduct apiProduct)
        => apiProduct.Adapt<BlProduct>();

    public static ApiPagedResult MapToApiPagedResult(this BlPagedResult blPaged)
        => new()
        {
            Items = [.. blPaged.Items.Adapt<IEnumerable<ApiProduct>>()],
            TotalCount = blPaged.TotalCount,
            Page = blPaged.Page,
            PageSize = blPaged.PageSize
        };
}
