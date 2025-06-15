using System.Text.Json.Serialization;
using Google.Apis.Auth.OAuth2;

namespace SpreadsheetManagerApi.Model;

[JsonSerializable(typeof(JsonCredentialParameters))]
internal partial class JsonCredentialContext : JsonSerializerContext
{
}