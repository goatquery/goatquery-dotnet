using System;
using System.Collections.Generic;

public enum TokenType
{
    EOF,
    ILLEGAL,
    IDENT,
    STRING,
    INT,

    // Keywords
    ASC,
    DESC,
    EQ,
    AND,
}

public sealed class Token
{
    public TokenType Type { get; set; }
    public string Literal { get; set; } = string.Empty;

    private static readonly Dictionary<string, TokenType> _keywords = new Dictionary<string, TokenType>(StringComparer.CurrentCultureIgnoreCase)
    {
        { "asc", TokenType.ASC },
        { "desc", TokenType.DESC },
        { "eq", TokenType.EQ },
        { "and", TokenType.AND },
    };

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

    public static TokenType GetIdentifierTokenType(string identifier)
    {
        if (_keywords.TryGetValue(identifier, out var token))
        {
            return token;
        }

        return TokenType.IDENT;
    }
}