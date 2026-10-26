namespace Enfilade;

public sealed class FieldException(IReadOnlyDictionary<string, string> errors) : Exception(string.Join("; ", errors.Values))
{
    public FieldException(string field, string message)
        : this(new Dictionary<string, string> { [field] = message })
    {
    }

    public IReadOnlyDictionary<string, string> Errors { get; } = errors;
}
