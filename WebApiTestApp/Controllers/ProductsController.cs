using AppBL.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using WebApiTestApp.ApiModels;
using WebApiTestApp.Mappers;

namespace WebApiTestApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(
    IProductService productService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<Product>>> GetAll([FromQuery] PagingQuery paging, CancellationToken cancellationToken)
    {
        var paged = await productService.GetAllProductsAsync(paging.EffectivePage, paging.EffectivePageSize, cancellationToken);
        return Ok(paged.MapToApiPagedResult());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Product>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var product = await productService.GetProductByIdAsync(id, cancellationToken);
        if (product == null)
        {
            return NotFound();
        }
        return Ok(product.MapToApiProduct());
    }

    [HttpPost]
    public async Task<ActionResult<Product>> Create([FromBody] Product product, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }
        var createdProduct = await productService.AddProductAsync(product.MapToBlProduct(), cancellationToken);
        var apiModel = createdProduct.MapToApiProduct();
        return CreatedAtAction(nameof(GetById), new { id = apiModel.Id }, apiModel);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Product>> Update(Guid id, [FromBody] Product product, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }
        var updatedProduct = await productService.UpdateProductAsync(id, product.MapToBlProduct(), cancellationToken);
        if (updatedProduct == null)
        {
            return NotFound();
        }
        return Ok(updatedProduct.MapToApiProduct());
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await productService.DeleteProductAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }
        return NoContent();
    }
}