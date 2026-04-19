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
        var customerCount = await (from user in _context.Users
                                   join userRole in _context.UserRoles on user.Id equals userRole.UserId
                                   join role in _context.Roles on userRole.RoleId equals role.Id
                                   where role.Name == "Customer"
                                   select user.Id).CountAsync();
        var customerCountFromOrders = await _context.Orders.Select(o => o.CustomerEmail).Distinct().CountAsync();

        return Ok(new
        {
            TotalRevenue = totalRevenue,
            TotalOrders = totalOrders,
            TotalCustomers = customerCountFromOrders
        });
    }
}
