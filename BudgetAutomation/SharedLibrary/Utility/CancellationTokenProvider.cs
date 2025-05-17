namespace SharedLibrary.Utility;

public static class CancellationTokenProvider
{
    private static readonly TimeSpan DefaultGracefulStopTimeLimit = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Generates a cancellation token that triggers slightly before the specified remaining time expires,
    /// allowing a graceful shutdown. The cutoff is controlled by an optional grace period.
    /// </summary>
    /// <param name="remainingTime">The total time remaining for execution (e.g., from <see cref="Amazon.Lambda.Core.ILambdaContext.RemainingTime"/>).</param>
    /// <param name="gracefulStopTimeLimit">
    /// Optional time to reserve before the actual timeout for graceful termination. Defaults to 1 second.
    /// </param>
    /// <returns>A CancellationToken that will be triggered before the remaining time elapses.</returns>
    public static CancellationToken GetCancellationToken(TimeSpan remainingTime, TimeSpan? gracefulStopTimeLimit = null)
    {
        var limit = gracefulStopTimeLimit ?? DefaultGracefulStopTimeLimit;
        var actualRemainingTime = remainingTime;
        // Ensure we don't go negative if remaining time is already less than the limit
        var timeout = actualRemainingTime > limit ? actualRemainingTime.Subtract(limit) : TimeSpan.Zero;

        return new CancellationTokenSource(timeout).Token;
    }
}