using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using SpreadsheetManagerApi.Misc;
using SpreadsheetManagerApi.Service;
using SharedLibrary.Dto;
using SharedLibrary.Enum;
using SpreadsheetManagerApi.Interface;
using Results = SharedLibrary.Lambda.Results;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SpreadsheetManagerApi;

/// <summary>
/// Handles Spreadsheet manipulation operations.
/// </summary>
public class Functions
{
    /// <summary>
    /// Default constructor.
    /// </summary>
    public Functions()
    {
    }

    [LambdaFunction(
        ResourceName = "Default",
        Policies = "AWSLambdaBasicExecutionRole, " +
                   "arn:aws:iam::795287297286:policy/Configurations_Read",
        MemorySize = 128,
        Timeout = 3)]
    [HttpApi(LambdaHttpMethod.Get, "/")]
    public string Default()
    {
        return "SpreadsheetManager is running!";
    }

    [LambdaFunction(
        ResourceName = "LogExpense",
        Policies = "AWSLambdaBasicExecutionRole, " +
                   "arn:aws:iam::aws:policy/AmazonSQSReadOnlyAccess, " +
                   "arn:aws:iam::795287297286:policy/Configurations_Read",
        MemorySize = 128,
        Timeout = 15)]
    [HttpApi(LambdaHttpMethod.Put, "/log-expense")]
    public async Task<APIGatewayHttpApiV2ProxyResponse> LogExpenseAsync(ILambdaContext context,
        [FromServices] ExpenseLoggerService sheetsLogger,
        [FromBody] LogExpenseRequest request)
    {
        var logger = context.Logger;
        logger.LogInformation("LogExpenseAsyncAsync: Received request for SpreadsheetId: {SpreadsheetId}", request.SpreadsheetId);

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
            logger.LogWarning("LogExpenseAsyncAsync: Invalid amount format provided: {Amount}", request.Amount);
            return Results.Ok(new LogExpenseResponse { Success = false });
        }
        catch (Exception ex) when (ex is SheetNotFoundException or SpreadsheetNotFoundException)
        {
            logger.LogWarning(ex.Message);
            return Results.Ok(new RemoveExpenseResponse
            {
                Success = false,
                Message = "Spreadsheet or sheet doesn't exist.",
                ErrorCode = ErrorCodeEnum.ResourceNotFound
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex.Message);
            return Results.Ok(new RemoveExpenseResponse
            {
                Success = false,
                Message = "Not able to access the spreadsheet.",
                ErrorCode = ErrorCodeEnum.UnauthorizedAccess
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "LogExpenseAsyncAsync: Failed to log expense for SpreadsheetId: {SpreadsheetId}", request.SpreadsheetId);
            return Results.InternalServerError("An error occurred while logging the expense.");
        }
    }

    [LambdaFunction(
        ResourceName = "ValidateSpreadsheet",
        Policies = "AWSLambdaBasicExecutionRole, " +
                   "arn:aws:iam::795287297286:policy/Configurations_Read",
        MemorySize = 128,
        Timeout = 15)]
    [HttpApi(LambdaHttpMethod.Post, "/validate-spreadsheet")]
    public async Task<APIGatewayHttpApiV2ProxyResponse> ValidateSpreadsheetAsync(ILambdaContext context,
        [FromServices] SpreadsheetService sheetService, [FromBody] SpreadsheetValidationRequest request)
    {
        try
        {
            var response = await sheetService.ValidateSpreadsheetId(request.SpreadsheetId);
            return Results.Ok(response);
        }
        catch (SpreadsheetNotFoundException ex)
        {
            context.Logger.LogWarning(ex.Message);
            return Results.Ok(new RemoveExpenseResponse
            {
                Success = false,
                Message = "Spreadsheet doesn't exist.",
                ErrorCode = ErrorCodeEnum.ResourceNotFound
            });
        }
        catch (SheetNotFoundException ex)
        {
            context.Logger.LogWarning(ex.Message);
            return Results.Ok(new RemoveExpenseResponse
            {
                Success = false,
                Message = "Sheet doesn't exist.",
                ErrorCode = ErrorCodeEnum.TransactionsSheetNotFound
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            context.Logger.LogWarning(ex.Message);
            return Results.Ok(new RemoveExpenseResponse
            {
                Success = false,
                Message = "Not able to access the spreadsheet.",
                ErrorCode = ErrorCodeEnum.UnauthorizedAccess
            });
        }
        catch (Exception ex)
        {
            context.Logger.LogError(ex, "Error while validating the spreadsheet.");
            return Results.InternalServerError("Internal error while validating the spreadsheet.");
        }
    }

    [LambdaFunction(
        ResourceName = "RemoveLastExpense",
        Policies = "AWSLambdaBasicExecutionRole, " +
                   "arn:aws:iam::795287297286:policy/Configurations_Read",
        MemorySize = 128,
        Timeout = 15)]
    [HttpApi(LambdaHttpMethod.Delete, "/undo")]
    public async Task<APIGatewayHttpApiV2ProxyResponse> RemoveLastExpenseAsync(ILambdaContext context,
        [FromServices] SpreadsheetService spreadsheetService,
        [FromQuery] string spreadsheetId)
    {
        var logger = context.Logger;
        logger.LogInformation("RemoveLastExpenseAsync: Received remove request for SpreadsheetId: {SpreadsheetId}", spreadsheetId);

        try
        {
            var response = await spreadsheetService.RemoveLastExpenseAsync(spreadsheetId);
            return Results.Ok(response);
        }
        catch (Exception ex) when (ex is SheetNotFoundException or SpreadsheetNotFoundException)
        {
            logger.LogWarning(ex.Message);
            return Results.Ok(new RemoveExpenseResponse
            {
                Success = false,
                Message = "Spreadsheet or sheet doesn't exist.",
                ErrorCode = ErrorCodeEnum.ResourceNotFound
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex.Message);
            return Results.Ok(new RemoveExpenseResponse
            {
                Success = false,
                Message = "Not able to access the spreadsheet.",
                ErrorCode = ErrorCodeEnum.UnauthorizedAccess
            });
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "RemoveLastExpenseAsync: Failed to remove expense for SpreadsheetId: {SpreadsheetId}", spreadsheetId);
            return Results.InternalServerError("An error occurred while removing the expense");
        }
    }

    [LambdaFunction(
        ResourceName = "GetLastExpense",
        Policies = "AWSLambdaBasicExecutionRole, " +
                   "arn:aws:iam::795287297286:policy/Configurations_Read",
        MemorySize = 128,
        Timeout = 15)]
    [HttpApi(LambdaHttpMethod.Get, "/lastitem")]
    public async Task<APIGatewayHttpApiV2ProxyResponse> GetLastExpenseAsync(ILambdaContext context,
        [FromServices] SpreadsheetService spreadsheetService,
        [FromQuery] string spreadsheetId)
    {
        var logger = context.Logger;
        logger.LogInformation("GetLastExpenseAsync: Received request for SpreadsheetId: {SpreadsheetId}", spreadsheetId);

        try
        {
            var response = await spreadsheetService.GetLastExpenseAsync(spreadsheetId);
            return Results.Ok(response);
        }
        catch (Exception ex) when (ex is SheetNotFoundException or SpreadsheetNotFoundException)
        {
            logger.LogWarning(ex.Message);
            return Results.Ok(new RemoveExpenseResponse
            {
                Success = false,
                Message = "Spreadsheet or sheet doesn't exist.",
                ErrorCode = ErrorCodeEnum.ResourceNotFound
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex.Message);
            return Results.Ok(new RemoveExpenseResponse
            {
                Success = false,
                Message = "Not able to access the spreadsheet.",
                ErrorCode = ErrorCodeEnum.UnauthorizedAccess
            });
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "GetLastExpenseAsync: Failed to get expense for SpreadsheetId: {SpreadsheetId}", spreadsheetId);
            return Results.InternalServerError("An error occurred while getting the expense");
        }
    }

    [LambdaFunction(
        ResourceName = "AddCategoryRule",
        Policies = "AWSLambdaBasicExecutionRole, " +
                   "arn:aws:iam::795287297286:policy/Configurations_Read",
        MemorySize = 128,
        Timeout = 15)]
    [HttpApi(LambdaHttpMethod.Post, "/add-category-rule")]
    public async Task<APIGatewayHttpApiV2ProxyResponse> AddCategoryRuleAsync(ILambdaContext context,
        [FromServices] ICategoryService categoryService,
        [FromBody] AddCategoryRuleRequest request)
    {
        var logger = context.Logger;
        logger.LogInformation("AddCategoryRuleAsync: Received request for SpreadsheetId: {SpreadsheetId}", request.SpreadsheetId);

        try
        {
            var response = await categoryService.AddCategoryRuleAsync(request.SpreadsheetId, request.Category, request.DescriptionPattern);
            return Results.Ok(response);
        }
        catch (Exception ex) when (ex is SheetNotFoundException or SpreadsheetNotFoundException)
        {
            logger.LogWarning(ex.Message);
            return Results.Ok(new RemoveExpenseResponse
            {
                Success = false,
                Message = "Spreadsheet or sheet doesn't exist.",
                ErrorCode = ErrorCodeEnum.ResourceNotFound
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex.Message);
            return Results.Ok(new RemoveExpenseResponse
            {
                Success = false,
                Message = "Not able to access the spreadsheet.",
                ErrorCode = ErrorCodeEnum.UnauthorizedAccess
            });
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "AddCategoryRuleAsync: Failed for SpreadsheetId: {SpreadsheetId}", request.SpreadsheetId);
            return Results.InternalServerError("Error while adding category rule.");
        }
    }
}