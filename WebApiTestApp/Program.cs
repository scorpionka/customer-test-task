using AppBL.BlModels;
using AppBL.Services;
using AppBL.Services.Interfaces;
using AppDAL.Repositories;
using AppDAL.Repositories.Interfaces;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;

using DalProduct = AppDAL.DalModels.Product;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Product Catalog API", Version = "v1" });
});
builder.Services.AddOpenApi();

// Configure Redis cache
var redisConfiguration = builder.Configuration.GetSection("Redis").GetValue<string>("Configuration");
if (string.IsNullOrEmpty(redisConfiguration))
{
    throw new InvalidOperationException("Redis configuration is missing in appsettings.json");
}
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConfiguration;
});
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    return ConnectionMultiplexer.Connect(redisConfiguration);
});

// Register application services
builder.Services.AddSingleton<IRepository<DalProduct>, ProductRepository>();
builder.Services.AddSingleton<ICacheService<Product>, RedisCacheService<Product>>();
builder.Services.AddScoped<IProductService, ProductService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product Catalog API v1");
    });
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
