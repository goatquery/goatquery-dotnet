using System;
using System.Collections.Generic;
using System.Linq;

public static class QueryableExtension
{
    public static Dictionary<string, string> _filterOperations => new Dictionary<string, string>
    {
        {"eq", "=="},
        {"ne", "!="},
        {"contains", "Contains"},
    };

    public static (IQueryable<T>, int?) Apply<T>(this IQueryable<T> queryable, Query query)
    {
        // Order by
        if (!string.IsNullOrEmpty(query.OrderBy))
        {
            var lexer = new QueryLexer(query.OrderBy);
            var parser = new QueryParser(lexer);

            parser.ParseOrderBy();

            // queryable = queryable.OrderBy();
        }

        return (queryable, 0);
    }
}

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

public sealed class QueryParser
{
    private readonly QueryLexer _lexer;
    private Token _currentToken { get; set; } = default!;
    private Token _peekToken { get; set; } = default!;

    public QueryParser(QueryLexer lexer)
    {
        _lexer = lexer;

        NextToken();
        NextToken();
    }

    private void NextToken()
    {
        _currentToken = _peekToken;
        _peekToken = _lexer.NextToken();
    }

    public void ParseOrderBy()
    {
        while (!CurrentTokenIs(TokenType.EOF))
        {
            Console.WriteLine(_currentToken);
        }
    }

    private bool CurrentTokenIs(TokenType token)
    {
        return _currentToken.Type == token;
    }
}

public sealed class QueryLexer
{
    private readonly string _input;
    private int _position { get; set; }
    private int _readPosition { get; set; }
    private char _character { get; set; }

    public QueryLexer(string input)
    {
        _input = input;

        ReadCharacter();
    }

    private void ReadCharacter()
    {
        if (_readPosition >= _input.Length)
        {
            _character = char.MinValue;
        }
        else
        {
            _character = _input[_readPosition];
        }

        _position = _readPosition;
        _readPosition++;
    }

    public Token NextToken()
    {
        var token = new Token(TokenType.ILLEGAL, _character);

        SkipWhitespace();

        switch (_character)
        {
            default:
                if (IsLetter(_character))
                {
                    token.Literal = ReadIdentifier();
                    token.Type = Token.GetIdentifierTokenType(token.Literal);
                    return token;
                }
                break;
        }

        ReadCharacter();

        return token;
    }

    private string ReadIdentifier()
    {
        var currentPosition = _position;

        while (IsLetter(_character))
        {
            ReadCharacter();
        }

        return _input.Substring(currentPosition, _position - currentPosition);
    }

    private bool IsLetter(char ch)
    {
        return 'a' <= ch && ch <= 'z' || 'A' <= ch && ch <= 'Z' || ch == '_';
    }

    private void SkipWhitespace()
    {
        while (_character == ' ' || _character == '\t' || _character == '\n' || _character == '\r')
        {
            ReadCharacter();
        }
    }
}