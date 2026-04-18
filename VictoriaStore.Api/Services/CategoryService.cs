using Microsoft.EntityFrameworkCore;
using VictoriaStore.Api.Data;
using VictoriaStore.Api.DTOs;
using VictoriaStore.Api.Models;

namespace VictoriaStore.Api.Services;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllActiveAsync();
    Task<IEnumerable<CategoryDto>> GetAllAdminAsync();
    Task<CategoryDto?> GetByIdAsync(Guid id);
    Task<CategoryDto> CreateAsync(CreateCategoryRequest request);
    Task<CategoryDto> UpdateAsync(Guid id, CreateCategoryRequest request);
    Task DeleteAsync(Guid id);
}

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;

    public CategoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllActiveAsync()
    {
        return await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .Select(c => MapToDto(c))
            .ToListAsync();
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAdminAsync()
    {
        return await _context.Categories
            .OrderBy(c => c.DisplayOrder)
            .Select(c => MapToDto(c))
            .ToListAsync();
    }

    public async Task<CategoryDto?> GetByIdAsync(Guid id)
    {
        var category = await _context.Categories.FindAsync(id);
        return category == null ? null : MapToDto(category);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request)
    {
        var category = new Category
        {
            Name = request.Name.Trim(),
            Slug = BuildSlug(string.IsNullOrWhiteSpace(request.Slug) ? request.Name : request.Slug),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(category.Id) ?? throw new Exception("Failed to create category.");
    }

    public async Task<CategoryDto> UpdateAsync(Guid id, CreateCategoryRequest request)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category == null)
            throw new KeyNotFoundException("Category not found.");

        category.Name = request.Name.Trim();
        category.Slug = BuildSlug(string.IsNullOrWhiteSpace(request.Slug) ? request.Name : request.Slug);
        category.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        category.DisplayOrder = request.DisplayOrder;
        category.IsActive = request.IsActive;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(category.Id) ?? throw new Exception("Failed to update category.");
    }

    public async Task DeleteAsync(Guid id)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category == null)
            throw new KeyNotFoundException("Category not found.");

        var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);
        if (hasProducts)
            throw new InvalidOperationException("Cannot delete this category because products are still assigned to it.");

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
    }

    private static CategoryDto MapToDto(Category c)
    {
        return new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Slug = c.Slug,
            Description = c.Description,
            BannerImageUrl = c.BannerImageUrl,
            DisplayOrder = c.DisplayOrder,
            IsActive = c.IsActive

        };
    }

    private static string BuildSlug(string value)
    {
        return value.Trim().ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("'", string.Empty);
    }
}
