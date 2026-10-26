using Microsoft.Extensions.Options;

namespace Enfilade;

internal sealed class ViewerZone(IOptions<EnfiladeOptions> options)
{
    public TimeZoneInfo Zone { get; private set; } =
        options.Value.TimeZone is { } id ? TimeZoneInfo.FindSystemTimeZoneById(id) : TimeZoneInfo.Utc;

    public bool Known { get; private set; } = options.Value.TimeZone is not null;

    public void Settle(string? id)
    {
        Zone = id is not null && TimeZoneInfo.TryFindSystemTimeZoneById(id, out var zone) ? zone : TimeZoneInfo.Utc;
        Known = true;
    }
}
