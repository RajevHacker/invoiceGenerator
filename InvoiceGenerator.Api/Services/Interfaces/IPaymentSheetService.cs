using InvoiceGenerator.Api.Models;
namespace InvoiceGenerator.Api.Services.Interfaces;
public interface IPaymentSheetService
{
    Task AppendPaymentAsync(string partnerName, PaymentEntry payment);
}