using BlPagedResult = AppBL.BlModels.PagedResult<AppBL.BlModels.Product>;
using BlProduct = AppBL.BlModels.Product;
using DalPagedResult = AppDAL.DalModels.PagedResult<AppDAL.DalModels.Product>;
using DalProduct = AppDAL.DalModels.Product;

namespace AppBL.Mappers;

public static class ProductMapper
{
    public static BlProduct MapToBlProduct(this DalProduct dalProduct)
    => new()
    {
        Id = dalProduct.Id,
        Name = dalProduct.Name,
        Description = dalProduct.Description,
        Price = dalProduct.Price,
        Category = dalProduct.Category
    };

    public static DalProduct MapToDalProduct(this BlProduct blProduct)
    => new()
    {
        Id = blProduct.Id,
        Name = blProduct.Name,
        Description = blProduct.Description,
        Price = blProduct.Price,
        Category = blProduct.Category
    };

    public static BlPagedResult MapToBlPagedResult(this DalPagedResult dal)
    {
        return new BlPagedResult
        {
            Items = dal.Items.Select(x => x.MapToBlProduct()),
            TotalCount = dal.TotalCount,
            Page = dal.Page,
            PageSize = dal.PageSize
        };
    }
}
