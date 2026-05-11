namespace GoatQuery;

internal abstract class QueryExpression : Node
{
    public QueryExpression(Token token)
        : base(token) { }
}
