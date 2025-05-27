using ExpenseLoggerApi.Extension;
using ExpenseLoggerApi.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SharedLibrary.Dto;
using SharedLibrary.Settings;

var builder = WebApplication.CreateSlimBuilder(args);

var localDevelopment = builder.Environment.IsDevelopment();

builder.Configuration.AddProjectSpecificConfigurations(localDevelopment);

builder.Services.AddProjectSpecificServices(builder.Configuration);

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