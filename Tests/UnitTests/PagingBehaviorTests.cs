using AppBL.Services;
using AppDAL.DalModels;
using AppDAL.Repositories;
using AppDAL.Repositories.Interfaces;
using FluentAssertions;
using Tests.Infrastructure;
using Xunit;
using BlProduct = AppBL.BlModels.Product;

namespace Tests.UnitTests;

public class PagingBehaviorTests
{
    private readonly IRepository<Product> _repo = new ProductRepository();
    private readonly ProductService _service;

    public PagingBehaviorTests()
    {
        var cache = new FakeCacheService<BlProduct>();
        _service = new ProductService(_repo, cache);
    }

    [Fact]
    public async Task GetAllProducts_WithoutPaging_ReturnsAllProducts()
    {
        var result = await _service.GetAllProductsAsync();

        result.Should().NotBeNull();
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(result.TotalCount);
        result.Items.Should().NotBeEmpty();
        result.Items.Count().Should().Be(result.TotalCount);
    }

    [Fact]
    public async Task GetAllProducts_WithPaging_ReturnsCorrectPage()
    {
        var pageSize = 5;
        var page = 1;

        var result = await _service.GetAllProductsAsync(page, pageSize);

        result.Should().NotBeNull();
        result.Page.Should().Be(page);
        result.PageSize.Should().Be(pageSize);
        result.Items.Should().HaveCountLessOrEqualTo(pageSize);
        result.TotalCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetAllProducts_FirstAndSecondPage_ReturnDifferentItems()
    {
        var pageSize = 3;

        var firstPage = await _service.GetAllProductsAsync(1, pageSize);
        var secondPage = await _service.GetAllProductsAsync(2, pageSize);

        if (firstPage.TotalCount > pageSize)
        {
            firstPage.Items.Should().HaveCount(pageSize);
            var firstPageIds = firstPage.Items.Select(p => p.Id).ToHashSet();
            var secondPageIds = secondPage.Items.Select(p => p.Id).ToHashSet();

            firstPageIds.Should().NotIntersectWith(secondPageIds);
        }
        else
        {
            secondPage.Items.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task GetAllProducts_PageBeyondTotal_ReturnsEmptyResults()
    {
        var pageSize = 5;
        var firstPage = await _service.GetAllProductsAsync(1, pageSize);
        var totalPages = (int)Math.Ceiling((double)firstPage.TotalCount / pageSize);

        var beyondPage = await _service.GetAllProductsAsync(totalPages + 1, pageSize);

        beyondPage.Items.Should().BeEmpty();
        beyondPage.Page.Should().Be(totalPages + 1);
        beyondPage.PageSize.Should().Be(pageSize);
        beyondPage.TotalCount.Should().Be(firstPage.TotalCount);
    }

    [Fact]
    public async Task GetAllProducts_WithZeroPageSize_ReturnsAllProducts()
    {
        var result = await _service.GetAllProductsAsync(1, 0);

        result.Should().NotBeNull();
        result.PageSize.Should().Be(0);
        result.Items.Count().Should().Be(result.TotalCount);
    }

    [Fact]
    public async Task GetAllProducts_WithNegativePageSize_ReturnsAllProducts()
    {
        var result = await _service.GetAllProductsAsync(1, -5);

        result.Should().NotBeNull();
        result.Items.Count().Should().Be(result.TotalCount);
    }

    [Fact]
    public async Task GetAllProducts_WithZeroPage_ReturnsFirstPage()
    {
        var pageSize = 5;
        var result = await _service.GetAllProductsAsync(0, pageSize);

        result.Should().NotBeNull();
        result.Items.Count().Should().Be(result.TotalCount);
    }

    [Fact]
    public async Task GetAllProducts_DifferentPageSizes_ReturnCorrectCounts()
    {
        var pageSizes = new[] { 1, 3, 5, 10 };

        foreach (var pageSize in pageSizes)
        {
            var result = await _service.GetAllProductsAsync(1, pageSize);
            result.Items.Should().HaveCountLessOrEqualTo(pageSize);
            result.PageSize.Should().Be(pageSize);
        }
    }

    [Fact]
    public async Task GetAllProducts_ConsistentTotalCount_AcrossPages()
    {
        var pageSize = 3;
        var firstPage = await _service.GetAllProductsAsync(1, pageSize);
        var secondPage = await _service.GetAllProductsAsync(2, pageSize);

        firstPage.TotalCount.Should().Be(secondPage.TotalCount);
    }

    [Fact]
    public async Task GetAllProducts_AllPagesSum_EqualsTotal()
    {
        var pageSize = 4;
        var firstPage = await _service.GetAllProductsAsync(1, pageSize);
        var totalPages = (int)Math.Ceiling((double)firstPage.TotalCount / pageSize);

        var allItemsFromPages = new List<BlProduct>();

        for (int page = 1; page <= totalPages; page++)
        {
            var pageResult = await _service.GetAllProductsAsync(page, pageSize);
            allItemsFromPages.AddRange(pageResult.Items);
        }

        allItemsFromPages.Should().HaveCount(firstPage.TotalCount);
        var uniqueIds = allItemsFromPages.Select(p => p.Id).Distinct().Count();
        uniqueIds.Should().Be(allItemsFromPages.Count);
    }

    [Fact]
    public async Task GetAllProducts_LargePageSize_ReturnsAllProducts()
    {
        var allProducts = await _service.GetAllProductsAsync();
        var largePageSize = allProducts.TotalCount * 2;

        var result = await _service.GetAllProductsAsync(1, largePageSize);

        result.Items.Should().HaveCount(allProducts.TotalCount);
        result.PageSize.Should().Be(largePageSize);
        result.TotalCount.Should().Be(allProducts.TotalCount);
    }
}