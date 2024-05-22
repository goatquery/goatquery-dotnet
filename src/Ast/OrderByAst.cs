public enum OrderByDirection
{
    Ascending,
    Descending
}

public sealed class OrderByStatement : Node
{
    public OrderByDirection Direction { get; set; }

    public OrderByStatement(Token token) : base(token) { }
    public OrderByStatement(Token token, OrderByDirection direction) : base(token)
    {
        Direction = direction;
    }
}