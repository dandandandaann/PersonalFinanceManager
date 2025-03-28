using System.Threading.RateLimiting;
using ExpenseLoggerApi;
using ExpenseLoggerApi.Model;

var builder = WebApplication.CreateSlimBuilder(args);
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var apiKey = builder.Configuration["apikey"];
if (apiKey is null)
    throw new InvalidOperationException($"{nameof(apiKey)} not found in configuration");

var credentials = builder.Configuration.GetSection("credentials").Get<Dictionary<string, object>>();
if (credentials is null)
    throw new InvalidOperationException($"{nameof(credentials)} not found in configuration");

var spreadsheetId = builder.Configuration["spreadsheetId"];
if (spreadsheetId is null)
    throw new InvalidOperationException($"{nameof(spreadsheetId)} not found in configuration");

var jsonString = JsonHandler.ToJson(credentials);

var maxDailyRequest = builder.Configuration["maxDailyRequest"] ?? "20";

// Add Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            apiKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = int.Parse(maxDailyRequest), // Allow X requests
                Window = TimeSpan.FromDays(1), // Per day
            }));
});

var app = builder.Build();

app.Use(async (context, next) =>
{
    if (!context.Request.Headers.TryGetValue("X-Api-Key", out var extractedApiKey) || extractedApiKey != apiKey)
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("Unauthorized");
        return;
    }
    await next();
});

app.UseRateLimiter();

var sheetsLogger = new GoogleSheetsExpenseLogger(jsonString, spreadsheetId);

var expenseLoggerApi = app.MapGroup("/log-expense");
expenseLoggerApi.MapPut("/", async (string description, string amount, string category = "") =>
{
    try
    {
        await sheetsLogger.LogExpense(description, amount, category);
    }
    catch (Exception e)
    {
        return Results.Ok(new ResponseModel { Success = false });
    }

    return Results.Ok(new ResponseModel { Success = true });
});

app.Run();