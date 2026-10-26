namespace Enfilade.Tests;

public class ViewerTimeTests
{
    public ViewerTimeTests() => Words.Zone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");

    [Fact]
    public void Viewer_sees_wall_clock_time() =>
        Assert.Equal(new DateTime(2026, 7, 1, 12, 0, 0), ViewerTime.ToViewer(new DateTimeOffset(2026, 7, 1, 10, 0, 0, TimeSpan.Zero)));

    [Fact]
    public void Entered_time_is_stored_in_utc()
    {
        var stored = ViewerTime.FromViewer(new DateTime(2026, 7, 1, 12, 0, 0));

        Assert.Equal(new DateTimeOffset(2026, 7, 1, 10, 0, 0, TimeSpan.Zero), stored);
        Assert.Equal(TimeSpan.Zero, stored.Offset);
    }

    [Fact]
    public void Time_inside_the_spring_gap_moves_past_it() =>
        Assert.Equal(new DateTimeOffset(2026, 3, 29, 1, 30, 0, TimeSpan.Zero), ViewerTime.FromViewer(new DateTime(2026, 3, 29, 2, 30, 0)));

    [Fact]
    public void Ambiguous_autumn_time_takes_standard_time() =>
        Assert.Equal(new DateTimeOffset(2026, 10, 25, 1, 30, 0, TimeSpan.Zero), ViewerTime.FromViewer(new DateTime(2026, 10, 25, 2, 30, 0)));
}
