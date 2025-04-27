using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using SharedLibrary;
using SharedLibrary.UserClasses;

namespace UserManagerApi.AotTypes;

[JsonSerializable(typeof(LogExpenseResponse))]
[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(UserResponse))]
[JsonSerializable(typeof(UserSignupRequest))]
[JsonSerializable(typeof(UserExistsResponse))]
[JsonSerializable(typeof(UserSignupRequest))]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyRequest))]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyResponse))]
public partial class AppJsonSerializerContext : JsonSerializerContext
{
}
