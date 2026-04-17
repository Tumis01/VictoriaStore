namespace VictoriaStore.Api.Models;

public class Category
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; } // e.g., Household Items [cite: 106]
    public required string Slug { get; set; }
    public string? Description { get; set; }
    public string? BannerImageUrl { get; set; } // Local path: /uploads/categories/...
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation property
    public ICollection<Product> Products { get; set; } = new List<Product>();
}