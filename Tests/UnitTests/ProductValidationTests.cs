using FluentAssertions;
using System.ComponentModel.DataAnnotations;
using WebApiTestApp.ApiModels;
using Xunit;

namespace Tests.UnitTests;

public class ProductValidationTests
{
    private static List<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, validationContext, validationResults, true);
        return validationResults;
    }

    [Fact]
    public void Product_WithValidData_PassesValidation()
    {
        var product = new Product
        {
            Name = "Valid Product",
            Description = "Valid description",
            Price = 99.99m,
            Category = "Valid Category"
        };

        var results = ValidateModel(product);

        results.Should().BeEmpty();
    }

    [Fact]
    public void Product_WithEmptyName_FailsValidation()
    {
        var product = new Product
        {
            Name = "",
            Description = "Valid description",
            Price = 99.99m,
            Category = "Valid Category"
        };

        var results = ValidateModel(product);

        results.Should().NotBeEmpty();
        results.Should().Contain(r => r.MemberNames.Contains("Name"));
    }

    [Fact]
    public void Product_WithNullName_FailsValidation()
    {
        var product = new Product
        {
            Name = null!,
            Description = "Valid description",
            Price = 99.99m,
            Category = "Valid Category"
        };

        var results = ValidateModel(product);

        results.Should().NotBeEmpty();
        results.Should().Contain(r => r.MemberNames.Contains("Name"));
    }

    [Fact]
    public void Product_WithEmptyCategory_FailsValidation()
    {
        var product = new Product
        {
            Name = "Valid Product",
            Description = "Valid description",
            Price = 99.99m,
            Category = ""
        };

        var results = ValidateModel(product);

        results.Should().NotBeEmpty();
        results.Should().Contain(r => r.MemberNames.Contains("Category"));
    }

    [Fact]
    public void Product_WithNegativePrice_FailsValidation()
    {
        var product = new Product
        {
            Name = "Valid Product",
            Description = "Valid description",
            Price = -10m,
            Category = "Valid Category"
        };

        var results = ValidateModel(product);

        results.Should().NotBeEmpty();
        results.Should().Contain(r => r.MemberNames.Contains("Price"));
    }

    [Fact]
    public void Product_WithTooLargeName_FailsValidation()
    {
        var product = new Product
        {
            Name = new string('A', 101),
            Description = "Valid description",
            Price = 99.99m,
            Category = "Valid Category"
        };

        var results = ValidateModel(product);

        results.Should().NotBeEmpty();
        results.Should().Contain(r => r.MemberNames.Contains("Name"));
    }

    [Fact]
    public void Product_WithTooLargeDescription_FailsValidation()
    {
        var product = new Product
        {
            Name = "Valid Product",
            Description = new string('A', 501),
            Price = 99.99m,
            Category = "Valid Category"
        };

        var results = ValidateModel(product);

        results.Should().NotBeEmpty();
        results.Should().Contain(r => r.MemberNames.Contains("Description"));
    }

    [Fact]
    public void Product_WithTooLargeCategory_FailsValidation()
    {
        var product = new Product
        {
            Name = "Valid Product",
            Description = "Valid description",
            Price = 99.99m,
            Category = new string('A', 101)
        };

        var results = ValidateModel(product);

        results.Should().NotBeEmpty();
        results.Should().Contain(r => r.MemberNames.Contains("Category"));
    }

    [Fact]
    public void Product_WithMaximumValidPrice_PassesValidation()
    {
        var product = new Product
        {
            Name = "Valid Product",
            Description = "Valid description",
            Price = 9999999m,
            Category = "Valid Category"
        };

        var results = ValidateModel(product);

        results.Should().BeEmpty();
    }

    [Fact]
    public void Product_WithTooLargePrice_FailsValidation()
    {
        var product = new Product
        {
            Name = "Valid Product",
            Description = "Valid description",
            Price = 10000000m,
            Category = "Valid Category"
        };

        var results = ValidateModel(product);

        results.Should().NotBeEmpty();
        results.Should().Contain(r => r.MemberNames.Contains("Price"));
    }

    [Fact]
    public void Product_WithZeroPrice_PassesValidation()
    {
        var product = new Product
        {
            Name = "Free Product",
            Description = "This is free",
            Price = 0m,
            Category = "Free"
        };

        var results = ValidateModel(product);

        results.Should().BeEmpty();
    }

    [Fact]
    public void Product_WithMaximumLengthStrings_PassesValidation()
    {
        var product = new Product
        {
            Name = new string('A', 100),
            Description = new string('B', 500),
            Price = 99.99m,
            Category = new string('C', 100)
        };

        var results = ValidateModel(product);

        results.Should().BeEmpty();
    }
}