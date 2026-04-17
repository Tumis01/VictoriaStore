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
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Where(p => p.IsActive && !p.IsDeleted)
            .Select(p => MapToDto(p))
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductDto>> GetAllAdminAsync()
    {
        // Admin sees inactive products, but usually not soft-deleted ones 
        // unless you specifically want an archive view later.
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Where(p => !p.IsDeleted)
            .Select(p => MapToDto(p))
            .ToListAsync();
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
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Handle Multiple Image Uploads to wwwroot/uploads/products
        if (request.Images != null && request.Images.Any())
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "products");
            Directory.CreateDirectory(uploadsFolder);

            int displayOrder = 1;
            foreach (var file in request.Images)
            {
                if (file.Length > 0)
                {
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    product.Images.Add(new ProductImage
                    {
                        ImageUrl = $"/uploads/products/{uniqueFileName}",
                        DisplayOrder = displayOrder,
                        IsMain = displayOrder == 1 // Make the first uploaded image the main one
                    });

                    displayOrder++;
                }
            }
        }

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(product.Id) ?? throw new Exception("Failed to retrieve created product.");
    }

    public async Task<bool> SoftDeleteAsync(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null || product.IsDeleted) return false;

        product.IsDeleted = true;
        product.IsActive = false; // Ensure it hides from the storefront immediately
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    // Helper method to keep projection clean
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
            IsActive = p.IsActive,
            CategoryId = p.CategoryId,
            CategoryName = p.Category?.Name,
            Images = p.Images.OrderBy(i => i.DisplayOrder).Select(i => new ProductImageDto
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                DisplayOrder = i.DisplayOrder,
                IsMain = i.IsMain
            }).ToList()
        };
    }
}