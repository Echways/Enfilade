using Enfilade.Tests.Hosts;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace Enfilade.Tests;

public class FieldTests : PanelContext
{
    private readonly Profile _profile = new()
    {
        PaidAt = new DateTimeOffset(2026, 7, 1, 10, 0, 0, TimeSpan.Zero),
        Day = new DateOnly(2026, 10, 12),
    };

    public FieldTests() => Words.Zone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");

    [Fact]
    public void Label_comes_from_display()
    {
        var form = Render<ProfileForm>(parameters => parameters.Add(host => host.Profile, _profile));

        var labels = form.FindAll("label").Select(label => label.TextContent.Trim()).ToList();
        Assert.Contains("Клиент", labels);
        Assert.Contains(nameof(Profile.Note), labels);
        Assert.Contains("Как в договоре", form.Markup);
    }

    [Fact]
    public void Required_marks_the_input()
    {
        var form = Render<ProfileForm>(parameters => parameters.Add(host => host.Profile, _profile));

        var customer = form.FindComponents<MudTextField<string>>().Single(field => field.Instance.Label == "Клиент");
        Assert.True(customer.Instance.Required);
        Assert.Equal("true", customer.Find("input").GetAttribute("aria-required"));
    }

    [Fact]
    public void Enum_offers_its_members_with_display_names()
    {
        var popovers = Render<MudPopoverProvider>();
        var form = Render<ProfileForm>(parameters => parameters.Add(host => host.Profile, _profile));

        form.FindComponent<MudSelect<OrderStatus>>().Find(".mud-input-control").MouseDown();

        popovers.WaitForAssertion(() =>
        {
            Assert.Contains("Оплачен", popovers.Markup);
            Assert.Contains(nameof(OrderStatus.New), popovers.Markup);
        });
    }

    [Fact]
    public async Task Moment_is_edited_in_the_viewer_zone()
    {
        var form = Render<ProfileForm>(parameters => parameters.Add(host => host.Profile, _profile));

        Assert.Equal("2026-07-01", form.FindComponent<MudDatePicker>().Find("input").GetAttribute("value"));
        var time = form.FindComponent<MudTimePicker>();
        Assert.Equal("12:00", time.Find("input").GetAttribute("value"));

        await form.InvokeAsync(() => time.Instance.TimeChanged.InvokeAsync(new TimeSpan(13, 0, 0)));

        Assert.Equal(new DateTimeOffset(2026, 7, 1, 11, 0, 0, TimeSpan.Zero), _profile.PaidAt);
        Assert.Equal(TimeSpan.Zero, _profile.PaidAt.Offset);
    }

    [Fact]
    public async Task Date_only_keeps_the_day()
    {
        var form = Render<ProfileForm>(parameters => parameters.Add(host => host.Profile, _profile));
        var day = form.FindComponents<MudDatePicker>()[1];
        Assert.Equal("2026-10-12", day.Find("input").GetAttribute("value"));

        await form.InvokeAsync(() => day.Instance.DateChanged.InvokeAsync(new DateTime(2026, 10, 13)));

        Assert.Equal(new DateOnly(2026, 10, 13), _profile.Day);
    }

    [Fact]
    public async Task Another_model_is_edited_after_a_swap()
    {
        var next = new Profile { Note = "новый" };
        var form = Render<SwappedProfile>(parameters => parameters.Add(host => host.Profile, _profile));

        form.Render(parameters => parameters.Add(host => host.Profile, next));
        var note = form.FindComponent<MudTextField<string>>();
        Assert.Equal("новый", note.Find("input").GetAttribute("value"));
        await form.InvokeAsync(() => note.Instance.ValueChanged.InvokeAsync("срочно"));

        Assert.Equal("срочно", next.Note);
        Assert.Equal("", _profile.Note);
    }

    [Fact]
    public async Task Editing_notifies_the_form()
    {
        var form = Render<ProfileForm>(parameters => parameters.Add(host => host.Profile, _profile));
        var edit = form.FindComponent<EditForm>().Instance.EditContext!;
        var note = form.FindComponents<MudTextField<string>>().Single(field => field.Instance.Label == nameof(Profile.Note));

        await form.InvokeAsync(() => note.Instance.ValueChanged.InvokeAsync("срочно"));

        Assert.Equal("срочно", _profile.Note);
        Assert.True(edit.IsModified(() => _profile.Note));
    }
}
