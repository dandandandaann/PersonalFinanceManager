using Amazon.Lambda.SQSEvents;
using Amazon.Lambda.TestUtilities;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Options;
using SharedLibrary.Settings;

namespace BudgetAutomation.Engine.Misc;

public class SqsListenerForTestingService(
    ILogger<SqsListenerForTestingService> logger,
    IAmazonSQS sqsClient,
    IOptions<TelegramListenerSettings> options,
    IServiceScopeFactory scopeFactory)
    : BackgroundService
{
    private readonly TelegramListenerSettings _options = options.Value;
    // _options.MaxNumberOfMessages = Math.Clamp(_options.MaxNumberOfMessages, 1, 10);
    // _options.WaitTimeSeconds = Math.Clamp(_options.WaitTimeSeconds, 0, 20); // 0 disables long polling

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("SQS Listener Service starting. Polling queue: {QueueUrl}", _options.TelegramUpdateQueue);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                logger.LogDebug("Polling SQS queue for messages...");

                var receiveRequest = new ReceiveMessageRequest
                {
                    QueueUrl = _options.TelegramUpdateQueue,
                    // MaxNumberOfMessages = _options.MaxNumberOfMessages,
                    // WaitTimeSeconds = _options.WaitTimeSeconds,
                    MessageAttributeNames = ["All"] // Get all message attributes
                };

                var messageReceiver = await sqsClient.ReceiveMessageAsync(receiveRequest, cancellationToken);

                if (messageReceiver.Messages is { Count: > 0 })
                {
                    logger.LogInformation("Received {Count} messages from SQS.", messageReceiver.Messages.Count);

                    // Create the SQSEvent object expected by the processor
                    var sqsEventMessages = ConvertToSqsMessages(messageReceiver.Messages);

                    // Create a scope to resolve scoped services for the processor invocation
                    using var scope = scopeFactory.CreateScope();
                    var processor = scope.ServiceProvider.GetRequiredService<SqsUpdateProcessor>();


                    var context = new TestLambdaContext // Use a test context locally
                    {
                        // Set any relevant properties if needed, e.g., RemainingTime
                        RemainingTime = TimeSpan.FromMinutes(10),
                        Logger = new TestLambdaLogger() // Basic logger for the context
                    };
                    var messageIndex = 0;
                    try
                    {
                        for (messageIndex = 0; messageIndex < sqsEventMessages.Count; messageIndex++)
                        {
                            // Call the processor method
                            await processor.ProcessSqsMessagesAsync(new SQSEvent { Records = [sqsEventMessages[messageIndex]] }, context);

                            // If the processor completed WITHOUT exception, delete the messages from SQS.
                            logger.LogInformation(
                                "Message {Current} of {Total} processed successfully by SqsUpdateProcessor. Deleting message.", messageIndex + 1,
                                sqsEventMessages.Count);
                            await DeleteMessageAsync(sqsEventMessages[messageIndex].ReceiptHandle, cancellationToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        // If ProcessSqsMessagesAsync throws, log the error.
                        // This might prevent other messages from completing, since they are being executed one-by-one
                        // Messages will NOT be deleted and will become visible again in SQS after timeout.
                        // TODO: figure out which message failed and add its ID to the log
                        logger.LogError(ex, "SqsUpdateProcessor failed to process message batch. " +
                            "{Current} of {Total} messages were processed, but message ID '{FailedMessageID}' failed. " +
                            "Messages will return to queue.",
                            messageIndex, sqsEventMessages.Count, sqsEventMessages[messageIndex].MessageId);
                    }
                }
                else
                {
                    logger.LogDebug("No messages received from SQS.");
                }
            }
            catch (AmazonSQSException sqsEx)
            {
                logger.LogError(sqsEx, "SQS Exception during polling or processing.");
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                logger.LogInformation("SQS Listener Service stopping.");
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception in SQS Listener Service loop.");
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
        }

        logger.LogInformation("SQS Listener Service stopped.");
    }

    private List<SQSEvent.SQSMessage> ConvertToSqsMessages(List<Message> messages)
    {
        // Initialize the list for the SQSEvent records
        var eventRecords = new List<SQSEvent.SQSMessage>(messages.Count);

        // Pre-calculate parts that are the same for all messages in the batch if possible
        // Ensure _sqsClient and _options are accessible instance fields/properties
        string region = sqsClient.Config.RegionEndpoint?.SystemName ?? "local";
        string eventSourceArn =
            $"arn:aws:sqs:{region}:000000000000:{ExtractQueueName(_options.TelegramUpdateQueue)}";

        // Loop through each message received from SQS SDK
        foreach (var sdkMessage in messages)
        {
            // Create the corresponding Lambda event message object
            var lambdaMessage = new SQSEvent.SQSMessage
            {
                // --- Direct property mappings ---
                MessageId = sdkMessage.MessageId,
                ReceiptHandle = sdkMessage.ReceiptHandle,
                Body = sdkMessage.Body,
                Md5OfBody = sdkMessage.MD5OfBody,
                Md5OfMessageAttributes = sdkMessage.MD5OfMessageAttributes,
                Attributes = sdkMessage.Attributes, // Dictionary<string, string> maps directly

                MessageAttributes = sdkMessage.MessageAttributes != null
                    ? sdkMessage.MessageAttributes.ToDictionary(
                        kvp => kvp.Key,
                        kvp => new SQSEvent.MessageAttribute
                        {
                            StringValue = kvp.Value.StringValue,
                            BinaryValue = kvp.Value.BinaryValue,
                            DataType = kvp.Value.DataType,
                            StringListValues = kvp.Value.StringListValues,
                            BinaryListValues = kvp.Value.BinaryListValues
                        })
                    : new Dictionary<string, SQSEvent.MessageAttribute>(), // Assign empty dictionary if null
                // --- Common event source info ---
                EventSourceArn = eventSourceArn, // Use pre-calculated value
                EventSource = "aws:sqs",
                AwsRegion = region // Use pre-calculated value
            };

            // Add the converted message to our list
            eventRecords.Add(lambdaMessage);
        }

        return eventRecords;
    }

    private string ExtractQueueName(string queueUrl) // Helper to make fake ARN look better
    {
        try
        {
            return new Uri(queueUrl).Segments.LastOrDefault() ?? "unknown-queue";
        }
        catch
        {
            return "unknown-queue";
        }
    }

    private async Task DeleteMessageAsync(string receiptHandle, CancellationToken cancellationToken)
    {
        try
        {
            var deleteRequest = new DeleteMessageRequest
            {
                QueueUrl = _options.TelegramUpdateQueue,
                ReceiptHandle = receiptHandle
            };
            await sqsClient.DeleteMessageAsync(deleteRequest, cancellationToken);
            logger.LogDebug("Deleted message with receipt handle: {ReceiptHandle}", receiptHandle);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete message with receipt handle: {ReceiptHandle}", receiptHandle);
            throw;
        }
    }
}