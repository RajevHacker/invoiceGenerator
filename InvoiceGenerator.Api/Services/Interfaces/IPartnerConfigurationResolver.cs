using InvoiceGenerator.Api.Models;

namespace InvoiceGenerator.Api.Services.Interfaces;

public interface IPartnerConfigurationResolver
{
    PartnerSettings GetSettings(string partnerName);
}