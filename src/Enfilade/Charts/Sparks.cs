using System.Globalization;

namespace Enfilade;

internal readonly record struct Spot(double X, double Y);

internal readonly record struct Pillar(double X, double Y, double Width, double Height);

internal static class Sparks
{
    public const double Width = 96;
    public const double Height = 24;
    public const double Margin = 2;

    private const double Fill = 0.7;
    private const double Thinnest = 1;

    public static IReadOnlyList<Spot> Line(IReadOnlyList<double> values)
    {
        if (values.Count == 0)
            return [];

        var low = values.Min();
        var span = values.Max() - low;
        return [.. values.Select((value, index) => new Spot(
            values.Count == 1 ? Width / 2 : Margin + index * (Width - 2 * Margin) / (values.Count - 1),
            span == 0 ? Height / 2 : Down(value, low, span)))];
    }

    public static IReadOnlyList<Pillar> Bars(IReadOnlyList<double> values)
    {
        if (values.Count == 0)
            return [];

        var low = Math.Min(0, values.Min());
        var span = Math.Max(0, values.Max()) - low;
        var floor = span == 0 ? Height - Margin : Down(0, low, span);
        var slot = (Width - 2 * Margin) / values.Count;
        return [.. values.Select((value, index) =>
        {
            var top = span == 0 ? floor : Down(value, low, span);
            return new Pillar(
                Margin + index * slot + slot * (1 - Fill) / 2,
                Math.Min(top, floor),
                slot * Fill,
                Math.Max(Math.Abs(top - floor), Thinnest));
        })];
    }

    public static string Number(double value) => Math.Round(value, 2).ToString("0.##", CultureInfo.InvariantCulture);

    private static double Down(double value, double low, double span) => Height - Margin - (value - low) / span * (Height - 2 * Margin);
}
