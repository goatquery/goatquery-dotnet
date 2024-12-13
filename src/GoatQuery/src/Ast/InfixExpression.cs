public sealed class InfixExpression : QueryExpression
{
    public QueryExpression Left { get; set; } = default;
    public string Operator { get; set; } = string.Empty;
    public QueryExpression Right { get; set; } = default;

    public InfixExpression(Token token, QueryExpression left, string op) : base(token)
    {
        Left = left;
        Operator = op;
    }

    public InfixExpression(Token token) : base(token)
    {
    }
}