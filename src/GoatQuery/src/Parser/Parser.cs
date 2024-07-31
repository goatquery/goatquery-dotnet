using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentResults;

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

    public Result<ExpressionStatement> ParseFilter()
    {
        var expression = ParseExpression();
        if (expression.IsFailed)
        {
            return Result.Fail(expression.Errors);
        }

        var statement = new ExpressionStatement(_currentToken)
        {
            Expression = expression.Value
        };

        return statement;
    }

    private Result<InfixExpression> ParseExpression(int precedence = 0)
    {
        var left = CurrentTokenIs(TokenType.LPAREN) ? ParseGroupedExpression() : ParseFilterStatement();
        if (left.IsFailed)
        {
            return left;
        }

        NextToken();

        while (!CurrentTokenIs(TokenType.EOF) && precedence < GetPrecedence(_currentToken.Type))
        {
            if (CurrentIdentifierIs(Keywords.And) || CurrentIdentifierIs(Keywords.Or))
            {
                left = new InfixExpression(_currentToken, left.Value, _currentToken.Literal);
                var currentPrecedence = GetPrecedence(_currentToken.Type);

                NextToken();

                var right = ParseExpression(currentPrecedence);
                if (right.IsFailed)
                {
                    return right;
                }
                left.Value.Right = right.Value;
            }
            else
            {
                break;
            }
        }

        return left;
    }

    private Result<InfixExpression> ParseGroupedExpression()
    {
        NextToken();

        var exp = ParseExpression();

        if (!CurrentTokenIs(TokenType.RPAREN))
        {
            return Result.Fail("Expected closing parenthesis");
        }

        return exp;
    }

    private Result<InfixExpression> ParseFilterStatement()
    {
        var identifier = new Identifier(_currentToken, _currentToken.Literal);

        if (!PeekIdentifierIn(Keywords.Eq, Keywords.Ne, Keywords.Contains, Keywords.Lt, Keywords.Lte, Keywords.Gt, Keywords.Gte))
        {
            return Result.Fail("Invalid conjunction within filter");
        }

        NextToken();

        var statement = new InfixExpression(_currentToken, identifier, _currentToken.Literal);

        if (!PeekTokenIn(TokenType.STRING, TokenType.INT, TokenType.GUID, TokenType.DATETIME, TokenType.DECIMAL, TokenType.FLOAT, TokenType.DOUBLE))
        {
            return Result.Fail("Invalid value type within filter");
        }

        NextToken();

        if (statement.Operator.Equals(Keywords.Contains) && _currentToken.Type != TokenType.STRING)
        {
            return Result.Fail("Value must be a string when using 'contains' operand");
        }

        if (statement.Operator.In(Keywords.Lt, Keywords.Lte, Keywords.Gt, Keywords.Gte) && !CurrentTokenIn(TokenType.INT, TokenType.DECIMAL, TokenType.FLOAT, TokenType.DOUBLE, TokenType.DATETIME))
        {
            return Result.Fail($"Value must be an integer when using '{statement.Operator}' operand");
        }

        switch (_currentToken.Type)
        {
            case TokenType.GUID:
                if (Guid.TryParse(_currentToken.Literal, out var guidValue))
                {
                    statement.Right = new GuidLiteral(_currentToken, guidValue);
                }
                break;
            case TokenType.STRING:
                statement.Right = new StringLiteral(_currentToken, _currentToken.Literal);
                break;
            case TokenType.INT:
                if (int.TryParse(_currentToken.Literal, out var intValue))
                {
                    statement.Right = new IntegerLiteral(_currentToken, intValue);
                }
                break;
            case TokenType.FLOAT:
                var floatValueWithoutSuffixLiteral = _currentToken.Literal.TrimEnd('f');

                if (float.TryParse(floatValueWithoutSuffixLiteral, out var floatValue))
                {
                    statement.Right = new FloatLiteral(_currentToken, floatValue);
                }
                break;
            case TokenType.DECIMAL:
                var decimalValueWithoutSuffixLiteral = _currentToken.Literal.TrimEnd('m');

                if (decimal.TryParse(decimalValueWithoutSuffixLiteral, out var decimalValue))
                {
                    statement.Right = new DecimalLiteral(_currentToken, decimalValue);
                }
                break;
            case TokenType.DOUBLE:
                var doubleValueWithoutSuffixLiteral = _currentToken.Literal.TrimEnd('d');

                if (double.TryParse(doubleValueWithoutSuffixLiteral, out var doubleValue))
                {
                    statement.Right = new DoubleLiteral(_currentToken, doubleValue);
                }
                break;
            case TokenType.DATETIME:
                if (DateTime.TryParse(_currentToken.Literal, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var dateTimeValue))
                {
                    statement.Right = new DateTimeLiteral(_currentToken, dateTimeValue);
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

    private bool CurrentTokenIn(params TokenType[] tokens)
    {
        return tokens.Contains(_currentToken.Type);
    }

    private bool PeekTokenIn(params TokenType[] tokens)
    {
        return tokens.Contains(_peekToken.Type);
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