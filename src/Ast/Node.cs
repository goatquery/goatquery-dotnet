public abstract class Node
{
    private readonly Token _token;

    public Node(Token token)
    {
        _token = token;
    }

    public string TokenLiteral()
    {
        return _token.Literal;
    }
}