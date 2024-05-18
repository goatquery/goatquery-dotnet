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

    public IEnumerable<InfixExpression> ParseFilter()
    {
        var statements = new List<InfixExpression>();

        while (!CurrentTokenIs(TokenType.EOF))
        {
            if (!CurrentTokenIs(TokenType.IDENT))
            {
                NextToken();
                continue;
            }

            var statement = ParseFilterStatement();
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
        var statement = new OrderByStatement(_currentToken);

        if (PeekTokenIs(TokenType.DESC))
        {
            statement.Direction = OrderByDirection.Descending;

            NextToken();
        }
        else if (PeekTokenIs(TokenType.ASC))
        {
            statement.Direction = OrderByDirection.Ascending;

            NextToken();
        }

        return statement;
    }

    private InfixExpression? ParseFilterStatement()
    {
        var identifier = new Identifier(_currentToken, _currentToken.Literal);

        if (!PeekTokenIs(TokenType.EQ))
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