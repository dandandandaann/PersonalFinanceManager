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
    Task<bool> ProcessUpdateAsync(Update update, ILambdaContext context);
}

public class TelegramUpdateProcessor(
    IAmazonSQS sqsClient,
    IOptions<TelegramListenerSettings> listenerOptions) : ITelegramUpdateProcessor
{
    private readonly TelegramListenerSettings _listenerSettings = listenerOptions.Value;

    public async Task<bool> ProcessUpdateAsync(Update update, ILambdaContext context)
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
                return false;
            }

            logger.LogInformation("Successfully queued Update to SQS. SQS Message ID: {SqsMessageId}",
                sendMessageResponse.MessageId);
            return true;
        }
        catch (AmazonSQSException sqsEx)
        {
            logger.LogError(sqsEx, "AWS SQS Exception while sending Update to queue.");
            return false;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Generic failure while processing webhook and sending Update to SQS.");
            return false;
        }
    }
}