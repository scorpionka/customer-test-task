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
    public async Task<PagedResult<Product>> GetAllProductsAsync(int? page = null, int? pageSize = null, CancellationToken cancellationToken = default)
    {
        var cacheKey = page.HasValue && pageSize.HasValue
            ? $"products_page_{page}_size_{pageSize}"
            : "products_all";

        return await cacheService.GetAllAsync(
            async () =>
                {
                    var dalPagedResult = await productRepository.GetAllAsync(page, pageSize, cancellationToken);
                    return dalPagedResult.MapToBlPagedResult();
                },
                cacheKey,
                cancellationToken
        );
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
        await cacheService.InvalidateByPrefixAsync("products", cancellationToken);
        return dalCreated.MapToBlProduct();
    }

    public async Task<Product?> UpdateProductAsync(Guid id, Product product, CancellationToken cancellationToken = default)
    {
        var dalUpdated = await productRepository.UpdateAsync(id, product.MapToDalProduct(), cancellationToken);
        if (dalUpdated != null)
        {
            await cacheService.InvalidateByPrefixAsync("products", cancellationToken);
        }
        return dalUpdated?.MapToBlProduct();
    }

    public async Task<bool> DeleteProductAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var deleted = await productRepository.DeleteAsync(id, cancellationToken);
        if (deleted)
        {
            await cacheService.InvalidateByPrefixAsync("products", cancellationToken);
        }
        return deleted;
    }
}
