using System.Net.Http.Json;
using BudgetAutomation.Engine.AtoTypes;
using BudgetAutomation.Engine.Interface;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using SharedLibrary.Dto;
using SharedLibrary.Enum;
using SharedLibrary.Model;
using SharedLibrary.Settings;

namespace BudgetAutomation.Engine.ApiClient;

public class ExpenseLoggerApiClient : IExpenseLoggerApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExpenseLoggerApiClient> _logger;

    public ExpenseLoggerApiClient(HttpClient httpClient,
        IOptions<ExpenseLoggerApiClientSettings> options,
        ILogger<ExpenseLoggerApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        // Configure HttpClient base address and default headers
        _httpClient.BaseAddress = new Uri(options.Value.Url);
        _httpClient.DefaultRequestHeaders.Add("x-api-key", options.Value.Key);
    }

    public async Task<LogExpenseResponse> LogExpenseAsync(
        string spreadsheetId, Expense expense, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending request /log-expense for '{Description}'.", expense.Description);

        var endpointUri = new Uri(_httpClient.BaseAddress!, "log-expense");

        var logExpenseRequest = new LogExpenseRequest
        {
            SpreadsheetId = spreadsheetId,
            Description = expense.Description,
            Amount = expense.Amount,
            Category = expense.Category
        };
        var content = JsonContent.Create(logExpenseRequest, AppJsonSerializerContext.Default.LogExpenseRequest);

        var request = new HttpRequestMessage( HttpMethod.Put, endpointUri);
        request.Content = content;

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to log expense in Spreadsheet {SpreadsheetId}", spreadsheetId);
            return new LogExpenseResponse { Success = false };
        }

        _logger.LogInformation("Log expense request sent. Response code: {StatusCode}", response.StatusCode);

        if (response.Content is { Headers.ContentType.MediaType: "application/json" })
        {
            var responseExpense = await response.Content.ReadFromJsonAsync(
                AppJsonSerializerContext.Default.LogExpenseResponse,
                cancellationToken);

            if (responseExpense is not null)
            {
                return responseExpense;
            }

            _logger.LogError("Received successful status code but failed to deserialize {ResponseObject} from response body.",
                typeof(LogExpenseResponse));
        }
        else if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            _logger.LogError("Request successful, no content returned.");
        }
        else
        {
            _logger.LogError("Received status code {StatusCode}, but content was null or not JSON.", response.StatusCode);
        }

        return new LogExpenseResponse { Success = false };
    }

    public async Task<SpreadsheetValidationResponse> ValidateSpreadsheet(
        string spreadSheetId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending request to /validate-spreadsheet");

        var endpointUri = new Uri(_httpClient.BaseAddress!, "validate-spreadsheet");

        var validationRequest = new SpreadsheetValidationRequest { SpreadsheetId = spreadSheetId };
        var content = JsonContent.Create(validationRequest, AppJsonSerializerContext.Default.SpreadsheetValidationRequest);

        var request = new HttpRequestMessage(HttpMethod.Post, endpointUri);
        request.Content = content;

        var httpResponse = await _httpClient.SendAsync(request, cancellationToken);

        if (!httpResponse.IsSuccessStatusCode)
        {
            _logger.LogError("Validation failed with status code: {StatusCode}", httpResponse.StatusCode);
            return new SpreadsheetValidationResponse
            {
                Success = false,
                Message = $"Validation failed with status code: {httpResponse.StatusCode}"
            };
        }

        var result = await httpResponse.Content.ReadFromJsonAsync(
            AppJsonSerializerContext.Default.SpreadsheetValidationResponse, cancellationToken);

        if (result == null)
        {
            _logger.LogError("Validation response is null or malformed.");
            return new SpreadsheetValidationResponse
            {
                Success = false,
                Message = "Validation response was null or malformed."
            };
        }

        _logger.LogInformation("Validation result: {Message}", result.Message);
        return result;
    }

    public async Task<RemoveExpenseResponse> RemoveLastExpenseAsync(
        string spreadsheetId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending request /undo");

        var endpointUri = new Uri(_httpClient.BaseAddress!, "undo");

        var queryParams = new Dictionary<string, string?>
        {
            ["spreadsheetId"] = spreadsheetId
        };

        var request = new HttpRequestMessage
        (
            HttpMethod.Delete,
            QueryHelpers.AddQueryString(endpointUri.ToString(), queryParams)
        );

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to remove expense in Spreadsheet {SpreadsheetId}. Response code: {StatusCode}",
                spreadsheetId, response.StatusCode);
            return new RemoveExpenseResponse { Success = false };
        }

        _logger.LogInformation("Remove expense request sent. Response code: {StatusCode}", response.StatusCode);

        var responseExpense = await response.Content.ReadFromJsonAsync(
            AppJsonSerializerContext.Default.RemoveExpenseResponse,
            cancellationToken);

        if (responseExpense == null)
        {
            _logger.LogError("Received successful status code but failed to deserialize {ResponseObject} from response body.",
                typeof(RemoveExpenseResponse));

            return new RemoveExpenseResponse { Success = false };
        }

        return responseExpense;
    }

    public async Task<ExpenseResponse> GetLastExpenseAsync(string spreadsheetId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sending request /lastitem");

        var endpointUri = new Uri(_httpClient.BaseAddress!, "lastitem");

        var queryParams = new Dictionary<string, string?>
        {
            ["spreadsheetId"] = spreadsheetId
        };

        var request = new HttpRequestMessage
        (
            HttpMethod.Get,
            QueryHelpers.AddQueryString(endpointUri.ToString(), queryParams)
        );

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to retrieve expense in Spreadsheet {SpreadsheetId}. Response code: {StatusCode}",
                spreadsheetId, response.StatusCode);
            return new ExpenseResponse { Success = false };
        }

        _logger.LogInformation("Get last expense request sent. Response code: {StatusCode}", response.StatusCode);

        var responseExpense = await response.Content.ReadFromJsonAsync(
            AppJsonSerializerContext.Default.ExpenseResponse,
            cancellationToken);

        if (responseExpense == null)
        {
            _logger.LogError("Received successful status code but failed to deserialize {ResponseObject} from response body.",
                typeof(ExpenseResponse));

            return new ExpenseResponse { Success = false };
        }

        return responseExpense;
    }
}