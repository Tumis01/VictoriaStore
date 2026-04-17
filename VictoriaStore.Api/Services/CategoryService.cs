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
}

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    public CategoryService(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllActiveAsync()
    {
        return await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Description = c.Description,
                BannerImageUrl = c.BannerImageUrl,
                DisplayOrder = c.DisplayOrder,
                IsActive = c.IsActive
            }).ToListAsync();
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAdminAsync()
    {
        // Admin sees everything, including inactive
        return await _context.Categories
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Description = c.Description,
                BannerImageUrl = c.BannerImageUrl,
                DisplayOrder = c.DisplayOrder,
                IsActive = c.IsActive
            }).ToListAsync();
    }

    public async Task<CategoryDto?> GetByIdAsync(Guid id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return null;

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            Description = category.Description,
            BannerImageUrl = category.BannerImageUrl,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive
        };
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request)
    {
        var category = new Category
        {
            Name = request.Name,
            Slug = request.Slug,
            Description = request.Description,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive
        };

        // Handle File Upload to wwwroot/uploads/categories
        if (request.BannerImage != null && request.BannerImage.Length > 0)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "categories");
            Directory.CreateDirectory(uploadsFolder); // Ensure folder exists

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + request.BannerImage.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await request.BannerImage.CopyToAsync(fileStream);
            }

            category.BannerImageUrl = $"/uploads/categories/{uniqueFileName}";
        }

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(category.Id) ?? throw new Exception("Failed to create category");
    }
}