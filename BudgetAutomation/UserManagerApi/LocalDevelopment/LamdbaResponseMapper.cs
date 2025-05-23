// using System.Text;
// using Amazon.Lambda.APIGatewayEvents;
// using Microsoft.Net.Http.Headers;
//
// namespace UserManagerApi.LocalDevelopment;
//
// public static class LambdaResponseMapper
// {
//     public static IResult ToMinimalApiResult(APIGatewayHttpApiV2ProxyResponse lambdaResponse)
//     {
//         var contentType = "application/json; charset=utf-8"; // Default
//         if (lambdaResponse.Headers != null)
//         {
//             if (lambdaResponse.Headers.TryGetValue(HeaderNames.ContentType, out var ctValue) ||
//                 lambdaResponse.Headers.TryGetValue("content-type", out ctValue))
//             {
//                 contentType = ctValue;
//             }
//         }
//
//         byte[]? bodyBytes = null;
//         string? bodyString = null;
//
//         if (!string.IsNullOrEmpty(lambdaResponse.Body))
//         {
//             if (lambdaResponse.IsBase64Encoded)
//             {
//                 bodyBytes = Convert.FromBase64String(lambdaResponse.Body);
//             }
//             else
//             {
//                 bodyString = lambdaResponse.Body;
//             }
//         }
//
//         string? locationHeader = null;
//         if (lambdaResponse is { StatusCode: StatusCodes.Status201Created, Headers: not null })
//         {
//             if (lambdaResponse.Headers.TryGetValue(HeaderNames.Location, out var locValue) ||
//                 lambdaResponse.Headers.TryGetValue("location", out locValue))
//             {
//                 locationHeader = locValue;
//             }
//         }
//
//         if (locationHeader != null)
//         {
//             object? createdBodyObject = null;
//             if (bodyBytes != null) {
//                  // If the created body is JSON text, deserialize or pass as string
//                 if (contentType.Contains("json") || contentType.StartsWith("text/")) {
//                     createdBodyObject = Encoding.UTF8.GetString(bodyBytes);
//                 } else {
//                     // For truly binary created body, Results.Created might not be ideal
//                     // as it often expects an object to serialize or a string.
//                     // You might need a more specialized handling or ensure created bodies are text.
//                     createdBodyObject = bodyBytes; // Pass as is, Results.Created might serialize it differently
//                 }
//             } else if (bodyString != null) {
//                 createdBodyObject = bodyString;
//             }
//             return Results.Created(locationHeader, createdBodyObject);
//         }
//
//         if (bodyBytes != null)
//         {
//             return new ByteArrayResult(bodyBytes, contentType, lambdaResponse.StatusCode);
//         }
//
//         if (bodyString != null)
//         {
//             return Results.Content(bodyString, contentType, Encoding.UTF8, lambdaResponse.StatusCode);
//         }
//
//         return Results.StatusCode(lambdaResponse.StatusCode);
//     }
// }
//
//
// // Custom IResult for byte array with status code and content type
// public class ByteArrayResult(byte[] bytes, string contentType, int statusCode) : IResult
// {
//     public Task ExecuteAsync(HttpContext httpContext)
//     {
//         httpContext.Response.StatusCode = statusCode;
//         httpContext.Response.ContentType = contentType;
//         return Task.FromResult(httpContext.Response.Body.WriteAsync(bytes, 0, bytes.Length));
//     }
// }