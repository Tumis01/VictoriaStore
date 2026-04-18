using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VictoriaStore.Api.Data;
using VictoriaStore.Api.DTOs;

namespace VictoriaStore.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "SuperAdmin")]
public class CustomerController : ControllerBase
{
    private readonly AppDbContext _context;

    public CustomerController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
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
}
