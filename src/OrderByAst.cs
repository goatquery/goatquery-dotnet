public enum OrderByDirection
{
    Ascending,
    Descending
}

public sealed class OrderByStatement : Node
{
    public OrderByDirection Direction { get; set; }

    public OrderByStatement(Token token, string value) : base(token, value) { }
}