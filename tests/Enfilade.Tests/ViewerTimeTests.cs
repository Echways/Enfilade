using Microsoft.Extensions.Options;

namespace Enfilade.Tests;

public class ViewerTimeTests
{
    private readonly ViewerTime _berlin = Settled("Europe/Berlin");

    [Fact]
    public void Viewer_sees_wall_clock_time() =>
        Assert.Equal(new DateTime(2026, 7, 1, 12, 0, 0), _berlin.ToViewer(new DateTimeOffset(2026, 7, 1, 10, 0, 0, TimeSpan.Zero)));

    [Fact]
    public void Entered_time_is_stored_in_utc()
    {
        var stored = _berlin.FromViewer(new DateTime(2026, 7, 1, 12, 0, 0));

        Assert.Equal(new DateTimeOffset(2026, 7, 1, 10, 0, 0, TimeSpan.Zero), stored);
        Assert.Equal(TimeSpan.Zero, stored.Offset);
    }

    [Fact]
    public void Time_inside_the_spring_gap_moves_past_it() =>
        Assert.Equal(new DateTimeOffset(2026, 3, 29, 1, 30, 0, TimeSpan.Zero), _berlin.FromViewer(new DateTime(2026, 3, 29, 2, 30, 0)));

    [Fact]
    public void Ambiguous_autumn_time_takes_standard_time() =>
        Assert.Equal(new DateTimeOffset(2026, 10, 25, 1, 30, 0, TimeSpan.Zero), _berlin.FromViewer(new DateTime(2026, 10, 25, 2, 30, 0)));

    [Fact]
    public void Formats_follow_the_viewer_zone()
    {
        var moment = new DateTimeOffset(2026, 7, 1, 22, 30, 0, TimeSpan.Zero);

        Assert.Equal("2026-07-02", _berlin.Day(moment));
        Assert.Equal("2026-07-02 00:30", _berlin.Minute(moment));
        Assert.Equal("00:30", _berlin.Clock(moment));
        Assert.Equal("2026-07-02 00:30", _berlin.Of(moment));
    }

    [Fact]
    public void Configured_zone_is_known_at_once()
    {
        var time = new ViewerTime(Options.Create(new EnfiladeOptions { TimeZone = "UTC" }));

        Assert.True(time.Known);
        Assert.Equal(TimeZoneInfo.Utc, time.Zone);
    }

    [Fact]
    public void Browser_zone_settles_the_viewer()
    {
        var time = new ViewerTime(Options.Create(new EnfiladeOptions()));
        Assert.False(time.Known);

        time.Settle("Asia/Tokyo");

        Assert.True(time.Known);
        Assert.Equal(TimeZoneInfo.FindSystemTimeZoneById("Asia/Tokyo").Id, time.Zone.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Mars/Olympus")]
    public void Unknown_or_missing_browser_zone_falls_back_to_utc(string? id)
    {
        var time = new ViewerTime(Options.Create(new EnfiladeOptions()));

        time.Settle(id);

        Assert.True(time.Known);
        Assert.Equal(TimeZoneInfo.Utc, time.Zone);
    }

    private static ViewerTime Settled(string zone)
    {
        var time = new ViewerTime(Options.Create(new EnfiladeOptions()));
        time.Settle(zone);
        return time;
    }
}
