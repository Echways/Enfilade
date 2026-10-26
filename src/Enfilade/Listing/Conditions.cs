using System.Linq.Expressions;
using System.Reflection;

namespace Enfilade;

internal static class Conditions
{
    private static readonly MethodInfo Lower = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!;

    private static readonly MethodInfo Contains = typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!;

    public static Expression<Func<TItem, bool>> Search<TItem>(IReadOnlyList<Expression<Func<TItem, string?>>> fields, string term)
    {
        var item = Expression.Parameter(typeof(TItem), "item");
        var lowered = Expression.Constant(term.ToLower());
        var matches = fields
            .Select(field => new Rebound(field.Parameters[0], item).Visit(field.Body))
            .Select(field => (Expression)Expression.AndAlso(
                Expression.NotEqual(field, Expression.Constant(null, typeof(string))),
                Expression.Call(Expression.Call(field, Lower), Contains, lowered)))
            .Aggregate(Expression.OrElse);
        return Expression.Lambda<Func<TItem, bool>>(matches, item);
    }

    public static Expression<Func<TItem, bool>> Equal<TItem, TValue>(Expression<Func<TItem, TValue>> by, TValue value)
    {
        var compared = Comparable(typeof(TValue));
        Expression Compared(Expression side) => compared == typeof(TValue) ? side : Expression.Convert(side, compared);
        return Expression.Lambda<Func<TItem, bool>>(
            Expression.Equal(Compared(by.Body), Compared(Expression.Constant(value, typeof(TValue)))),
            by.Parameters);
    }

    private static Type Comparable(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type);
        var plain = underlying ?? type;
        if (!plain.IsEnum)
            return type;

        var number = Enum.GetUnderlyingType(plain);
        return underlying is null ? number : typeof(Nullable<>).MakeGenericType(number);
    }

    private sealed class Rebound(ParameterExpression from, ParameterExpression to) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node) => node == from ? to : node;
    }
}
