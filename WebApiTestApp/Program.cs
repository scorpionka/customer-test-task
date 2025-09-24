using AppBL.BlModels;
using AppBL.Configuration;
using AppBL.Services;
using AppBL.Services.Interfaces;
using AppDAL.Repositories;
using AppDAL.Repositories.Interfaces;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using System.Text.Json;
using WebApiTestApp.Health;

using DalProduct = AppDAL.DalModels.Product;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

builder.Services.AddSingleton(new JsonSerializerOptions(JsonSerializerDefaults.Web));

builder.Services.AddProblemDetails();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Product Catalog API", Version = "v1" });
});
builder.Services.AddOpenApi();

// Configure Redis cache
var redisSection = builder.Configuration.GetSection("Redis");
builder.Services.AddOptions<RedisCacheOptions>()
    .Bind(redisSection)
    .ValidateDataAnnotations()
    .ValidateOnStart();

var redisConfig = redisSection.GetValue<string>(nameof(RedisCacheOptions.Configuration));

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConfig;
});

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configurationOptions = ConfigurationOptions.Parse(redisConfig!);
    configurationOptions.AbortOnConnectFail = false;
    configurationOptions.ConnectRetry = 3;
    configurationOptions.ReconnectRetryPolicy = new ExponentialRetry(5000);
    configurationOptions.KeepAlive = 60;
    return ConnectionMultiplexer.Connect(configurationOptions);
});

// Register application services
builder.Services.AddSingleton<IRepository<DalProduct>, ProductRepository>();
builder.Services.AddSingleton<ICacheService<Product>, RedisCacheService<Product>>();
builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddHealthChecks().AddCheck<RedisHealthCheck>("redis");

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

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
