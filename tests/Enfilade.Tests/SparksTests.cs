namespace Enfilade.Tests;

public class SparksTests
{
    [Fact]
    public void Line_runs_from_edge_to_edge_with_the_highest_value_on_top()
    {
        var line = Sparks.Line([1, 3, 2]);

        Assert.Equal([2.0, 48.0, 94.0], line.Select(spot => spot.X));
        Assert.Equal([22.0, 2.0, 12.0], line.Select(spot => spot.Y));
    }

    [Fact]
    public void Flat_line_runs_through_the_middle() =>
        Assert.All(Sparks.Line([5, 5, 5]), spot => Assert.Equal(Sparks.Height / 2, spot.Y));

    [Fact]
    public void Single_value_sits_in_the_middle() =>
        Assert.Equal(new Spot(Sparks.Width / 2, Sparks.Height / 2), Assert.Single(Sparks.Line([7])));

    [Fact]
    public void Nothing_draws_nothing()
    {
        Assert.Empty(Sparks.Line([]));
        Assert.Empty(Sparks.Bars([]));
    }

    [Fact]
    public void Bars_grow_from_the_floor_in_proportion()
    {
        var bars = Sparks.Bars([0, 2, 4]);

        Assert.Equal([1.0, 10.0, 20.0], bars.Select(bar => bar.Height));
        Assert.Equal([22.0, 12.0, 2.0], bars.Select(bar => bar.Y));
    }

    [Fact]
    public void Bars_stay_inside_their_slots()
    {
        var bars = Sparks.Bars([1, 2, 3, 4]);

        Assert.All(bars, bar => Assert.InRange(bar.X, Sparks.Margin, Sparks.Width - Sparks.Margin - bar.Width));
        Assert.Equal(bars.Count, bars.Select(bar => bar.X).Distinct().Count());
    }

    [Fact]
    public void Negative_bars_hang_below_the_zero_line()
    {
        var bars = Sparks.Bars([-2, 2]);

        Assert.Equal([12.0, 2.0], bars.Select(bar => bar.Y));
        Assert.Equal([10.0, 10.0], bars.Select(bar => bar.Height));
    }

    [Fact]
    public void Zero_bars_stay_visible()
    {
        var bars = Sparks.Bars([0, 0]);

        Assert.All(bars, bar => Assert.Equal(1.0, bar.Height));
        Assert.All(bars, bar => Assert.Equal(22.0, bar.Y));
    }

    [Theory]
    [InlineData(2, "2")]
    [InlineData(2.5, "2.5")]
    [InlineData(2.3456, "2.35")]
    public void Numbers_are_written_the_same_in_every_culture(double value, string expected) =>
        Assert.Equal(expected, Sparks.Number(value));
}
