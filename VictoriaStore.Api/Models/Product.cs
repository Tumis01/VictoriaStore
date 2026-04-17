namespace VictoriaStore.Api.Models;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }
    public required string SKU { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Key
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    // Navigation properties
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
}
public class ProductImage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    // Will store paths like /uploads/products/image.webp
    public required string ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsMain { get; set; }
}