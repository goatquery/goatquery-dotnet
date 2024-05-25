using Xunit;

public sealed class FilterLexerTest
{
    [Fact]
    public void Test_FilterNextToken()
    {
        var input = @"Name eq 'john'
        Id eq 1
        Name eq 'john' and Id eq 1
        eq eq 1
        Name eq 'john' or Id eq 1
        ";

        var tests = new Expected[]
        {
            new Expected(TokenType.IDENT, "Name"),
            new Expected(TokenType.IDENT, "eq"),
            new Expected(TokenType.STRING, "john"),

            new Expected(TokenType.IDENT, "Id"),
            new Expected(TokenType.IDENT, "eq"),
            new Expected(TokenType.INT, "1"),

            new Expected(TokenType.IDENT, "Name"),
            new Expected(TokenType.IDENT, "eq"),
            new Expected(TokenType.STRING, "john"),
            new Expected(TokenType.IDENT, "and"),
            new Expected(TokenType.IDENT, "Id"),
            new Expected(TokenType.IDENT, "eq"),
            new Expected(TokenType.INT, "1"),

            new Expected(TokenType.IDENT, "eq"),
            new Expected(TokenType.IDENT, "eq"),
            new Expected(TokenType.INT, "1"),

            new Expected(TokenType.IDENT, "Name"),
            new Expected(TokenType.IDENT, "eq"),
            new Expected(TokenType.STRING, "john"),
            new Expected(TokenType.IDENT, "or"),
            new Expected(TokenType.IDENT, "Id"),
            new Expected(TokenType.IDENT, "eq"),
            new Expected(TokenType.INT, "1"),
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