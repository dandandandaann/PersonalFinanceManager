﻿using Amazon.Lambda.APIGatewayEvents;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Lambda.LocalDevelopment;
using TelegramListener;
using TelegramListener.Extension;
using TelegramListener.Service;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

builder.Configuration.AddProjectSpecificConfigurations(builder.Environment.IsDevelopment());

services.AddProjectSpecificServices(builder.Configuration);

// Register Functions
services.AddScoped<Functions>();

var app = builder.Build();

// SetupWebhook
app.MapGet("/setupWebhook", async (
    Functions functions, ConfigureWebhook configureWebhook, ILoggerFactory loggerFactory,
    [FromQuery] string apiDomain) =>
{
    var lambdaContextLogger = loggerFactory.CreateLogger(typeof(Functions).FullName ?? "Functions");
    var localContext = new LocalLambdaContext(lambdaContextLogger, nameof(functions.SetupWebhook));

    var apiGatewayRequest = new APIGatewayHttpApiV2ProxyRequest
    {
        RequestContext = new APIGatewayHttpApiV2ProxyRequest.ProxyRequestContext
        {
            DomainName = apiDomain.TrimEnd('/')
        }
    };

    APIGatewayHttpApiV2ProxyResponse lambdaResponse = await functions.SetupWebhook(
        apiGatewayRequest, configureWebhook, localContext);

    return LambdaToApiResponseMapper.ToMinimalApiResult(lambdaResponse);
});

// Webhook
app.MapPost("/webhook", async (
    [FromQuery] string token,
    [FromBody] Telegram.Bot.Types.Update update,
    IAuthenticationService authenticator,
    ITelegramUpdateProcessor updateProcessor,
    Functions functions,
    ILoggerFactory loggerFactory) =>
{
    var lambdaContextLogger = loggerFactory.CreateLogger(typeof(Functions).FullName ?? "Functions");
    var localContext = new LocalLambdaContext(lambdaContextLogger, nameof(functions.Webhook));

    APIGatewayHttpApiV2ProxyResponse lambdaResponse =
        await functions.Webhook(token, update, localContext, authenticator, updateProcessor);

    return LambdaToApiResponseMapper.ToMinimalApiResult(lambdaResponse);
});

app.MapGet("/", () => "Hello World! This is the TelegramListener running locally.");

app.Run();