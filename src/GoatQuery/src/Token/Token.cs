public enum TokenType
{
    EOF = 1,
    ILLEGAL,
    IDENT,
    STRING,
    INT,
    GUID,
    DATETIME,
    LPAREN,
    RPAREN,
}

public static class Keywords
{
    internal const string Asc = "asc";
    internal const string Desc = "desc";
    internal const string Eq = "eq";
    internal const string Ne = "ne";
    internal const string Contains = "contains";
    internal const string Lt = "lt";
    internal const string Lte = "lte";
    internal const string Gt = "gt";
    internal const string Gte = "gte";
    internal const string And = "and";
    internal const string Or = "or";
}

public sealed class Token
{
    public TokenType Type { get; set; }
    public string Literal { get; set; } = string.Empty;

    public Token(TokenType type, char literal)
    {
        Type = type;
        Literal = literal.ToString();
    }

    public Token(TokenType type, string literal)
    {
        Type = type;
        Literal = literal;
    }
}