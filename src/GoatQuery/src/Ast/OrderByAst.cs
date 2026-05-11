namespace GoatQuery;

using System.Collections.Generic;

internal enum OrderByDirection
{
    Ascending = 1,
    Descending,
}

internal sealed class OrderByStatement : Node
{
    public OrderByDirection Direction { get; }
    public List<string> Segments { get; }

    public OrderByStatement(Token token, OrderByDirection direction)
        : this(token, new List<string> { token.Literal }, direction) { }

    public OrderByStatement(Token token, List<string> segments, OrderByDirection direction)
        : base(token)
    {
        Direction = direction;
        Segments = segments;
    }

    public override string TokenLiteral()
    {
        return string.Join("/", Segments);
    }
}
