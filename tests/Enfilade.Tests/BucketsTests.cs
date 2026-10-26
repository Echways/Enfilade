namespace Enfilade.Tests;

public class BucketsTests
{
    public BucketsTests() => Words.Zone = TimeZoneInfo.Utc;

    [Fact]
    public void Day_buckets_follow_the_viewer_zone()
    {
        Words.Zone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Yekaterinburg");
        var trend = new Trend("Заказы", [Reading(2026, 10, 11, 20, 1), Reading(2026, 10, 11, 18, 2)]);

        var buckets = Buckets.Of([trend], Period.Day, null, null);

        Assert.Equal([new DateTime(2026, 10, 11), new DateTime(2026, 10, 12)], buckets.Select(bucket => bucket.Start));
        Assert.Equal([2.0, 1.0], buckets.Select(bucket => bucket.Sums[0]));
    }

    [Fact]
    public void Gaps_are_zero()
    {
        var trend = new Trend("Заказы", [Reading(2026, 10, 1, 9, 4), Reading(2026, 10, 3, 9, 6)]);

        var buckets = Buckets.Of([trend], Period.Day, null, null);

        Assert.Equal([4.0, 0.0, 6.0], buckets.Select(bucket => bucket.Sums[0]));
    }

    [Fact]
    public void Weeks_start_on_monday()
    {
        var sunday = Reading(2026, 10, 25, 12, 1);

        var bucket = Assert.Single(Buckets.Of([new Trend("Заказы", [sunday])], Period.Week, null, null));

        Assert.Equal(new DateTime(2026, 10, 19), bucket.Start);
        Assert.Equal(DayOfWeek.Monday, bucket.Start.DayOfWeek);
    }

    [Fact]
    public void Months_and_hours_have_their_own_starts()
    {
        var reading = Reading(2026, 10, 25, 12, 1) with { At = new DateTimeOffset(2026, 10, 25, 12, 40, 0, TimeSpan.Zero) };

        Assert.Equal(new DateTime(2026, 10, 1), Assert.Single(Buckets.Of([new Trend("Заказы", [reading])], Period.Month, null, null)).Start);
        Assert.Equal(new DateTime(2026, 10, 25, 12, 0, 0), Assert.Single(Buckets.Of([new Trend("Заказы", [reading])], Period.Hour, null, null)).Start);
    }

    [Fact]
    public void Range_extends_the_buckets()
    {
        var trend = new Trend("Заказы", [Reading(2026, 10, 2, 9, 5)]);

        var buckets = Buckets.Of([trend], Period.Day, new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2026, 10, 3, 0, 0, 0, TimeSpan.Zero));

        Assert.Equal([0.0, 5.0, 0.0], buckets.Select(bucket => bucket.Sums[0]));
    }

    [Fact]
    public void Each_trend_gets_its_own_sum()
    {
        var orders = new Trend("Заказы", [Reading(2026, 10, 1, 9, 2), Reading(2026, 10, 1, 10, 3)]);
        var refunds = new Trend("Возвраты", [Reading(2026, 10, 1, 11, 1)]);

        var bucket = Assert.Single(Buckets.Of([orders, refunds], Period.Day, null, null));

        Assert.Equal([5.0, 1.0], bucket.Sums);
    }

    [Fact]
    public void Nothing_to_show_has_no_buckets() => Assert.Empty(Buckets.Of([new Trend("Заказы", [])], Period.Day, null, null));

    private static Reading Reading(int year, int month, int day, int hour, double value) =>
        new(new DateTimeOffset(year, month, day, hour, 0, 0, TimeSpan.Zero), value);
}
