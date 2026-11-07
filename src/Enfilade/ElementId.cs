namespace Enfilade;

internal static class ElementId
{
    public static string New(string prefix) => $"{prefix}-{Guid.NewGuid().ToString("N")[..8]}";
}
