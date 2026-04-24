using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VictoriaStore.API.Models;
using VictoriaStore.Api.Data;

namespace VictoriaStores.API.Controllers;

[ApiController]
[Route("api/store-settings")]
public class StoreSettingsController : ControllerBase
{
    private readonly AppDbContext _context;

    public StoreSettingsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<StoreSettings>> Get()
    {
        var settings = await _context.StoreSettings.FirstOrDefaultAsync();

        if (settings == null)
        {
            settings = new StoreSettings();
            _context.StoreSettings.Add(settings);
            await _context.SaveChangesAsync();
        }

        return Ok(settings);
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPut]
    public async Task<ActionResult<StoreSettings>> Update([FromBody] StoreSettings request)
    {
        var settings = await _context.StoreSettings.FirstOrDefaultAsync();

        if (settings == null)
        {
            settings = new StoreSettings();
            _context.StoreSettings.Add(settings);
        }

        settings.StoreName = request.StoreName;
        settings.SupportEmail = request.SupportEmail;
        settings.WhatsAppNumber = request.WhatsAppNumber;
        settings.Address = request.Address;
        settings.BankName = request.BankName;
        settings.AccountNumber = request.AccountNumber;
        settings.AccountName = request.AccountName;
        settings.NewOrderEmail = request.NewOrderEmail;
        settings.LowStockAlert = request.LowStockAlert;
        settings.SeoTitle = request.SeoTitle;
        settings.SeoDescription = request.SeoDescription;
        settings.SeoKeywords = request.SeoKeywords;

        await _context.SaveChangesAsync();

        return Ok(settings);
    }
}
