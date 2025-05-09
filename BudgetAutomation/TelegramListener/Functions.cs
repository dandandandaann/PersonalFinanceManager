using System.Net;
using Amazon.Lambda.Core;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Options;
using SharedLibrary.Settings;
using SharedLibrary.Dto;

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
    public Functions() { }

    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Get, "/")]
    public string Default()
    {
        return "Telegram Bot Webhook receiver is running!";
    }

    [LambdaFunction(
        Policies = "AWSLambdaBasicExecutionRole, " +
                   "arn:aws:iam::795287297286:policy/Configurations_Read" +
                   "arn:aws:iam::795287297286:policy/SQS_CRUD",
        MemorySize = 128,
        Timeout = 15)]
    [HttpApi(LambdaHttpMethod.Post, "/webhook")]
    public async Task<APIGatewayHttpApiV2ProxyResponse> Webhook([FromQuery] string token,
        APIGatewayHttpApiV2ProxyRequest lambdaRequest,
        [FromServices] IOptions<TelegramListenerSettings> listenerOptions,
        [FromServices] IOptions<TelegramBotSettings> telegramBotOptions,
        ILambdaContext context, [FromServices] IAmazonSQS sqsClient)
    {

        var logger = context.Logger;

        if (token != telegramBotOptions.Value.WebhookToken)
        {
            logger.LogWarning("Unauthorized webhook attempt with invalid token: {SuppliedToken}", token);
            return ApiResponse.Unauthorized("Unauthorized");
        }

        var requestBody = lambdaRequest.Body;

        if (string.IsNullOrEmpty(requestBody))
        {
            logger.LogError("Received empty payload body from Telegram.");
            return ApiResponse.BadRequest("Request body cannot be empty.");
        }

        try
        {
            var queueUrl = listenerOptions.Value.TelegramUpdateQueue;
            var sendMessageRequest = new SendMessageRequest
            {
                QueueUrl = queueUrl,
                MessageBody = requestBody
            };

            logger.LogInformation("Attempting to send the Telegram Update to SQS queue: {QueueUrl}",
                 queueUrl.Substring(queueUrl.LastIndexOf('/') + 1));

            // Send the message to SQS
            var sendMessageResponse = await sqsClient.SendMessageAsync(sendMessageRequest, GenerateCancellationToken());

            if (sendMessageResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                logger.LogError(
                    "SQS did not accept message for Update. SQS Response Status: {SqsStatus}, SQS Message ID (if any): {SqsMessageId}",
                    sendMessageResponse.HttpStatusCode, sendMessageResponse.MessageId);
                return ApiResponse.InternalServerError("Failed to queue message.");
            }

            logger.LogInformation("Successfully queued Update to SQS. SQS Message ID: {SqsMessageId}",
                sendMessageResponse.MessageId);

            // Return OK to Telegram immediately AFTER successfully queuing
            return ApiResponse.Ok();
        }
        catch (AmazonSQSException sqsEx)
        {
            logger.LogError(sqsEx, "AWS SQS Exception while sending Update to queue.");
            return ApiResponse.InternalServerError("Error communicating with SQS.");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Generic failure while processing webhook and sending Update to SQS.");
            return ApiResponse.InternalServerError("An unexpected error occurred.");
        }

        CancellationToken GenerateCancellationToken()
        {
            var gracefulStopTimeLimit = TimeSpan.FromSeconds(1);
            return new CancellationTokenSource(context.RemainingTime.Subtract(gracefulStopTimeLimit)).Token;
        }
    }
}