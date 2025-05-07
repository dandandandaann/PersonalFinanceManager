using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Serialization.SystemTextJson;
using BudgetBotTelegram;
using BudgetBotTelegram.ApiClient;
using BudgetBotTelegram.AtoTypes;
using BudgetBotTelegram.Handler;
using BudgetBotTelegram.Handler.Command;
using BudgetBotTelegram.Interface;
using BudgetBotTelegram.Other;
using BudgetBotTelegram.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SharedLibrary.Settings;
using SharedLibrary.Validator;
using Telegram.Bot;
using Telegram.Bot.Types;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

LocalDev.CheckNgrok(builder);
var isLocalDev = builder.Environment.IsDevelopment() ? "dev-" : "";

// Serialize Options for AOT
builder.Services.ConfigureTelegramBot<Microsoft.AspNetCore.Http.Json.JsonOptions>(opt => opt.SerializerOptions);

// Bind Bot configuration

// Configure AWS Parameter Store
config.AddSystemsManager($"/{isLocalDev}{BudgetAutomationSettings.Configuration}/");

builder.Services.Configure<BotSettings>(config.GetSection(BotSettings.Configuration));
builder.Services.AddSingleton<IValidateOptions<BotSettings>, BotSettingsValidator>();

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

builder.Services.Configure<ExpenseLoggerApiClientSettings>(config.GetSection(ExpenseLoggerApiClientSettings.Configuration));
builder.Services.AddSingleton<IValidateOptions<ExpenseLoggerApiClientSettings>, ExpenseLoggerApiClientSettingsValidator>();
builder.Services.AddHttpClient<IExpenseLoggerApiClient, ExpenseLoggerApiClient>();

builder.Services.Configure<UserApiClientSettings>(config.GetSection(UserApiClientSettings.Configuration));
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
builder.Services.AddScoped<IUserManagerService, UserManagerService>();

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
app.MapPost("/webhook", ([FromQuery] string? token, [FromBody] Update update,
    [FromServices] IOptions<BotSettings> botOptions,
    [FromServices] ILogger<Program> logger,
    [FromServices] IServiceScopeFactory scopeFactory,
    CancellationToken cancellationToken = default) =>
{
    if (update == null!)
    {
        logger.LogError("Received null update payload.");
        return Task.FromResult(Results.BadRequest());
    }

    if (token != botOptions.Value.WebhookToken)
        return Task.FromResult(Results.Unauthorized());

    _ = Task.Run(async () =>
    {
        using var scope = scopeFactory.CreateScope();
        var scopedUpdateHandler = scope.ServiceProvider.GetRequiredService<IUpdateHandler>();

        await scopedUpdateHandler.HandleUpdateAsync(update, cancellationToken);
    }, cancellationToken);

    // Return OK to Telegram quickly before it retries
    return Task.FromResult(Results.Ok());
});

app.MapGet("/", () => "Telegram Bot Webhook receiver is running!");

app.MapPost("/telegram/message", async ([FromQuery] string? token, [FromBody] Update update,
    [FromServices] IOptions<BotSettings> botOptions,
    [FromServices] ILogger<Program> logger,
    [FromServices] IUpdateHandler updateHandler,
    CancellationToken cancellationToken = default) =>
{
    if (update == null!)
    {
        logger.LogError("Received null update payload.");
        return Task.FromResult(Results.BadRequest());
    }

    if (token != botOptions.Value.WebhookToken)
        return Task.FromResult(Results.Unauthorized());
    await updateHandler.HandleUpdateAsync(update, cancellationToken);

    // Return OK to Telegram quickly before it retries
    return Task.FromResult(Results.Ok());
});


app.Run();