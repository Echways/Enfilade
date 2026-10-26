namespace Enfilade;

public sealed record SortOrder(string Key, bool Descending)
{
    public static SortOrder? Parse(string? text)
    {
        var descending = text?.StartsWith('-') == true;
        var key = descending ? text![1..] : text;
        return string.IsNullOrEmpty(key) ? null : new SortOrder(key, descending);
    }

    public override string ToString() => Descending ? "-" + Key : Key;
}
