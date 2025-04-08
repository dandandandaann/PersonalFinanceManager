using Amazon.Lambda.Serialization.SystemTextJson;
using BudgetBotTelegram;
using BudgetBotTelegram.ApiClient;
using BudgetBotTelegram.Handler;
using BudgetBotTelegram.Handler.Command;
using BudgetBotTelegram.Interface;
using BudgetBotTelegram.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureTelegramBot<Microsoft.AspNetCore.Http.Json.JsonOptions>(opt => opt.SerializerOptions);

// 1. Bind Bot configuration
var botConfigurationSection = builder.Configuration.GetSection(BotConfiguration.Configuration);
builder.Services.Configure<BotConfiguration>(botConfigurationSection);
// Register typed HttpClient directly (optional, but good practice if you need custom HttpClient settings)
builder.Services.AddHttpClient("telegram_bot_client")
    .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
    {
        var botConfig = sp.GetRequiredService<IOptions<BotConfiguration>>().Value;
        TelegramBotClientOptions options = new(botConfig.Token);
        return new TelegramBotClient(options, httpClient);
    });
    // .SetHandlerLifetime(TimeSpan.FromMinutes(5)); // Configure lifetime as needed

// 2. Bind Expense Logger API configuration
var expenseLoggerApiSection = builder.Configuration.GetSection(ExpenseLoggerApiOptions.Configuration);
builder.Services.Configure<ExpenseLoggerApiOptions>(expenseLoggerApiSection);

// 3. Register Expense Logger API Client
builder.Services.AddHttpClient<IExpenseLoggerApiClient, ExpenseLoggerApiClient>();

// Register the background service that sets the webhook
builder.Services.AddHostedService<ConfigureWebhook>();

builder.Services.AddSingleton<ISenderGateway, SenderGateway>();

// Register handlers
builder.Services.AddScoped<UpdateHandler>();
builder.Services.AddScoped<IMessageHandler, MessageHandler>();
builder.Services.AddScoped<ITextMessageHandler, TextMessageHandler>();
builder.Services.AddScoped<ICommandHandler, CommandHandler>();

// Register commands
builder.Services.AddScoped<ILogCommand, LogCommand>();

#pragma warning disable IL2026
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi, options =>
{
    options.Serializer = new SourceGeneratorLambdaJsonSerializer<AppJsonSerializerContext>();
});
#pragma warning restore IL2026

var app = builder.Build();

// Configure the webhook endpoint
app.MapPost("/webhook", async ([FromBody] Update update, [FromServices] UpdateHandler updateHandler, CancellationToken cancellationToken) =>
{
    await updateHandler.HandleUpdateAsync(update, cancellationToken);
    return Results.Ok(); // Always return OK to Telegram quickly
});

// Optional: Map a root endpoint for basic checks
app.MapGet("/", () => "Telegram Bot Webhook receiver is running!");

// TODO: when not in development only allow calls from telegram

app.Run();