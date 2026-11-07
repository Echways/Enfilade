namespace Enfilade;

internal sealed class SheetGuard
{
    private readonly List<Form> _forms = [];

    public bool Dirty => _forms.Exists(form => form.IsDirty);

    public void Watch(Form form) => _forms.Add(form);

    public void Forget(Form form) => _forms.Remove(form);
}
