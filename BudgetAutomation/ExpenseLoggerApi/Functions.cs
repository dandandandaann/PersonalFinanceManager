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
    public async Task<APIGatewayHttpApiV2ProxyResponse> LogExpenseAsyncAsync(ILambdaContext context,
        [FromServices] ExpenseLoggerService sheetsLogger,
        [FromQuery] string spreadsheetId,
        [FromQuery] string description, [FromQuery] string amount, [FromQuery] string category = "")
    {
        var logger = context.Logger;
        logger.LogInformation("LogExpenseAsyncAsync: Received request for SpreadsheetId: {SpreadsheetId}", spreadsheetId);

        try
        {
            var expense = await sheetsLogger.LogExpense(spreadsheetId, description, amount, category);
            return ApiGatewayResult.Ok(new LogExpenseResponse { Success = true, expense = expense });
        }
        catch (ArgumentException ex) when (ex.ParamName == "amount")
        {
            logger.LogWarning("LogExpenseAsyncAsync: Invalid amount format provided: {Amount}", amount);
            return ApiGatewayResult.Ok(new LogExpenseResponse { Success = false });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "LogExpenseAsyncAsync: Failed to log expense for SpreadsheetId: {SpreadsheetId}", spreadsheetId);
            return ApiGatewayResult.InternalServerError("An error occurred while logging the expense.");
        }
    }
}