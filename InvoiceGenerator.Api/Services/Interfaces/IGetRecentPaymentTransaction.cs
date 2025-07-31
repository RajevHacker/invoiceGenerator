using InvoiceGenerator.Api.Models;

namespace InvoiceGenerator.Api.Services.Interfaces;

public interface IGetRecentPaymentTransaction
{
    public Task<IList<PaymentReport>> getPaymentReport(string partnerName, string paymentType);
}
