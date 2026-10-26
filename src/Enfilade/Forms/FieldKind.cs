namespace Enfilade;

internal enum FieldKind
{
    Text,
    Number,
    Switch,
    Check,
    Select,
    Date,
    Moment,
}

internal static class FieldKinds
{
    public static FieldKind Of(Type type, bool hasOptions)
    {
        var plain = Nullable.GetUnderlyingType(type) ?? type;
        if (hasOptions || plain.IsEnum)
            return FieldKind.Select;
        if (type == typeof(string))
            return FieldKind.Text;
        if (plain == typeof(bool))
            return type == typeof(bool) ? FieldKind.Switch : FieldKind.Check;
        if (plain == typeof(DateOnly))
            return FieldKind.Date;
        if (plain == typeof(DateTime) || plain == typeof(DateTimeOffset))
            return FieldKind.Moment;
        if (Type.GetTypeCode(plain) is >= TypeCode.SByte and <= TypeCode.Decimal)
            return FieldKind.Number;

        throw new NotSupportedException($"Enfilade: Field cannot edit {type.Name}; put a MudBlazor input with For= inside the Form instead");
    }
}
