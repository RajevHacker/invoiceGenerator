# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY InvoiceGenerator.Api.csproj ./
RUN dotnet restore

# Copy the rest of the source code
COPY . ./

# Build and publish the app
RUN dotnet publish -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy published files
COPY --from=build /app/publish .

# Copy the secure encrypted settings file into the container
COPY Config/secure.Development.appsettings.json ./Config/secure.Development.appsettings.json

# Copy the token_store directory (OAuth token cache)
COPY token_store ./token_store

# Set environment variables (can be overridden by docker-compose)
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ENCRYPTED_JSON_CONFIG=/app/Config/secure.Development.appsettings.json
ENV CONFIG_KEY=your_secret_key

# Expose default port
EXPOSE 8080

# Run the app
ENTRYPOINT ["dotnet", "InvoiceGenerator.Api.dll"]