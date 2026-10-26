namespace Enfilade;

internal static class ViewerTime
{
    public static DateTime ToViewer(DateTimeOffset moment) => TimeZoneInfo.ConvertTime(moment, Words.Zone).DateTime;

    public static DateTimeOffset FromViewer(DateTime wallClock)
    {
        var zone = Words.Zone;
        var local = DateTime.SpecifyKind(wallClock, DateTimeKind.Unspecified);
        if (zone.IsInvalidTime(local))
            local += DaylightDelta(zone, local);

        var offset = zone.IsAmbiguousTime(local) ? zone.BaseUtcOffset : zone.GetUtcOffset(local);
        return new DateTimeOffset(local - offset, TimeSpan.Zero);
    }

    private static TimeSpan DaylightDelta(TimeZoneInfo zone, DateTime local) =>
        zone.GetAdjustmentRules().FirstOrDefault(rule => rule.DateStart <= local && local <= rule.DateEnd)?.DaylightDelta
        ?? TimeSpan.FromHours(1);
}
