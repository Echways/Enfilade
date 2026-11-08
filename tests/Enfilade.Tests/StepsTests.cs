namespace Enfilade.Tests;

public class StepsTests : PanelContext
{
    private static readonly Step[] Items =
    [
        new("Оплата", StepState.Done, "12 мая"),
        new("Сборка", StepState.Current),
        new("Доставка", StepState.Pending),
        new("Возврат", StepState.Failed),
    ];

    [Fact]
    public void Every_step_is_listed_in_order_with_its_state_class()
    {
        var steps = Render<Steps>(parameters => parameters.Add(steps => steps.Items, Items));

        var rows = steps.FindAll("li");
        Assert.Equal(["Оплата", "Сборка", "Доставка", "Возврат"], rows.Select(row => row.QuerySelector(".step-title")!.TextContent));
        Assert.Equal(["done", "current", "pending", "failed"], rows.Select(row => row.ClassList.Last()));
    }

    [Fact]
    public void Only_the_current_step_is_marked_as_the_current_one()
    {
        var steps = Render<Steps>(parameters => parameters.Add(steps => steps.Items, Items));

        var current = Assert.Single(steps.FindAll("[aria-current]"));
        Assert.Equal("Сборка", current.QuerySelector(".step-title")!.TextContent);
    }

    [Fact]
    public void Detail_is_shown_only_when_given()
    {
        var steps = Render<Steps>(parameters => parameters.Add(steps => steps.Items, Items));

        Assert.Equal("12 мая", Assert.Single(steps.FindAll(".step-detail")).TextContent);
    }

    [Fact]
    public void State_is_also_told_in_words_for_screen_readers()
    {
        var steps = Render<Steps>(parameters => parameters.Add(steps => steps.Items, Items));

        Assert.Equal(
            [EnfiladeText.StepDone, EnfiladeText.StepCurrent, EnfiladeText.StepFailed],
            steps.FindAll(".sr").Select(text => text.TextContent));
    }
}
