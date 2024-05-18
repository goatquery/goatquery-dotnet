public sealed class StringLiteral : Node
{
    public string Value { get; set; }

    public StringLiteral(Token token, string value) : base(token)
    {
        Value = value;
    }
}

public sealed class IntegerLiteral : Node
{
    public int Value { get; set; }

    public IntegerLiteral(Token token, int value) : base(token)
    {
        Value = value;
    }
}