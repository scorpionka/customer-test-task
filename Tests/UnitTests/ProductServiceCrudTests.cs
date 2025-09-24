using AppBL.Services;
using AppDAL.Repositories;
using AppDAL.Repositories.Interfaces;
using FluentAssertions;
using Tests.Infrastructure;
using Xunit;
using BlProduct = AppBL.BlModels.Product;
using DalProduct = AppDAL.DalModels.Product;

namespace Tests.UnitTests;

public class ProductServiceCrudTests
{
    private readonly IRepository<DalProduct> _repo = new ProductRepository();
    private readonly FakeCacheService<BlProduct> _cache = new();
    private readonly ProductService _service;

    public ProductServiceCrudTests()
    {
        _service = new ProductService(_repo, _cache);
    }

    [Fact]
    public async Task GetAllProductsAsync_ReturnsAllProducts()
    {
        var result = await _service.GetAllProductsAsync();

        result.Should().NotBeNull();
        result.Items.Should().NotBeEmpty();
        result.TotalCount.Should().BeGreaterThan(0);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(result.TotalCount);
    }

    [Fact]
    public async Task GetAllProductsAsync_WithPaging_ReturnsCorrectPage()
    {
        var result = await _service.GetAllProductsAsync(1, 5);

        result.Should().NotBeNull();
        result.Items.Should().HaveCountLessOrEqualTo(5);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetProductByIdAsync_WithValidId_ReturnsProduct()
    {
        var allProducts = await _service.GetAllProductsAsync();
        var firstProduct = allProducts.Items.First();

        var result = await _service.GetProductByIdAsync(firstProduct.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(firstProduct.Id);
        result.Name.Should().Be(firstProduct.Name);
    }

    [Fact]
    public async Task GetProductByIdAsync_WithInvalidId_ReturnsNull()
    {
        var nonExistentId = Guid.NewGuid();

        var result = await _service.GetProductByIdAsync(nonExistentId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task AddProductAsync_WithValidProduct_AddsAndReturnsProduct()
    {
        var newProduct = new BlProduct
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            Category = "Test Category"
        };

        var result = await _service.AddProductAsync(newProduct);

        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be(newProduct.Name);
        result.Description.Should().Be(newProduct.Description);
        result.Price.Should().Be(newProduct.Price);
        result.Category.Should().Be(newProduct.Category);
    }

    [Fact]
    public async Task UpdateProductAsync_WithValidProduct_UpdatesAndReturnsProduct()
    {
        var allProducts = await _service.GetAllProductsAsync();
        var existingProduct = allProducts.Items.First();
        var updatedProduct = existingProduct with { Name = "Updated Name", Price = 199.99m };

        var result = await _service.UpdateProductAsync(existingProduct.Id, updatedProduct);

        result.Should().NotBeNull();
        result!.Id.Should().Be(existingProduct.Id);
        result.Name.Should().Be("Updated Name");
        result.Price.Should().Be(199.99m);
    }

    [Fact]
    public async Task UpdateProductAsync_WithInvalidId_ReturnsNull()
    {
        var nonExistentId = Guid.NewGuid();
        var product = new BlProduct
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            Category = "Test Category"
        };

        var result = await _service.UpdateProductAsync(nonExistentId, product);

        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteProductAsync_WithValidId_DeletesProductAndReturnsTrue()
    {
        var newProduct = new BlProduct
        {
            Name = "Product To Delete",
            Description = "Will be deleted",
            Price = 50.00m,
            Category = "Temporary"
        };
        var created = await _service.AddProductAsync(newProduct);

        var result = await _service.DeleteProductAsync(created.Id);

        result.Should().BeTrue();

        var deletedProduct = await _service.GetProductByIdAsync(created.Id);
        deletedProduct.Should().BeNull();
    }

    [Fact]
    public async Task DeleteProductAsync_WithInvalidId_ReturnsFalse()
    {
        var nonExistentId = Guid.NewGuid();

        var result = await _service.DeleteProductAsync(nonExistentId);

        result.Should().BeFalse();
    }
}