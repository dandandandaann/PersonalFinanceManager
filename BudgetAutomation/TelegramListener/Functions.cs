using Amazon.Lambda.Core;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Extensions.Options;
using SharedLibrary.Settings;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace TelegramListener;

/// <summary>
/// A collection of Lambda functions.
/// </summary>
public class Functions
{
    /// <summary>
    /// Default constructor.
    /// </summary>
    public Functions()
    {
    }

    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Get, "/")]
    public string Default()
    {
        return "Telegram Bot Webhook receiver is running!";
    }

    [LambdaFunction(
        Policies = "AWSLambdaBasicExecutionRole, " +
                   "arn:aws:iam::795287297286:policy/Configurations_Read", // TODO: add SQS policy
        MemorySize = 128,
        Timeout = 15)]
    // [HttpApi(LambdaHttpMethod.Post, "/webhook?token={token}")]
    [HttpApi(LambdaHttpMethod.Post, "/webhook")]
    public string Webhook([FromQuery] string token, APIGatewayHttpApiV2ProxyRequest lambdaRequest,
        [FromServices] IOptions<BotSettings> botOptions, [FromServices] HttpClient httpClient, ILambdaContext context)
    {
        try
        {
            // No need to deserialize here, this function just forwards the payload
            var requestBody = lambdaRequest.Body;

            if (string.IsNullOrEmpty(requestBody))
            {
                context.Logger.LogError("Received empty payload body.");
                return "Task.FromResult(Results.BadRequest())";
            }


            if (token != botOptions.Value.WebhookToken)
                return "Task.FromResult(Results.Unauthorized())" + botOptions.Value.WebhookToken;


            // ---

            // context.Logger.LogInformation("Starting request to BudgetBot get.");
            // // Configure HttpClient base address and default headers
            // httpClient.BaseAddress = new Uri("host.docker.internal:6001/"); // TODO: leave constant because it will be replaced by SQS
            // var request2 = new HttpRequestMessage(HttpMethod.Get, httpClient.BaseAddress);
            //
            // request2.Content = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");
            //
            // var response2 = httpClient.SendAsync(request2).GetAwaiter().GetResult(); // TODO: make it async
            //
            // if (!response2.IsSuccessStatusCode)
            //     context.Logger.LogInformation($"Request to BudgetBot get was successful. Response code: {response2.StatusCode}.");
            //
            // context.Logger.LogInformation("Finished BudgetBot get request.");
            // return "finished BudgetBot get request.";
            // ---
            // Configure HttpClient base address and default headers

            context.Logger.LogInformation("Starting BudgetBot request.");

            httpClient.BaseAddress = new Uri("http://host.docker.internal:6001"); // TODO: leave constant because it will be replaced by SQS
            var requestUri = new Uri(httpClient.BaseAddress, $"/telegram/message?token={botOptions.Value.WebhookToken}");
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

            request.Content = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");

            var response = httpClient.SendAsync(request).GetAwaiter().GetResult(); // TODO: make it async

            if (!response.IsSuccessStatusCode)
                context.Logger.LogInformation($"Received a message.");
        }
        catch (Exception e)
        {
            return e.ToString(); // TODO: fix catch
        }
        return "Task.FromResult(Results.Ok());"; // TODO: fix return type (copy UserManager)
    }
}