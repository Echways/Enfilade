using System.Linq.Expressions;

namespace Enfilade;

internal sealed class ExpressionShape : ExpressionVisitor
{
    private static readonly object End = new();

    private readonly List<object?> _parts = [];

    private readonly List<object> _captured = [];

    private readonly Dictionary<ParameterExpression, int> _parameters = [];

    public static bool Same(Expression? left, Expression right)
    {
        if (ReferenceEquals(left, right))
            return true;
        if (left is null)
            return false;

        var (before, after) = (Of(left), Of(right));
        return before._parts.SequenceEqual(after._parts) && before._captured.SequenceEqual(after._captured, ReferenceEqualityComparer.Instance);
    }

    public override Expression? Visit(Expression? node)
    {
        if (node is null)
            return null;
        if (Opaque(node.NodeType))
        {
            _parts.Add(node);
            return node;
        }

        _parts.Add(node.NodeType);
        _parts.Add(node.Type);
        var visited = base.Visit(node);
        _parts.Add(End);
        return visited;
    }

    protected override Expression VisitConstant(ConstantExpression node)
    {
        if (node.Value is null or string || node.Value.GetType().IsValueType)
            _parts.Add(node.Value);
        else
            _captured.Add(node.Value);
        return node;
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        if (!_parameters.TryGetValue(node, out var index))
            _parameters[node] = index = _parameters.Count;
        _parts.Add(index);
        return node;
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        _parts.Add(node.Member);
        return base.VisitMember(node);
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        _parts.Add(node.Method);
        return base.VisitMethodCall(node);
    }

    protected override Expression VisitBinary(BinaryExpression node)
    {
        _parts.Add(node.Method);
        _parts.Add(node.IsLiftedToNull);
        return base.VisitBinary(node);
    }

    protected override Expression VisitUnary(UnaryExpression node)
    {
        _parts.Add(node.Method);
        return base.VisitUnary(node);
    }

    protected override Expression VisitNew(NewExpression node)
    {
        _parts.Add(node.Constructor);
        return base.VisitNew(node);
    }

    protected override Expression VisitTypeBinary(TypeBinaryExpression node)
    {
        _parts.Add(node.TypeOperand);
        return base.VisitTypeBinary(node);
    }

    protected override Expression VisitIndex(IndexExpression node)
    {
        _parts.Add(node.Indexer);
        return base.VisitIndex(node);
    }

    protected override MemberBinding VisitMemberBinding(MemberBinding node)
    {
        _parts.Add(node.BindingType);
        _parts.Add(node.Member);
        var visited = base.VisitMemberBinding(node);
        _parts.Add(End);
        return visited;
    }

    protected override ElementInit VisitElementInit(ElementInit node)
    {
        _parts.Add(node.AddMethod);
        var visited = base.VisitElementInit(node);
        _parts.Add(End);
        return visited;
    }

    private static ExpressionShape Of(Expression expression)
    {
        var shape = new ExpressionShape();
        shape.Visit(expression);
        return shape;
    }

    private static bool Opaque(ExpressionType type) => type is ExpressionType.Block or ExpressionType.DebugInfo or ExpressionType.Dynamic
        or ExpressionType.Extension or ExpressionType.Goto or ExpressionType.Label or ExpressionType.Loop or ExpressionType.RuntimeVariables
        or ExpressionType.Switch or ExpressionType.Try;
}
