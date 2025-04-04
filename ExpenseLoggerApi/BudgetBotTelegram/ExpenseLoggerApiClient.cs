using BudgetBotTelegram.Model;
using Microsoft.Extensions.Options;

namespace BudgetBotTelegram
{
    public class ExpenseLoggerApiClient
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

        public async Task<Expense> LogExpenseAsync(string message, CancellationToken cancellationToken)
        {
            var messageSplit = message.Split(' ');
            if (messageSplit.Length < 3)
            {
                throw new ArgumentException("Invalid message format for logging expense.", nameof(message));
            }

            if (!decimal.TryParse(messageSplit[2], out var amount))
            {
                // Handle error: invalid amount format
                throw new ArgumentException("Invalid amount format.", nameof(message));
            }

            var expense = new Expense
            {
                Description = messageSplit[1],
                Amount = amount,
                Category = messageSplit.Length >= 4 ? messageSplit[3] : string.Empty
            };

            await LogExpenseAsync(expense, cancellationToken);

            return expense;
        }

        public async Task LogExpenseAsync(Expense expense, CancellationToken cancellationToken = default)
        {
            // Construct the URL with query parameters
            var uriBuilder = new UriBuilder(_httpClient.BaseAddress!)
            {
                Path = "/log-expense",
                Query =
                    $"description={Uri.EscapeDataString(expense.Description)}" +
                    $"&amount={expense.Amount}" +
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