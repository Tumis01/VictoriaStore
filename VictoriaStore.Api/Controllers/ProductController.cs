using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VictoriaStore.Api.DTOs;
using VictoriaStore.Api.Services;

namespace VictoriaStore.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetActiveProducts()
    {
        var products = await _productService.GetAllActiveAsync();
        return Ok(products);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchProducts([FromQuery] string q, [FromQuery] string? category)
    {
        var products = await _productService.GetAllActiveAsync();

        if (!string.IsNullOrWhiteSpace(q))
        {
            products = products.Where(p =>
                p.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrWhiteSpace(p.Description) &&
                 p.Description.Contains(q, StringComparison.OrdinalIgnoreCase)));
        }

        if (!string.IsNullOrWhiteSpace(category) &&
            !string.Equals(category, "all", StringComparison.OrdinalIgnoreCase))
        {
            products = products.Where(p =>
                !string.IsNullOrWhiteSpace(p.CategoryName) &&
                p.CategoryName.Equals(category, StringComparison.OrdinalIgnoreCase));
        }

        return Ok(products);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProduct(Guid id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null)
            return NotFound("Product not found.");

        return Ok(product);
    }

    [HttpGet("admin")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetAllForAdmin()
    {
        var products = await _productService.GetAllAdminAsync();
        return Ok(products);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Create([FromForm] CreateProductRequest request)
    {
        try
        {
            var product = await _productService.CreateAsync(request);
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }
        catch (DbUpdateException ex)
        {
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Update(Guid id, [FromForm] CreateProductRequest request)
    {
        try
        {
            var product = await _productService.UpdateAsync(id, request);
            return Ok(product);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var entries = ex.Entries.Select(e => new
            {
                Entity = e.Metadata.ClrType.Name,
                State = e.State.ToString(),
                Keys = e.Properties
                    .Where(p => p.Metadata.IsPrimaryKey())
                    .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue?.ToString()),
                CurrentValues = e.Properties.ToDictionary(
                    p => p.Metadata.Name,
                    p => p.CurrentValue?.ToString()
                ),
                OriginalValues = e.Properties.ToDictionary(
                    p => p.Metadata.Name,
                    p => p.OriginalValue?.ToString()
                )
            });

            return Conflict(new
            {
                message = "This product was changed or removed while you were editing it.",
                error = ex.Message,
                inner = ex.InnerException?.Message,
                entries
            });
        }

        catch (DbUpdateException ex)
        {
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _productService.SoftDeleteAsync(id);

        if (!result)
            return NotFound("Product not found.");

        return NoContent();
    }

    [HttpDelete("{productId:guid}/images/{imageId:guid}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> DeleteImage(Guid productId, Guid imageId)
    {
        try
        {
            var deleted = await _productService.DeleteImageAsync(productId, imageId);

            if (!deleted)
                return NotFound("Product image not found.");

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
