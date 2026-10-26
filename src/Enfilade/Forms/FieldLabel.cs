using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Enfilade;

internal static class FieldLabel
{
    public static string Of(MemberInfo member) => member.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? member.Name;

    public static string? HintOf(MemberInfo member) => member.GetCustomAttribute<DisplayAttribute>()?.GetDescription();
}
