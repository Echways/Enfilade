namespace Enfilade.Tests;

public class ListingAddressTests
{
    [Fact]
    public void Address_reads_its_keys()
    {
        var address = ListingAddress.Read("http://x/orders?page=3&sort=-total&q=ан&status=paid", null);

        Assert.Equal(3, address.Page);
        Assert.Equal(new SortOrder("total", true), address.Sort);
        Assert.Equal("ан", address.Search);
        Assert.Equal("paid", address.Filters["status"]);
    }

    [Fact]
    public void Prefixed_address_reads_only_its_own_keys()
    {
        var address = ListingAddress.Read("http://x/?a.page=2&page=5&a.status=paid&status=new", "a");

        Assert.Equal(2, address.Page);
        Assert.Equal("paid", Assert.Single(address.Filters).Value);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("-3")]
    [InlineData("0")]
    [InlineData("99999999999")]
    public void Bad_page_numbers_mean_the_first_page(string page) =>
        Assert.Equal(1, ListingAddress.Read($"http://x/?page={page}", null).Page);

    [Fact]
    public void First_page_stays_out_of_the_address()
    {
        var address = new ListingAddress(null, 1, null, null, new Dictionary<string, string>());

        Assert.Null(address.ToQuery()["page"]);
    }

    [Fact]
    public void Prefix_names_every_key()
    {
        var address = new ListingAddress("a", 2, "ан", new SortOrder("total", true), new Dictionary<string, string> { ["status"] = "paid" });

        Assert.Equal(
            new Dictionary<string, object?> { ["a.page"] = "2", ["a.sort"] = "-total", ["a.q"] = "ан", ["a.status"] = "paid" },
            address.ToQuery());
    }
}
