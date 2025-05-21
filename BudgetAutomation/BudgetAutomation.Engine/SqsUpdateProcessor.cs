using System.Text.Json;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using BudgetAutomation.Engine.AtoTypes;
using BudgetAutomation.Engine.Interface;
using SharedLibrary.Telegram;
using SharedLibrary.Utility;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace BudgetAutomation.Engine;

public class SqsUpdateProcessor(IUpdateHandler updateHandler, ILogger<SqsUpdateProcessor> logger)
{
    [LambdaFunction(
        Policies = "AWSLambdaBasicExecutionRole, " +
                   "arn:aws:iam::795287297286:policy/Configurations_Read, " +
                   "arn:aws:iam::795287297286:policy/SQS_CRUD, " +
                   "arn:aws:iam::795287297286:policy/DB_chat_state_CRUD",
        MemorySize = 128,
        Timeout = 15)]
    public async Task ProcessSqsMessagesAsync(SQSEvent sqsEvent, ILambdaContext context)
    {
        if (sqsEvent.Records == null)
        {
            logger.LogWarning("SQS event or records are null.");
            return;
        }

        logger.LogInformation("Processing {Count} SQS messages.", sqsEvent.Records.Count);

        foreach (var message in sqsEvent.Records)
        {
            try
            {
                logger.LogInformation("Processing SQS Message ID: {MessageId}", message.MessageId);
                await HandleSqsRecordAsync(message, CancellationTokenProvider.GetCancellationToken(context.RemainingTime));
                logger.LogInformation("Successfully processed SQS Message ID: {MessageId}", message.MessageId);
            }
            catch (JsonException jsonEx)
            {
                // Critical error: If we can't deserialize, we can't process.
                logger.LogError(jsonEx, "JSON Deserialization failed for SQS Message ID {MessageId}. Body: {Body}",
                    message.MessageId, message.Body);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing SQS Message ID {MessageId}. " +
                                    "Message will return to queue for potential retry.", message.MessageId);
                throw;
            }
        }
        logger.LogInformation("Finished processing SQS message batch.");
    }

    private async Task HandleSqsRecordAsync(SQSEvent.SQSMessage sqsMessage, CancellationToken cancellationToken)
    {
        logger.LogDebug("SQS Message Body: {Body}", sqsMessage.Body);

        // Deserialize the Telegram Update object from the SQS message body
        Update? update;
        try
        {
            update = JsonSerializer.Deserialize(sqsMessage.Body, AppTelegramJsonSerializerContext.Default.Update);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Failed to deserialize {ResponseObject} from SQS message body. Body: {Body}",
                typeof(Telegram.Bot.Types.Update), sqsMessage.Body);
            throw; // Re-throw to be caught by the outer handler and potentially go to DLQ
        }

        if (update == null)
        {
            // This case should ideally be caught by the JsonException above if deserialization fails
            logger.LogError("Deserialized Telegram Update is null for SQS Message ID {MessageId}. Body: {Body}", sqsMessage.MessageId, sqsMessage.Body);
            // Consider this a non-recoverable error for this message.
            throw new InvalidOperationException($"Deserialized Telegram Update is null. SQS Message ID: {sqsMessage.MessageId}");
        }

        logger.LogInformation("Handling deserialized Telegram Update ID {UpdateId} from SQS Message ID {SqsMessageId}", update.Id, sqsMessage.MessageId);

        await updateHandler.HandleUpdateAsync(update, cancellationToken);
    }
}