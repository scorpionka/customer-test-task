using AppDAL.DalModels;
using AppDAL.Repositories.Interfaces;
using System.Collections.Immutable;

namespace AppDAL.Repositories;

public class ProductRepository() : IRepository<Product>
{
    private ImmutableList<Product> _productList = [.. GetSeedData()];
    private readonly Lock _syncRoot = new();

    public async Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        var newProduct = product with { Id = Guid.NewGuid() };
        lock (_syncRoot)
        {
            _productList = _productList.Add(newProduct);
        }

        return await Task.FromResult(newProduct);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        bool isRemoved = false;
        lock (_syncRoot)
        {
            var existingProduct = _productList.FirstOrDefault(p => p.Id == id);
            if (existingProduct != null)
            {
                _productList = _productList.Remove(existingProduct);
                isRemoved = true;
            }
        }

        return await Task.FromResult(isRemoved);
    }

    public async Task<PagedResult<Product>> GetAllAsync(int? page = null, int? pageSize = null, CancellationToken cancellationToken = default)
    {
        IEnumerable<Product> items;
        int totalCount;

        lock (_syncRoot)
        {
            items = _productList;
            totalCount = _productList.Count;
        }

        if (page.HasValue && pageSize.HasValue && page > 0 && pageSize > 0)
        {
            items = items.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value);
        }

        var pagedResult = new PagedResult<Product>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page ?? 1,
            PageSize = pageSize ?? totalCount
        };

        return await Task.FromResult(pagedResult);
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = _productList.FirstOrDefault(p => p.Id == id);
        return await Task.FromResult(product);
    }

    public async Task<Product?> UpdateAsync(Guid id, Product updated, CancellationToken cancellationToken = default)
    {
        Product? newProduct = null;
        lock (_syncRoot)
        {
            var index = _productList.FindIndex(p => p.Id == id);
            if (index >= 0)
            {
                newProduct = updated with { Id = _productList[index].Id };
                _productList = _productList.SetItem(index, newProduct);
            }
        }
        return await Task.FromResult(newProduct);
    }

    private static IEnumerable<Product> GetSeedData() =>
    [
        new Product { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Laptop", Description = "15 inch laptop", Price = 1200.50m, Category = "Electronics" },
        new Product { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Headphones", Description = "Noise cancelling", Price = 199.99m, Category = "Electronics" },
        new Product { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Coffee Mug", Description = "Ceramic mug", Price = 9.99m, Category = "Kitchen" },
        new Product { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Name = "Smartphone", Description = "6.5 inch OLED display", Price = 799.00m, Category = "Electronics" },
        new Product { Id = Guid.Parse("55555555-5555-5555-5555-555555555555"), Name = "Tablet", Description = "10 inch Android tablet", Price = 299.99m, Category = "Electronics" },
        new Product { Id = Guid.Parse("66666666-6666-6666-6666-666666666666"), Name = "Gaming Mouse", Description = "Wireless RGB gaming mouse", Price = 59.99m, Category = "Electronics" },
        new Product { Id = Guid.Parse("77777777-7777-7777-7777-777777777777"), Name = "Mechanical Keyboard", Description = "Backlit mechanical keyboard", Price = 129.99m, Category = "Electronics" },
        new Product { Id = Guid.Parse("88888888-8888-8888-8888-888888888888"), Name = "Bluetooth Speaker", Description = "Portable waterproof speaker", Price = 49.99m, Category = "Electronics" },
        new Product { Id = Guid.Parse("99999999-9999-9999-9999-999999999999"), Name = "External Hard Drive", Description = "2TB portable storage", Price = 89.99m, Category = "Electronics" },
        new Product { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Name = "Smartwatch", Description = "Fitness tracking smartwatch", Price = 149.99m, Category = "Electronics" },
        new Product { Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Name = "Frying Pan", Description = "Non-stick frying pan", Price = 25.50m, Category = "Kitchen" },
        new Product { Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), Name = "Chef Knife", Description = "Stainless steel kitchen knife", Price = 35.00m, Category = "Kitchen" },
        new Product { Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"), Name = "Cutting Board", Description = "Wooden cutting board", Price = 19.99m, Category = "Kitchen" },
        new Product { Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), Name = "Blender", Description = "High-speed kitchen blender", Price = 89.99m, Category = "Kitchen" },
        new Product { Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"), Name = "Electric Kettle", Description = "1.7L fast boil kettle", Price = 39.99m, Category = "Kitchen" },
        new Product { Id = Guid.Parse("10101010-1010-1010-1010-101010101010"), Name = "Office Chair", Description = "Adjustable ergonomic office chair", Price = 199.99m, Category = "Furniture" },
        new Product { Id = Guid.Parse("20202020-2020-2020-2020-202020202020"), Name = "Desk Lamp", Description = "LED desk lamp with dimmer", Price = 29.99m, Category = "Furniture" },
        new Product { Id = Guid.Parse("30303030-3030-3030-3030-303030303030"), Name = "Bookshelf", Description = "5-tier wooden bookshelf", Price = 120.00m, Category = "Furniture" },
        new Product { Id = Guid.Parse("40404040-4040-4040-4040-404040404040"), Name = "Bean Bag", Description = "Comfortable large bean bag", Price = 89.99m, Category = "Furniture" },
        new Product { Id = Guid.Parse("50505050-5050-5050-5050-505050505050"), Name = "Table", Description = "Wooden dining table", Price = 250.00m, Category = "Furniture" }
    ];
}