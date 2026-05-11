namespace GoatQuery;

internal sealed class QueryLambdaExpression : QueryExpression
{
    public QueryExpression Property { get; }
    public string Function { get; }
    public string Parameter { get; }
    public QueryExpression Body { get; set; }

    public QueryLambdaExpression(
        Token token,
        QueryExpression property,
        string function,
        string parameter
    )
        : base(token)
    {
        Property = property;
        Function = function;
        Parameter = parameter;
    }

    public override string TokenLiteral()
    {
        return $"{Property.TokenLiteral()}/{Function}({Parameter}: {Body?.TokenLiteral()})";
    }
}
