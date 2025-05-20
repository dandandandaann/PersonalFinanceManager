namespace SharedLibrary.Interface;

/// <summary>
/// Common interface for response DTOs
/// </summary>
public interface IApiResponse
{
    bool Success { get; set; }
}