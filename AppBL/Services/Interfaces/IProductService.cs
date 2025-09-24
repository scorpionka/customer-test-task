using AppBL.BlModels;

namespace AppBL.Services.Interfaces;

public interface IProductService
{
    Task<PagedResult<Product>> GetAllProductsAsync(int? page = null, int? pageSize = null);
    Task<Product?> GetProductByIdAsync(Guid id);
    Task<Product> AddProductAsync(Product product);
    Task<Product?> UpdateProductAsync(Guid id, Product updatedProduct);
    Task<bool> DeleteProductAsync(Guid id);
}
