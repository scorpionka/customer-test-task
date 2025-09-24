using AppBL.Services;
using AppDAL.Repositories;
using AppDAL.Repositories.Interfaces;
using FluentAssertions;
using Tests.Infrastructure;
using Xunit;
using BlProduct = AppBL.BlModels.Product;
using DalProduct = AppDAL.DalModels.Product;

namespace Tests.UnitTests;

public class ProductServicePerformanceTests
{
    private readonly IRepository<DalProduct> _repo = new ProductRepository();

    [Fact]
    public async Task GetAllProducts_ConcurrentCalls_DoNotCauseRaceConditions()
    {
        var cache = new FakeCacheService<BlProduct>();
        var service = new ProductService(_repo, cache);

        var tasks = Enumerable.Range(0, 10)
            .Select(_ => service.GetAllProductsAsync())
            .ToArray();

        var results = await Task.WhenAll(tasks);

        results.Should().AllSatisfy(result =>
        {
            result.Should().NotBeNull();
            result.Items.Should().NotBeEmpty();
        });

        var firstResult = results[0];
        results.Should().AllSatisfy(result =>
        {
            result.TotalCount.Should().Be(firstResult.TotalCount);
        });
    }

    [Fact]
    public async Task GetProductById_ConcurrentCallsForSameId_DoNotCauseRaceConditions()
    {
        var cache = new FakeCacheService<BlProduct>();
        var service = new ProductService(_repo, cache);

        var products = await service.GetAllProductsAsync();
        var productId = products.Items.First().Id;

        var tasks = Enumerable.Range(0, 10)
            .Select(_ => service.GetProductByIdAsync(productId))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        results.Should().AllSatisfy(result =>
        {
            result.Should().NotBeNull();
            result!.Id.Should().Be(productId);
        });
    }

    [Fact]
    public async Task AddProduct_ConcurrentCalls_AllSucceed()
    {
        var cache = new FakeCacheService<BlProduct>();
        var service = new ProductService(_repo, cache);

        var initialCount = (await service.GetAllProductsAsync()).TotalCount;

        var tasks = Enumerable.Range(0, 5)
            .Select(i => service.AddProductAsync(new BlProduct
            {
                Name = $"Concurrent Product {i}",
                Description = $"Description {i}",
                Price = 100m + i,
                Category = $"Category {i}"
            }))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        results.Should().AllSatisfy(result =>
        {
            result.Should().NotBeNull();
            result.Id.Should().NotBeEmpty();
        });

        var finalCount = (await service.GetAllProductsAsync()).TotalCount;
        finalCount.Should().Be(initialCount + 5);
    }

    [Fact]
    public async Task MixedOperations_ConcurrentExecution_MaintainsDataIntegrity()
    {
        var cache = new FakeCacheService<BlProduct>();
        var service = new ProductService(_repo, cache);

        var readTasks = Enumerable.Range(0, 5)
            .Select(_ => service.GetAllProductsAsync());

        var writeTasks = Enumerable.Range(0, 3)
            .Select(i => service.AddProductAsync(new BlProduct
            {
                Name = $"Mixed Operation Product {i}",
                Description = $"Description {i}",
                Price = 150m + i,
                Category = $"Mixed {i}"
            }));

        var allTasks = readTasks.Cast<Task>().Concat(writeTasks.Cast<Task>()).ToArray();

        await Task.WhenAll(allTasks);

        var finalProducts = await service.GetAllProductsAsync();
        finalProducts.Should().NotBeNull();
        finalProducts.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CacheInvalidation_DuringConcurrentReads_DoesNotCauseErrors()
    {
        var cache = new FakeCacheService<BlProduct>();
        var service = new ProductService(_repo, cache);

        await service.GetAllProductsAsync();

        var readTasks = Enumerable.Range(0, 8)
            .Select(_ => service.GetAllProductsAsync());

        var writeTask = service.AddProductAsync(new BlProduct
        {
            Name = "Cache Invalidation Test",
            Description = "This will invalidate cache",
            Price = 200m,
            Category = "Cache Test"
        });

        var allTasks = readTasks.Cast<Task>().Append(writeTask).ToArray();

        await Task.WhenAll(allTasks);

        var finalProducts = await service.GetAllProductsAsync();
        finalProducts.Should().NotBeNull();
        finalProducts.Items.Should().Contain(p => p.Name == "Cache Invalidation Test");
    }

    [Fact]
    public async Task GetProducts_WithDifferentPagingParameters_ConcurrentlyExecuted()
    {
        var cache = new FakeCacheService<BlProduct>();
        var service = new ProductService(_repo, cache);

        var pagingTasks = new[]
        {
            service.GetAllProductsAsync(1, 5),
            service.GetAllProductsAsync(2, 5),
            service.GetAllProductsAsync(1, 10),
            service.GetAllProductsAsync(3, 3),
            service.GetAllProductsAsync()
        };

        var results = await Task.WhenAll(pagingTasks);

        results.Should().AllSatisfy(result =>
        {
            result.Should().NotBeNull();
            result.TotalCount.Should().BeGreaterThan(0);
        });

        results[0].PageSize.Should().Be(5);
        results[1].PageSize.Should().Be(5);
        results[2].PageSize.Should().Be(10);
        results[3].PageSize.Should().Be(3);
        results[4].PageSize.Should().Be(results[4].TotalCount);
    }
}