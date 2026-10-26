namespace Enfilade;

internal sealed record Bucket(DateTime Start, IReadOnlyList<double> Sums);

internal static class Buckets
{
    public static IReadOnlyList<Bucket> Of(IReadOnlyList<Trend> trends, Period step, DateTimeOffset? from, DateTimeOffset? to)
    {
        var sums = new SortedDictionary<DateTime, double[]>();
        for (var index = 0; index < trends.Count; index++)
        {
            foreach (var reading in trends[index].Readings)
                Sums(sums, Start(reading.At, step), trends.Count)[index] += reading.Value;
        }

        foreach (var bound in new[] { from, to }.OfType<DateTimeOffset>())
            Sums(sums, Start(bound, step), trends.Count);
        if (sums.Count == 0)
            return [];

        var last = sums.Keys.Last();
        for (var start = sums.Keys.First(); start < last; start = Next(start, step))
            Sums(sums, start, trends.Count);
        return [.. sums.Select(pair => new Bucket(pair.Key, pair.Value))];
    }

    private static double[] Sums(SortedDictionary<DateTime, double[]> sums, DateTime start, int trends)
    {
        if (!sums.TryGetValue(start, out var values))
            sums[start] = values = new double[trends];
        return values;
    }

    private static DateTime Start(DateTimeOffset moment, Period step)
    {
        var local = ViewerTime.ToViewer(moment);
        return step switch
        {
            Period.Hour => local.Date.AddHours(local.Hour),
            Period.Week => local.Date.AddDays(-(((int)local.DayOfWeek + 6) % 7)),
            Period.Month => new DateTime(local.Year, local.Month, 1),
            _ => local.Date,
        };
    }

    private static DateTime Next(DateTime start, Period step) => step switch
    {
        Period.Hour => start.AddHours(1),
        Period.Week => start.AddDays(7),
        Period.Month => start.AddMonths(1),
        _ => start.AddDays(1),
    };
}
