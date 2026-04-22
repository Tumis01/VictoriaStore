using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VictoriaStore.Api.Data;
using VictoriaStore.Api.Models;

namespace VictoriaStore.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "SuperAdmin")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardController(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        // 1. Only sum revenue for orders that are successfully processed
        var totalRevenue = await _context.Orders
            .Where(o => o.Status == "Confirmed" || o.Status == "Shipped" || o.Status == "Delivered")
            .SumAsync(o => o.TotalAmount);

        // 2. Count total orders placed
        var totalOrders = await _context.Orders.CountAsync();

        // 3. CORRECT IDENTITY APPROACH: Fetch users in the "Customer" role
        var customers = await _userManager.GetUsersInRoleAsync("Customer");
        var totalCustomers = customers.Count;

        return Ok(new
        {
            TotalRevenue = totalRevenue,
            TotalOrders = totalOrders,
            TotalCustomers = totalCustomers
        });
    }
}
