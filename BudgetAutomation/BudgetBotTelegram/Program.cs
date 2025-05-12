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
using BudgetBotTelegram.Service;
using Microsoft.Extensions.Options;
using SharedLibrary.Settings;
using SharedLibrary.Validator;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

var isLocalDev = builder.Environment.IsDevelopment() ? "dev-" : "";

// Configure AWS Parameter Store
config.AddSystemsManager($"/{isLocalDev}{BudgetAutomationSettings.Configuration}/");

// Bind Bot configuration
builder.Services.Configure<BotSettings>(config.GetSection(BotSettings.Configuration));
builder.Services.AddSingleton<IValidateOptions<BotSettings>, BotSettingsValidator>();

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

builder.Services.AddTransient<SqsUpdateProcessor>();

#pragma warning disable IL2026
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi,
    options => { options.Serializer = new SourceGeneratorLambdaJsonSerializer<AppJsonSerializerContext>(); });
#pragma warning restore IL2026

var app = builder.Build();

app.Run();