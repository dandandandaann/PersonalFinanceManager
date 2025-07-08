using SpreadsheetManagerApi.Extension;
using SpreadsheetManagerApi.Misc;
using SpreadsheetManagerApi.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SharedLibrary.Dto;
using SharedLibrary.Enum;
using SharedLibrary.Settings;
using SpreadsheetManagerApi.Interface;

var builder = WebApplication.CreateSlimBuilder(args);

var localDevelopment = builder.Environment.IsDevelopment();

builder.Configuration.AddProjectSpecificConfigurations(localDevelopment);

builder.Services.AddProjectSpecificServices(builder.Configuration);

var app = builder.Build();

app.Use(async (context, next) =>
{
    var settings = context.RequestServices.GetRequiredService<IOptions<SpreadsheetManagerApiSettings>>().Value;

    if (!context.Request.Headers.TryGetValue("X-Api-Key", out var extractedApiKey) || extractedApiKey != settings.googleApiKey)
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("Unauthorized");
        return;
    }

    await next(context);
});

app.UseRateLimiter();

app.MapGet("", () => "SpreadsheetManager Api is running!");

app.MapPut("/log-expense",
    async ([FromServices] ExpenseLoggerService sheetsLogger,
        [FromBody] LogExpenseRequest request) =>
    {
        try
        {
            var expense = await sheetsLogger.LogExpense(
                request.SpreadsheetId,
                request.Description,
                request.Amount,
                request.Category
            );

            return Results.Ok(new LogExpenseResponse { Success = true, expense = expense });
        }
        catch (ArgumentException ex) when (ex.ParamName == "amount")
        {
            app.Logger.LogWarning("Invalid amount format provided: {Amount}", request.Amount);
            return Results.Ok(new LogExpenseResponse { Success = false });
        }
        catch (Exception ex) when (ex is SheetNotFoundException or SpreadsheetNotFoundException)
        {
            app.Logger.LogWarning(ex.Message);
            return Results.Ok(new RemoveExpenseResponse
            {
                Success = false,
                Message = "Spreadsheet or sheet doesn't exist.",
                ErrorCode = ErrorCodeEnum.ResourceNotFound
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            app.Logger.LogWarning(ex.Message);
            return Results.Ok(new RemoveExpenseResponse
            {
                Success = false,
                Message = "Not able to access the spreadsheet.",
                ErrorCode = ErrorCodeEnum.UnauthorizedAccess
            });
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Failed to log expense for description: {Description}", request.Description);
            return Results.Problem(detail: "An error occurred while logging the expense.", statusCode: 500);
        }
    });

app.MapPost("/validate-spreadsheet",
    async ([FromServices] SpreadsheetService sheetService,
        [FromBody] SpreadsheetValidationRequest request) =>
    {
        try
        {
            var response = await sheetService.ValidateSpreadsheetId(request.SpreadsheetId);
            return Results.Ok(response);
        }
        catch (SpreadsheetNotFoundException ex)
        {
            app.Logger.LogWarning(ex.Message);
            return Results.Ok(new RemoveExpenseResponse
            {
                Success = false,
                Message = "Spreadsheet doesn't exist.",
                ErrorCode = ErrorCodeEnum.ResourceNotFound
            });
        }
        catch (SheetNotFoundException ex)
        {
            app.Logger.LogWarning(ex.Message);
            return Results.Ok(new RemoveExpenseResponse
            {
                Success = false,
                Message = "Sheet doesn't exist.",
                ErrorCode = ErrorCodeEnum.TransactionsSheetNotFound
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            app.Logger.LogWarning(ex.Message);
            return Results.Ok(new RemoveExpenseResponse
            {
                Success = false,
                Message = "Not able to access the spreadsheet.",
                ErrorCode = ErrorCodeEnum.UnauthorizedAccess
            });
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Error while validating the spreadsheet.");
            return Results.Problem(detail: "Internal error while validating the spreadsheet.", statusCode: 500);
        }
    });

app.MapDelete("/undo",
    async ([FromServices] SpreadsheetService spreadsheetService,
        [FromQuery] string spreadsheetId) =>
    {
        try
        {
            var response = await spreadsheetService.RemoveLastExpenseAsync(spreadsheetId);
            return Results.Ok(response);
        }
        catch (Exception ex) when (ex is SheetNotFoundException or SpreadsheetNotFoundException)
        {
            app.Logger.LogWarning(ex.Message);
            return Results.Ok(new RemoveExpenseResponse
            {
                Success = false,
                Message = "Spreadsheet or sheet doesn't exist.",
                ErrorCode = ErrorCodeEnum.ResourceNotFound
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            app.Logger.LogWarning(ex.Message);
            return Results.Ok(new RemoveExpenseResponse
            {
                Success = false,
                Message = "Not able to access the spreadsheet.",
                ErrorCode = ErrorCodeEnum.UnauthorizedAccess
            });
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Failed to remove expense");
            return Results.Problem(detail: "An error occured while logging the expense.", statusCode: 500);
        }
    });

app.MapGet("/lastitem",
    async ([FromServices] SpreadsheetService spreadsheetService,
        [FromQuery] string spreadsheetId) =>
    {
        try
        {
            var response = await spreadsheetService.GetLastExpenseAsync(spreadsheetId);
            return Results.Ok(response);
        }
        catch (Exception ex) when (ex is SheetNotFoundException or SpreadsheetNotFoundException)
        {
            app.Logger.LogWarning(ex.Message);
            return Results.Ok(new RemoveExpenseResponse
            {
                Success = false,
                Message = "Spreadsheet or sheet doesn't exist.",
                ErrorCode = ErrorCodeEnum.ResourceNotFound
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            app.Logger.LogWarning(ex.Message);
            return Results.Ok(new RemoveExpenseResponse
            {
                Success = false,
                Message = "Not able to access the spreadsheet.",
                ErrorCode = ErrorCodeEnum.UnauthorizedAccess
            });
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "GetLastExpenseAsync: Failed to get expense for SpreadsheetId: {SpreadsheetId}",
                spreadsheetId);
            return Results.Problem("An error occurred while getting the expense");
        }
    });


app.MapPost("/add-category-rule",
    async ([FromServices] ICategoryService categoryService,
        [FromBody] AddCategoryRuleRequest request) =>
    {
        app.Logger.LogInformation("AddCategoryRuleAsync: Received request for SpreadsheetId: {SpreadsheetId}",
            request.SpreadsheetId);

        try
        {
            var response =
                await categoryService.AddCategoryRuleAsync(request.SpreadsheetId, request.Category, request.DescriptionPattern);
            return Results.Ok(response);
        }
        catch (Exception ex) when (ex is SheetNotFoundException or SpreadsheetNotFoundException)
        {
            app.Logger.LogWarning(ex.Message);
            return Results.Ok(new RemoveExpenseResponse
            {
                Success = false,
                Message = "Spreadsheet or sheet doesn't exist.",
                ErrorCode = ErrorCodeEnum.ResourceNotFound
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            app.Logger.LogWarning(ex.Message);
            return Results.Ok(new RemoveExpenseResponse
            {
                Success = false,
                Message = "Not able to access the spreadsheet.",
                ErrorCode = ErrorCodeEnum.UnauthorizedAccess
            });
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "AddCategoryRuleAsync: Failed for SpreadsheetId: {SpreadsheetId}", request.SpreadsheetId);
            return Results.Problem("Error while adding category rule.");
        }
    });


app.Run();