using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using WebApiTestApp.ApiModels;
using Xunit;

namespace Tests.IntegrationTests;

public class ProductsControllerIntegrationTests(WebApplicationFactory<WebApiTestApp.Controllers.ProductsController> factory) : IClassFixture<WebApplicationFactory<WebApiTestApp.Controllers.ProductsController>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetAllProducts_ReturnsSuccessAndProducts()
    {
        var response = await _client.GetAsync("/api/products");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var products = await response.Content.ReadFromJsonAsync<PagedResult<Product>>();
        products.Should().NotBeNull();
        products!.Items.Should().NotBeEmpty();
        products.TotalCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetAllProducts_WithPaging_ReturnsCorrectPage()
    {
        var response = await _client.GetAsync("/api/products?page=1&pageSize=5");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var products = await response.Content.ReadFromJsonAsync<PagedResult<Product>>();
        products.Should().NotBeNull();
        products!.Items.Should().HaveCountLessOrEqualTo(5);
        products.Page.Should().Be(1);
        products.PageSize.Should().Be(5);
    }

    [Fact]
    public async Task GetProductById_WithValidId_ReturnsProduct()
    {
        var allResponse = await _client.GetAsync("/api/products");
        var allProducts = await allResponse.Content.ReadFromJsonAsync<PagedResult<Product>>();
        var firstProduct = allProducts!.Items.First();

        var response = await _client.GetAsync($"/api/products/{firstProduct.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var product = await response.Content.ReadFromJsonAsync<Product>();
        product.Should().NotBeNull();
        product!.Id.Should().Be(firstProduct.Id);
    }

    [Fact]
    public async Task GetProductById_WithInvalidId_ReturnsNotFound()
    {
        var invalidId = Guid.NewGuid();

        var response = await _client.GetAsync($"/api/products/{invalidId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateProduct_WithValidProduct_ReturnsCreatedProduct()
    {
        var newProduct = new Product
        {
            Name = "Integration Test Product",
            Description = "Created via integration test",
            Price = 149.99m,
            Category = "Test Category"
        };

        var response = await _client.PostAsJsonAsync("/api/products", newProduct);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdProduct = await response.Content.ReadFromJsonAsync<Product>();
        createdProduct.Should().NotBeNull();
        createdProduct!.Name.Should().Be(newProduct.Name);
        createdProduct.Price.Should().Be(newProduct.Price);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateProduct_WithInvalidProduct_ReturnsBadRequest()
    {
        var invalidProduct = new Product
        {
            Name = "",
            Description = "Missing name",
            Price = -10m,
            Category = ""
        };

        var response = await _client.PostAsJsonAsync("/api/products", invalidProduct);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateProduct_WithValidProduct_ReturnsUpdatedProduct()
    {
        var createProduct = new Product
        {
            Name = "Product to Update",
            Description = "Will be updated",
            Price = 99.99m,
            Category = "Update Test"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/products", createProduct);
        var created = await createResponse.Content.ReadFromJsonAsync<Product>();

        var updatedProduct = created! with { Name = "Updated Product", Price = 199.99m };
        var response = await _client.PutAsJsonAsync($"/api/products/{created.Id}", updatedProduct);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<Product>();
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Updated Product");
        updated.Price.Should().Be(199.99m);
    }

    [Fact]
    public async Task UpdateProduct_WithInvalidId_ReturnsNotFound()
    {
        var invalidId = Guid.NewGuid();
        var product = new Product
        {
            Name = "Test Product",
            Description = "Test",
            Price = 99.99m,
            Category = "Test"
        };

        var response = await _client.PutAsJsonAsync($"/api/products/{invalidId}", product);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateProduct_WithInvalidProduct_ReturnsBadRequest()
    {
        var createProduct = new Product
        {
            Name = "Product for Invalid Update",
            Description = "Will be updated with invalid data",
            Price = 99.99m,
            Category = "Update Test"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/products", createProduct);
        var created = await createResponse.Content.ReadFromJsonAsync<Product>();

        var invalidUpdate = created! with { Name = "", Price = -100m, Category = "" };
        var response = await _client.PutAsJsonAsync($"/api/products/{created.Id}", invalidUpdate);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteProduct_WithValidId_ReturnsNoContent()
    {
        var createProduct = new Product
        {
            Name = "Product to Delete",
            Description = "Will be deleted",
            Price = 79.99m,
            Category = "Delete Test"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/products", createProduct);
        var created = await createResponse.Content.ReadFromJsonAsync<Product>();

        var response = await _client.DeleteAsync($"/api/products/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/products/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteProduct_WithInvalidId_ReturnsNotFound()
    {
        var invalidId = Guid.NewGuid();

        var response = await _client.DeleteAsync($"/api/products/{invalidId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CrudWorkflow_CompleteScenario_WorksCorrectly()
    {
        var newProduct = new Product
        {
            Name = "CRUD Test Product",
            Description = "Full CRUD workflow test",
            Price = 299.99m,
            Category = "CRUD Test"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/products", newProduct);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<Product>();
        created.Should().NotBeNull();

        var getResponse = await _client.GetAsync($"/api/products/{created!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var retrieved = await getResponse.Content.ReadFromJsonAsync<Product>();
        retrieved!.Name.Should().Be(newProduct.Name);

        var updated = created with { Name = "Updated CRUD Product", Price = 399.99m };
        var updateResponse = await _client.PutAsJsonAsync($"/api/products/{created.Id}", updated);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedResult = await updateResponse.Content.ReadFromJsonAsync<Product>();
        updatedResult!.Name.Should().Be("Updated CRUD Product");

        var deleteResponse = await _client.DeleteAsync($"/api/products/{created.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getFinalResponse = await _client.GetAsync($"/api/products/{created.Id}");
        getFinalResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}