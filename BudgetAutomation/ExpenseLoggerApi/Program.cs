using System.Threading.RateLimiting;
using Amazon.Lambda.Serialization.SystemTextJson;
using ExpenseLoggerApi.AotTypes;
using ExpenseLoggerApi.Interface;
using ExpenseLoggerApi.Service;
using Google.Apis.Sheets.v4;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SharedLibrary.Dto;
using SharedLibrary.LocalTesting;
using SharedLibrary.Settings;
using SharedLibrary.Validator;

var builder = WebApplication.CreateSlimBuilder(args);

var isLocalDev = LocalDevelopment.Prefix(builder.Environment.IsDevelopment());

// Configure AWS Parameter Store
builder.Configuration.AddSystemsManager($"/{isLocalDev}{BudgetAutomationSettings.Configuration}/");

builder.Services.Configure<ExpenseLoggerSettings>(builder.Configuration.GetSection(ExpenseLoggerSettings.Configuration));
builder.Services.AddSingleton<IValidateOptions<ExpenseLoggerSettings>, ExpenseLoggerSettingsValidator>();

// Add Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        // Get settings via IOptions within the rate limiter setup
        var settings = httpContext.RequestServices.GetRequiredService<IOptions<ExpenseLoggerSettings>>().Value;

        if (!int.TryParse(settings.maxDailyRequest, out var rateLimit))
        {
            rateLimit = 20; // Default if parsing fails
        }

        return RateLimitPartition.GetFixedWindowLimiter(
            settings.googleApiKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = rateLimit,
                Window = TimeSpan.FromDays(1),
            });
    });
    // Added OnRejected handler for better debugging
    options.OnRejected = (context, _) =>
    {
        context.HttpContext.Response.StatusCode = 429; // Too Many Requests
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogWarning("Rate limit reached for request to {Path}.", context.HttpContext.Request.Path);
        return new ValueTask();
    };
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

#pragma warning disable IL2026
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi,
    options => { options.Serializer = new SourceGeneratorLambdaJsonSerializer<AppJsonSerializerContext>(); });
#pragma warning restore IL2026

builder.Services.AddSingleton<GoogleSheetsClientFactory>();

builder.Services.AddSingleton<SheetsService>(sp =>
{
    var factory = sp.GetRequiredService<GoogleSheetsClientFactory>();
    var settings = sp.GetRequiredService<IOptions<ExpenseLoggerSettings>>().Value;

    return factory.CreateSheetsService(settings.credentials);
});

builder.Services.AddSingleton<ISheetsDataAccessor, GoogleSheetsDataAccessor>();

builder.Services.AddScoped<ExpenseLoggerService>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<ExpenseLoggerSettings>>().Value;

    return new ExpenseLoggerService(
        sp.GetRequiredService<ISheetsDataAccessor>(),
        settings.Categories,
        sp.GetRequiredService<ILogger<ExpenseLoggerService>>()
    );
});

var app = builder.Build();

app.Use(async (context, next) =>
{
    var settings = context.RequestServices.GetRequiredService<IOptions<ExpenseLoggerSettings>>().Value;

    if (!context.Request.Headers.TryGetValue("X-Api-Key", out var extractedApiKey) || extractedApiKey != settings.googleApiKey)
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("Unauthorized");
        return;
    }

    await next(context);
});

app.UseRateLimiter();

app.MapGet("", () => "ExpenseLogger Api is running!");

app.MapPut("/log-expense",
    async ([FromServices] ExpenseLoggerService sheetsLogger,
        [FromQuery] string spreadsheetId,
        [FromQuery] string description, [FromQuery] string amount, [FromQuery] string category = "") =>
    {
        try
        {
            var expense = await sheetsLogger.LogExpense(spreadsheetId, description, amount, category);
            return Results.Ok(new LogExpenseResponse { Success = true, expense = expense });
        }
        catch (ArgumentException ex) when (ex.ParamName == "amount")
        {
            app.Logger.LogWarning("Invalid amount format provided: {Amount}", amount);
            return Results.Ok(new LogExpenseResponse { Success = false });
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Failed to log expense for description: {Description}", description);
            return Results.Problem(detail: "An error occurred while logging the expense.", statusCode: 500);
        }
    });

app.Run();