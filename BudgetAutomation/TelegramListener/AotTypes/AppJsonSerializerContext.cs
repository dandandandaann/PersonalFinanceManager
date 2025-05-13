using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;

namespace TelegramListener.AotTypes;

[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyRequest))]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyResponse))]
[JsonSerializable(typeof(SharedLibrary.Telegram.Update))]
public partial class AppJsonSerializerContext : JsonSerializerContext
{
}
