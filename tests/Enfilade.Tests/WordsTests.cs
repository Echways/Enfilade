using Enfilade.Tests.Hosts;

namespace Enfilade.Tests;

public class WordsTests
{
    private const string Clients =
        "{n, plural, one {{number} клиент} few {{number} клиента} many {{number} клиентов} other {{number} клиента}}";

    private static readonly DateTimeOffset Moment = new(2026, 10, 12, 2, 30, 0, TimeSpan.FromHours(3));

    public WordsTests() => Words.Zone = TimeZoneInfo.Utc;

    [Fact]
    public void Middle_keeps_short_text_whole() => Assert.Equal("abc", Words.Middle("abc", 10));

    [Fact]
    public void Middle_cuts_long_text_in_the_middle() =>
        Assert.Equal("tnk_AB…XYZ", Words.Middle("tnk_ABCDEFGHIJKLMNOPQRSTUVWXYZ", 10));

    [Theory]
    [InlineData(1, "1 клиент")]
    [InlineData(2, "2 клиента")]
    [InlineData(5, "5 клиентов")]
    [InlineData(21, "21 клиент")]
    public void Count_uses_russian_plural_forms(long count, string expected) => Assert.Equal(expected, Words.Count(Clients, count));

    [Fact]
    public void Number_groups_thousands_the_russian_way() => Assert.Equal("1 000", Words.Number(1000));

    [Fact]
    public void Day_minute_and_clock_are_in_utc()
    {
        Assert.Equal("2026-10-11", Words.Day(Moment));
        Assert.Equal("2026-10-11 23:30", Words.Minute(Moment));
        Assert.Equal("23:30", Words.Clock(Moment));
    }

    [Fact]
    public void Day_minute_and_clock_follow_the_viewer_zone()
    {
        Words.Zone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Yekaterinburg");

        Assert.Equal("2026-10-12", Words.Day(Moment));
        Assert.Equal("2026-10-12 04:30", Words.Minute(Moment));
        Assert.Equal("04:30", Words.Clock(Moment));
    }

    [Theory]
    [InlineData(0, "0\u00A0мс")]
    [InlineData(999, "999\u00A0мс")]
    [InlineData(1_240, "1,2\u00A0с")]
    [InlineData(12_600, "13\u00A0с")]
    public void Duration_switches_to_seconds_from_one_second(int milliseconds, string expected) =>
        Assert.Equal(expected, Words.Duration(milliseconds));

    [Theory]
    [InlineData(0, "0\u00A0Б")]
    [InlineData(1_023, "1\u00A0023\u00A0Б")]
    [InlineData(1_536, "1,5\u00A0КБ")]
    [InlineData(150 * 1_024, "150\u00A0КБ")]
    [InlineData(5L * 1_024 * 1_024 * 1_024 * 1_024, "5\u00A0120\u00A0ГБ")]
    public void Size_picks_the_largest_unit_below_1024(long bytes, string expected) => Assert.Equal(expected, Words.Size(bytes));

    [Theory]
    [InlineData(0, "0\u00A0%")]
    [InlineData(0.1234, "12,3\u00A0%")]
    [InlineData(1, "100\u00A0%")]
    public void Share_is_a_percent_with_one_decimal(double share, string expected) => Assert.Equal(expected, Words.Share(share));

    [Theory]
    [InlineData(null, "–")]
    [InlineData(1234, "1\u00A0234")]
    [InlineData(1234L, "1\u00A0234")]
    [InlineData((byte)5, "5")]
    [InlineData(0.25, "0,25")]
    [InlineData(2f, "2")]
    [InlineData(true, "Да")]
    [InlineData(false, "Нет")]
    [InlineData(OrderStatus.Paid, "Оплачен")]
    [InlineData(OrderStatus.New, "New")]
    [InlineData("text", "text")]
    public void Of_formats_by_type(object? value, string expected) => Assert.Equal(expected, Words.Of(value));

    [Fact]
    public void Of_formats_money_times_and_spans()
    {
        Assert.Equal("1\u00A0234,5", Words.Of(1234.5m));
        Assert.Equal(Words.Minute(Moment), Words.Of(Moment));
        Assert.Equal("2026-10-11 23:30", Words.Of(new DateTime(2026, 10, 11, 23, 30, 0)));
        Assert.Equal("2026-10-12", Words.Of(new DateOnly(2026, 10, 12)));
        Assert.Equal(Words.Duration(1_240), Words.Of(TimeSpan.FromMilliseconds(1_240)));
        Assert.Equal("2\u00A0592\u00A0000\u00A0с", Words.Of(TimeSpan.FromDays(30)));
    }
}
