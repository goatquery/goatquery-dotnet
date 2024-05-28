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

    public IEnumerable<OrderByStatement> ParseOrderBy()
    {
        var statements = new List<OrderByStatement>();

        while (!CurrentTokenIs(TokenType.EOF))
        {
            if (!CurrentTokenIs(TokenType.IDENT))
            {
                NextToken();
                continue;
            }

            var statement = ParseOrderByStatement();
            if (statement != null)
            {
                statements.Add(statement);
            }

            NextToken();
        }

        return statements;
    }

    public ExpressionStatement ParseFilter()
    {
        var statement = new ExpressionStatement(_currentToken)
        {
            Expression = ParseExpression()
        };

        return statement;
    }

    private InfixExpression ParseExpression()
    {
        var left = ParseFilterStatement();
        if (left is null)
        {
            throw new Exception("bad");
        }

        NextToken();

        while (CurrentIdentifierIs(Keywords.And) || CurrentIdentifierIs(Keywords.Or))
        {
            left = new InfixExpression(_currentToken, left, _currentToken.Literal);

            NextToken();

            var right = ParseFilterStatement();
            if (right is null)
            {
                throw new Exception("bad");
            }

            left.Right = right;

            NextToken();
        }

        return left;
    }

    private OrderByStatement? ParseOrderByStatement()
    {
        var statement = new OrderByStatement(_currentToken);

        if (PeekIdentifierIs(Keywords.Desc))
        {
            statement.Direction = OrderByDirection.Descending;

            NextToken();
        }
        else if (PeekIdentifierIs(Keywords.Asc))
        {
            statement.Direction = OrderByDirection.Ascending;

            NextToken();
        }

        return statement;
    }

    private InfixExpression? ParseFilterStatement()
    {
        var identifier = new Identifier(_currentToken, _currentToken.Literal);

        if (!PeekIdentifierIs(Keywords.Eq))
        {
            return null;
        }

        NextToken();

        var statement = new InfixExpression(_currentToken, identifier, _currentToken.Literal);

        if (!PeekTokenIs(TokenType.STRING) && !PeekTokenIs(TokenType.INT))
        {
            return null;
        }

        NextToken();

        switch (_currentToken.Type)
        {
            case TokenType.STRING:
                statement.Right = new StringLiteral(_currentToken, _currentToken.Literal);
                break;
            case TokenType.INT:
                if (int.TryParse(_currentToken.Literal, out var value))
                {
                    statement.Right = new IntegerLiteral(_currentToken, value);
                }
                break;
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

    private bool PeekIdentifierIs(string identifier)
    {
        return _peekToken.Type == TokenType.IDENT && _peekToken.Literal.Equals(identifier, StringComparison.OrdinalIgnoreCase);
    }

    private bool CurrentIdentifierIs(string identifier)
    {
        return _currentToken.Type == TokenType.IDENT && _currentToken.Literal.Equals(identifier, StringComparison.OrdinalIgnoreCase);
    }
}