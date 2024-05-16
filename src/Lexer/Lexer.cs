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
            case char.MinValue:
                token.Literal = "";
                token.Type = TokenType.EOF;
                break;
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