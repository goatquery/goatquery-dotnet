public abstract class Node
{
    private readonly Token _token;
    private readonly string _value;

    public Node(Token token, string value)
    {
        _token = token;
        _value = value;
    }

    public string TokenLiteral()
    {
        return _token.Literal;
    }
}