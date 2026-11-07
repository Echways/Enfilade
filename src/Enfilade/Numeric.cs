namespace Enfilade;

internal static class Numeric
{
    public static bool Is(Type type)
    {
        var plain = Nullable.GetUnderlyingType(type) ?? type;
        return !plain.IsEnum && Type.GetTypeCode(plain) is >= TypeCode.SByte and <= TypeCode.Decimal;
    }
}
