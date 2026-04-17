using Microsoft.AspNetCore.Identity;

namespace VictoriaStore.Api.Models;

public class ApplicationUser : IdentityUser<Guid>
{
    public required string FullName { get; set; }
    public string? DefaultDeliveryAddress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}