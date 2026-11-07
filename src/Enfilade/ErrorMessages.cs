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
}

internal sealed class ErrorMessage<TException>(Func<TException, string?> message) : IErrorMessage
    where TException : Exception
{
    public string? For(Exception exception) => exception is TException known ? message(known) : null;
}
