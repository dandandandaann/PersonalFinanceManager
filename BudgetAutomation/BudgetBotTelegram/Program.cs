using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.SQS;
using BudgetBotTelegram;
using BudgetBotTelegram.ApiClient;
using BudgetBotTelegram.AtoTypes;
using BudgetBotTelegram.Handler;
using BudgetBotTelegram.Handler.Command;
using BudgetBotTelegram.Interface;
using BudgetBotTelegram.Other;
using BudgetBotTelegram.Service;
using Microsoft.Extensions.Options;
using SharedLibrary.Settings;
using SharedLibrary.Validator;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var config = builder.Configuration;

var isLocalDev = builder.Environment.IsDevelopment() ? "dev-" : "";

// Configure AWS Parameter Store ---
config.AddSystemsManager($"/{isLocalDev}{BudgetAutomationSettings.Configuration}/");

// Bind Bot configuration ---
services.Configure<TelegramBotSettings>(config.GetSection(TelegramBotSettings.Configuration));
services.AddSingleton<IValidateOptions<TelegramBotSettings>, TelegramBotSettingsValidator>();

builder.Services.Configure<TelegramBotSettings>(config.GetSection(TelegramBotSettings.Configuration));
// Register typed HttpClient directly (optional, but good practice if you need custom HttpClient settings)
builder.Services.AddHttpClient("telegram_bot_client")
    .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
    {
        var botConfig = sp.GetRequiredService<IOptions<TelegramBotSettings>>().Value;
        TelegramBotClientOptions options = new(botConfig.Token);
        return new TelegramBotClient(options, httpClient);
    });
// .SetHandlerLifetime(TimeSpan.FromMinutes(5)); // Configure lifetime as needed

services.Configure<ExpenseLoggerApiClientSettings>(config.GetSection(ExpenseLoggerApiClientSettings.Configuration));
services.AddSingleton<IValidateOptions<ExpenseLoggerApiClientSettings>, ExpenseLoggerApiClientSettingsValidator>();
services.AddHttpClient<IExpenseLoggerApiClient, ExpenseLoggerApiClient>();

services.Configure<UserApiClientSettings>(config.GetSection(UserApiClientSettings.Configuration));
services.AddHttpClient<IUserApiClient, UserApiClient>();

// Register AWS Services ---
services.AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient(RegionEndpoint.USEast2));
services.AddScoped<IDynamoDBContext>(sp =>
{
    var client = sp.GetRequiredService<IAmazonDynamoDB>();
    var contextBuilder = new DynamoDBContextBuilder()
        .WithDynamoDBClient(() => client);
    // contextBuilder = contextBuilder.WithTableNamePrefix("DEV_");
    return contextBuilder.Build();
});
builder.Services.AddAWSService<IAmazonSQS>();

services.AddScoped<IChatStateService, ChatStateService>();
services.AddScoped<IUserManagerService, UserManagerService>();

services.AddSingleton<ISenderGateway, SenderGateway>();

// Register handlers ---
services.AddScoped<IUpdateHandler, UpdateHandler>();
services.AddScoped<IMessageHandler, MessageHandler>();
services.AddScoped<ITextMessageHandler, TextMessageHandler>();
services.AddScoped<ICommandHandler, CommandHandler>();

// Register commands ---
services.AddScoped<ICommand, LogCommand>();
services.AddScoped<ICommand, CancelCommand>();
services.AddScoped<ICommand, SignupCommand>();

services.AddTransient<SqsUpdateProcessor>();

#pragma warning disable IL2026
services.AddAWSLambdaHosting(LambdaEventSource.HttpApi,
    options => { options.Serializer = new SourceGeneratorLambdaJsonSerializer<AppJsonSerializerContext>(); });
#pragma warning restore IL2026

#if DEBUG
    // Bind test configurations
    services.Configure<TelegramListenerSettings>(config.GetSection(TelegramListenerSettings.Configuration));
    services.AddSingleton<IValidateOptions<TelegramListenerSettings>, TelegramListenerSettingsValidator>();
    services.AddHostedService<SqsListenerForTestingService>();
#endif

var app = builder.Build();

app.Run();