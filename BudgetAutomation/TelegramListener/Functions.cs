using Amazon.Lambda.Core;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.APIGatewayEvents;
using Telegram.Bot.Types;
using TelegramListener.Service;
using ApiResponse = SharedLibrary.Utility.ApiResponse;

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

    [LambdaFunction(
        Policies = "AWSLambdaBasicExecutionRole",
        MemorySize = 128,
        Timeout = 10)]
    [HttpApi(LambdaHttpMethod.Get, "/")]
    public string Default()
    {
        return "Telegram Bot Webhook receiver is running!";
    }

    [LambdaFunction(
        Policies = "AWSLambdaBasicExecutionRole, " +
                   "arn:aws:iam::795287297286:policy/Configurations_Read",
        MemorySize = 128,
        Timeout = 10)]
    [HttpApi(LambdaHttpMethod.Get, "/setupWebhook")]
    public async Task<APIGatewayHttpApiV2ProxyResponse> SetupWebhook(
        [FromServices] ConfigureWebhook configureWebhook, ILambdaContext context)
    {
        try
        {
            await configureWebhook.StartAsync(context.Logger);
        }
        catch (Exception e)
        {
            context.Logger.LogError(e.ToString());
            return ApiResponse.InternalServerError();
        }

        return ApiResponse.Ok("Webhook setup complete");
    }

    [LambdaFunction(
        Policies = "AWSLambdaBasicExecutionRole, " +
                   "arn:aws:iam::795287297286:policy/Configurations_Read, " +
                   "arn:aws:iam::795287297286:policy/SQS_CRUD",
        MemorySize = 128,
        Timeout = 15)]
    [HttpApi(LambdaHttpMethod.Post, "/webhook")]
    public async Task<APIGatewayHttpApiV2ProxyResponse> Webhook(
        [FromQuery] string token, [FromBody] Update update, ILambdaContext context,
        [FromServices] IAuthenticationService authenticator,
        [FromServices] ITelegramUpdateProcessor updateProcessor)
    {
        var logger = context.Logger;

        if (!authenticator.IsAuthorized(token))
        {
            logger.LogWarning("Unauthorized webhook attempt with invalid token: {SuppliedToken}", token);
            return ApiResponse.Unauthorized("Unauthorized");
        }

        if (update == null!)
        {
            logger.LogError("Received null update payload.");
            return ApiResponse.BadRequest();
        }

        try
        {
            var result = await updateProcessor.ProcessUpdateAsync(update, context);

            if (!result.IsSuccess)
            {
                logger.LogError("Failed to process update: {Status} - {ErrorMessage}", result.Status, result.ErrorMessage);
                return ApiResponse.InternalServerError(result.ErrorMessage ?? "Failed to process update.");
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unexpected error invoking TelegramUpdateProcessor.");
            return ApiResponse.InternalServerError("An unexpected error occurred.");
        }

        // Return OK to Telegram immediately AFTER successfully queuing
        return ApiResponse.Ok();
    }
}