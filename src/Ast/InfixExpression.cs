public sealed class InfixExpression : Node
{
    public Identifier Left { get; set; }
    public string Operator { get; set; }
    public Node Right { get; set; } = default!;

    public InfixExpression(Token token, Identifier left, string op) : base(token)
    {
        Left = left;
        Operator = op;
    }
}