
using InvoiceGenerator.Api.Models;
using InvoiceGenerator.Api.Services.Interfaces;
using Microsoft.Extensions.Configuration;
namespace InvoiceGenerator.Api.Services;
public class PartnerConfigurationResolver : IPartnerConfigurationResolver
{
    private readonly IConfiguration _configuration;

    public PartnerConfigurationResolver(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public PartnerSettings GetSettings(string partnerName)
    {
        if (string.IsNullOrWhiteSpace(partnerName))
            throw new ArgumentException("Partner name cannot be null or empty.");

        var section = _configuration.GetSection($"Partners:{partnerName.ToUpper()}");

        if (!section.Exists())
            throw new KeyNotFoundException($"Partner '{partnerName}' not found in configuration.");

        return section.Get<PartnerSettings>();
    }
}