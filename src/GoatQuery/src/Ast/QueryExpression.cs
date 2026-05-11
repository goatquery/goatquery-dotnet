namespace GoatQuery;

internal abstract class QueryExpression : Node
{
    protected QueryExpression(Token token)
        : base(token) { }
}
