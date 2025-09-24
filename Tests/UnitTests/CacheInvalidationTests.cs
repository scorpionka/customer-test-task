using AppBL.Services;
using AppDAL.Repositories;
using AppDAL.Repositories.Interfaces;
using FluentAssertions;
using Tests.Infrastructure;
using Xunit;
using BlProduct = AppBL.BlModels.Product;
using DalProduct = AppDAL.DalModels.Product;

namespace Tests.UnitTests;

public class CacheInvalidationTests
{
    private readonly IRepository<DalProduct> _repo = new ProductRepository();

    [Fact]
    public async Task AddProduct_InvalidatesAllCaches()
    {
        var cache = new FakeCacheService<BlProduct>();
        var service = new ProductService(_repo, cache);

        await service.GetAllProductsAsync();
        await service.GetAllProductsAsync(1, 5);
        cache.HasKey("product:list:all").Should().BeTrue();
        cache.HasKey("product:page:1:5").Should().BeTrue();

        await service.AddProductAsync(new BlProduct
        {
            Name = "New Product",
            Category = "Test",
            Price = 100m,
            Description = "Test product"
        });

        cache.HasKey("product:list:all").Should().BeFalse();
        cache.HasKey("product:page:1:5").Should().BeFalse();
    }

    [Fact]
    public async Task UpdateProduct_InvalidatesAllCaches()
    {
        var cache = new FakeCacheService<BlProduct>();
        var service = new ProductService(_repo, cache);

        var products = await service.GetAllProductsAsync();
        var firstProduct = products.Items.First();
        await service.GetProductByIdAsync(firstProduct.Id);

        cache.HasKey("product:list:all").Should().BeTrue();
        cache.HasKey($"product:id:{firstProduct.Id}").Should().BeTrue();

        await service.UpdateProductAsync(firstProduct.Id, firstProduct with { Name = "Updated" });

        cache.HasKey("product:list:all").Should().BeFalse();
        cache.HasKey($"product:id:{firstProduct.Id}").Should().BeFalse();
    }

    [Fact]
    public async Task DeleteProduct_InvalidatesAllCaches()
    {
        var cache = new FakeCacheService<BlProduct>();
        var service = new ProductService(_repo, cache);

        var newProduct = await service.AddProductAsync(new BlProduct
        {
            Name = "Product To Delete",
            Category = "Test",
            Price = 50m,
            Description = "Will be deleted"
        });

        await service.GetAllProductsAsync();
        await service.GetProductByIdAsync(newProduct.Id);

        cache.HasKey("product:list:all").Should().BeTrue();
        cache.HasKey($"product:id:{newProduct.Id}").Should().BeTrue();

        await service.DeleteProductAsync(newProduct.Id);

        cache.HasKey("product:list:all").Should().BeFalse();
        cache.HasKey($"product:id:{newProduct.Id}").Should().BeFalse();
    }

    [Fact]
    public async Task UpdateNonExistentProduct_DoesNotInvalidateCache()
    {
        var cache = new FakeCacheService<BlProduct>();
        var service = new ProductService(_repo, cache);

        await service.GetAllProductsAsync();
        cache.HasKey("product:list:all").Should().BeTrue();

        var result = await service.UpdateProductAsync(Guid.NewGuid(), new BlProduct
        {
            Name = "Non-existent",
            Category = "Test",
            Price = 100m,
            Description = "Does not exist"
        });

        result.Should().BeNull();
        cache.HasKey("product:list:all").Should().BeTrue();
    }

    [Fact]
    public async Task DeleteNonExistentProduct_DoesNotInvalidateCache()
    {
        var cache = new FakeCacheService<BlProduct>();
        var service = new ProductService(_repo, cache);

        await service.GetAllProductsAsync();
        cache.HasKey("product:list:all").Should().BeTrue();

        var result = await service.DeleteProductAsync(Guid.NewGuid());

        result.Should().BeFalse();
        cache.HasKey("product:list:all").Should().BeTrue();
    }

    [Fact]
    public async Task GetProductById_UsesCacheAfterFirstCall()
    {
        var cache = new FakeCacheService<BlProduct>();
        var service = new ProductService(_repo, cache);

        var products = await service.GetAllProductsAsync();
        var productId = products.Items.First().Id;

        var firstCall = await service.GetProductByIdAsync(productId);
        cache.HasKey($"product:id:{productId}").Should().BeTrue();

        var secondCall = await service.GetProductByIdAsync(productId);

        firstCall.Should().NotBeNull();
        secondCall.Should().NotBeNull();
        firstCall!.Id.Should().Be(secondCall!.Id);
        cache.GetCallCount($"product:id:{productId}").Should().Be(1);
    }

    [Fact]
    public async Task GetAllProducts_UsesCacheAfterFirstCall()
    {
        var cache = new FakeCacheService<BlProduct>();
        var service = new ProductService(_repo, cache);

        var firstCall = await service.GetAllProductsAsync();
        cache.HasKey("product:list:all").Should().BeTrue();

        var secondCall = await service.GetAllProductsAsync();

        firstCall.TotalCount.Should().Be(secondCall.TotalCount);
        cache.GetCallCount("product:list:all").Should().Be(1);
    }

    [Fact]
    public async Task GetAllProductsWithPaging_UsesSeparateCache()
    {
        var cache = new FakeCacheService<BlProduct>();
        var service = new ProductService(_repo, cache);

        await service.GetAllProductsAsync();
        await service.GetAllProductsAsync(1, 5);
        await service.GetAllProductsAsync(2, 5);

        cache.HasKey("product:list:all").Should().BeTrue();
        cache.HasKey("product:page:1:5").Should().BeTrue();
        cache.HasKey("product:page:2:5").Should().BeTrue();
    }
}