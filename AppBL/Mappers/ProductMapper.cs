using BlPagedResult = AppBL.BlModels.PagedResult<AppBL.BlModels.Product>;
using BlProduct = AppBL.BlModels.Product;
using DalPagedResult = AppDAL.DalModels.PagedResult<AppDAL.DalModels.Product>;
using DalProduct = AppDAL.DalModels.Product;
using Mapster;

namespace AppBL.Mappers;

public static class ProductMapper
{
    public static BlProduct MapToBlProduct(this DalProduct dalProduct)
        => dalProduct.Adapt<BlProduct>();

    public static DalProduct MapToDalProduct(this BlProduct blProduct)
        => blProduct.Adapt<DalProduct>();

    public static BlPagedResult MapToBlPagedResult(this DalPagedResult dal)
    {
        var result = new BlPagedResult
        {
            Items = [.. dal.Items.Adapt<IEnumerable<BlProduct>>()],
            TotalCount = dal.TotalCount,
            Page = dal.Page,
            PageSize = dal.PageSize
        };
        return result;
    }
}
