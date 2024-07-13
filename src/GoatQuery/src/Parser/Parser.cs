using System;
using System.Collections.Generic;
using System.Linq;

public sealed class QueryParser
{
    private readonly QueryLexer _lexer;
    private Token _currentToken { get; set; } = default;
    private Token _peekToken { get; set; } = default;

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

    private OrderByStatement ParseOrderByStatement()
    {
        var statement = new OrderByStatement(_currentToken, OrderByDirection.Ascending);

        if (PeekIdentifierIs(Keywords.Desc))
        {
            statement.Direction = OrderByDirection.Descending;
        }

        NextToken();

        return statement;
    }

    public ExpressionStatement ParseFilter()
    {
        var statement = new ExpressionStatement(_currentToken)
        {
            Expression = ParseExpression()
        };

        return statement;
    }

    private InfixExpression ParseExpression(int precedence = 0)
    {
        var left = CurrentTokenIs(TokenType.LPAREN) ? ParseGroupedExpression() : ParseFilterStatement();

        NextToken();

        while (!CurrentTokenIs(TokenType.EOF) && precedence < GetPrecedence(_currentToken.Type))
        {
            if (CurrentIdentifierIs(Keywords.And) || CurrentIdentifierIs(Keywords.Or))
            {
                left = new InfixExpression(_currentToken, left, _currentToken.Literal);
                var currentPrecedence = GetPrecedence(_currentToken.Type);

                NextToken();

                var right = ParseExpression(currentPrecedence);
                left.Right = right;
            }
            else
            {
                break;
            }
        }

        return left;
    }

    private InfixExpression ParseGroupedExpression()
    {
        NextToken();

        var exp = ParseExpression();

        if (!CurrentTokenIs(TokenType.RPAREN))
        {
            throw new GoatQueryException("Expected closing parenthesis");
        }

        return exp;
    }

    private InfixExpression ParseFilterStatement()
    {
        var identifier = new Identifier(_currentToken, _currentToken.Literal);

        if (!PeekIdentifierIn(Keywords.Eq, Keywords.Ne, Keywords.Contains))
        {
            throw new GoatQueryException("Invalid conjunction within filter");
        }

        NextToken();

        var statement = new InfixExpression(_currentToken, identifier, _currentToken.Literal);

        if (!PeekTokenIn(TokenType.STRING, TokenType.INT))
        {
            throw new GoatQueryException("Invalid value type within filter");
        }

        NextToken();

        if (statement.Operator.Equals(Keywords.Contains) && _currentToken.Type != TokenType.STRING)
        {
            throw new GoatQueryException("Value must be a string when using contains operand");
        }

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

    private int GetPrecedence(TokenType tokenType)
    {
        switch (tokenType)
        {
            case TokenType.IDENT when CurrentIdentifierIs(Keywords.And):
                return 2;
            case TokenType.IDENT when CurrentIdentifierIs(Keywords.Or):
                return 1;
            default:
                return 0;
        }
    }

    private bool CurrentTokenIs(TokenType token)
    {
        return _currentToken.Type == token;
    }

    private bool PeekTokenIs(TokenType token)
    {
        return _peekToken.Type == token;
    }

    private bool PeekTokenIn(params TokenType[] token)
    {
        return token.Contains(_peekToken.Type);
    }

    private bool PeekIdentifierIs(string identifier)
    {
        return _peekToken.Type == TokenType.IDENT && _peekToken.Literal.Equals(identifier, StringComparison.OrdinalIgnoreCase);
    }

    private bool PeekIdentifierIn(params string[] identifier)
    {
        return _peekToken.Type == TokenType.IDENT && identifier.Contains(_peekToken.Literal, StringComparer.OrdinalIgnoreCase);
    }

    private bool CurrentIdentifierIs(string identifier)
    {
        return _currentToken.Type == TokenType.IDENT && _currentToken.Literal.Equals(identifier, StringComparison.OrdinalIgnoreCase);
    }
}