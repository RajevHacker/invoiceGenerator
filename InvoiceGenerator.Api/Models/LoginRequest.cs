namespace InvoiceGenerator.Api.Models;

public class LoginRequest
{
    public string partnerName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}