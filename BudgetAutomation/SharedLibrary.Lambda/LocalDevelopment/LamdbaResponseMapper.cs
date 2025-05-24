// This file was mostly AI generated.

using System.Text;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace SharedLibrary.Lambda.LocalDevelopment;

public static class LambdaResponseMapper
{
    public static IResult ToMinimalApiResult(APIGatewayHttpApiV2ProxyResponse lambdaResponse)
    {
        var contentType = "application/json; charset=utf-8"; // Default
        if (lambdaResponse.Headers != null)
        {
            if (lambdaResponse.Headers.TryGetValue(HeaderNames.ContentType, out var ctValue) ||
                lambdaResponse.Headers.TryGetValue("content-type", out ctValue))
            {
                contentType = ctValue;
            }
        }

        byte[]? bodyBytes = null;
        string? bodyString = null;

        if (!string.IsNullOrEmpty(lambdaResponse.Body))
        {
            if (lambdaResponse.IsBase64Encoded)
            {
                bodyBytes = Convert.FromBase64String(lambdaResponse.Body);
            }
            else
            {
                bodyString = lambdaResponse.Body;
            }
        }

        string? locationHeader = null;
        if (lambdaResponse.StatusCode == StatusCodes.Status201Created && lambdaResponse.Headers != null)
        {
            if (lambdaResponse.Headers.TryGetValue(HeaderNames.Location, out var locValue) ||
                lambdaResponse.Headers.TryGetValue("location", out locValue)) // Case-insensitive
            {
                locationHeader = locValue;
            }
        }

        // Handle 201 Created with Location specifically
        if (locationHeader != null)
        {
            if (bodyString != null) // If the body is a string (potentially JSON)
                return new CreatedWithRawBodyResult(locationHeader, bodyString, contentType);

            if (bodyBytes == null)
                return new CreatedWithRawBodyResult(locationHeader, null, contentType);

            if (!contentType.Contains("json") && !contentType.StartsWith("text/"))
                return new CreatedWithRawBodyResult(locationHeader, null, contentType);

            var decodedStringBody = Encoding.UTF8.GetString(bodyBytes);
            return new CreatedWithRawBodyResult(locationHeader, decodedStringBody, contentType);
        }

        if (bodyBytes != null)
        {
            return new ByteArrayResult(bodyBytes, contentType, lambdaResponse.StatusCode);
        }

        if (bodyString != null)
        {
            return Results.Content(bodyString, contentType, Encoding.UTF8, lambdaResponse.StatusCode);
        }

        return Results.StatusCode(lambdaResponse.StatusCode);
    }
}


// Custom IResult for byte array with status code and content type
public class ByteArrayResult(byte[] bytes, string contentType, int statusCode) : IResult
{
    public Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = contentType;
        return httpContext.Response.Body.WriteAsync(bytes, 0, bytes.Length);
    }
}

public class CreatedWithRawBodyResult(string location, string? body, string contentType) : IResult
{
    public Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = StatusCodes.Status201Created;
        httpContext.Response.Headers[HeaderNames.Location] = location;

        if (string.IsNullOrEmpty(body))
            return Task.CompletedTask;

        httpContext.Response.ContentType = contentType;
        // httpContext.Response.ContentLength = Encoding.UTF8.GetByteCount(_body); // Optional
        return httpContext.Response.WriteAsync(body, Encoding.UTF8);
    }
}