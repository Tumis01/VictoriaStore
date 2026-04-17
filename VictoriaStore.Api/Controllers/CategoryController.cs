using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VictoriaStore.Api.DTOs;
using VictoriaStore.Api.Services;

namespace VictoriaStore.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet] // Public - for the storefront
    public async Task<IActionResult> GetActiveCategories()
    {
        return Ok(await _categoryService.GetAllActiveAsync());
    }

    [HttpGet("admin")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetAllForAdmin()
    {
        return Ok(await _categoryService.GetAllAdminAsync());
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Create([FromForm] CreateCategoryRequest request)
    {
        var category = await _categoryService.CreateAsync(request);
        return CreatedAtAction(nameof(GetActiveCategories), new { id = category.Id }, category);
    }
}   