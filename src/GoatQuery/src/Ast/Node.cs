namespace GoatQuery;

internal abstract class Node
{
    private readonly Token _token;

    protected Node(Token token)
    {
        _token = token;
    }

    public virtual string TokenLiteral()
    {
        return _token.Literal;
    }
}
