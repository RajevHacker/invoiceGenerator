namespace InvoiceGenerator.Api.Models;
public class productsItem
{
    public int SNo { get; set; }
    public string ProductName { get; set; } = "";
    public string HSN { get; set; } = "";
    public int Qty { get; set; }
    public decimal Price { get; set; }
}
public class InvoiceRequest
{
    public string? invoiceNumber { get; set; }
    public string Name { get; set; } = "";
    public DateTime CurrentDate { get; set; }
    public int NoOfBales { get; set; }
    public string Transport { get; set; } = "";
    public List<productsItem> Items { get; set; } = new();
}

public class InvoiceResponse
{
    public string InvoiceNumber { get; set; } = "";
    public string FileUrl { get; set; } = "";
}