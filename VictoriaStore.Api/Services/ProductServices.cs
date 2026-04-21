using Microsoft.EntityFrameworkCore;
using VictoriaStore.Api.Data;
using VictoriaStore.Api.DTOs;
using VictoriaStore.Api.Models;

namespace VictoriaStore.Api.Services;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllActiveAsync();
    Task<IEnumerable<ProductDto>> GetAllAdminAsync();
    Task<ProductDto?> GetByIdAsync(Guid id);
    Task<ProductDto> CreateAsync(CreateProductRequest request);
    Task<bool> SoftDeleteAsync(Guid id);
    Task<ProductDto> UpdateAsync(Guid id, CreateProductRequest request);
}

public class ProductService : IProductService
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    public ProductService(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    public async Task<IEnumerable<ProductDto>> GetAllActiveAsync()
    {
        // 1. Fetch the data from the database first
        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Where(p => p.IsActive && !p.IsDeleted)
            .ToListAsync();

        // 2. Map to DTOs in memory using your custom method
        return products.Select(p => MapToDto(p));
    }

    public async Task<IEnumerable<ProductDto>> GetAllAdminAsync()
    {
        // 1. Fetch the data from the database first
        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Where(p => !p.IsDeleted)
            .ToListAsync();

        // 2. Map to DTOs in memory
        return products.Select(p => MapToDto(p));
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        return product == null ? null : MapToDto(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request)
    {
        var product = new Product
        {
            Name = request.Name,
            Slug = request.Slug,
            Description = request.Description,
            Price = request.Price,
            SalePrice = request.SalePrice,
            SKU = request.SKU,
            StockQuantity = request.StockQuantity,
            CategoryId = request.CategoryId,
            IsActive = request.IsActive,
            Colors = request.Colors ?? new List<string>(), // Add colors
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        if (request.Images != null && request.Images.Any())
        {
            var uploadsFolder = Path.Combine(
                _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
                "images",
                "products");

            Directory.CreateDirectory(uploadsFolder);

            var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg"
            };

            var displayOrder = 1;

            foreach (var file in request.Images)
            {
                if (file.Length <= 0)
                    continue;

                var extension = Path.GetExtension(file.FileName);
                if (!allowedExtensions.Contains(extension))
                    continue;

                var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                await using var fileStream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(fileStream);

                product.Images.Add(new ProductImage
                {
                    ImageUrl = $"/images/products/{uniqueFileName}",
                    DisplayOrder = displayOrder,
                    IsMain = displayOrder == 1
                });

                displayOrder++;
            }
        }

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(product.Id) ?? throw new Exception("Failed to retrieve created product.");
    }
    public async Task<ProductDto> UpdateAsync(Guid id, CreateProductRequest request)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (product == null)
            throw new KeyNotFoundException("Product not found.");

        // 1. Update basic properties
        product.Name = request.Name.Trim();
        product.Slug = request.Slug.Trim();
        product.Description = request.Description;
        product.Price = request.Price;
        product.SalePrice = request.SalePrice;
        product.SKU = request.SKU.Trim().ToUpperInvariant();
        product.StockQuantity = request.StockQuantity;
        product.CategoryId = request.CategoryId;
        product.IsActive = request.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        // Safely update primitive collections (Colors) by clearing and re-adding
        // to prevent collection tracking reference errors
        var newColors = (request.Colors ?? new List<string>())
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        product.Colors.Clear();
        foreach (var c in newColors)
        {
            product.Colors.Add(c);
        }

        // 2. Handle New Images Safely
        if (request.Images != null && request.Images.Any(f => f.Length > 0))
        {
            // STEP A: Reset existing main images and force a save state 
            // to prevent EF Core tracking confusion
            foreach (var img in product.Images)
            {
                img.IsMain = false;
                _context.Entry(img).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync(); // Commit the UPDATEs first!

            var uploadsFolder = Path.Combine(
                _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
                "images",
                "products");

            Directory.CreateDirectory(uploadsFolder);

            var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg"
            };

            var nextDisplayOrder = product.Images.Any() ? product.Images.Max(i => i.DisplayOrder) + 1 : 1;
            var firstNewImage = true;

            // STEP B: Process and add the new images
            foreach (var file in request.Images)
            {
                if (file.Length <= 0) continue;

                var extension = Path.GetExtension(file.FileName);
                if (!allowedExtensions.Contains(extension)) continue;

                var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                await using var fileStream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(fileStream);

                product.Images.Add(new ProductImage
                {
                    ImageUrl = $"/images/products/{uniqueFileName}",
                    DisplayOrder = nextDisplayOrder++,
                    IsMain = firstNewImage // First new image takes over as main
                });

                firstNewImage = false;
            }
        }

        // 3. Final save for the new images and standard properties
        await _context.SaveChangesAsync();

        return await GetByIdAsync(product.Id)
            ?? throw new Exception("Failed to retrieve updated product.");
    }
    private static ProductDto MapToDto(Product p)
    {
        return new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Slug = p.Slug,
            Description = p.Description,
            Price = p.Price,
            SalePrice = p.SalePrice,
            SKU = p.SKU,
            StockQuantity = p.StockQuantity,
            Colors = p.Colors?.ToList() ?? new List<string>(),
            IsActive = p.IsActive,
            CategoryId = p.CategoryId,
            CategoryName = p.Category?.Name,
            CreatedAt = p.CreatedAt,
            Images = p.Images
                .OrderBy(i => i.DisplayOrder)
                .Select(i => new ProductImageDto
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    DisplayOrder = i.DisplayOrder,
                    IsMain = i.IsMain
                })
                .ToList()
        };
    }

    public async Task<bool> SoftDeleteAsync(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null || product.IsDeleted) return false;

        product.IsDeleted = true;
        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

}
