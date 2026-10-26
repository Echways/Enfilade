using Microsoft.Extensions.Options;

namespace Enfilade.Tests;

public class ViewerZoneTests
{
    [Fact]
    public void Configured_zone_is_known_at_once()
    {
        var zone = new ViewerZone(Options.Create(new EnfiladeOptions { TimeZone = "UTC" }));

        Assert.True(zone.Known);
        Assert.Equal(TimeZoneInfo.Utc, zone.Zone);
    }

    [Fact]
    public void Browser_zone_settles_the_viewer()
    {
        var zone = new ViewerZone(Options.Create(new EnfiladeOptions()));
        Assert.False(zone.Known);

        zone.Settle("Asia/Tokyo");

        Assert.True(zone.Known);
        Assert.Equal(TimeZoneInfo.FindSystemTimeZoneById("Asia/Tokyo").Id, zone.Zone.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Mars/Olympus")]
    public void Unknown_or_missing_browser_zone_falls_back_to_utc(string? id)
    {
        var zone = new ViewerZone(Options.Create(new EnfiladeOptions()));

        zone.Settle(id);

        Assert.True(zone.Known);
        Assert.Equal(TimeZoneInfo.Utc, zone.Zone);
    }
}
