using Microsoft.Extensions.Options;

namespace Enfilade;

public sealed class ViewerTime(IOptions<EnfiladeOptions> options)
{
    public TimeZoneInfo Zone { get; private set; } =
        options.Value.TimeZone is { } id ? TimeZoneInfo.FindSystemTimeZoneById(id) : TimeZoneInfo.Utc;

    internal bool Known { get; private set; } = options.Value.TimeZone is not null;

    public string Day(DateTimeOffset moment) => Words.Day(moment, Zone);

    public string Minute(DateTimeOffset moment) => Words.Minute(moment, Zone);

    public string Clock(DateTimeOffset moment, bool seconds = false) => Words.Clock(moment, Zone, seconds);

    public string Of(object? value) => Words.Of(value, Zone);

    public DateTime ToViewer(DateTimeOffset moment) => TimeZoneInfo.ConvertTime(moment, Zone).DateTime;

    public DateTimeOffset FromViewer(DateTime wallClock)
    {
        var local = DateTime.SpecifyKind(wallClock, DateTimeKind.Unspecified);
        if (Zone.IsInvalidTime(local))
            local += DaylightDelta(Zone, local);

        var offset = Zone.IsAmbiguousTime(local) ? Zone.BaseUtcOffset : Zone.GetUtcOffset(local);
        return new DateTimeOffset(local - offset, TimeSpan.Zero);
    }

    internal void Settle(string? id)
    {
        Zone = id is not null && TimeZoneInfo.TryFindSystemTimeZoneById(id, out var zone) ? zone : TimeZoneInfo.Utc;
        Known = true;
    }

    private static TimeSpan DaylightDelta(TimeZoneInfo zone, DateTime local) =>
        zone.GetAdjustmentRules().FirstOrDefault(rule => rule.DateStart <= local && local <= rule.DateEnd)?.DaylightDelta
        ?? TimeSpan.FromHours(1);
}
