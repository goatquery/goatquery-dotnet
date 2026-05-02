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
        var startToken = _currentToken;
        var segments = new List<string> { _currentToken.Literal };

        // Handle nested property paths (e.g., company/name)
        while (_peekToken.Type == TokenType.SLASH)
        {
            NextToken(); // consume current identifier
            NextToken(); // consume slash

            if (_currentToken.Type != TokenType.IDENT)
            {
                break;
            }

            segments.Add(_currentToken.Literal);
        }

        var direction = OrderByDirection.Ascending;

        if (PeekIdentifierIs(Keywords.Desc))
        {
            direction = OrderByDirection.Descending;
        }

        NextToken();

        return new OrderByStatement(startToken, segments, direction);
    }

    public Result<ExpressionStatement> ParseFilter()
    {
        var expression = ParseExpression();
        if (expression.IsFailed)
        {
            return Result.Fail(expression.Errors);
        }

        var statement = new ExpressionStatement(_currentToken) { Expression = expression.Value };

        return statement;
    }

    private Result<InfixExpression> ParseExpression(int precedence = 0)
    {
        var left = CurrentTokenIs(TokenType.LPAREN)
            ? ParseGroupedExpression()
            : ParseFilterStatement();
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
        QueryExpression leftExpression = null;

        if (_peekToken.Type == TokenType.SLASH)
        {
            // We are filtering by a property on an object or lambda expression
            var segments = new List<string> { _currentToken.Literal };
            var startToken = _currentToken;

            while (_peekToken.Type == TokenType.SLASH)
            {
                NextToken(); // consume current identifier
                NextToken(); // consume slash

                if (_currentToken.Type != TokenType.IDENT)
                {
                    return Result.Fail("Expected identifier after '/' in property path");
                }

                // Check if this is a lambda function (any/all followed by parenthesis)
                if (
                    (
                        _currentToken.Literal.Equals(
                            Keywords.Any,
                            StringComparison.OrdinalIgnoreCase
                        )
                        || _currentToken.Literal.Equals(
                            Keywords.All,
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    && _peekToken.Type == TokenType.LPAREN
                )
                {
                    var lambdaResult = ParseLambdaExpression(
                        new PropertyPath(startToken, segments),
                        _currentToken.Literal
                    );
                    if (lambdaResult.IsFailed)
                    {
                        return Result.Fail(lambdaResult.Errors);
                    }
                    leftExpression = lambdaResult.Value;
                    break;
                }

                segments.Add(_currentToken.Literal);
            }

            // If we didn't parse a lambda, create a regular property path
            if (leftExpression == null)
            {
                leftExpression = new PropertyPath(startToken, segments);
            }
        }
        else
        {
            leftExpression = new Identifier(_currentToken, _currentToken.Literal);
        }

        // Lambda expressions don't need an operator after them - they are complete expressions
        if (leftExpression is QueryLambdaExpression)
        {
            return new InfixExpression(_currentToken, leftExpression, string.Empty);
        }

        if (
            !PeekIdentifierIn(
                Keywords.Eq,
                Keywords.Ne,
                Keywords.Contains,
                Keywords.Lt,
                Keywords.Lte,
                Keywords.Gt,
                Keywords.Gte
            )
        )
        {
            return Result.Fail("Invalid conjunction within filter");
        }

        NextToken();

        var statement = new InfixExpression(_currentToken, leftExpression, _currentToken.Literal);

        if (
            !PeekTokenIn(
                TokenType.STRING,
                TokenType.INT,
                TokenType.LONG,
                TokenType.GUID,
                TokenType.DATETIME,
                TokenType.DECIMAL,
                TokenType.FLOAT,
                TokenType.DOUBLE,
                TokenType.DATE,
                TokenType.NULL,
                TokenType.BOOLEAN
            )
        )
        {
            return Result.Fail("Invalid value type within filter");
        }

        NextToken();

        if (statement.Operator.Equals(Keywords.Contains) && _currentToken.Type != TokenType.STRING)
        {
            return Result.Fail("Value must be a string when using 'contains' operand");
        }

        if (
            statement.Operator.In(Keywords.Lt, Keywords.Lte, Keywords.Gt, Keywords.Gte)
            && !CurrentTokenIn(
                TokenType.INT,
                TokenType.LONG,
                TokenType.DECIMAL,
                TokenType.FLOAT,
                TokenType.DOUBLE,
                TokenType.DATETIME,
                TokenType.DATE
            )
        )
        {
            return Result.Fail(
                $"Value must be a numeric or date type when using '{statement.Operator}' operand"
            );
        }

        statement.Right = ParseLiteral(_currentToken);

        if (statement.Right == null)
        {
            return Result.Fail(
                $"Could not parse value '{_currentToken.Literal}' as {_currentToken.Type}"
            );
        }

        return statement;
    }

    private Result<QueryLambdaExpression> ParseLambdaExpression(
        QueryExpression property,
        string function
    )
    {
        var startToken = _currentToken;

        // Consume opening parenthesis
        if (!PeekTokenIs(TokenType.LPAREN))
        {
            return Result.Fail("Expected '(' after lambda function");
        }
        NextToken(); // consume function name (any/all)
        NextToken(); // consume '('

        // Parse parameter name
        if (!CurrentTokenIs(TokenType.IDENT))
        {
            return Result.Fail("Expected parameter name in lambda expression");
        }
        var parameter = _currentToken.Literal;
        NextToken();

        // Parse colon
        if (!CurrentTokenIs(TokenType.COLON))
        {
            return Result.Fail("Expected ':' after lambda parameter");
        }
        NextToken();

        // Parse lambda body (recursive expression parsing)
        var bodyResult = ParseExpression();
        if (bodyResult.IsFailed)
        {
            return Result.Fail(bodyResult.Errors);
        }

        // Expect closing parenthesis
        if (!CurrentTokenIs(TokenType.RPAREN))
        {
            return Result.Fail("Expected ')' to close lambda expression");
        }

        var lambda = new QueryLambdaExpression(startToken, property, function, parameter)
        {
            Body = bodyResult.Value,
        };

        return lambda;
    }

    private QueryExpression ParseLiteral(Token token)
    {
        return token.Type switch
        {
            TokenType.GUID => Guid.TryParse(token.Literal, out var guidValue)
                ? new GuidLiteral(token, guidValue)
                : null,
            TokenType.STRING => new StringLiteral(token, token.Literal),
            TokenType.INT => ParseIntegerOrLong(token),
            TokenType.LONG => long.TryParse(token.Literal.TrimEnd('l', 'L'), out var longValue)
                ? new LongLiteral(token, longValue)
                : null,
            TokenType.FLOAT => float.TryParse(token.Literal.TrimEnd('f'), out var floatValue)
                ? new FloatLiteral(token, floatValue)
                : null,
            TokenType.DECIMAL => decimal.TryParse(token.Literal.TrimEnd('m'), out var decimalValue)
                ? new DecimalLiteral(token, decimalValue)
                : null,
            TokenType.DOUBLE => double.TryParse(token.Literal.TrimEnd('d'), out var doubleValue)
                ? new DoubleLiteral(token, doubleValue)
                : null,
            TokenType.DATETIME => DateTime.TryParse(
                token.Literal,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal,
                out var dateTimeValue
            )
                ? new DateTimeLiteral(token, dateTimeValue)
                : null,
            TokenType.DATE => DateTime.TryParse(
                token.Literal,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal,
                out var dateValue
            )
                ? new DateLiteral(token, dateValue)
                : null,
            TokenType.NULL => new NullLiteral(token),
            TokenType.BOOLEAN => bool.TryParse(token.Literal, out var boolValue)
                ? new BooleanLiteral(token, boolValue)
                : null,
            _ => null,
        };
    }

    private QueryExpression ParseIntegerOrLong(Token token)
    {
        if (int.TryParse(token.Literal, out var intValue))
            return new IntegerLiteral(token, intValue);

        if (long.TryParse(token.Literal, out var longValue))
            return new LongLiteral(token, longValue);

        return null;
    }

    private bool PeekTokenIs(TokenType tokenType)
    {
        return _peekToken.Type == tokenType;
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
        return _peekToken.Type == TokenType.IDENT
            && _peekToken.Literal.Equals(identifier, StringComparison.OrdinalIgnoreCase);
    }

    private bool PeekIdentifierIn(params string[] identifier)
    {
        return _peekToken.Type == TokenType.IDENT
            && identifier.Contains(_peekToken.Literal, StringComparer.OrdinalIgnoreCase);
    }

    private bool CurrentIdentifierIs(string identifier)
    {
        return _currentToken.Type == TokenType.IDENT
            && _currentToken.Literal.Equals(identifier, StringComparison.OrdinalIgnoreCase);
    }
}
