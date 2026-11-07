using Microsoft.Extensions.Logging;

namespace Enfilade;

internal static partial class Log
{
    [LoggerMessage(Level = LogLevel.Error, Message = "Enfilade: an unexpected failure was shown to the user")]
    public static partial void UnexpectedFailure(this ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Enfilade: the badge of the section {Section} could not be counted")]
    public static partial void BadgeFailed(this ILogger logger, Exception exception, string section);
}
