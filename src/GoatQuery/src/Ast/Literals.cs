namespace GoatQuery;

using System;

internal sealed class StringLiteral : QueryExpression
{
    public string Value { get; }

    public StringLiteral(Token token, string value)
        : base(token)
    {
        Value = value;
    }
}

internal sealed class GuidLiteral : QueryExpression
{
    public Guid Value { get; }

    public GuidLiteral(Token token, Guid value)
        : base(token)
    {
        Value = value;
    }
}

internal sealed class IntegerLiteral : QueryExpression
{
    public long Value { get; }

    public IntegerLiteral(Token token, long value)
        : base(token)
    {
        Value = value;
    }
}

internal sealed class DoubleLiteral : QueryExpression
{
    public double Value { get; }

    public DoubleLiteral(Token token, double value)
        : base(token)
    {
        Value = value;
    }
}

internal sealed class DateTimeLiteral : QueryExpression
{
    public DateTime Value { get; }

    public DateTimeLiteral(Token token, DateTime value)
        : base(token)
    {
        Value = value;
    }
}

internal sealed class DateLiteral : QueryExpression
{
    public DateTime Value { get; }

    public DateLiteral(Token token, DateTime value)
        : base(token)
    {
        Value = value;
    }
}

internal sealed class NullLiteral : QueryExpression
{
    public NullLiteral(Token token)
        : base(token) { }
}

internal sealed class BooleanLiteral : QueryExpression
{
    public bool Value { get; }

    public BooleanLiteral(Token token, bool value)
        : base(token)
    {
        Value = value;
    }
}
