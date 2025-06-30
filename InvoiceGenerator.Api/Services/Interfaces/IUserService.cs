namespace InvoiceGenerator.Api.Services.Interfaces;

public interface IUserService
{
    Task<bool> ValidateUserAsync(string username, string password);
}