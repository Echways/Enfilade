using System.Linq.Expressions;
using Order = Enfilade.Tests.ListQueryTests.Order;

namespace Enfilade.Tests;

public class ExpressionShapeTests
{
    [Fact]
    public void Same_lambda_written_twice_is_the_same() =>
        Assert.True(ExpressionShape.Same(Of(order => order.Total * 2 + order.Customer.Length), Of(order => order.Total * 2 + order.Customer.Length)));

    [Fact]
    public void Another_member_differs() => Assert.False(ExpressionShape.Same(Of(order => order.Total), Of(order => order.Customer.Length)));

    [Fact]
    public void Another_constant_differs() => Assert.False(ExpressionShape.Same(Of(order => order.Total * 2), Of(order => order.Total * 3)));

    [Fact]
    public void Another_operator_differs() => Assert.False(ExpressionShape.Same(Of(order => order.Total * 2), Of(order => order.Total + 2)));

    [Fact]
    public void Captured_values_are_compared_by_reference()
    {
        var first = new Order("Анна", 1, ListQueryTests.Status.Paid);
        var second = first with { };

        Assert.True(ExpressionShape.Same(Of(order => order.Total + first.Total), Of(order => order.Total + first.Total)));
        Assert.False(ExpressionShape.Same(Of(order => order.Total + first.Total), Of(order => order.Total + second.Total)));
    }

    [Fact]
    public void Nested_parameters_keep_their_places()
    {
        Expression<Func<int, Func<int, int>>> outer = left => right => left;
        Expression<Func<int, Func<int, int>>> inner = left => right => right;

        Assert.False(ExpressionShape.Same(outer, inner));
    }

    [Fact]
    public void Nothing_before_is_never_the_same() => Assert.False(ExpressionShape.Same(null, Of(order => order.Total)));

    private static Expression<Func<Order, decimal>> Of(Expression<Func<Order, decimal>> value) => value;
}
