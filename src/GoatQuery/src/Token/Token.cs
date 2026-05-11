namespace GoatQuery;

internal enum TokenType
{
    EOF = 1,
    ILLEGAL,
    IDENT,
    STRING,
    INT,
    DOUBLE,
    GUID,
    DATETIME,
    DATE,
    NULL,
    BOOLEAN,
    LPAREN,
    RPAREN,
    SLASH,
    COLON,
}

internal static class Keywords
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
    internal const string Null = "null";
    internal const string True = "true";
    internal const string False = "false";
    internal const string Any = "any";
    internal const string All = "all";
}

internal sealed class Token
{
    public TokenType Type { get; }
    public string Literal { get; }

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
