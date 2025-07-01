namespace InvoiceGenerator.Api.Services.Interfaces
{
    public interface IInvoiceNumberGenerator
    {
        Task<string> GenerateNextInvoiceNumberAsync(string partnerName);
    }
}