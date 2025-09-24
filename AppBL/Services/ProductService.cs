using AppBL.BlModels;
using AppBL.Mappers;
using AppBL.Services.Interfaces;
using AppDAL.Repositories.Interfaces;

using DalProduct = AppDAL.DalModels.Product;

namespace AppBL.Services;

public class ProductService(
    IRepository<DalProduct> productRepository,
    ICacheService<Product> productCache) : IProductService
{
    public async Task<PagedResult<Product>> GetAllProductsAsync(int? page = null, int? pageSize = null)
    {
        var key = page.HasValue && pageSize.HasValue
            ? $"products_page_{page}_size_{pageSize}"
            : "products_all";

        return await productCache.GetAllAsync(
            async () =>
                {
                    var dalPaged = await productRepository.GetAllAsync(page, pageSize);
                    return dalPaged.MapToBlPagedResult();
                },
                key
        );
    }

    public async Task<Product?> GetProductByIdAsync(Guid id)
    {
        return await productCache.GetByIdAsync(
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
        var created = await productRepository.AddAsync(product.MapToDalProduct());
        await productCache.InvalidateByPrefixAsync("products");
        return created.MapToBlProduct();
    }

    public async Task<Product?> UpdateProductAsync(Guid id, Product product)
    {
        var updated = await productRepository.UpdateAsync(id, product.MapToDalProduct());
        if (updated != null)
        {
            await productCache.InvalidateByPrefixAsync("products");
        }
        return updated?.MapToBlProduct();
    }

    public async Task<bool> DeleteProductAsync(Guid id)
    {
        var deleted = await productRepository.DeleteAsync(id);
        if (deleted)
        {
            await productCache.InvalidateByPrefixAsync("products");
        }
        return deleted;
    }
}
