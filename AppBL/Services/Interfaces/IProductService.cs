using AppBL.BlModels;

namespace AppBL.Services.Interfaces;

public interface IProductService
{
    Task<PagedResult<Product>> GetAllProductsAsync(int? page = null, int? pageSize = null, CancellationToken cancellationToken = default);
    Task<Product?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Product> AddProductAsync(Product product, CancellationToken cancellationToken = default);
    Task<Product?> UpdateProductAsync(Guid id, Product product, CancellationToken cancellationToken = default);
    Task<bool> DeleteProductAsync(Guid id, CancellationToken cancellationToken = default);
}
