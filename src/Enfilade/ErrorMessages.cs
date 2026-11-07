using Microsoft.Extensions.Logging;

namespace Enfilade;

public sealed class PanelException(string message) : Exception(message);

public interface IErrorMessage
{
    string? For(Exception exception);
}

public sealed class ErrorMessages(IEnumerable<IErrorMessage> messages)
{
    public string? Of(Exception exception) => exception is PanelException
        ? exception.Message
        : messages.Select(message => message.For(exception)).FirstOrDefault(text => !string.IsNullOrWhiteSpace(text));

    internal static bool Cancelled(Exception exception) =>
        exception is OperationCanceledException and not TaskCanceledException { InnerException: TimeoutException };

    internal string Describe(Exception exception, ILogger logger, string fallback)
    {
        if (Of(exception) is { } message)
            return message;

        logger.UnexpectedFailure(exception);
        return fallback;
    }
}

internal sealed class ErrorMessage<TException>(Func<TException, string?> message) : IErrorMessage
    where TException : Exception
{
    public string? For(Exception exception) => exception is TException known ? message(known) : null;
}
