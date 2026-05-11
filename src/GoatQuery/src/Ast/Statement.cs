namespace GoatQuery;

internal abstract class Statement : Node
{
    public Statement(Token token)
        : base(token) { }
}
