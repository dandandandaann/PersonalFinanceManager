using System.Text.Json.Serialization;
using ExpenseLoggerApi;

var builder = WebApplication.CreateSlimBuilder(args);
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var credentials = builder.Configuration.GetSection("credentials").Get<Dictionary<string, object>>();
if (credentials is null)
    throw new InvalidOperationException("Google credentials not found in configuration");

var jsonString = JsonHandler.ToJson(credentials);

var app = builder.Build();

var todosApi = app.MapGroup("/log-expense");
todosApi.MapPut("/", async (string description, float amount, string category = "") =>
{
    var sheetsLogger = new GoogleSheetsExpenseLogger(jsonString);
    await sheetsLogger.LogExpense(description, amount, category);

    return Results.Ok(new ResponseModel { Success = true });
});

app.Run();


public class ResponseModel
{
    public bool Success { get; set; }
}

[JsonSerializable(typeof(ResponseModel))]
public partial class AppJsonSerializerContext : JsonSerializerContext
{
}