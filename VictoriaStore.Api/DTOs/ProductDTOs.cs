namespace VictoriaStore.Api.DTOs;

public class ProductDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }
    public required string SKU { get; set; }
    public int StockQuantity { get; set; }
    public List<string> Colors { get; set; } = new();
    public bool IsActive { get; set; }
    public Guid CategoryId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CategoryName { get; set; }
    public List<ProductImageDto> Images { get; set; } = new();
}

public class ProductImageDto
{
    public Guid Id { get; set; }
    public required string ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsMain { get; set; }
}

public class CreateProductRequest
{
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }
    public required string SKU { get; set; }
    public int StockQuantity { get; set; }
    public List<string> Colors { get; set; } = new();
    public Guid CategoryId { get; set; }
    public bool IsActive { get; set; } = true;

    // Accept multiple files for product gallery
    public List<IFormFile>? Images { get; set; }
}