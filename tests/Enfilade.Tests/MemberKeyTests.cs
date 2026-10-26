using System.Linq.Expressions;
using Order = Enfilade.Tests.ListQueryTests.Order;

namespace Enfilade.Tests;

public class MemberKeyTests
{
    [Fact]
    public void Member_key_is_camel_case() => Assert.Equal("customer", MemberKey.Of((Expression<Func<Order, string>>)(order => order.Customer)));

    [Fact]
    public void Nested_members_join_with_dots() =>
        Assert.Equal("buyer.name", MemberKey.Of((Expression<Func<Order, string>>)(order => order.Buyer!.Name)));

    [Fact]
    public void Boxing_is_seen_through() => Assert.Equal("total", MemberKey.Of((Expression<Func<Order, object?>>)(order => order.Total)));

    [Fact]
    public void Computed_value_has_no_key() => Assert.Null(MemberKey.Of((Expression<Func<Order, decimal>>)(order => order.Total * 2)));
}
