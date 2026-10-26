namespace Enfilade.Tests;

public class FieldKindTests
{
    [Theory]
    [InlineData(typeof(string), false, "Text")]
    [InlineData(typeof(string), true, "Select")]
    [InlineData(typeof(int), false, "Number")]
    [InlineData(typeof(long?), false, "Number")]
    [InlineData(typeof(decimal), false, "Number")]
    [InlineData(typeof(double?), false, "Number")]
    [InlineData(typeof(bool), false, "Switch")]
    [InlineData(typeof(bool?), false, "Check")]
    [InlineData(typeof(DayOfWeek), false, "Select")]
    [InlineData(typeof(DayOfWeek?), false, "Select")]
    [InlineData(typeof(DateOnly), false, "Date")]
    [InlineData(typeof(DateOnly?), false, "Date")]
    [InlineData(typeof(DateTime), false, "Moment")]
    [InlineData(typeof(DateTimeOffset?), false, "Moment")]
    public void Input_follows_the_type(Type type, bool options, string kind) => Assert.Equal(kind, FieldKinds.Of(type, options).ToString());

    [Fact]
    public void Unsupported_type_names_itself() =>
        Assert.Contains(nameof(Guid), Assert.Throws<NotSupportedException>(() => FieldKinds.Of(typeof(Guid), false)).Message);
}
