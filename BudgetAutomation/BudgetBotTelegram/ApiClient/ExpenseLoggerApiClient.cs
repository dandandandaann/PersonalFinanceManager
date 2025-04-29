using BudgetBotTelegram.AtoTypes;
using BudgetBotTelegram.Interface;
using Microsoft.Extensions.Options;
using SharedLibrary;
using SharedLibrary.Settings;

namespace BudgetBotTelegram.ApiClient;

public class ExpenseLoggerApiClient : IExpenseLoggerApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExpenseLoggerApiClient> _logger;

    public ExpenseLoggerApiClient(HttpClient httpClient, IOptions<ExpenseLoggerApiSettings> options,
        ILogger<ExpenseLoggerApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        if (options.Value is not { } apiOptions ||
            string.IsNullOrEmpty(apiOptions.Url) ||
            string.IsNullOrEmpty(apiOptions.Key))
            throw new ArgumentNullException(nameof(apiOptions));

        // Configure HttpClient base address and default headers
        _httpClient.BaseAddress = new Uri(apiOptions.Url);
        _httpClient.DefaultRequestHeaders.Add("x-api-key", apiOptions.Key);
    }

    public async Task<Expense> LogExpenseAsync(Expense expense, CancellationToken cancellationToken = default)
    {
        // Construct the URL with query parameters
        var uriBuilder = new UriBuilder(_httpClient.BaseAddress!)
        {
            Path = "/log-expense",
            Query =
                $"description={Uri.EscapeDataString(expense.Description)}" +
                $"&amount={Uri.EscapeDataString(expense.Amount)}" +
                $"&category={Uri.EscapeDataString(expense.Category)}"
        };

        // Create the request message
        var request = new HttpRequestMessage(HttpMethod.Put, uriBuilder.Uri);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        response.EnsureSuccessStatusCode();

        _logger.LogInformation(
            $"Log expense request sent for {expense.Description}. Response: {response.StatusCode}");

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
        else if
            (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            _logger.LogError("Request successful, no content returned.");
        }
        else
        {
            _logger.LogError($"Received successful status code {response.StatusCode} but content was null or not JSON.");
        }

        return new Expense();
    }
}