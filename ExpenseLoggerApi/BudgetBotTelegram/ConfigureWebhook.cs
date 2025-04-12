using BudgetBotTelegram.Settings;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BudgetBotTelegram;

public class ConfigureWebhook(
    ILogger<ConfigureWebhook> logger,
    IServiceProvider serviceProvider,
    IOptions<BotSettings> botOptions) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

        var webhookAddress = $"{botOptions.Value.HostAddress.TrimEnd('/')}/webhook";
        logger.LogInformation("Setting webhook: {WebhookAddress}", webhookAddress);

        try
        {
            await botClient.SetWebhook(
                url: webhookAddress,
                allowedUpdates: Array.Empty<UpdateType>(), // Handle all update types
                // dropPendingUpdates: true, // Consider dropping old updates on startup
                cancellationToken: cancellationToken);

            // You can optionally get webhook info to confirm
            var webhookInfo = await botClient.GetWebhookInfo(cancellationToken);
            logger.LogInformation("Webhook info: Pending updates = {PendingUpdates}, Last error date = {LastErrorDate}",
                webhookInfo.PendingUpdateCount, webhookInfo.LastErrorDate);

            try
            {
                await GetBotId();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error while getting the bot id.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to set webhook to {WebhookAddress}", webhookAddress);
            throw;
            // TODO: Consider implementing retry logic
        }

        async Task GetBotId()
        {
            User me = await botClient.GetMe(cancellationToken: cancellationToken);
            BotSettings.BotId = me.Id;
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