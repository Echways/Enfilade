using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using Jeffijoe.MessageFormat;

namespace Enfilade;

public static class Words
{
    public static string Sentence(string text) => text.Length == 0 ? text : char.ToUpperInvariant(text[0]) + text[1..];

    public static string Day(DateTimeOffset moment, TimeZoneInfo zone) => In(moment, zone, "yyyy-MM-dd");

    public static string Minute(DateTimeOffset moment, TimeZoneInfo zone) => In(moment, zone, "yyyy-MM-dd HH:mm");

    public static string Clock(DateTimeOffset moment, TimeZoneInfo zone, bool seconds = false) => In(moment, zone, seconds ? "HH:mm:ss" : "HH:mm");

    public static string Middle(string text, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);
        if (text.Length <= length)
            return text;

        var head = length * 3 / 5;
        return string.Concat(text.AsSpan(0, head), "…", text.AsSpan(text.Length - (length - head - 1)));
    }

    public static string Number(long value) => value.ToString("N0", CultureInfo.CurrentCulture);

    public static string Duration(long milliseconds) => milliseconds < 1_000
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

    public static string Of(object? value, TimeZoneInfo zone) => value switch
    {
        null => "–",
        sbyte or byte or short or ushort or int or uint or long or ulong => ((IFormattable)value).ToString("N0", CultureInfo.CurrentCulture),
        decimal or double or float => ((IFormattable)value).ToString("#,##0.##", CultureInfo.CurrentCulture),
        DateTimeOffset moment => Minute(moment, zone),
        DateTime moment => Minute(moment.Kind == DateTimeKind.Local ? new DateTimeOffset(moment) : new DateTimeOffset(DateTime.SpecifyKind(moment, DateTimeKind.Utc)), zone),
        DateOnly day => day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        TimeSpan span => Duration(span.Ticks / TimeSpan.TicksPerMillisecond),
        bool yes => yes ? EnfiladeText.Yes : EnfiladeText.No,
        Enum member => Named(member),
        _ => value.ToString() ?? "",
    };

    private static string In(DateTimeOffset moment, TimeZoneInfo zone, string format) =>
        TimeZoneInfo.ConvertTime(moment, zone).ToString(format, CultureInfo.InvariantCulture);

    internal static string Named(Enum member)
    {
        var name = member.ToString();
        return member.GetType().GetField(name)?.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? name;
    }
}
