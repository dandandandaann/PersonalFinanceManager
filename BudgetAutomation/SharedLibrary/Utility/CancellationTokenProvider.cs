namespace SharedLibrary.Utility;

public static class CancellationTokenProvider
{
    private static readonly TimeSpan DefaultGracefulStopTimeLimit = TimeSpan.FromSeconds(1);

    public static CancellationToken GetCancellationToken(TimeSpan remainingTime, TimeSpan? gracefulStopTimeLimit = null)
    {
        var limit = gracefulStopTimeLimit ?? DefaultGracefulStopTimeLimit;
        // Ensure we don't go negative if remaining time is already less than the limit
        var actualRemainingTime = remainingTime;
        var timeout = actualRemainingTime > limit ? actualRemainingTime.Subtract(limit) : TimeSpan.Zero;

        return new CancellationTokenSource(timeout).Token;
    }
}