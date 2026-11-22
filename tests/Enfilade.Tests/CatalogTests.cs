using Microsoft.AspNetCore.Components;

namespace Enfilade.Tests;

public class CatalogTests : PanelContext
{
    [Fact]
    public void Catalog_renders_its_contents_only_while_expanded()
    {
        var catalog = Render<Catalog>(parameters => parameters
            .Add(catalog => catalog.Title, "Группа 124")
            .Add(catalog => catalog.ChildContent, "<p>Анна</p>"));

        var toggle = catalog.Find("button");
        var panelId = toggle.GetAttribute("aria-controls");
        Assert.False(string.IsNullOrWhiteSpace(panelId));
        Assert.Equal("false", toggle.GetAttribute("aria-expanded"));
        Assert.True(catalog.Find($"#{panelId}").HasAttribute("hidden"));
        Assert.Empty(catalog.FindAll("p"));

        toggle.Click();

        Assert.Equal("true", catalog.Find("button").GetAttribute("aria-expanded"));
        Assert.False(catalog.Find($"#{panelId}").HasAttribute("hidden"));
        Assert.Equal("Анна", catalog.Find($"#{panelId} p").TextContent);

        catalog.Find("button").Click();

        Assert.Equal("false", catalog.Find("button").GetAttribute("aria-expanded"));
        Assert.True(catalog.Find($"#{panelId}").HasAttribute("hidden"));
        Assert.Empty(catalog.FindAll("p"));
    }

    [Fact]
    public void Catalog_actions_are_separate_from_the_disclosure()
    {
        var clicks = 0;
        RenderFragment actions = builder =>
        {
            builder.OpenElement(0, "button");
            builder.AddAttribute(1, "onclick", EventCallback.Factory.Create(this, () => clicks++));
            builder.AddContent(2, "Переименовать");
            builder.CloseElement();
        };
        var catalog = Render<Catalog>(parameters => parameters
            .Add(catalog => catalog.Title, "Группа 134")
            .Add(catalog => catalog.Count, 0L)
            .Add(catalog => catalog.Actions, actions)
            .Add(catalog => catalog.ChildContent, "<p>Клиентов пока нет</p>"));

        var toggle = catalog.Find("button[aria-expanded]");
        Assert.Equal("0", toggle.QuerySelector(".catalog-count")?.TextContent);
        Assert.Empty(toggle.QuerySelectorAll("button"));

        catalog.Find(".catalog-actions button").Click();

        Assert.Equal(1, clicks);
        Assert.Equal("false", catalog.Find("button[aria-expanded]").GetAttribute("aria-expanded"));
    }
}
