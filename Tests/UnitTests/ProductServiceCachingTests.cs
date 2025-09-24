using AppBL.Services;
using AppBL.Services.Interfaces;
using AppDAL.Repositories;
using AppDAL.Repositories.Interfaces;
using FluentAssertions;
using Tests.Infrastructure;
using Xunit;
using BlProduct = AppBL.BlModels.Product;
using DalProduct = AppDAL.DalModels.Product;

namespace Tests.UnitTests;

public class ProductServiceCachingTests
{
    private readonly IRepository<DalProduct> _repo = new ProductRepository();

    private ProductService CreateService(ICacheService<BlProduct> cache) => new(_repo, cache);

    [Fact]
    public async Task AddingProductInvalidatesCachedAllList()
    {
        var cache = new FakeCacheService<BlProduct>();
        var service = CreateService(cache);
        var first = await service.GetAllProductsAsync();
        var initialCount = first.TotalCount;
        await service.AddProductAsync(new BlProduct { Name = "New", Category = "C", Price = 1 }, default);
        var after = await service.GetAllProductsAsync();
        after.TotalCount.Should().Be(initialCount + 1);
    }

    [Fact]
    public async Task UpdatingProductInvalidatesCaches()
    {
        var cache = new FakeCacheService<BlProduct>();
        var service = CreateService(cache);
        var list = await service.GetAllProductsAsync();
        var p = list.Items.First();
        var updated = await service.UpdateProductAsync(p.Id, p with { Name = "Changed" });
        updated!.Name.Should().Be("Changed");
    }

    [Fact]
    public async Task DeletingProductInvalidatesCaches()
    {
        var cache = new FakeCacheService<BlProduct>();
        var service = CreateService(cache);
        var list = await service.GetAllProductsAsync();
        var p = list.Items.First();
        var ok = await service.DeleteProductAsync(p.Id);
        ok.Should().BeTrue();
        var after = await service.GetAllProductsAsync();
        after.TotalCount.Should().Be(list.TotalCount - 1);
    }
}
