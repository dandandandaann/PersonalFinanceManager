using Amazon.Lambda.Core;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
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
        Timeout = 10)]
    // [HttpApi(LambdaHttpMethod.Post, "/webhook?token={token}")]
    [HttpApi(LambdaHttpMethod.Post, "/webhook")]
    public string Webhook([FromQuery] string token, [FromBody] Telegram.Bot.Types.Update update,
        [FromServices] IOptions<BotSettings> botOptions, [FromServices] HttpClient httpClient, ILambdaContext context)
    {
        try
        {
            if (update == null!)
            {
                context.Logger.LogError("Received null update payload.");
                return "Task.FromResult(Results.BadRequest())";
            }

            if (token != botOptions.Value.WebhookToken)
                return "Task.FromResult(Results.Unauthorized())" + botOptions.Value.WebhookToken;

            // Configure HttpClient base address and default headers
            httpClient.BaseAddress = new Uri("http://localhost:6001"); // TODO: leave constant because it will be replaced by SQS
            var requestUri = new Uri(httpClient.BaseAddress, $"/telegram/message?token={botOptions.Value.WebhookToken}");
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

            // var content = JsonContent.Create(signupRequest, AppJsonSerializerContext.Default.UserSignupRequest);
            request.Content = new StringContent("update message will go here");

            var response = httpClient.SendAsync(request).GetAwaiter().GetResult(); // TODO: make it async

            if (!response.IsSuccessStatusCode)

                context.Logger.LogInformation($"Received a message.");
        }
        catch (Exception e)
        {
            return $"ToString: {e}"; // TODO: fix catch
        }
        return "Task.FromResult(Results.Ok());"; // TODO: fix return type (copy UserManager)
    }
}