using AppDAL.Repositories;
using FluentAssertions;
using Xunit;

namespace Tests.UnitTests;

public class PagingTests
{
    private readonly ProductRepository _repo = new();

    [Fact]
    public async Task PageBeyondTotalReturnsEmpty()
    {
        var first = await _repo.GetAllAsync(page: 1, pageSize: 5);
        var totalPages = (int)Math.Ceiling(first.TotalCount / 5.0);
        var beyond = await _repo.GetAllAsync(page: totalPages + 1, pageSize: 5);
        beyond.Items.Should().BeEmpty();
        beyond.Page.Should().Be(totalPages + 1);
    }

    [Fact]
    public async Task PageSizeZeroIgnoredAndReturnsAll()
    {
        var result = await _repo.GetAllAsync(page: 1, pageSize: 0);
        result.PageSize.Should().Be(0);
        result.Items.Count().Should().Be(20);
    }
}
