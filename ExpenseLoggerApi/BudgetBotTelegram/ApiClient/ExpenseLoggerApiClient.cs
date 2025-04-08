using BudgetBotTelegram.Model;
using Microsoft.Extensions.Options;

namespace BudgetBotTelegram.ApiClient
{
    public interface IExpenseLoggerApiClient
    {
        Task LogExpenseAsync(Expense expense, CancellationToken cancellationToken = default);
    }

    public class ExpenseLoggerApiClient : IExpenseLoggerApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ExpenseLoggerApiClient> _logger;

        public ExpenseLoggerApiClient(HttpClient httpClient, IOptions<ExpenseLoggerApiOptions> options,
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

        public async Task LogExpenseAsync(Expense expense, CancellationToken cancellationToken = default)
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

            // Send the request
            var response = await _httpClient.SendAsync(request, cancellationToken);

            // Ensure the request was successful
            response.EnsureSuccessStatusCode(); // TODO: better error handling and return bool result?

            _logger.LogInformation($"Log expense request sent for {expense.Description}. Response: {response.StatusCode}");
        }
    }
}