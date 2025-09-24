using AppBL.BlModels;
using AppBL.Mappers;
using AppBL.Services.Interfaces;
using AppBL.Utilities;
using AppDAL.Repositories.Interfaces;

using DalProduct = AppDAL.DalModels.Product;

namespace AppBL.Services;

public class ProductService(
    IRepository<DalProduct> productRepository,
    ICacheService<Product> cacheService) : IProductService
{
    public async Task<PagedResult<Product>> GetAllProductsAsync(int? page = null, int? pageSize = null, CancellationToken cancellationToken = default)
    {
        if (page.HasValue && pageSize.HasValue)
        {
            var cacheKey = CacheKeyUtil.Page<Product>(page.Value, pageSize.Value);
            return await cacheService.GetAllAsync(async () =>
            {
                var dalPagedResult = await productRepository.GetAllAsync(page, pageSize, cancellationToken);
                return dalPagedResult.MapToBlPagedResult();
            }, cacheKey, CacheKeyUtil.GroupPage<Product>(), cancellationToken);
        }
        else
        {
            var cacheKey = CacheKeyUtil.All<Product>();
            return await cacheService.GetAllAsync(async () =>
            {
                var dalPagedResult = await productRepository.GetAllAsync(null, null, cancellationToken);
                return dalPagedResult.MapToBlPagedResult();
            }, cacheKey, CacheKeyUtil.GroupAll<Product>(), cancellationToken);
        }
    }

    public async Task<Product?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await cacheService.GetByIdAsync(
        id,
        async () =>
        {
            var dalProduct = await productRepository.GetByIdAsync(id, cancellationToken);
            return dalProduct?.MapToBlProduct();
        },
        cancellationToken
    );
    }

    public async Task<Product> AddProductAsync(Product product, CancellationToken cancellationToken = default)
    {
        var dalCreated = await productRepository.AddAsync(product.MapToDalProduct(), cancellationToken);
        await cacheService.InvalidateByPrefixAsync(CacheKeyUtil.Prefix<Product>(), cancellationToken);
        return dalCreated.MapToBlProduct();
    }

    public async Task<Product?> UpdateProductAsync(Guid id, Product product, CancellationToken cancellationToken = default)
    {
        var dalUpdated = await productRepository.UpdateAsync(id, product.MapToDalProduct(), cancellationToken);
        if (dalUpdated != null)
        {
            await cacheService.InvalidateByPrefixAsync(CacheKeyUtil.Prefix<Product>(), cancellationToken);
        }
        return dalUpdated?.MapToBlProduct();
    }

    public async Task<bool> DeleteProductAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var deleted = await productRepository.DeleteAsync(id, cancellationToken);
        if (deleted)
        {
            await cacheService.InvalidateByPrefixAsync(CacheKeyUtil.Prefix<Product>(), cancellationToken);
        }
        return deleted;
    }
}
