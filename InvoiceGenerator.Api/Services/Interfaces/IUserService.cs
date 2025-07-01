namespace InvoiceGenerator.Api.Services.Interfaces;

public interface IUserService
{
    Task<bool> ValidateUserAsync(string partnerName, string username, string password);
}