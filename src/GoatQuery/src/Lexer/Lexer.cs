namespace GoatQuery;

using System;
using System.Globalization;
using System.Text;

internal sealed class QueryLexer
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
        SkipWhitespace();

        Token token;

        switch (_character)
        {
            case char.MinValue:
                token = new Token(TokenType.EOF, "");
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
                var str = ReadString();
                token = new Token(TokenType.STRING, str);
                break;
            case '-' when _readPosition < _input.Length && char.IsDigit(_input[_readPosition]):
                var negLiteral = ReadNegativeNumeric();
                var negType = negLiteral.Contains(".") ? TokenType.DOUBLE : TokenType.INT;
                return new Token(negType, negLiteral);
            case var c when char.IsDigit(c):
                var numLiteral = ReadNumericOrDateTime();
                var numType = DetermineNumericTokenType(numLiteral);
                return new Token(numType, numLiteral);
            default:
                if (IsLetter(_character))
                {
                    var identifier = ReadIdentifier();
                    var identType = ClassifyIdentifier(identifier);
                    return new Token(identType, identifier);
                }
                token = new Token(TokenType.ILLEGAL, _character);
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
        // DateTime.TryParse is too permissive (e.g., parses "1.5" as Jan 5).
        // Require the value to contain 'T' (datetime) or be exactly a date (yyyy-MM-dd format, 10 chars with dashes).
        if (value.Contains("T") || value.Contains("t"))
        {
            return DateTime.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _
            );
        }

        if (value.Length >= 10 && value[4] == '-' && value[7] == '-')
        {
            return DateTime.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _
            );
        }

        return false;
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

    private string ReadNegativeNumeric()
    {
        var startPosition = _position;

        // Consume the '-'
        ReadCharacter();

        // Read digits and at most one '.'
        var hasDot = false;
        while (_character != char.MinValue && (IsDigit(_character) || _character == '.'))
        {
            if (_character == '.')
            {
                if (hasDot)
                    break;
                hasDot = true;
            }
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

        // Decimal point means it's a floating-point number
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
