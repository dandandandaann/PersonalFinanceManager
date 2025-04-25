using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Serialization.SystemTextJson;
using BudgetBotTelegram;
using BudgetBotTelegram.ApiClient;
using BudgetBotTelegram.Handler;
using BudgetBotTelegram.Handler.Command;
using BudgetBotTelegram.Interface;
using BudgetBotTelegram.Other;
using BudgetBotTelegram.Service;
using BudgetBotTelegram.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using AppJsonSerializerContext = BudgetBotTelegram.AtoTypes.AppJsonSerializerContext;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

LocalDev.CheckNgrok(builder);

// Serialize Options for AOT
builder.Services.ConfigureTelegramBot<Microsoft.AspNetCore.Http.Json.JsonOptions>(opt => opt.SerializerOptions);

// Bind Bot configuration
builder.Services.Configure<BotSettings>(config.GetSection(BotSettings.Configuration));
// Register typed HttpClient directly (optional, but good practice if you need custom HttpClient settings)
builder.Services.AddHttpClient("telegram_bot_client")
    .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
    {
        var botConfig = sp.GetRequiredService<IOptions<BotSettings>>().Value;
        TelegramBotClientOptions options = new(botConfig.Token);
        return new TelegramBotClient(options, httpClient);
    });
// .SetHandlerLifetime(TimeSpan.FromMinutes(5)); // Configure lifetime as needed

builder.Services.Configure<ExpenseLoggerApiSettings>(config.GetSection(ExpenseLoggerApiSettings.Configuration));
builder.Services.AddHttpClient<IExpenseLoggerApiClient, ExpenseLoggerApiClient>();

// Register UserApiClient
// TODO: Add UserManagerApiSettings configuration if needed (e.g., for base URL)
builder.Services.AddHttpClient<IUserApiClient, UserApiClient>();

builder.Services.AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient(RegionEndpoint.USEast2));

builder.Services.AddScoped<IDynamoDBContext>(sp =>
{
    var client = sp.GetRequiredService<IAmazonDynamoDB>();
    var contextBuilder = new DynamoDBContextBuilder()
        .WithDynamoDBClient(() => client);
    // contextBuilder = contextBuilder.WithTableNamePrefix("DEV_");
    return contextBuilder.Build();
});

builder.Services.AddScoped<IChatStateService, ChatStateService>();

builder.Services.AddHostedService<ConfigureWebhook>();
builder.Services.AddSingleton<ISenderGateway, SenderGateway>();

// Register handlers
builder.Services.AddScoped<IUpdateHandler, UpdateHandler>();
builder.Services.AddScoped<IMessageHandler, MessageHandler>();
builder.Services.AddScoped<ITextMessageHandler, TextMessageHandler>();
builder.Services.AddScoped<ICommandHandler, CommandHandler>();

// Register commands
builder.Services.AddScoped<ILogCommand, LogCommand>();
builder.Services.AddScoped<ICancelCommand, CancelCommand>();
builder.Services.AddScoped<ISignupCommand, SignupCommand>();

#pragma warning disable IL2026
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi,
    options => { options.Serializer = new SourceGeneratorLambdaJsonSerializer<AppJsonSerializerContext>(); });
#pragma warning restore IL2026

var app = builder.Build();

// Configure the webhook endpoint
app.MapPost("/webhook",
    async ([FromQuery] string? token, [FromBody] Update update,
        [FromServices] IUpdateHandler updateHandler, [FromServices] IOptions<BotSettings> botOptions, CancellationToken cancellationToken) =>
    {
        if (token != botOptions.Value.WebhookToken)
            return Results.Unauthorized();

        await updateHandler.HandleUpdateAsync(update, cancellationToken);
        return Results.Ok(); // Always return OK to Telegram quickly
    });

app.MapGet("/", () => "Telegram Bot Webhook receiver is running!");

app.Run();
