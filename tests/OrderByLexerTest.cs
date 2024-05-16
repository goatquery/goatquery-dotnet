using Xunit;

public sealed record Expected
{
    public TokenType Token { get; set; }
    public string Literal { get; set; } = string.Empty;

    public Expected(TokenType token, string literal)
    {
        Token = token;
        Literal = literal;
    }
}

public sealed class OrderByLexerTest
{
    [Fact]
    public void Test_OrderByNextToken()
    {
        var input = @"id asc
        iD desc
        id aSc
        id DeSc
        id AsC
        ";

        var tests = new Expected[]
        {
            new Expected(TokenType.IDENT, "id"),
            new Expected(TokenType.ASC, "asc"),
            new Expected(TokenType.IDENT, "iD"),
            new Expected(TokenType.DESC, "desc"),
            new Expected(TokenType.IDENT, "id"),
            new Expected(TokenType.ASC, "aSc"),
            new Expected(TokenType.IDENT, "id"),
            new Expected(TokenType.DESC, "DeSc"),
            new Expected(TokenType.IDENT, "id"),
            new Expected(TokenType.ASC, "AsC"),
        };

        var lexer = new QueryLexer(input);

        foreach (var test in tests)
        {
            var token = lexer.NextToken();

            Assert.Equal(test.Token, token.Type);
            Assert.Equal(test.Literal, token.Literal);
        }
    }
}