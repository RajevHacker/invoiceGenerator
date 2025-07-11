using System.Text;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using InvoiceGenerator.Api.Models;
using InvoiceGenerator.Api.Services;
using InvoiceGenerator.Api.Services.Interfaces;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var env = builder.Environment;
var key = Environment.GetEnvironmentVariable("CONFIG_KEY");

// Decrypt JSON config first, then add it to Configuration before accessing any config values
// var decryptedJson = BlowFishDecryption.JsonDecryptor.DecryptFile($"secure.{env.EnvironmentName}.appsettings.json", key);


var basePath = AppContext.BaseDirectory; // or Directory.GetCurrentDirectory()
var encryptedJson = Environment.GetEnvironmentVariable("ENCRYPTED_JSON_CONFIG");
if (string.IsNullOrEmpty(encryptedJson))
    throw new Exception("ENCRYPTED_JSON_CONFIG is not set.");

var decryptedJson = BlowFishDecryption.JsonDecryptor.DecryptFile(encryptedJson, key); // Add this overload if needed
var decryptedStream = new MemoryStream(Encoding.UTF8.GetBytes(decryptedJson));
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
    .AddJsonStream(decryptedStream);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Now bind JwtSettings after the decrypted config is loaded
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

// Validate JwtSettings to avoid null reference exceptions
if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.SecretKey))
{
    throw new Exception("JWT settings are not configured properly.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Register other settings now
builder.Services.Configure<SheetSettings>(builder.Configuration.GetSection("SheetSettings"));
builder.Services.Configure<GoogleDriveSettings>(builder.Configuration.GetSection("GoogleDriveSettings"));
builder.Services.Configure<GoogleApiSettings>(builder.Configuration.GetSection("GoogleApi"));

// Register your services
builder.Services.AddHttpClient();
builder.Services.AddSingleton(sp =>
{
    var factory = sp.GetRequiredService<GoogleServiceFactory>();
    return factory.CreateSheetsService();
}); // ToDO: I am not sure why we have this kind
builder.Services.AddScoped<DriveService>(provider =>
{
    var factory = provider.GetRequiredService<GoogleServiceFactory>();
    return factory.CreateDriveService();
});

builder.Services.AddSingleton<GoogleServiceFactory>();
builder.Services.AddTransient<IGoogleSheetsService, GoogleSheetsService>();
builder.Services.AddTransient<IPaymentSheetService, PaymentSheetService>();
builder.Services.AddTransient<IBillHistorySheetService, BillHistorySheetService>();
builder.Services.AddTransient<ICustomerInfoService, CustomerInfoService>();
builder.Services.AddTransient<IProductService, ProductService>();
builder.Services.AddTransient<IInvoiceSheetWriter, InvoiceSheetWriter>();
builder.Services.AddTransient<InvoiceService>();
builder.Services.AddSingleton<IInvoiceNumberGenerator, InvoiceNumberGenerator>();
builder.Services.AddTransient<IFileDownloader, FileDownloader>();
builder.Services.AddScoped<IGoogleSheetsExporter, GoogleSheetsExporter>();
builder.Services.AddScoped<IDriveUploader, GoogleDriveUploader>();
builder.Services.AddScoped<IInvoicePdfExporter, InvoicePdfExporter>();
builder.Services.AddTransient<IGetInvoiceSummary, GetInvoiceSummaryService>();
builder.Services.AddTransient<IInvoiceCancellationService, InvoiceCancellationService>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddSingleton<IPartnerConfigurationResolver, PartnerConfigurationResolver>();
builder.Services.AddTransient<ISearchValueService, searchValueService>();
builder.Services.AddTransient<IGetBillHistortyInfo, GetBillHistortyInfoService>();
builder.Services.AddTransient<IpurchaseOrderEntryService, purchaseOrderEntryService>();
builder.Services.AddTransient<IAddPurchaseConsumerRecord, addPurchaseConsumerRecord>();

builder.Services.AddAuthorization();
builder.Services.AddControllers();

var app = builder.Build();
app.UseCors("AllowAll");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
// CONFIG_KEY=your_secret_key ASPNETCORE_ENVIRONMENT=Development dotnet run
// docker-compose up --build

// ASPNETCORE_ENVIRONMENT=Development ENCRYPTED_JSON_CONFIG="Config/secure.Development.appsettings.json" CONFIG_KEY="your_secret_key" dotnet run