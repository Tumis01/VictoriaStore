using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Added for FirstOrDefaultAsync
using System.Security.Claims;
using VictoriaStore.Api.Data;        // Added for AppDbContext
using VictoriaStore.Api.DTOs;
using VictoriaStore.Api.Services;

namespace VictoriaStore.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IEmailService _emailService;
    private readonly AppDbContext _context; // 1. Added Database Context

    // 2. Injected AppDbContext into the constructor
    public OrderController(IOrderService orderService, IEmailService emailService, AppDbContext context)
    {
        _orderService = orderService;
        _emailService = emailService;
        _context = context;
    }

    [HttpPost] // Public guest checkout for Phase 1
    public async Task<IActionResult> PlaceOrder([FromBody] CheckoutRequest request)
    {
        try
        {
            var order = await _orderService.PlaceOrderAsync(request);

            // Fire and forget email notifications
            _ = _emailService.SendEmailAsync(order.CustomerEmail, "Order Confirmation", $"Thank you! Your order {order.OrderNumber} is confirmed.");
            _ = _emailService.SendEmailAsync("admin@victoriajames.com", "New Order Received", $"Order {order.OrderNumber} was just placed.");

            return Ok(order);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetAllOrders()
    {
        return Ok(await _orderService.GetAllAdminAsync());
    }

    [HttpGet("{id:guid}")]
    [Authorize] // Admin or the Customer (if we implement logged-in customers)
    public async Task<IActionResult> GetOrder(Guid id)
    {
        var order = await _orderService.GetByIdAsync(id);
        if (order == null) return NotFound();
        return Ok(order);
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
    {
        var adminUser = User.FindFirstValue(ClaimTypes.Name) ?? "Admin";
        var result = await _orderService.UpdateOrderStatusAsync(id, request.Status, adminUser, request.Note);

        if (!result) return NotFound();

        // If status changes, notify customer (Placeholder logic)
        var order = await _orderService.GetByIdAsync(id);
        if (order != null)
        {
            _ = _emailService.SendEmailAsync(order.CustomerEmail, "Order Status Update", $"Your order {order.OrderNumber} is now {request.Status}.");
        }

        return Ok(new { Message = "Status updated successfully" });
    }

    // 3. The newly added Tracking Endpoint
    [HttpGet("track")]
    public async Task<IActionResult> TrackOrder([FromQuery] string orderNumber, [FromQuery] string phone)
    {
        if (string.IsNullOrWhiteSpace(orderNumber) || string.IsNullOrWhiteSpace(phone))
            return BadRequest("Order number and phone are required.");

        var order = await _context.Orders
            .FirstOrDefaultAsync(o =>
                o.OrderNumber.ToLower() == orderNumber.Trim().ToLower() &&
                o.CustomerPhone == phone.Trim());

        if (order == null)
            return NotFound("Order not found or phone number does not match.");

        var result = new OrderResponseDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerName = order.CustomerName,
            CustomerPhone = order.CustomerPhone,
            CustomerEmail = order.CustomerEmail,
            DeliveryAddress = order.DeliveryAddress,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt
        };

        return Ok(result);
    }
}

// Small DTO for the Patch request
public class UpdateOrderStatusRequest
{
    public required string Status { get; set; }
    public string? Note { get; set; }
}