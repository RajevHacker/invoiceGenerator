namespace InvoiceGenerator.Api.Models;

public class GoogleApiSettings
{
    public string ProjectId { get; set; }
    public string ClientEmail { get; set; }
    public string PrivateKey { get; set; }
    public string PrivateKeyId { get; set; }
    public string ClientId { get; set; }
    public string TokenUri { get; set; }
    public List<string> Scopes { get; set; }
}