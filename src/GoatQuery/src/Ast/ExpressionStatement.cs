public sealed class ExpressionStatement : Statement
{
    public InfixExpression Expression { get; set; } = default!;

    public ExpressionStatement(Token token) : base(token)
    {
    }
}