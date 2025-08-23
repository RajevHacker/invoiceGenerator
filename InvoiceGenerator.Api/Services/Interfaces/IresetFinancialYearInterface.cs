namespace InvoiceGenerator.Api.Services.Interfaces;

public interface IresetFinancialYearInterface
{
    public Task<string> resetFinancialYear(string partnerName, string financialYear);
}
