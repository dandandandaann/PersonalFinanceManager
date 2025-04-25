using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using SharedLibrary;
using SharedLibrary.UserClasses;

namespace BudgetBotTelegram.AtoTypes;

[JsonSerializable(typeof(LogExpenseResponse))]
[JsonSerializable(typeof(UserResponse))]
[JsonSerializable(typeof(UserSignupRequest))]
[JsonSerializable(typeof(UserExistsResponse))]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyRequest))]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyResponse))]
public partial class AppJsonSerializerContext : JsonSerializerContext
{
}