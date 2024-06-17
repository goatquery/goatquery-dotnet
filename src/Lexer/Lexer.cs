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

        SkipCharacters();

        switch (_character)
        {
            case char.MinValue:
                token = new Token(TokenType.EOF, "");
                break;
            case '\'':
                token.Type = TokenType.STRING;
                token.Literal = ReadString();
                break;
            case ',':
                token = new Token(TokenType.COMMA, _character);
                break;
            default:
                if (IsLetter(_character))
                {
                    token.Literal = ReadIdentifier();
                    token.Type = TokenType.IDENT;
                    return token;
                }
                else if (IsDigit(_character))
                {
                    token.Literal = ReadNumber();
                    token.Type = TokenType.INT;
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

    private bool IsDigit(char ch)
    {
        return '0' <= ch && ch <= '9';
    }

    private void SkipCharacters()
    {
        while (_character == ' ' || _character == '\t' || _character == '\n' || _character == '\r')
        {
            ReadCharacter();
        }
    }

    private string ReadString()
    {
        var currentPosition = _position + 1;

        while (true)
        {
            ReadCharacter();
            if (_character == '\'' || _character == 0)
            {
                break;
            }
        }

        return _input.Substring(currentPosition, _position - currentPosition);
    }

    private string ReadNumber()
    {
        var currentPosition = _position;

        while (IsDigit(_character))
        {
            ReadCharacter();
        }

        return _input.Substring(currentPosition, _position - currentPosition);
    }
}