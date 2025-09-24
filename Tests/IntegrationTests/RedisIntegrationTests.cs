using AppBL.BlModels;
using AppBL.Configuration;
using AppBL.Services;
using AppBL.Services.Interfaces;
using AppDAL.Repositories;
using AppDAL.Repositories.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using DalProduct = AppDAL.DalModels.Product;

namespace Tests.IntegrationTests;

public class RedisIntegrationTests : IAsyncLifetime
{
    private const string RedisConnectionString = "localhost:6379"; // Redis running in WSL
    private ServiceProvider? _provider;
    private IProductService? _service;

    public async Task InitializeAsync()
    {
        var services = new ServiceCollection();

        services.AddLogging();

        services.AddOptions<RedisCacheOptions>().Configure(o =>
        {
            o.Configuration = RedisConnectionString;
            o.CacheDurationSeconds = 300;
        });
        services.AddStackExchangeRedisCache(opt => opt.Configuration = RedisConnectionString);

        var connectionMultiplexer = StackExchange.Redis.ConnectionMultiplexer.Connect(RedisConnectionString);
        services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(connectionMultiplexer);

        services.AddSingleton<IRepository<DalProduct>, ProductRepository>();
        services.AddSingleton<ICacheService<Product>, RedisCacheService<Product>>();
        services.AddScoped<IProductService, ProductService>();
        services.AddSingleton(new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));
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
    public async Task GetProducts_CachesResponse()
    {
        var first = await _service!.GetAllProductsAsync();
        first.Items.Should().NotBeEmpty();
        var second = await _service.GetAllProductsAsync();
        second.Items.Count().Should().Be(first.Items.Count());
    }
}
