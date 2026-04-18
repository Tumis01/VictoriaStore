using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VictoriaStore.Api.Data;

namespace VictoriaStore.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "SuperAdmin")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var totalRevenue = await _context.Orders.Where(o => o.Status != "Cancelled").SumAsync(o => o.TotalAmount);
        var totalOrders = await _context.Orders.CountAsync();
        var totalCustomers = await _context.Users.Where(u => u.Role == "Customer").CountAsync(); 
        // Note: Assuming there is a Role or distinguishing factor for customers, 
        // if not we will just use a mock or count of unique emails in orders.
        var customerCountFromOrders = await _context.Orders.Select(o => o.CustomerEmail).Distinct().CountAsync();

        return Ok(new
        {
            TotalRevenue = totalRevenue,
            TotalOrders = totalOrders,
            TotalCustomers = customerCountFromOrders
        });
    }
}
