namespace InvoiceGenerator.Api.Models;
public class PaymentEntry
{
    public string CustomerName { get; set; }
    public string Date { get; set; }
    public string BankName { get; set; }
    public string Amount { get; set; }
}
