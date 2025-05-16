using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using BudgetBotTelegram.AtoTypes;
using BudgetBotTelegram.Interface;
using SharedLibrary.Telegram;

namespace BudgetBotTelegram;

public class SqsUpdateProcessor(IServiceProvider serviceProvider, ILogger<SqsUpdateProcessor> logger)
{
    // This is the method AWS Lambda will call when triggered by SQS
    // It must match the handler string you configure in AWS Lambda settings.
    // Format: YourAssemblyName::YourNamespace.SqsUpdateProcessor::ProcessSqsMessagesAsync
    public async Task ProcessSqsMessagesAsync(SQSEvent sqsEvent, ILambdaContext context)
    {
        if (sqsEvent?.Records == null)
        {
            logger.LogWarning("SQS event or records are null.");
            return;
        }

        logger.LogInformation("Processing {Count} SQS messages.", sqsEvent.Records.Count);

        foreach (var message in sqsEvent.Records)
        {
            // Create a new DI scope for each message to ensure service lifetimes are correct
            // (especially for scoped services like DbContexts or your IUpdateHandler if it's scoped).
            using var scope = serviceProvider.CreateScope();
            var scopedUpdateHandler = scope.ServiceProvider.GetRequiredService<IUpdateHandler>();
            // Resolve a logger from the scope if you want more granular logging tied to the message processing
            try
            {
                logger.LogInformation("Processing SQS Message ID: {MessageId}", message.MessageId);
                await HandleSqsRecordAsync(message, scopedUpdateHandler, logger, GenerateCancellationToken());
                logger.LogInformation("Successfully processed SQS Message ID: {MessageId}", message.MessageId);
                // If the handler completes without exception, Lambda automatically deletes the message
                // from the SQS queue (if ReportBatchItemFailures is not used or if this item isn't marked as failed).
            }
            catch (JsonException jsonEx)
            {
                // Critical error: If we can't deserialize, we can't process.
                // This message will likely end up in a DLQ after retries.
                logger.LogError(jsonEx, "JSON Deserialization failed for SQS Message ID {MessageId}. Body: {Body}", message.MessageId, message.Body);
                // Re-throw to mark this specific message processing as failed for SQS batch item failure reporting (if enabled)
                // or to let Lambda retry the whole batch.
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing SQS Message ID {MessageId}. Message will return to queue for potential retry.", message.MessageId);
                // Re-throw to mark this specific message processing as failed for SQS batch item failure reporting (if enabled)
                // or to let Lambda retry the whole batch.
                throw;
            }
        }
        logger.LogInformation("Finished processing SQS message batch.");
        CancellationToken GenerateCancellationToken() // TODO: share this method because it's replicated in other services
        {
            var gracefulStopTimeLimit = TimeSpan.FromSeconds(1);
            return new CancellationTokenSource(context.RemainingTime.Subtract(gracefulStopTimeLimit)).Token;
        }
    }

    private async Task HandleSqsRecordAsync(SQSEvent.SQSMessage sqsMessage, IUpdateHandler updateHandler, ILogger logger, CancellationToken cancellationToken)
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
            logger.LogError(ex, "Failed to deserialize Telegram Update from SQS message body. Body: {Body}", sqsMessage.Body);
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

        // Now, use your existing IUpdateHandler to process the update.
        // Note: The 'token' validation and 'Results.Ok()' are not relevant here as this isn't an HTTP request.
        // The IUpdateHandler should contain the core logic to process the message content.
        await updateHandler.HandleUpdateAsync(update, cancellationToken);
    }
}