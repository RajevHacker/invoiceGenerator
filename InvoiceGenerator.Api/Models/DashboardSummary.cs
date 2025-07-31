namespace InvoiceGenerator.Api.Models;

public class DashboardSummary
{
    public decimal TotalOutstanding { get; set; } 
    public decimal TotalPurchasePaymentPending { get; set; } 
    public List<DuePayment> TotOutstandingPayment { get; set; }
    public List<DuePayment> VendorPaymentDues { get; set; }
}

public class DuePayment
{
    public string Name { get; set; }
    public decimal Amount { get; set; } 
}