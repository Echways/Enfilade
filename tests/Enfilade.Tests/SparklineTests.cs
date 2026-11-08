namespace Enfilade.Tests;

public class SparklineTests : PanelContext
{
    [Fact]
    public void Line_draws_every_value_and_marks_the_last_one()
    {
        var spark = Render<Sparkline>(parameters => parameters.Add(spark => spark.Values, [1, 3, 2]));

        Assert.Equal("2,22 48,2 94,12", spark.Find("polyline").GetAttribute("points"));
        var last = spark.Find("circle");
        Assert.Equal("94", last.GetAttribute("cx"));
        Assert.Equal("12", last.GetAttribute("cy"));
        Assert.Empty(spark.FindAll("rect"));
    }

    [Fact]
    public void Bars_draw_a_bar_per_value()
    {
        var spark = Render<Sparkline>(parameters => parameters
            .Add(spark => spark.Values, [1, 3, 2])
            .Add(spark => spark.Bars, true));

        Assert.Equal(3, spark.FindAll("rect").Count);
        Assert.Empty(spark.FindAll("polyline"));
        Assert.Empty(spark.FindAll("circle"));
    }

    [Fact]
    public void Without_values_nothing_is_drawn() =>
        Assert.Empty(Render<Sparkline>(parameters => parameters.Add(spark => spark.Values, [])).FindAll("svg"));

    [Theory]
    [InlineData(Tone.Neutral, "spark neutral")]
    [InlineData(Tone.Good, "spark good")]
    [InlineData(Tone.Warn, "spark warn")]
    [InlineData(Tone.Bad, "spark bad")]
    public void Tone_picks_the_class(Tone tone, string classes)
    {
        var spark = Render<Sparkline>(parameters => parameters.Add(spark => spark.Values, [1, 2]).Add(spark => spark.Tone, tone));

        Assert.Equal(classes, spark.Find("svg").GetAttribute("class"));
    }

    [Fact]
    public void Summary_makes_it_an_image_for_screen_readers()
    {
        var spark = Render<Sparkline>(parameters => parameters.Add(spark => spark.Values, [1, 2]).Add(spark => spark.Summary, "Заказы за неделю: рост"));

        var svg = spark.Find("svg");
        Assert.Equal("img", svg.GetAttribute("role"));
        Assert.Equal("Заказы за неделю: рост", svg.GetAttribute("aria-label"));
        Assert.Null(svg.GetAttribute("aria-hidden"));
    }

    [Fact]
    public void Without_a_summary_it_is_decoration()
    {
        var svg = Render<Sparkline>(parameters => parameters.Add(spark => spark.Values, [1, 2])).Find("svg");

        Assert.Equal("true", svg.GetAttribute("aria-hidden"));
        Assert.Null(svg.GetAttribute("role"));
    }
}
