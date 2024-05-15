using System;
using System.Collections.Generic;

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

    // "Id asc"

    public IEnumerable<OrderByStatement> ParseOrderBy()
    {
        var statements = new List<OrderByStatement>();

        while (!CurrentTokenIs(TokenType.EOF))
        {
            Console.WriteLine($"literal: {_currentToken.Literal}");

            var statement = ParseOrderByStatement();
            if (statement != null)
            {
                statements.Add(statement);
            }

            NextToken();
        }

        return statements;
    }

    private OrderByStatement? ParseOrderByStatement()
    {
        var statement = new OrderByStatement(_currentToken, _currentToken.Literal)
        {
            Direction = OrderByDirection.Ascending
        };

        if (!ExpectPeek(TokenType.ASC) && !ExpectPeek(TokenType.DESC))
        {
            return null;
        }

        if (PeekTokenIs(TokenType.DESC))
        {
            statement.Direction = OrderByDirection.Descending;

            NextToken();
        }

        return statement;
    }

    private bool CurrentTokenIs(TokenType token)
    {
        return _currentToken.Type == token;
    }

    private bool PeekTokenIs(TokenType token)
    {
        return _peekToken.Type == token;
    }

    private bool ExpectPeek(TokenType token)
    {
        if (PeekTokenIs(token))
        {
            NextToken();
            return true;
        }

        return false;
    }
}