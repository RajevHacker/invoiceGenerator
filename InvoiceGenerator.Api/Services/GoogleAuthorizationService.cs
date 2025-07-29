using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace InvoiceGenerator.Api.Services;

public class GoogleAuthorizationService
{
    private readonly string _credentialsFile;
    private readonly string _tokenFolder;

    public GoogleAuthorizationService(string credentialsFile = "credentials.json", string tokenFolder = "token_store")
    {
        _credentialsFile = credentialsFile;
        _tokenFolder = tokenFolder;
    }

    public async Task<UserCredential> GetUserCredentialAsync(string[] scopes)
    {
        using var stream = new FileStream(_credentialsFile, FileMode.Open, FileAccess.Read);

        var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            GoogleClientSecrets.FromStream(stream).Secrets,
            scopes,
            "user",
            CancellationToken.None,
            new FileDataStore(_tokenFolder, true));

        return credential;
    }

    public BaseClientService.Initializer GetServiceInitializer(UserCredential credential, string appName)
    {
        return new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = appName
        };
    }
}