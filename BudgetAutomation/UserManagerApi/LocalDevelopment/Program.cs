using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Dto;
using SharedLibrary.Lambda.LocalDevelopment;
using UserManagerApi;
using UserManagerApi.Extension;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddProjectSpecificServices(new ConfigurationManager());

// Register Functions
services.AddScoped<Functions>();

var app = builder.Build();

// SignupUserAsync
app.MapPost("/user/signup", async (
    [FromBody] UserSignupRequest request,
    Functions functions,
    ILoggerFactory loggerFactory
) =>
{
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

app.MapGet("/", () => "Hello World! This is the UserManagerApi running locally.");

app.Run();