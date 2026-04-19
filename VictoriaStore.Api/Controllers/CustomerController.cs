using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VictoriaStore.Api.Data;
using VictoriaStore.Api.DTOs;
using VictoriaStore.Api.Models;

namespace VictoriaStore.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // 1. Allow ANY authenticated user to access this controller by default
public class CustomerController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    // 2. Inject the UserManager so we can look up the logged-in customer
    public CustomerController(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // 3. Move the SuperAdmin restriction here, so only admins can view all customers
    [HttpGet]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetCustomers()
    {
        var customers = await _context.Orders
            .GroupBy(o => new { o.CustomerEmail, o.CustomerName, o.CustomerPhone })
            .Select(g => new
            {
                Name = g.Key.CustomerName,
                Email = g.Key.CustomerEmail,
                Phone = g.Key.CustomerPhone,
                OrderCount = g.Count(),
                TotalSpent = g.Sum(o => o.TotalAmount),
                LastOrderDate = g.Max(o => o.CreatedAt)
            })
            .ToListAsync();

        return Ok(customers);
    }

    // 4. NEW: Endpoint to get the logged-in customer's profile & order history
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        // Extract the user ID from the JWT token
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

        // Fetch the user from the database
        var user = await _userManager.FindByIdAsync(userIdStr);
        if (user == null) return NotFound("User not found.");

        // Fetch the user's orders using their email address
        var orders = await _context.Orders
            .Where(o => o.CustomerEmail == user.Email)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        // Map it to the CustomerDto expected by the frontend
        var profile = new CustomerDto
        {
            Id = user.Id,
            Name = user.FullName,
            Email = user.Email!,
            Phone = user.PhoneNumber ?? "",
            JoinedDate = user.CreatedAt,
            OrdersCount = orders.Count,
            TotalSpent = orders.Sum(o => o.TotalAmount),
            RecentOrders = orders.Select(o => new CustomerOrderHistoryDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                CreatedAt = o.CreatedAt,
                Status = o.Status,
                TotalAmount = o.TotalAmount
            }).ToList()
        };

        return Ok(profile);
    }

    // 5. NEW: Endpoint to update the logged-in customer's profile
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

        var user = await _userManager.FindByIdAsync(userIdStr);
        if (user == null) return NotFound("User not found.");

        // Update the user's details
        user.FullName = request.Name;
        user.PhoneNumber = request.Phone;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new { Message = "Profile updated successfully." });
    }
}

// DTO to handle incoming update requests
public class UpdateProfileRequest
{
    public required string Name { get; set; }
    public required string Phone { get; set; }
}