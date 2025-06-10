using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using ExpenseLoggerApi.Service;
using SharedLibrary.Dto;
using SharedLibrary.Lambda.LocalDevelopment;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ExpenseLoggerApi;

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
        Policies = "AWSLambdaBasicExecutionRole, " +
                   "arn:aws:iam::795287297286:policy/Configurations_Read",
        MemorySize = 128,
        Timeout = 3)]
    [HttpApi(LambdaHttpMethod.Get, "/")]
    public string Default()
    {
        return "ExpenseLogger is running!";
    }

    [LambdaFunction(
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
            return ApiGatewayResult.Ok(new LogExpenseResponse { Success = true, expense = expense });
        }
        catch (ArgumentException ex) when (ex.ParamName == "amount")
        {
            logger.LogWarning("LogExpenseAsyncAsync: Invalid amount format provided: {Amount}", request.Amount);
            return ApiGatewayResult.Ok(new LogExpenseResponse { Success = false });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "LogExpenseAsyncAsync: Failed to log expense for SpreadsheetId: {SpreadsheetId}", request.SpreadsheetId);
            return ApiGatewayResult.InternalServerError("An error occurred while logging the expense.");
        }
    }

    [LambdaFunction(
        Policies = "AWSLambdaBasicExecutionRole, " +
                   "arn:aws:iam::795287297286:policy/Configurations_Read",
        MemorySize = 128,
        Timeout = 15)]
    [HttpApi(LambdaHttpMethod.Put, "/validate-spreadsheet")]
    public async Task<APIGatewayHttpApiV2ProxyResponse> ValidateSpreadsheetAsync(ILambdaContext context,
        [FromServices] SpreadsheetService sheetService, [FromBody] SpreadsheetValidationRequest request)
    {
        try
        {
            var response = await sheetService.ValidateSpreadsheetId(request.SpreadsheetId);
            return ApiGatewayResult.Ok(response);
        }
        catch (Exception ex)
        {
            context.Logger.LogError(ex, "Error while validating the spreadsheet.");
            return ApiGatewayResult.InternalServerError("Internal error while validating the spreadsheet.");
        }
    }
    [HttpApi(LambdaHttpMethod.Delete, "/undo")]
    public async Task<APIGatewayHttpApiV2ProxyResponse> RemoveExpenseAsync(ILambdaContext context,
        [FromServices] SpreadsheetService removeLogger,
        [FromQuery] string spreadsheetId)
    {
        var logger = context.Logger;
        logger.LogInformation("RemoveExpenseAsync: Received remove request for SpreadsheetId: {SpreadsheetId}", spreadsheetId);

        try
        {
            var response = await removeLogger.RemoveLastExpense(spreadsheetId);
            return ApiGatewayResult.Ok(response);
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "RemoveExpenseAsync: Failed to remove expense for SpreadsheetId: {SpreadsheetId}", spreadsheetId);
            return ApiGatewayResult.InternalServerError("An erroroccurred while removing the expense");
        }
    }
}