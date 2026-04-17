namespace VictoriaStore.Api.DTOs;

public class CheckoutRequest
{
    public required string CustomerName { get; set; }
    public required string CustomerEmail { get; set; }
    public required string CustomerPhone { get; set; }
    public string? DeliveryAddress { get; set; }
    public required List<CartItemDto> Items { get; set; } = new();
}

public class CartItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

public class OrderResponseDto
{
    public Guid Id { get; set; }
    public required string OrderNumber { get; set; }
    public required string CustomerName { get; set; }
    public required string CustomerEmail { get; set; }
    public required string CustomerPhone { get; set; }
    public string? DeliveryAddress { get; set; }
    public required string Status { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    public Guid ProductId { get; set; }
    public required string ProductName { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}