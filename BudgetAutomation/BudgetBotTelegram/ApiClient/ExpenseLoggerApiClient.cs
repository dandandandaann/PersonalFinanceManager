using BudgetBotTelegram.AtoTypes;
using BudgetBotTelegram.Interface;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using SharedLibrary.Model;
using SharedLibrary.Settings;

namespace BudgetBotTelegram.ApiClient;

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

    public async Task<Expense> LogExpenseAsync(
        string spreadsheetId, Expense expense, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending request /log-expense for '{Description}'.", expense.Description);

        var endpointUri = new Uri(_httpClient.BaseAddress!, "log-expense");

        var queryParams = new Dictionary<string, string?>
        {
            ["spreadsheetId"] = spreadsheetId,
            ["description"] = expense.Description,
            ["amount"] = expense.Amount,
            ["category"] = expense.Category
        };

        var request = new HttpRequestMessage
        (
            HttpMethod.Put,
            QueryHelpers.AddQueryString(endpointUri.ToString(), queryParams)
        );

        var response = await _httpClient.SendAsync(request, cancellationToken);

        response.EnsureSuccessStatusCode();

        _logger.LogInformation("Log expense request sent. Response code: {StatusCode}", response.StatusCode);

        if (response.Content is { Headers.ContentType.MediaType: "application/json" })
        {
            var responseExpense = await response.Content.ReadFromJsonAsync(
                AppJsonSerializerContext.Default.LogExpenseResponse,
                cancellationToken);

            if (responseExpense?.expense != null)
            {
                return responseExpense.expense;
            }

            _logger.LogError(
                "Received successful status code but failed to deserialize Expense object from response body.");
        }
        else if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            _logger.LogError("Request successful, no content returned.");
        }
        else
        {
            _logger.LogError("Received status code {StatusCode}, but content was null or not JSON.", response.StatusCode);
        }

        return new Expense();
    }
}