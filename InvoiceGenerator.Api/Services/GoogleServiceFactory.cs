using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Sheets.v4;
using Google.Apis.Services;
using InvoiceGenerator.Api.Models;
using Microsoft.Extensions.Options;
using System.Text;

namespace InvoiceGenerator.Api.Services;

public class GoogleServiceFactory
{
    private readonly GoogleApiSettings _settings;

    public GoogleServiceFactory(IOptions<GoogleApiSettings> settings)
    {
        _settings = settings.Value;
    }

    public SheetsService CreateSheetsService()
    {
        var credential = GetCredential();
        return new SheetsService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "Invoice Generator API"
        });
    }
    public GoogleCredential GetCredential()
    {
        var json = new StringBuilder()
            .AppendLine("{")
            .AppendLine($"\"type\": \"service_account\",")
            .AppendLine($"\"project_id\": \"{_settings.ProjectId}\",")
            .AppendLine($"\"private_key_id\": \"{_settings.PrivateKeyId}\",")
            .AppendLine($"\"private_key\": \"{_settings.PrivateKey.Replace("\n", "\\n")}\",")
            .AppendLine($"\"client_email\": \"{_settings.ClientEmail}\",")
            .AppendLine($"\"client_id\": \"{_settings.ClientId}\",")
            .AppendLine($"\"auth_uri\": \"https://accounts.google.com/o/oauth2/auth\",")
            .AppendLine($"\"token_uri\": \"{_settings.TokenUri}\",")
            .AppendLine($"\"auth_provider_x509_cert_url\": \"https://www.googleapis.com/oauth2/v1/certs\",")
            .AppendLine($"\"client_x509_cert_url\": \"https://www.googleapis.com/robot/v1/metadata/x509/{Uri.EscapeDataString(_settings.ClientEmail)}\"")
            .AppendLine("}")
            .ToString();

        return GoogleCredential.FromJson(json)
            .CreateScoped(_settings.Scopes);
    }
}