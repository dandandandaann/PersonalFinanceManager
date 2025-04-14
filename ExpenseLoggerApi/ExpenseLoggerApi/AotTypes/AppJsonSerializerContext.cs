using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using ExpenseLoggerApi.Model;

namespace ExpenseLoggerApi.AotTypes;

[JsonSerializable(typeof(ResponseModel))]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyRequest))]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyResponse))]
public partial class AppJsonSerializerContext : JsonSerializerContext
{
}
