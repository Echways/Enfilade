using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using Jeffijoe.MessageFormat;

namespace Enfilade;

public static class Words
{
    private static readonly AsyncLocal<TimeZoneInfo?> ViewerZone = new();

    public static TimeZoneInfo Zone
    {
        get => ViewerZone.Value ?? TimeZoneInfo.Utc;
        internal set => ViewerZone.Value = value;
    }

    public static string Sentence(string text) => text.Length == 0 ? text : char.ToUpperInvariant(text[0]) + text[1..];

    public static string Day(DateTimeOffset moment) => ViewerTime.ToViewer(moment).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    public static string Minute(DateTimeOffset moment) => ViewerTime.ToViewer(moment).ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

    public static string Clock(DateTimeOffset moment, bool seconds = false) =>
        ViewerTime.ToViewer(moment).ToString(seconds ? "HH:mm:ss" : "HH:mm", CultureInfo.InvariantCulture);

    public static string Middle(string text, int length) =>
        text.Length <= length ? text : text[..(length * 3 / 5)] + "…" + text[^(length * 2 / 5 - 1)..];

    public static string Number(long value) => value.ToString("N0", CultureInfo.CurrentCulture);

    public static string Duration(int milliseconds) => milliseconds < 1_000
        ? string.Format(EnfiladeText.Milliseconds, Number(milliseconds))
        : string.Format(EnfiladeText.Seconds, (milliseconds / 1_000.0).ToString(milliseconds < 10_000 ? "0.#" : "N0", CultureInfo.CurrentCulture));

    public static string Size(long bytes)
    {
        string[] units = [EnfiladeText.Bytes, EnfiladeText.Kilobytes, EnfiladeText.Megabytes, EnfiladeText.Gigabytes];
        var value = (double)bytes;
        var unit = 0;
        while (value >= 1_024 && unit < units.Length - 1)
        {
            value /= 1_024;
            unit++;
        }

        return string.Format(units[unit], value.ToString(unit == 0 || value >= 100 ? "N0" : "0.#", CultureInfo.CurrentCulture));
    }

    public static string Share(double share) =>
        string.Format(EnfiladeText.Percent, Math.Round(100 * share, 1).ToString("0.#", CultureInfo.CurrentCulture));

    public static string OrDash(long value) => value == 0 ? "–" : Number(value);

    public static string Count(string pattern, long count) => MessageFormatter.Format(
        pattern, new Dictionary<string, object?> { ["n"] = count, ["number"] = Number(count) }, CultureInfo.CurrentUICulture);

    public static string Of(object? value) => value switch
    {
        null => "–",
        sbyte or byte or short or ushort or int or uint or long or ulong => ((IFormattable)value).ToString("N0", CultureInfo.CurrentCulture),
        decimal or double or float => ((IFormattable)value).ToString("#,##0.##", CultureInfo.CurrentCulture),
        DateTimeOffset moment => Minute(moment),
        DateTime moment => Minute(moment.Kind == DateTimeKind.Local ? new DateTimeOffset(moment) : new DateTimeOffset(DateTime.SpecifyKind(moment, DateTimeKind.Utc))),
        DateOnly day => day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        TimeSpan span => Duration((int)span.TotalMilliseconds),
        bool yes => yes ? EnfiladeText.Yes : EnfiladeText.No,
        Enum member => Named(member),
        _ => value.ToString() ?? "",
    };

    private static string Named(Enum member)
    {
        var name = member.ToString();
        return member.GetType().GetField(name)?.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? name;
    }
}
