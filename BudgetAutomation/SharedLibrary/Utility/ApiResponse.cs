using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;

namespace SharedLibrary.Utility;

/// <summary>
/// Provides static factory methods for creating standardized <see cref="APIGatewayHttpApiV2ProxyResponse"/> objects.
/// This class simplifies the process of returning common HTTP responses from AWS Lambda functions
/// integrated with API Gateway (HTTP API V2).
/// </summary>
public static class ApiResponse
{
    /// <summary>
    /// Default headers for JSON responses.
    /// </summary>
    private static readonly Dictionary<string, string> JsonHeaders = new() { { "Content-Type", "application/json" } };

    /// <summary>
    /// Default headers for plain text responses.
    /// </summary>
    private static readonly Dictionary<string, string> TextHeaders = new() { { "Content-Type", "text/plain" } };

    /// <summary>
    /// Creates an HTTP 200 OK response.
    /// </summary>
    /// <param name="body">Optional. The object to serialize as JSON for the response body. Defaults to null.</param>
    /// <returns>An <see cref="APIGatewayHttpApiV2ProxyResponse"/> representing an HTTP 200 OK.</returns>
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

    /// <summary>
    /// Creates an HTTP 200 OK response with a plain text body.
    /// </summary>
    /// <param name="body">Optional. The string to use as the plain text response body. Defaults to null.</param>
    /// <returns>An <see cref="APIGatewayHttpApiV2ProxyResponse"/> representing an HTTP 200 OK with a text/plain content type.</returns>
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

    /// <summary>
    /// Creates an HTTP 201 Created response.
    /// </summary>
    /// <param name="location">The URI of the newly created resource. This will be set in the 'Location' header.</param>
    /// <param name="body">Optional. The object to serialize as JSON for the response body, typically representing the created resource. Defaults to null.</param>
    /// <returns>An <see cref="APIGatewayHttpApiV2ProxyResponse"/> representing an HTTP 201 Created.</returns>
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

    /// <summary>
    /// Creates an HTTP 204 No Content response.
    /// This response typically indicates that the request was successful but there is no content to return.
    /// </summary>
    /// <returns>An <see cref="APIGatewayHttpApiV2ProxyResponse"/> representing an HTTP 204 No Content.</returns>
    public static APIGatewayHttpApiV2ProxyResponse NoContent()
    {
        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = (int)HttpStatusCode.NoContent, // 204
            Body = null,
            Headers = null, // Explicitly null or empty is fine for 204
            IsBase64Encoded = false
        };
    }

    /// <summary>
    /// Creates an HTTP 400 Bad Request response.
    /// This response indicates that the server cannot or will not process the request due to something
    /// that is perceived to be a client error (e.g., malformed request syntax, invalid request message framing,
    /// or deceptive request routing).
    /// </summary>
    /// <param name="errorBody">Optional. An object to serialize as JSON for the error response body.
    /// If null, a default "Bad Request" message will be used.</param>
    /// <returns>An <see cref="APIGatewayHttpApiV2ProxyResponse"/> representing an HTTP 400 Bad Request.</returns>
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

    /// <summary>
    /// Creates an HTTP 401 Unauthorized response.
    /// This response indicates that the request has not been applied because it lacks valid authentication
    /// credentials for the target resource.
    /// </summary>
    /// <param name="errorBody">Optional. An object to serialize as JSON for the error response body.
    /// If null, a default "Unauthorized" message will be used.</param>
    /// <returns>An <see cref="APIGatewayHttpApiV2ProxyResponse"/> representing an HTTP 401 Unauthorized.</returns>
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

    /// <summary>
    /// Creates an HTTP 403 Forbidden response.
    /// This response indicates that the server understood the request but refuses to authorize it.
    /// Unlike 401, authentication will not help and the request should not be repeated.
    /// </summary>
    /// <param name="errorBody">Optional. An object to serialize as JSON for the error response body.
    /// If null, a default "Forbidden" message will be used.</param>
    /// <returns>An <see cref="APIGatewayHttpApiV2ProxyResponse"/> representing an HTTP 403 Forbidden.</returns>
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

    /// <summary>
    /// Creates an HTTP 404 Not Found response.
    /// This response indicates that the server has not found anything matching the Request-URI.
    /// No indication is given of whether the condition is temporary or permanent.
    /// </summary>
    /// <param name="errorBody">Optional. An object to serialize as JSON for the error response body.
    /// If null, a default "Not Found" message will be used.</param>
    /// <returns>An <see cref="APIGatewayHttpApiV2ProxyResponse"/> representing an HTTP 404 Not Found.</returns>
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

    /// <summary>
    /// Creates an HTTP 500 Internal Server Error response.
    /// This response indicates that the server encountered an unexpected condition that prevented it
    /// from fulfilling the request.
    /// </summary>
    /// <param name="errorBody">Optional. An object to serialize as JSON for the error response body.
    /// If null, a default "Internal Server Error" message will be used.
    /// Be cautious about leaking internal details in production environments.</param>
    /// <returns>An <see cref="APIGatewayHttpApiV2ProxyResponse"/> representing an HTTP 500 Internal Server Error.</returns>
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

    /// <summary>
    /// Creates a custom <see cref="APIGatewayHttpApiV2ProxyResponse"/> with the specified status code, body, and headers.
    /// This method provides flexibility for scenarios not covered by the other predefined methods.
    /// </summary>
    /// <param name="statusCode">The <see cref="HttpStatusCode"/> for the response.</param>
    /// <param name="body">Optional. The object for the response body.
    /// If headers specify 'text/plain' and body is a string, it's treated as plain text.
    /// Otherwise, it's serialized as JSON if not null. Defaults to null.</param>
    /// <param name="headers">Optional. A dictionary of headers for the response.
    /// If null and a body is provided, JSON headers will be used by default.
    /// If no body is provided, headers will be null unless specified. Defaults to null.</param>
    /// <returns>An <see cref="APIGatewayHttpApiV2ProxyResponse"/> configured with the provided parameters.</returns>
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