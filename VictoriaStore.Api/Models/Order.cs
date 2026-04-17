namespace VictoriaStore.Api.Models;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string OrderNumber { get; set; } // e.g., ORD-00123 [cite: 108]
    public required string CustomerName { get; set; }
    public required string CustomerEmail { get; set; }
    public required string CustomerPhone { get; set; }
    public string? DeliveryAddress { get; set; }

    public string Status { get; set; } = "Pending";
    public decimal TotalAmount { get; set; }
    public string? BankPaymentNote { get; set; }
    public string? AdminNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
}
public class OrderItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public Guid ProductId { get; set; }
    public required string ProductName { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal => UnitPrice * Quantity;
}
public class OrderStatusHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public required string Status { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public required string ChangedBy { get; set; } // Admin username [cite: 112]
    public string? Note { get; set; }
}