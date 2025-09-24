using AppBL.BlModels;
using AppBL.Mappers;
using AppBL.Services.Interfaces;
using AppDAL.Repositories.Interfaces;

using DalProduct = AppDAL.DalModels.Product;

namespace AppBL.Services;

public class ProductService(
    IRepository<DalProduct> productRepository,
    ICacheService<Product> cacheService) : IProductService
{
    public async Task<PagedResult<Product>> GetAllProductsAsync(int? page = null, int? pageSize = null)
    {
        var cacheKey = page.HasValue && pageSize.HasValue
            ? $"products_page_{page}_size_{pageSize}"
            : "products_all";

        return await cacheService.GetAllAsync(
            async () =>
                {
                    var dalPagedResult = await productRepository.GetAllAsync(page, pageSize);
                    return dalPagedResult.MapToBlPagedResult();
                },
                cacheKey
        );
    }

    public async Task<Product?> GetProductByIdAsync(Guid id)
    {
        return await cacheService.GetByIdAsync(
        id,
        async () =>
        {
            var dalProduct = await productRepository.GetByIdAsync(id);
            return dalProduct?.MapToBlProduct();
        }
    );
    }

    public async Task<Product> AddProductAsync(Product product)
    {
        var dalCreated = await productRepository.AddAsync(product.MapToDalProduct());
        await cacheService.InvalidateByPrefixAsync("products");
        return dalCreated.MapToBlProduct();
    }

    public async Task<Product?> UpdateProductAsync(Guid id, Product product)
    {
        var dalUpdated = await productRepository.UpdateAsync(id, product.MapToDalProduct());
        if (dalUpdated != null)
        {
            await cacheService.InvalidateByPrefixAsync("products");
        }
        return dalUpdated?.MapToBlProduct();
    }

    public async Task<bool> DeleteProductAsync(Guid id)
    {
        var deleted = await productRepository.DeleteAsync(id);
        if (deleted)
        {
            await cacheService.InvalidateByPrefixAsync("products");
        }
        return deleted;
    }
}
