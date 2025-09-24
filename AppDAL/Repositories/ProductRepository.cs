using AppDAL.DalModels;
using AppDAL.Repositories.Interfaces;
using System.Collections.Immutable;

namespace AppDAL.Repositories;

public class ProductRepository() : IRepository<Product>
{
    private ImmutableList<Product> _products = [.. GetDefaultSeedData()];
    private readonly Lock _lock = new();

    public async Task<Product> AddAsync(Product product)
    {
        product.Id = Guid.NewGuid();
        lock (_lock)
        {
            _products = _products.Add(product);
        }

        return await Task.FromResult(product);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        bool removed = false;
        lock (_lock)
        {
            var existingProduct = _products.FirstOrDefault(p => p.Id == id);
            if (existingProduct != null)
            {
                _products = _products.Remove(existingProduct);
                removed = true;
            }
        }

        return await Task.FromResult(removed);
    }

    public async Task<PagedResult<Product>> GetAllAsync(int? page = null, int? pageSize = null)
    {
        IEnumerable<Product> products;
        int totalCount;

        lock (_lock)
        {
            products = _products;
            totalCount = _products.Count;
        }

        if (page.HasValue && pageSize.HasValue && page > 0 && pageSize > 0)
        {
            products = products.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value);
        }

        var result = new PagedResult<Product>
        {
            Items = products,
            TotalCount = totalCount,
            Page = page ?? 1,
            PageSize = pageSize ?? totalCount
        };

        return await Task.FromResult(result);
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        return await Task.FromResult(product);
    }

    public async Task<Product?> UpdateAsync(Guid id, Product updatedProduct)
    {
        var existingProduct = _products.FirstOrDefault(p => p.Id == id);
        if (existingProduct == null) return null;

        lock (_lock)
        {
            existingProduct.Name = updatedProduct.Name;
            existingProduct.Description = updatedProduct.Description;
            existingProduct.Price = updatedProduct.Price;
            existingProduct.Category = updatedProduct.Category;
        }

        return await Task.FromResult<Product?>(existingProduct);
    }

    private static IEnumerable<Product> GetDefaultSeedData() =>
    [
        new Product { Name = "Laptop", Description = "15 inch laptop", Price = 1200.50m, Category = "Electronics" },
        new Product { Name = "Headphones", Description = "Noise cancelling", Price = 199.99m, Category = "Electronics" },
        new Product { Name = "Coffee Mug", Description = "Ceramic mug", Price = 9.99m, Category = "Kitchen" },
        new Product { Name = "Smartphone", Description = "6.5 inch OLED display", Price = 799.00m, Category = "Electronics" },
        new Product { Name = "Tablet", Description = "10 inch Android tablet", Price = 299.99m, Category = "Electronics" },
        new Product { Name = "Gaming Mouse", Description = "Wireless RGB gaming mouse", Price = 59.99m, Category = "Electronics" },
        new Product { Name = "Mechanical Keyboard", Description = "Backlit mechanical keyboard", Price = 129.99m, Category = "Electronics" },
        new Product { Name = "Bluetooth Speaker", Description = "Portable waterproof speaker", Price = 49.99m, Category = "Electronics" },
        new Product { Name = "External Hard Drive", Description = "2TB portable storage", Price = 89.99m, Category = "Electronics" },
        new Product { Name = "Smartwatch", Description = "Fitness tracking smartwatch", Price = 149.99m, Category = "Electronics" },
        new Product { Name = "Frying Pan", Description = "Non-stick frying pan", Price = 25.50m, Category = "Kitchen" },
        new Product { Name = "Chef Knife", Description = "Stainless steel kitchen knife", Price = 35.00m, Category = "Kitchen" },
        new Product { Name = "Cutting Board", Description = "Wooden cutting board", Price = 19.99m, Category = "Kitchen" },
        new Product { Name = "Blender", Description = "High-speed kitchen blender", Price = 89.99m, Category = "Kitchen" },
        new Product { Name = "Electric Kettle", Description = "1.7L fast boil kettle", Price = 39.99m, Category = "Kitchen" },
        new Product { Name = "Office Chair", Description = "Adjustable ergonomic office chair", Price = 199.99m, Category = "Furniture" },
        new Product { Name = "Desk Lamp", Description = "LED desk lamp with dimmer", Price = 29.99m, Category = "Furniture" },
        new Product { Name = "Bookshelf", Description = "5-tier wooden bookshelf", Price = 120.00m, Category = "Furniture" },
        new Product { Name = "Bean Bag", Description = "Comfortable large bean bag", Price = 89.99m, Category = "Furniture" },
        new Product { Name = "Table", Description = "Wooden dining table", Price = 250.00m, Category = "Furniture" }
    ];
}