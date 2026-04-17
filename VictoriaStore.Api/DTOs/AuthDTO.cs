namespace VictoriaStore.Api.DTOs;

public class RegisterRequest
{
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Password { get; set; }

    // We will pass "SuperAdmin" or "Customer" from the frontend
    public required string Role { get; set; }
}

public class LoginRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}

public class AuthResponse
{
    public required string Token { get; set; }
    public required string Email { get; set; }
    public required string FullName { get; set; }
    public required IList<string> Roles { get; set; }
}