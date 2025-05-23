using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Serialization.SystemTextJson;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Dto;
using SharedLibrary.Lambda.LocalDevelopment;
using SharedLibrary.Settings;
using UserManagerApi;
using UserManagerApi.AotTypes;
using UserManagerApi.Service;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var config = builder.Configuration;

var devPrefix = builder.Environment.IsDevelopment() ? "dev-" : "";

// Configure AWS Parameter Store
config.AddSystemsManager($"/{devPrefix}{BudgetAutomationSettings.Configuration}/");

// #pragma warning disable IL2026
services.AddAWSLambdaHosting(LambdaEventSource.HttpApi,
    options => { options.Serializer = new SourceGeneratorLambdaJsonSerializer<AppJsonSerializerContext>(); });
// #pragma warning restore IL2026

services.AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient(RegionEndpoint.USEast2));

services.AddScoped<IDynamoDBContext>(sp =>
{
    var client = sp.GetRequiredService<IAmazonDynamoDB>();
    var contextBuilder = new DynamoDBContextBuilder()
        .WithDynamoDBClient(() => client);
    // contextBuilder = contextBuilder.WithTableNamePrefix("DEV_");
    return contextBuilder.Build();
});

// Register services
services.AddScoped<IUserService, UserService>();

// Register Functions
services.AddScoped<Functions>();

var app = builder.Build();

// SignupUserAsync
app.MapPost("/user/signup", async (
    [FromBody] UserSignupRequest request,
    Functions functions, // DI will provide this
    ILoggerFactory loggerFactory // To create a logger for the context
) =>
{
    // Create a logger instance for the Lambda context, naming it after the function class
    var lambdaContextLogger = loggerFactory.CreateLogger(typeof(Functions).FullName ?? "UserManagerApi.Functions");
    var localContext = new LocalLambdaContext(lambdaContextLogger, nameof(functions.SignupUserAsync));

    var lambdaResponse = await functions.SignupUserAsync(request, localContext);
    return LambdaResponseMapper.ToMinimalApiResult(lambdaResponse);
});

// UpdateUserConfigurationAsync
app.MapPut("/user/{userId}/configuration", async (
    string userId,
    [FromBody] UserConfigurationUpdateRequest request,
    Functions functions,
    ILoggerFactory loggerFactory
) =>
{
    var lambdaContextLogger = loggerFactory.CreateLogger(typeof(Functions).FullName ?? "UserManagerApi.Functions");
    var localContext = new LocalLambdaContext(lambdaContextLogger, nameof(functions.UpdateUserConfigurationAsync));

    var lambdaResponse = await functions.UpdateUserConfigurationAsync(userId, request, localContext);
    return LambdaResponseMapper.ToMinimalApiResult(lambdaResponse);
});


// GetUserByTelegramIdAsync
app.MapGet("/user/telegram/{telegramId}", async (
    string telegramId,
    Functions functions,
    ILoggerFactory loggerFactory
) =>
{
    var lambdaContextLogger = loggerFactory.CreateLogger(typeof(Functions).FullName ?? "UserManagerApi.Functions");
    var localContext = new LocalLambdaContext(lambdaContextLogger, nameof(functions.GetUserByTelegramIdAsync));

    var lambdaResponse = await functions.GetUserByTelegramIdAsync(telegramId, localContext);
    return LambdaResponseMapper.ToMinimalApiResult(lambdaResponse);
});

// Your existing Hello World or other non-Lambda endpoints
app.MapGet("/", () => "Hello World! This is the UserManagerApi running locally.");

app.Run();