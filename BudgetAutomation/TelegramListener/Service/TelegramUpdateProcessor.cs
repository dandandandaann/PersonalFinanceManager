using SharedLibrary.Utility;
using TelegramListener.Mapper;
using System.Net;
using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Options;
using SharedLibrary.Settings;
using Telegram.Bot.Types;
using TelegramListener.AotTypes;

namespace TelegramListener.Service;

public interface ITelegramUpdateProcessor
{
    Task<ProcessUpdateResult> ProcessUpdateAsync(Update update, ILambdaContext context);
}

public enum ProcessUpdateStatus
{
    Success,
    SqsError,
    GenericError
}

public class ProcessUpdateResult
{
    public ProcessUpdateStatus Status { get; }
    public string? ErrorMessage { get; }
    public string? SqsMessageId { get; }

    private ProcessUpdateResult(ProcessUpdateStatus status, string? errorMessage = null, string? sqsMessageId = null)
    {
        Status = status;
        ErrorMessage = errorMessage;
        SqsMessageId = sqsMessageId;
    }

    public static ProcessUpdateResult Success(string? sqsMessageId = null) => new(ProcessUpdateStatus.Success, sqsMessageId: sqsMessageId);
    public static ProcessUpdateResult SqsError(string message) => new(ProcessUpdateStatus.SqsError, message);
    public static ProcessUpdateResult GenericError(string message) => new(ProcessUpdateStatus.GenericError, message);

    public bool IsSuccess => Status == ProcessUpdateStatus.Success;
}

public class TelegramUpdateProcessor(
    IAmazonSQS sqsClient,
    IOptions<TelegramListenerSettings> listenerOptions) : ITelegramUpdateProcessor
{
    private readonly TelegramListenerSettings _listenerSettings = listenerOptions.Value;

    public async Task<ProcessUpdateResult> ProcessUpdateAsync(Update update, ILambdaContext context)
    {
        var logger = context.Logger;
        var simplifiedUpdate = TelegramUpdateMapper.ConvertUpdate(update);

        try
        {
            var queueUrl = _listenerSettings.TelegramUpdateQueue;
            var messageGroupId = simplifiedUpdate.Message?.Chat.Id.ToString() ?? "default";
            var sendMessageRequest = new SendMessageRequest
            {
                QueueUrl = queueUrl,
                MessageBody = JsonSerializer.Serialize(simplifiedUpdate, AppJsonSerializerContext.Default.Update),
                MessageGroupId = messageGroupId,
                MessageDeduplicationId = simplifiedUpdate.Id.ToString()
            };

            logger.LogInformation("Attempting to send the Telegram Update to SQS queue: {QueueUrl}",
                queueUrl.Substring(queueUrl.LastIndexOf('/') + 1));

            var cancellationToken = CancellationTokenProvider.GetCancellationToken(context.RemainingTime);
            var sendMessageResponse = await sqsClient.SendMessageAsync(sendMessageRequest, cancellationToken);

            if (sendMessageResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                logger.LogError(
                    "SQS did not accept message for Update. SQS Response Status: {SqsStatus}, SQS Message ID (if any): {SqsMessageId}",
                    sendMessageResponse.HttpStatusCode, sendMessageResponse.MessageId);
                return ProcessUpdateResult.SqsError("Failed to queue message.");
            }

            logger.LogInformation("Successfully queued Update to SQS. SQS Message ID: {SqsMessageId}",
                sendMessageResponse.MessageId);
            return ProcessUpdateResult.Success(sendMessageResponse.MessageId);
        }
        catch (AmazonSQSException sqsEx)
        {
            logger.LogError(sqsEx, "AWS SQS Exception while sending Update to queue.");
            return ProcessUpdateResult.SqsError("Error communicating with SQS.");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Generic failure while processing webhook and sending Update to SQS.");
            return ProcessUpdateResult.GenericError("An unexpected error occurred.");
        }
    }
}