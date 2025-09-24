using AppBL.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using WebApiTestApp.ApiModels;
using WebApiTestApp.Mappers;

namespace WebApiTestApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(
    IProductService productService,
    ILogger<ProductsController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<Product>>> GetAll([FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken cancellationToken)
    {
        if (page.HasValue && page <= 0) return BadRequest("Page must be > 0");
        if (pageSize.HasValue && pageSize <= 0) return BadRequest("PageSize must be > 0");

        try
        {
            var paged = await productService.GetAllProductsAsync(page, pageSize, cancellationToken);
            return Ok(paged.MapToApiPagedResult());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving products");
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Product>> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var product = await productService.GetProductByIdAsync(id, cancellationToken);
            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving product {ProductId}", id);
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    [HttpPost]
    public async Task<ActionResult<Product>> Create([FromBody] Product product, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }
        if (string.IsNullOrWhiteSpace(product.Name))
        {
            return BadRequest("Name is required");
        }
        if (product.Price < 0)
        {
            return BadRequest("Price must be non-negative");
        }

        try
        {
            var createdProduct = await productService.AddProductAsync(product.MapToBlProduct(), cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = createdProduct.Id }, createdProduct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating product");
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Product>> Update(Guid id, [FromBody] Product product, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(product.Name))
        {
            return BadRequest("Name is required");
        }
        if (product.Price < 0)
        {
            return BadRequest("Price must be non-negative");
        }

        try
        {
            var updatedProduct = await productService.UpdateProductAsync(id, product.MapToBlProduct(), cancellationToken);
            if (updatedProduct == null)
            {
                return NotFound();
            }

            return Ok(updatedProduct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating product {ProductId}", id);
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await productService.DeleteProductAsync(id, cancellationToken);
            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting product {ProductId}", id);
            return StatusCode(500, "An unexpected error occurred.");
        }
    }
}