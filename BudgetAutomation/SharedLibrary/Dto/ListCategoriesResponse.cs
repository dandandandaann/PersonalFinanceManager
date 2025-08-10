using System.Text.Json.Serialization;

namespace SharedLibrary.Dto;

public class ListCategoriesResponse : ApiResponse
{
    [JsonPropertyName("categories")]
    public IList<string> Categories { get; set; } = new List<string>();
}


