namespace InvoiceGenerator.Api.Models;

public class PaymentReport
{
    public string CustomerName { get; set; }
    public DateOnly? Date { get; set; }
    public Decimal Amount { get; set; }
}
