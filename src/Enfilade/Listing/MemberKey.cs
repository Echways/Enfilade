using System.Linq.Expressions;

namespace Enfilade;

internal static class MemberKey
{
    public static string? Of(LambdaExpression selector)
    {
        var names = new List<string>();
        var node = Unboxed(selector.Body);
        while (node is MemberExpression member)
        {
            names.Insert(0, char.ToLowerInvariant(member.Member.Name[0]) + member.Member.Name[1..]);
            node = member.Expression;
        }

        return node is ParameterExpression && names.Count > 0 ? string.Join('.', names) : null;
    }

    public static Expression Unboxed(Expression body) =>
        body is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } convert ? convert.Operand : body;
}
