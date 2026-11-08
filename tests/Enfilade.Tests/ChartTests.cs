using System.Globalization;
using MudBlazor;

namespace Enfilade.Tests;

public class ChartTests : PanelContext
{
    [Fact]
    public void Chart_renders_one_series_per_trend_with_labels()
    {
        var orders = new Trend("Заказы", [Reading(1, 4), Reading(3, 6)]);
        var refunds = new Trend("Возвраты", [Reading(2, 1)]);

        var chart = Render<Chart>(parameters => parameters.Add(chart => chart.Trends, [orders, refunds]));

        var mud = chart.FindComponent<MudChart<double>>().Instance;
        Assert.Equal(["Заказы", "Возвраты"], mud.ChartSeries.Select(series => series.Name));
        Assert.Equal([4.0, 0.0, 6.0], mud.ChartSeries[0].Data.Values);
        Assert.Equal(
            Enumerable.Range(1, 3).Select(day => new DateTime(2026, 10, day).ToString("d MMM", CultureInfo.CurrentCulture)),
            mud.ChartLabels);
        Assert.Equal(ChartType.Line, mud.ChartType);
    }

    [Fact]
    public void Bars_and_months_change_the_kind_and_labels()
    {
        var chart = Render<Chart>(parameters => parameters
            .Add(chart => chart.Trends, [new Trend("Заказы", [Reading(1, 4)])])
            .Add(chart => chart.Kind, ChartKind.Bars)
            .Add(chart => chart.Step, Period.Month));

        var mud = chart.FindComponent<MudChart<double>>().Instance;
        Assert.Equal(ChartType.Bar, mud.ChartType);
        Assert.Equal([new DateTime(2026, 10, 1).ToString("MMM yyyy", CultureInfo.CurrentCulture)], mud.ChartLabels);
    }

    [Fact]
    public void Long_axes_keep_only_every_few_labels_so_they_stay_readable()
    {
        var readings = Enumerable.Range(1, 30).Select(day => Reading(day, day)).ToList();

        var chart = Render<Chart>(parameters => parameters.Add(chart => chart.Trends, [new Trend("Заказы", readings)]));

        var mud = chart.FindComponent<MudChart<double>>().Instance;
        Assert.Equal(30, mud.ChartLabels.Length);
        Assert.Equal(10, mud.ChartLabels.Count(label => label.Length > 0));
        Assert.Equal(new DateTime(2026, 10, 1).ToString("d MMM", CultureInfo.CurrentCulture), mud.ChartLabels[0]);
        Assert.Equal("", mud.ChartLabels[1]);
    }

    private static Reading Reading(int day, double value) => new(new DateTimeOffset(2026, 10, day, 12, 0, 0, TimeSpan.Zero), value);
}
