using System.Collections.Generic;

public enum TokenType
{
    EOF,
    ILLEGAL,
    IDENT,

    // Keywords
    ASC,
    DESC,
}

public class Token
{
    public TokenType Type { get; set; }
    public string Literal { get; set; } = string.Empty;

    private static readonly Dictionary<string, TokenType> _keywords = new Dictionary<string, TokenType>()
    {
        { "asc", TokenType.ASC },
        { "desc", TokenType.DESC },
    };

    public Token(TokenType type, char literal)
    {
        Type = type;
        Literal = literal.ToString();
    }

    public static TokenType GetIdentifierTokenType(string identifier)
    {
        if (_keywords.TryGetValue(identifier, out var token))
        {
            return token;
        }

        return TokenType.IDENT;
    }
}