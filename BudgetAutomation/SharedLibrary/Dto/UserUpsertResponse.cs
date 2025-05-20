using System.Text.Json.Serialization;
using SharedLibrary.Model;

namespace SharedLibrary.Dto;

public class UserUpsertResponse : ApiResponse
{
    [JsonPropertyName("user")]
    public User? User { get; set; }
}