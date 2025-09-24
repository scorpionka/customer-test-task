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
    public async Task<ActionResult<PagedResult<Product>>> GetAll([FromQuery] PagingQuery paging, CancellationToken cancellationToken)
    {
        try
        {
            var paged = await productService.GetAllProductsAsync(paging.EffectivePage, paging.EffectivePageSize, cancellationToken);
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

            return Ok(product.MapToApiProduct());
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

        try
        {
            var createdProduct = await productService.AddProductAsync(product.MapToBlProduct(), cancellationToken);
            var apiModel = createdProduct.MapToApiProduct();
            return CreatedAtAction(nameof(GetById), new { id = apiModel.Id }, apiModel);
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
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var updatedProduct = await productService.UpdateProductAsync(id, product.MapToBlProduct(), cancellationToken);
            if (updatedProduct == null)
            {
                return NotFound();
            }

            return Ok(updatedProduct.MapToApiProduct());
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