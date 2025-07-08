namespace InvoiceGenerator.Api.Services.Interfaces;

public interface IInvoiceCancellationService
{
    Task<bool> CancelInvoiceAsync(string invoiceNumber, string partnerName);
}