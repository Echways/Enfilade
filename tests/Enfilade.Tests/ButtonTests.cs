namespace Enfilade.Tests;

public class ButtonTests : PanelContext
{
    [Fact]
    public void Click_runs_the_action_once_even_when_pressed_twice()
    {
        var pending = new TaskCompletionSource();
        var clicks = 0;
        var button = Render<Button>(parameters => parameters.Add(button => button.OnClick, async () =>
        {
            clicks++;
            await pending.Task;
        }));

        button.Find("button").Click();
        button.Find("button").Click();
        pending.SetResult();

        button.WaitForAssertion(() => Assert.False(button.Find("button").HasAttribute("disabled")));
        Assert.Equal(1, clicks);
    }

    [Fact]
    public void Busy_button_is_disabled_and_says_so()
    {
        var pending = new TaskCompletionSource();
        var button = Render<Button>(parameters => parameters.Add(button => button.OnClick, () => pending.Task));

        button.Find("button").Click();

        var element = button.Find("button");
        Assert.True(element.HasAttribute("disabled"));
        Assert.Equal("true", element.GetAttribute("aria-busy"));
        pending.SetResult();
    }

    [Theory]
    [InlineData(ButtonKind.Primary, "btn-k pigment")]
    [InlineData(ButtonKind.Danger, "btn-danger")]
    [InlineData(ButtonKind.Quiet, "link")]
    public void Kind_picks_the_existing_classes(ButtonKind kind, string classes)
    {
        var button = Render<Button>(parameters => parameters.Add(button => button.Kind, kind));

        Assert.Contains(classes, button.Find("button").GetAttribute("class"));
    }

    [Fact]
    public void Submit_button_leaves_the_work_to_the_form()
    {
        var clicked = false;
        var button = Render<Button>(parameters => parameters
            .Add(button => button.Submit, true)
            .Add(button => button.OnClick, () => clicked = true));

        button.Find("button").Click();

        Assert.Equal("submit", button.Find("button").GetAttribute("type"));
        Assert.False(clicked);
    }
}
