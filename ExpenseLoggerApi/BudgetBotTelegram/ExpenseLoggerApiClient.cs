using BudgetBotTelegram.Model;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BudgetBotTelegram
{
    public class ExpenseLoggerApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ExpenseLoggerApiOptions _options;

        public ExpenseLoggerApiClient(HttpClient httpClient, IOptions<ExpenseLoggerApiOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;

            // Configure HttpClient base address and default headers
            _httpClient.BaseAddress = new Uri(_options.Url);
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _options.Key);
        }

        public async Task LogExpenseAsync(string description, string amount, string category = "", CancellationToken cancellationToken = default)
        {
            // Construct the URL with query parameters
            var uriBuilder = new UriBuilder(_httpClient.BaseAddress!)
            {
                Path = "/log-expense",
                Query = $"description={Uri.EscapeDataString(description)}&amount={Uri.EscapeDataString(amount)}&category={Uri.EscapeDataString(category)}"
            };

            // Create the request message
            var request = new HttpRequestMessage(HttpMethod.Put, uriBuilder.Uri);

            // Send the request
            var response = await _httpClient.SendAsync(request, cancellationToken);

            // Ensure the request was successful
            response.EnsureSuccessStatusCode();

            // Optionally, you could read and process the response body if needed
            // var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        }
    }
} 