using System.Threading.RateLimiting;
using ExpenseLoggerApi;
using ExpenseLoggerApi.Model;
using Amazon.Lambda.Serialization.SystemTextJson;

var builder = WebApplication.CreateSlimBuilder(args);

var googleApiKey = builder.Configuration["googleApiKey"];
if (googleApiKey is null)
    throw new InvalidOperationException($"{nameof(googleApiKey)} not found in configuration");

var googleCredentials = builder.Configuration["credentials"];
if (googleCredentials is null)
    throw new InvalidOperationException($"{nameof(googleCredentials)} not found in configuration");

var spreadsheetId = builder.Configuration["spreadsheetId"];
if (spreadsheetId is null)
    throw new InvalidOperationException($"{nameof(spreadsheetId)} not found in configuration");

var categories = builder.Configuration.GetSection("Categories").Get<List<Category>>();
if (categories is null || categories.Count == 0)
    throw new InvalidOperationException("Categories not found in configuration");

var maxDailyRequest = builder.Configuration["maxDailyRequest"] ?? "20";

// Add Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            googleApiKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = int.Parse(maxDailyRequest), // Allow X requests
                Window = TimeSpan.FromDays(1), // Per day
            }));
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

#pragma warning disable IL2026
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi, options =>
{
    options.Serializer = new SourceGeneratorLambdaJsonSerializer<AppJsonSerializerContext>();
});
#pragma warning restore IL2026

var app = builder.Build();

app.Logger.LogInformation($"Creating GoogleSheetsExpenseLogger with spreadsheetId '{spreadsheetId}'");
var sheetsLogger = new GoogleSheetsExpenseLogger(googleCredentials, spreadsheetId, categories, app.Logger);

app.Use(async (context, next) =>
{
    if (!context.Request.Headers.TryGetValue("X-Api-Key", out var extractedApiKey) || extractedApiKey != googleApiKey)
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("Unauthorized");
        return;
    }

    await next();
});

app.UseRateLimiter();

app.MapGet("", () => "Hello world!");

app.MapPut("/log-expense", async (string description, string amount, string category = "") =>
{
    try
    {
        await sheetsLogger.LogExpense(description, amount, category);
    }
    catch (Exception ex)
    {
        app.Logger.LogError("Failed to log expense. Exception: {ExceptionMessage}", ex.Message);
        return Results.Ok(new ResponseModel { Success = false });
    }

    return Results.Ok(new ResponseModel { Success = true });
});

app.Run();