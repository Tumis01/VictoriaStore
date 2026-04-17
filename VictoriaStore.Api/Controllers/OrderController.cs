using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VictoriaStore.Api.DTOs;
using VictoriaStore.Api.Services;

namespace VictoriaStore.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IEmailService _emailService;

    public OrderController(IOrderService orderService, IEmailService emailService)
    {
        _orderService = orderService;
        _emailService = emailService;
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
}

// Small DTO for the Patch request
public class UpdateOrderStatusRequest
{
    public required string Status { get; set; }
    public string? Note { get; set; }
}