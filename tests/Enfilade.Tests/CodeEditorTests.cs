namespace Enfilade.Tests;

public class CodeEditorTests : PanelContext
{
    private const string Module = "http://localhost/_content/Enfilade/editor.js";

    [Fact]
    public void Starts_the_editor_with_the_text_the_language_and_the_lock()
    {
        var module = JSInterop.SetupModule(Module);
        module.SetupVoid("create", _ => true).SetVoidResult();

        Render<CodeEditor>(parameters => parameters
            .Add(editor => editor.Value, "print(1)")
            .Add(editor => editor.Language, "python")
            .Add(editor => editor.ReadOnly, true));

        var call = Assert.Single(module.Invocations["create"]);
        Assert.Equal("print(1)", call.Arguments[2]);
        Assert.Equal(true, call.Arguments[3]);
        Assert.Equal("python", call.Arguments[4]);
    }

    [Fact]
    public async Task Reads_the_text_from_the_editor()
    {
        var module = JSInterop.SetupModule(Module);
        module.SetupVoid("create", _ => true).SetVoidResult();
        module.Setup<string>("getValue", _ => true).SetResult("typed");
        var editor = Render<CodeEditor>();

        Assert.Equal("typed", await editor.Instance.GetValueAsync());
    }

    [Fact]
    public async Task Replaces_the_text_without_telling_the_host_about_a_change()
    {
        var module = JSInterop.SetupModule(Module);
        module.SetupVoid("create", _ => true).SetVoidResult();
        module.SetupVoid("setValue", _ => true).SetVoidResult();
        var changes = 0;
        var editor = Render<CodeEditor>(parameters => parameters
            .Add(editor => editor.OnChange, () => changes++));

        await editor.Instance.SetValueAsync("new");

        Assert.Equal("new", Assert.Single(module.Invocations["setValue"]).Arguments[1]);
        Assert.Equal(0, changes);
    }

    [Fact]
    public async Task Tells_the_host_about_an_edit()
    {
        JSInterop.SetupModule(Module).SetupVoid("create", _ => true).SetVoidResult();
        var changes = 0;
        var editor = Render<CodeEditor>(parameters => parameters
            .Add(editor => editor.OnChange, () => changes++));

        await editor.Instance.EditorChanged();

        Assert.Equal(1, changes);
    }

    [Fact]
    public void Locks_and_unlocks_the_editor_when_the_parameter_changes()
    {
        var module = JSInterop.SetupModule(Module);
        module.SetupVoid("create", _ => true).SetVoidResult();
        module.SetupVoid("setReadOnly", _ => true).SetVoidResult();
        var editor = Render<CodeEditor>();

        editor.Render(parameters => parameters.Add(component => component.ReadOnly, true));

        Assert.Equal(true, Assert.Single(module.Invocations["setReadOnly"]).Arguments[1]);
    }

    [Fact]
    public async Task Destroys_the_editor_when_removed()
    {
        var module = JSInterop.SetupModule(Module);
        module.SetupVoid("create", _ => true).SetVoidResult();
        module.SetupVoid("dispose", _ => true).SetVoidResult();
        var editor = Render<CodeEditor>();

        await editor.Instance.DisposeAsync();

        Assert.Single(module.Invocations["dispose"]);
    }
}
