using System;
using System.Globalization;
using System.Text;

public sealed class QueryLexer
{
    private readonly string _input;
    private int _position;
    private int _readPosition;
    private char _character;

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
            case char.MinValue:
                token.Literal = "";
                token.Type = TokenType.EOF;
                break;
            case '(':
                token = new Token(TokenType.LPAREN, _character);
                break;
            case ')':
                token = new Token(TokenType.RPAREN, _character);
                break;
            case '/':
                token = new Token(TokenType.SLASH, _character);
                break;
            case ':':
                token = new Token(TokenType.COLON, _character);
                break;
            case '\'':
                token.Type = TokenType.STRING;
                token.Literal = ReadString();
                break;
            case var c when char.IsDigit(c):
                token.Literal = ReadNumericOrDateTime();
                token.Type = DetermineNumericTokenType(token.Literal);
                return token;
            default:
                if (IsLetter(_character))
                {
                    token.Literal = ReadIdentifier();
                    token.Type = ClassifyIdentifier(token.Literal);
                    return token;
                }
                break;
        }

        ReadCharacter();

        return token;
    }

    private bool IsDate(string value)
    {
        return DateTime.TryParseExact(
            value,
            new[] { "yyyy-MM-dd" },
            CultureInfo.InvariantCulture,
            DateTimeStyles.AdjustToUniversal,
            out _
        );
    }

    private bool IsDateTime(string value)
    {
        return DateTime.TryParse(value, out _);
    }

    private bool IsGuid(string value)
    {
        return Guid.TryParse(value, out _);
    }

    private string ReadIdentifier()
    {
        var startPosition = _position;

        while (IsIdentifierCharacter())
        {
            ReadCharacter();
        }

        return _input.Substring(startPosition, _position - startPosition);
    }

    private bool IsIdentifierCharacter()
    {
        return IsLetter(_character)
            || IsDigit(_character)
            || _character == '-'
            || _character == '.';
    }

    private string ReadNumericOrDateTime()
    {
        var startPosition = _position;

        // Read digits, and datetime/numeric characters (colons, dashes, dots, etc.)
        while (_character != char.MinValue && IsNumericOrDateTimeCharacter())
        {
            ReadCharacter();
        }

        return _input.Substring(startPosition, _position - startPosition);
    }

    private bool IsNumericOrDateTimeCharacter()
    {
        return IsDigit(_character)
            || _character == '-'
            || _character == ':'
            || _character == '.'
            || _character == 'T'
            || // DateTime separator
            _character == 'Z'
            || // UTC indicator
            _character == '+'
            || // Timezone offset
            _character == 'f'
            || _character == 'F'
            || // Float suffix
            _character == 'm'
            || _character == 'M'
            || // Decimal suffix
            _character == 'd'
            || _character == 'D'
            || // Double suffix
            _character == 'l'
            || _character == 'L'
            || // Long suffix
            ('a' <= _character && _character <= 'f')
            || // GUID hex chars
            ('A' <= _character && _character <= 'F'); // GUID hex chars (uppercase)
    }

    private TokenType ClassifyIdentifier(string literal)
    {
        if (IsGuid(literal))
            return TokenType.GUID;

        if (literal.Equals(Keywords.Null, StringComparison.OrdinalIgnoreCase))
            return TokenType.NULL;

        if (
            literal.Equals(Keywords.True, StringComparison.OrdinalIgnoreCase)
            || literal.Equals(Keywords.False, StringComparison.OrdinalIgnoreCase)
        )
            return TokenType.BOOLEAN;

        return TokenType.IDENT;
    }

    private TokenType DetermineNumericTokenType(string literal)
    {
        // Check for GUID first (may contain numbers and dashes)
        if (IsGuid(literal))
            return TokenType.GUID;

        // Check for date patterns before datetime (more specific first)
        if (IsDate(literal))
            return TokenType.DATE;

        // Check for datetime patterns (since they contain colons)
        if (IsDateTime(literal))
            return TokenType.DATETIME;

        // Check numeric suffixes
        if (literal.EndsWith("f", StringComparison.OrdinalIgnoreCase))
            return TokenType.FLOAT;

        if (literal.EndsWith("m", StringComparison.OrdinalIgnoreCase))
            return TokenType.DECIMAL;

        if (literal.EndsWith("d", StringComparison.OrdinalIgnoreCase))
            return TokenType.DOUBLE;

        if (literal.EndsWith("l", StringComparison.OrdinalIgnoreCase))
            return TokenType.LONG;

        // Unsuffixed decimal point defaults to double
        if (literal.Contains("."))
            return TokenType.DOUBLE;

        // Default to integer
        return TokenType.INT;
    }

    private bool IsLetter(char ch)
    {
        return 'a' <= ch && ch <= 'z' || 'A' <= ch && ch <= 'Z' || ch == '_';
    }

    private bool IsDigit(char ch)
    {
        return '0' <= ch && ch <= '9';
    }

    private void SkipWhitespace()
    {
        while (_character == ' ' || _character == '\t' || _character == '\n' || _character == '\r')
        {
            ReadCharacter();
        }
    }

    private string ReadString()
    {
        var result = new StringBuilder();

        while (true)
        {
            ReadCharacter();

            if (_character == '\\')
            {
                ReadCharacter();
                switch (_character)
                {
                    case '\'':
                        result.Append('\'');
                        break;
                    case '\\':
                        result.Append('\\');
                        break;
                    default:
                        result.Append('\\');
                        result.Append(_character);
                        break;
                }
                continue;
            }

            if (_character == '\'' || _character == 0)
            {
                break;
            }

            result.Append(_character);
        }

        return result.ToString();
    }
}
