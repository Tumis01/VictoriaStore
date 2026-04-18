using System.ComponentModel.DataAnnotations;

namespace VictoriaStores.Frontend.Models;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public List<string> EnumerableRoles { get; set; } = new();
}


public class CreateUpdateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; } = true;
    public string MainImageUrl { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
}

public class CartItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public decimal LineTotal => UnitPrice * Quantity;
}

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public string? BannerImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CreateCategoryRequest
{
    [Required(ErrorMessage = "Category Name is required.")]
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}

public class ProductDto
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Product Name is required.")]
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Price is required.")]
    [Range(1, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
    public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }
    public string SKU { get; set; } = string.Empty;

    [Required(ErrorMessage = "Stock quantity is required.")]
    [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative.")]
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; } = true;

    [Required(ErrorMessage = "Please select a category.")]
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public List<ProductImageDto> Images { get; set; } = new();

    // NEW: Colors Property
    public List<string> Colors { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public string MainImageUrl =>
        Images.FirstOrDefault(i => i.IsMain)?.ImageUrl
        ?? Images.FirstOrDefault()?.ImageUrl
        ?? string.Empty;
}
public class OrderResponseDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    // Optional: List of order items if your API returns them
}

public class UpdateOrderStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

// Settings DTOs
public class StoreSettingsDto
{
    public string StoreName { get; set; } = "Victoria James Store";
    public string ContactEmail { get; set; } = "support@victoriajames.com";
    public string SupportPhone { get; set; } = "08162809886";
    public string StoreAddress { get; set; } = string.Empty;

    public string BankName { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;

    public bool EmailNotificationsEnabled { get; set; } = true;
    public bool SmsNotificationsEnabled { get; set; } = false;
}
public class OrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending, Confirmed, Shipped, Delivered, Cancelled
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    public Guid Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
public class ProductImageDto
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsMain { get; set; }
}

public class CustomerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int OrdersCount { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime JoinedDate { get; set; }

    // Holds the nested order history for the accordion dropdown
    public List<CustomerOrderHistoryDto> RecentOrders { get; set; } = new();
}

public class CustomerOrderHistoryDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}
public class CheckoutRequest
{
    [Required(ErrorMessage = "Full Name is required.")]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email Address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email.")]
    public string CustomerEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone Number is required.")]
    public string CustomerPhone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Delivery Address is required.")]
    public string DeliveryAddress { get; set; } = string.Empty;

    public string? Notes { get; set; } // Optional special instructions

    public List<CartItemDto> Items { get; set; } = new();
}