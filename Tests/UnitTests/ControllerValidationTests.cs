using AppBL.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebApiTestApp.ApiModels;
using WebApiTestApp.Controllers;
using Xunit;

namespace Tests.UnitTests;

public class ControllerValidationTests
{
    private readonly Mock<IProductService> _productService = new();

    [Fact]
    public async Task CreateInvalidModel_ReturnsValidationProblem()
    {
        var controller = new ProductsController(_productService.Object);
        controller.ModelState.AddModelError("Name", "Required");
        var result = await controller.Create(new Product { Price = -1, Name = "" }, default);

        result.Result.Should().BeOfType<ObjectResult>();
        var obj = (ObjectResult)result.Result!;
        obj.Value.Should().BeOfType<ValidationProblemDetails>();

        var problemDetails = (ValidationProblemDetails)obj.Value!;
        problemDetails.Errors.Should().ContainKey("Name");
        problemDetails.Errors["Name"].Should().Contain("Required");
    }
}
