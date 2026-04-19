using Microsoft.EntityFrameworkCore;
using VictoriaStore.Api.Data;
using VictoriaStore.Api.DTOs;
using VictoriaStore.Api.Models;

namespace VictoriaStore.Api.Services;

public interface IOrderService
{
    Task<OrderResponseDto> PlaceOrderAsync(CheckoutRequest request);
    Task<OrderResponseDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<OrderResponseDto>> GetAllAdminAsync();
    Task<bool> UpdateOrderStatusAsync(Guid orderId, string newStatus, string adminUsername, string? note = null);
}
public class OrderService : IOrderService
{
    private readonly AppDbContext _context;

    public OrderService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<OrderResponseDto> PlaceOrderAsync(CheckoutRequest request)
    {
        if (request.Items == null || !request.Items.Any())
            throw new ArgumentException("Cart cannot be empty.");

        // Start a database transaction
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 1. Generate unique Order Number (e.g., ORD-2604-XYZ)
            var orderNumber = $"ORD-{DateTime.UtcNow:yyMM}-{Guid.NewGuid().ToString().Substring(0, 5).ToUpper()}";

            var order = new Order
            {
                OrderNumber = orderNumber,
                CustomerName = request.CustomerName,
                CustomerEmail = request.CustomerEmail,
                CustomerPhone = request.CustomerPhone,
                DeliveryAddress = request.DeliveryAddress,
                BankPaymentNote = request.Notes,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            decimal totalAmount = 0;

            // 2. Process each item, secure prices, and deduct stock
            foreach (var cartItem in request.Items)
            {
                var product = await _context.Products.FindAsync(cartItem.ProductId);

                if (product == null || !product.IsActive || product.IsDeleted)
                    throw new Exception($"Product with ID {cartItem.ProductId} is unavailable.");

                if (product.StockQuantity < cartItem.Quantity)
                    throw new Exception($"Insufficient stock for {product.Name}. Only {product.StockQuantity} left.");

                // Use SalePrice if it exists, otherwise use standard Price
                decimal activePrice = product.SalePrice ?? product.Price;

                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    UnitPrice = activePrice,
                    Quantity = cartItem.Quantity,
                    Order = order
                };

                order.Items.Add(orderItem);
                totalAmount += orderItem.LineTotal;

                // Deduct stock
                product.StockQuantity -= cartItem.Quantity;
                product.UpdatedAt = DateTime.UtcNow;
            }

            order.TotalAmount = totalAmount;

            // 3. Create initial history record
            var history = new OrderStatusHistory
            {
                Order = order,
                Status = "Pending",
                ChangedBy = "System", // Placed by customer
                Note = "Order placed successfully."
            };
            order.StatusHistory.Add(history);

            // 4. Save to Database
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return MapToDto(order);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw; // Re-throw to be handled by the controller
        }
    }

    public async Task<OrderResponseDto?> GetByIdAsync(Guid id)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        return order == null ? null : MapToDto(order);
    }

    public async Task<IEnumerable<OrderResponseDto>> GetAllAdminAsync()
    {
        return await _context.Orders
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => MapToDto(o))
            .ToListAsync();
    }

    public async Task<bool> UpdateOrderStatusAsync(Guid orderId, string newStatus, string adminUsername, string? note = null)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null) return false;

        order.Status = newStatus;
        order.UpdatedAt = DateTime.UtcNow;

        var history = new OrderStatusHistory
        {
            OrderId = order.Id,
            Status = newStatus,
            ChangedBy = adminUsername,
            Note = note,
            ChangedAt = DateTime.UtcNow
        };

        _context.OrderStatusHistory.Add(history);
        await _context.SaveChangesAsync();

        return true;
    }

    private static OrderResponseDto MapToDto(Order o)
    {
        return new OrderResponseDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            CustomerName = o.CustomerName,
            CustomerEmail = o.CustomerEmail,
            CustomerPhone = o.CustomerPhone,
            DeliveryAddress = o.DeliveryAddress,
            Status = o.Status,
            TotalAmount = o.TotalAmount,
            CreatedAt = o.CreatedAt,
            Items = o.Items.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity,
                LineTotal = i.LineTotal
            }).ToList()
        };
    }
}