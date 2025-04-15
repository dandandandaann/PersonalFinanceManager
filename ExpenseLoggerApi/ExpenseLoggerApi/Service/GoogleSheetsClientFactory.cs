using System.Text.Json;
using ExpenseLoggerApi.Model;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;

namespace ExpenseLoggerApi.Service;

public class GoogleSheetsClientFactory(ILogger<GoogleSheetsClientFactory> logger)
{
    private readonly string[] _scopes = [SheetsService.Scope.Spreadsheets];
    private const string ApplicationName = "Expense Logger";

    public SheetsService CreateSheetsService(string credentialsJson)
    {
        if (string.IsNullOrWhiteSpace(credentialsJson))
        {
            logger.LogError("Credentials JSON cannot be null or empty.");
            throw new ArgumentNullException(nameof(credentialsJson));
        }

        try
        {
            // Consider using FromJson directly if the structure matches Google's expected format
            logger.LogDebug("Deserializing credentials JSON.");
            var credentialParameters =
                JsonSerializer.Deserialize(credentialsJson, JsonCredentialContext.Default.JsonCredentialParameters);

            if (credentialParameters == null)
            {
                logger.LogError("Failed to deserialize credentials JSON.");
                throw new InvalidOperationException("Could not deserialize credentials JSON.");
            }

            logger.LogDebug("Creating Google Credential from parameters.");
            var credential = GoogleCredential.FromJsonParameters(credentialParameters)
                .CreateScoped(_scopes);

            logger.LogDebug("Initializing SheetsService.");
            var service = new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            return service;
        }
        catch (JsonException jsonEx)
        {
            logger.LogError(jsonEx, "Error deserializing Google credentials JSON.");
            throw new InvalidOperationException("Failed to parse credentials JSON.", jsonEx);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while creating the Google Sheets service client.");
            throw;
        }
    }
}