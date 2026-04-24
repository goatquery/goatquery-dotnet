using System.Collections.Generic;

public enum OrderByDirection
{
    Ascending = 1,
    Descending
}

public sealed class OrderByStatement : Node
{
    public OrderByDirection Direction { get; set; }
    public List<string> Segments { get; }

    public OrderByStatement(Token token) : base(token)
    {
        Segments = new List<string> { token.Literal };
    }

    public OrderByStatement(Token token, OrderByDirection direction) : base(token)
    {
        Direction = direction;
        Segments = new List<string> { token.Literal };
    }

    public OrderByStatement(Token token, List<string> segments, OrderByDirection direction) : base(token)
    {
        Direction = direction;
        Segments = segments;
    }

    public bool IsNestedPath => Segments.Count > 1;

    public override string TokenLiteral()
    {
        return string.Join("/", Segments);
    }
}
