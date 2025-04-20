using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;

namespace UserManagerApi.Common; // Or any appropriate namespace

public static class ApiResponse // Or name it ResponseHelpers, Results, etc.
{
    private static readonly Dictionary<string, string> JsonHeaders = new() { { "Content-Type", "application/json" } };

    private static readonly Dictionary<string, string> TextHeaders = new() { { "Content-Type", "text/plain" } };

    public static APIGatewayHttpApiV2ProxyResponse Ok(object? body = null)
    {
        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK, // 200
            Headers = JsonHeaders,
            Body = body != null ? JsonSerializer.Serialize(body) : null,
            IsBase64Encoded = false
        };
    }

    public static APIGatewayHttpApiV2ProxyResponse OkText(string? body = null)
    {
        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK, // 200
            Headers = TextHeaders,
            Body = body,
            IsBase64Encoded = false
        };
    }

    public static APIGatewayHttpApiV2ProxyResponse Created(string location, object? body = null)
    {
        // Clone JsonHeaders and add Location
        var headers = new Dictionary<string, string>(JsonHeaders)
        {
            { "Location", location }
        };

        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = (int)HttpStatusCode.Created, // 201
            Headers = headers,
            Body = body != null ? JsonSerializer.Serialize(body) : null,
            IsBase64Encoded = false
        };
    }

    public static APIGatewayHttpApiV2ProxyResponse NoContent()
    {
        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = (int)HttpStatusCode.NoContent, // 204
            Body = null,
            Headers = null, // Explicitly null or empty is fine
            IsBase64Encoded = false
        };
    }

    public static APIGatewayHttpApiV2ProxyResponse BadRequest(object? errorBody = null)
    {
        // Often good practice to return a consistent error structure
        var body = errorBody ?? new { message = "Bad Request" };
        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = (int)HttpStatusCode.BadRequest, // 400
            Headers = JsonHeaders, // Usually return error details as JSON
            Body = JsonSerializer.Serialize(body),
            IsBase64Encoded = false
        };
    }

    public static APIGatewayHttpApiV2ProxyResponse Unauthorized(object? errorBody = null)
    {
        var body = errorBody ?? new { message = "Unauthorized" };
        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = (int)HttpStatusCode.Unauthorized, // 401
            Headers = JsonHeaders,
            Body = JsonSerializer.Serialize(body),
            IsBase64Encoded = false
        };
    }

    public static APIGatewayHttpApiV2ProxyResponse Forbidden(object? errorBody = null)
    {
        var body = errorBody ?? new { message = "Forbidden" };
        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = (int)HttpStatusCode.Forbidden, // 403
            Headers = JsonHeaders,
            Body = JsonSerializer.Serialize(body),
            IsBase64Encoded = false
        };
    }


    public static APIGatewayHttpApiV2ProxyResponse NotFound(object? errorBody = null)
    {
        var body = errorBody ?? new { message = "Not Found" };
        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = (int)HttpStatusCode.NotFound, // 404
            Headers = JsonHeaders,
            Body = JsonSerializer.Serialize(body),
            IsBase64Encoded = false
        };
    }

    // --- Server Error Responses (5xx) ---

    public static APIGatewayHttpApiV2ProxyResponse InternalServerError(object? errorBody = null)
    {
        // Be cautious about leaking internal details in production
        var body = errorBody ?? new { message = "Internal Server Error" };
        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = (int)HttpStatusCode.InternalServerError, // 500
            Headers = JsonHeaders,
            Body = JsonSerializer.Serialize(body),
            IsBase64Encoded = false
        };
    }

    // --- Custom Response ---
    // Useful if you need more control occasionally

    public static APIGatewayHttpApiV2ProxyResponse Custom(HttpStatusCode statusCode, object? body = null, IDictionary<string, string>? headers = null)
    {
        var effectiveHeaders = headers ?? (body != null ? JsonHeaders : null); // Default to JSON if body exists
        string? serializedBody = null;

        if (body != null)
        {
            // Check if the provided headers specify plain text explicitly
            if (effectiveHeaders != null &&
                effectiveHeaders.TryGetValue("Content-Type", out var contentType) &&
                contentType.Equals("text/plain", StringComparison.OrdinalIgnoreCase) &&
                body is string stringBody)
            {
                serializedBody = stringBody;
            }
            else // Default to JSON serialization
            {
                serializedBody = JsonSerializer.Serialize(body);
            }
        }


        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = (int)statusCode,
            Headers = effectiveHeaders,
            Body = serializedBody,
            IsBase64Encoded = false // Assume false unless specifically handled
        };
    }
}