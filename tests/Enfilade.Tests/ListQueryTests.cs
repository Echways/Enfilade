using System.Linq.Expressions;

namespace Enfilade.Tests;

public class ListQueryTests
{
    private static readonly IQueryable<Order> Orders = new[]
    {
        new Order("Анна", 300, Status.Paid), new Order("борис", 100, Status.New), new Order("Вера", 200, Status.Paid),
    }.AsQueryable();

    private static readonly Expression<Func<Order, decimal>> Total = order => order.Total;

    [Fact]
    public void Page_takes_its_slice_and_counts_everything()
    {
        var page = Orders.ToPage(new ListQuery<Order> { Page = 2, Size = 2, Order = Total });

        Assert.Equal(3, page.Total);
        Assert.Equal(["Анна"], page.Items.Select(order => order.Customer));
    }

    [Fact]
    public void Descending_order_reverses()
    {
        var page = Orders.ToPage(new ListQuery<Order> { Order = Total, Sort = new SortOrder("total", true) });

        Assert.Equal([300m, 200m, 100m], page.Items.Select(order => order.Total));
    }

    [Fact]
    public void Search_ignores_case_across_fields()
    {
        var condition = Conditions.Search<Order>([order => order.Customer], "БОР");

        Assert.Equal(["борис"], Orders.Where(condition).Select(order => order.Customer));
    }

    [Fact]
    public void Equal_condition_matches_the_value()
    {
        var query = new ListQuery<Order> { Conditions = [Conditions.Equal<Order, Status>(order => order.Status, Status.Paid)] };

        Assert.Equal(2, Orders.Filter(query).Count());
    }

    [Fact]
    public void Boxed_order_still_sorts_by_the_value() =>
        Assert.Equal("борис", Orders.Slice(new ListQuery<Order> { Order = (Expression<Func<Order, object?>>)(order => order.Total) }).First().Customer);

    [Theory]
    [InlineData("total", "total", false)]
    [InlineData("-total", "total", true)]
    public void Sort_reads_its_text(string text, string key, bool descending) =>
        Assert.Equal(new SortOrder(key, descending), SortOrder.Parse(text));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("-")]
    public void Empty_sort_text_means_no_sort(string? text) => Assert.Null(SortOrder.Parse(text));

    [Fact]
    public void Sort_writes_the_text_it_reads() => Assert.Equal("-total", new SortOrder("total", true).ToString());

    public enum Status { New, Paid }

    public sealed record Buyer(string Name);

    public sealed record Order(string Customer, decimal Total, Status Status, Buyer? Buyer = null);
}
