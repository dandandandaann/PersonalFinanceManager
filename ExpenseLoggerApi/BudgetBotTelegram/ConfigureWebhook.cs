using BudgetBotTelegram.Model;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace BudgetBotTelegram;

public class ConfigureWebhook(
    ILogger<ConfigureWebhook> logger,
    IServiceProvider serviceProvider,
    Microsoft.Extensions.Options.IOptions<BotConfiguration> botOptions) : IHostedService
{
    private readonly BotConfiguration _botConfig = botOptions.Value;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

        var webhookAddress = $"{_botConfig.HostAddress.TrimEnd('/')}/webhook";
        logger.LogInformation("Setting webhook: {WebhookAddress}", webhookAddress);

        try
        {
            await botClient.SetWebhook(
                url: webhookAddress,
                allowedUpdates: Array.Empty<UpdateType>(), // Handle all update types
                // dropPendingUpdates: true, // Consider dropping old updates on startup
                cancellationToken: cancellationToken);
            logger.LogInformation("Webhook set successfully to {WebhookAddress}", webhookAddress);

            // You can optionally get webhook info to confirm
            var webhookInfo = await botClient.GetWebhookInfo(cancellationToken);
            logger.LogInformation("Webhook info: Pending updates = {PendingUpdates}, Last error date = {LastErrorDate}",
                webhookInfo.PendingUpdateCount, webhookInfo.LastErrorDate);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to set webhook to {WebhookAddress}", webhookAddress);
            throw;
            // TODO: Consider implementing retry logic
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // Clean up the webhook when the application stops
        using var scope = serviceProvider.CreateScope();
        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

        logger.LogInformation("Removing webhook");
        try
        {
            await botClient.DeleteWebhook(cancellationToken: cancellationToken);
            logger.LogInformation("Webhook removed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to remove webhook.");
        }
    }
}