using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using SharedLibrary;
using SharedLibrary.Dto;
using SharedLibrary.Model;

namespace UserManagerApi.AotTypes;

[JsonSerializable(typeof(LogExpenseResponse))]
[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(UserSignupResponse))]
[JsonSerializable(typeof(UserSignupRequest))]
[JsonSerializable(typeof(UserGetResponse))]
[JsonSerializable(typeof(UserSignupRequest))]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyRequest))]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyResponse))]
public partial class AppJsonSerializerContext : JsonSerializerContext
{
}
