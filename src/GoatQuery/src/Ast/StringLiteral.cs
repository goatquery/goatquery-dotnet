using System;

public sealed class StringLiteral : QueryExpression
{
    public string Value { get; set; }

    public StringLiteral(Token token, string value) : base(token)
    {
        Value = value;
    }
}

public sealed class GuidLiteral : QueryExpression
{
    public Guid Value { get; set; }

    public GuidLiteral(Token token, Guid value) : base(token)
    {
        Value = value;
    }
}

public sealed class IntegerLiteral : QueryExpression
{
    public int Value { get; set; }

    public IntegerLiteral(Token token, int value) : base(token)
    {
        Value = value;
    }
}