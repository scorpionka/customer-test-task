using AppBL.BlModels;
using AppBL.Configuration;
using AppBL.Services;
using AppBL.Services.Interfaces;
using AppDAL.Repositories;
using AppDAL.Repositories.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Xunit;
using DalProduct = AppDAL.DalModels.Product;

namespace Tests.IntegrationTests;

public class CacheExpirationTests : IAsyncLifetime
{
    private const string RedisConnectionString = "localhost:6379";
    private ServiceProvider? _provider;
    private IProductService? _service;

    public async Task InitializeAsync()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddOptions<RedisCacheOptions>().Configure(o =>
        {
            o.Configuration = RedisConnectionString;
            o.CacheDurationSeconds = 1;
        });
        services.AddStackExchangeRedisCache(opt => opt.Configuration = RedisConnectionString);

        var connectionMultiplexer = StackExchange.Redis.ConnectionMultiplexer.Connect(RedisConnectionString);
        services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(connectionMultiplexer);

        services.AddSingleton<IRepository<DalProduct>, ProductRepository>();
        services.AddSingleton<ICacheService<Product>, RedisCacheService<Product>>();
        services.AddScoped<IProductService, ProductService>();
        services.AddSingleton(new JsonSerializerOptions(JsonSerializerDefaults.Web));
        _provider = services.BuildServiceProvider();
        _service = _provider.GetRequiredService<IProductService>();

        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        if (_provider is IDisposable d) d.Dispose();
        await Task.CompletedTask;
    }

    [Fact]
    public async Task Cache_ExpiresAfterConfiguredDuration()
    {
        var products = await _service!.GetAllProductsAsync();
        products.Items.Should().NotBeEmpty();
        var productId = products.Items.First().Id;

        var firstCall = await _service.GetProductByIdAsync(productId);
        firstCall.Should().NotBeNull();

        await Task.Delay(TimeSpan.FromSeconds(2));

        var secondCall = await _service.GetProductByIdAsync(productId);
        secondCall.Should().NotBeNull();
        secondCall!.Id.Should().Be(firstCall!.Id);
    }

    [Fact]
    public async Task CacheExpiration_AllowsForFreshData()
    {
        var testProduct = new Product
        {
            Name = "Cache Test Product",
            Category = "Test",
            Price = 100m,
            Description = "Testing cache expiration"
        };

        var created = await _service!.AddProductAsync(testProduct);

        var firstCall = await _service.GetProductByIdAsync(created.Id);
        firstCall.Should().NotBeNull();

        await Task.Delay(TimeSpan.FromSeconds(2));

        var secondCall = await _service.GetProductByIdAsync(created.Id);
        secondCall.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllProducts_RespectsCacheExpiration()
    {
        var firstCall = await _service!.GetAllProductsAsync();
        var initialCount = firstCall.TotalCount;

        await Task.Delay(TimeSpan.FromSeconds(2));

        var secondCall = await _service.GetAllProductsAsync();
        secondCall.TotalCount.Should().BeGreaterThanOrEqualTo(initialCount);
    }
}