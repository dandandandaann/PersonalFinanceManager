using System.Text.Json.Serialization;

namespace ExpenseLoggerApi.Model;


public class ResponseModel
{
    public bool Success { get; set; }
}

[JsonSerializable(typeof(ResponseModel))]
public partial class AppJsonSerializerContext : JsonSerializerContext
{
}