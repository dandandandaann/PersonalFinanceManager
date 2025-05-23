using Amazon.Lambda.Core;
using Microsoft.Extensions.Logging;

namespace SharedLibrary.Lambda.LocalDevelopment;

public class LocalLambdaContext : ILambdaContext
{
    private readonly ILogger _logger;

    public string AwsRequestId { get; set; } = Guid.NewGuid().ToString();
    public IClientContext ClientContext { get; set; } = null!; // Or a mock if needed
    public string FunctionName { get; set; } = "LocalTestFunction";
    public string FunctionVersion { get; set; } = "$LATEST";
    public ICognitoIdentity Identity { get; set; } = null!; // Or a mock if needed
    public string InvokedFunctionArn { get; set; } = "arn:aws:lambda:us-east-1:123456789012:function:LocalTestFunction";
    public ILambdaLogger Logger => new LocalLambdaLogger(_logger);
    public string LogGroupName { get; set; } = "/aws/lambda/LocalTestFunction";
    public string LogStreamName { get; set; } = DateTime.UtcNow.ToString("yyyy/MM/dd") + "/[$LATEST]" + Guid.NewGuid().ToString("N");
    public int MemoryLimitInMB { get; set; } = 128;
    public TimeSpan RemainingTime => TimeSpan.FromSeconds(30); // Or a suitable default

    public LocalLambdaContext(ILogger logger, string? functionName = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        if (!string.IsNullOrWhiteSpace(functionName))
        {
            FunctionName = functionName;
            LogGroupName = $"/aws/lambda/{functionName}";
            InvokedFunctionArn = $"arn:aws:lambda:us-east-1:123456789012:function:{functionName}"; // Adjust account/region if needed
        }
    }
}

public class LocalLambdaLogger(ILogger msLogger) : ILambdaLogger
{
    public void Log(string level, string message, params object[] args)
    {
        Enum.TryParse<LogLevel>(level, true, out var logLevel);

        msLogger.Log(logLevel, message, args);
    }

    public void Log(string level, string message)
    {
        this.Log(level, message, args: null!);
    }


    // Implement ILambdaLogger.Log directly
    public void Log(string message)
    {
        // Use a specific log level from Microsoft.Extensions.Logging
        // Do NOT call _msLogger.LogInformation() here if that's an extension that might loop.
        // Call the core Log method on _msLogger.
        msLogger.Log(LogLevel.Information, message);
    }

    // Implement ILambdaLogger.LogLine directly
    public void LogLine(string message)
    {
        // For Microsoft.Extensions.Logging, Log and LogLine are often treated the same
        // as it typically handles newlines based on the sink.
        msLogger.Log(LogLevel.Information, message);
    }
}