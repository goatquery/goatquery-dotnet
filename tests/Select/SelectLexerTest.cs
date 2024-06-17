using Xunit;

public sealed class SelectLexerTest
{
    public static IEnumerable<object[]> Parameters()
    {
        yield return new object[]
        {
            "id, firstname",
            new KeyValuePair<TokenType, string>[]
            {
                new (TokenType.IDENT, "id"),
                new (TokenType.COMMA, ","),
                new (TokenType.IDENT, "firstname"),
            }
        };

        yield return new object[]
        {
            "id,firstname",
            new KeyValuePair<TokenType, string>[]
            {
                new (TokenType.IDENT, "id"),
                new (TokenType.COMMA, ","),
                new (TokenType.IDENT, "firstname"),
            }
        };

        yield return new object[]
        {
            "id",
            new KeyValuePair<TokenType, string>[]
            {
                new (TokenType.IDENT, "id"),
            }
        };
    }

    [Theory]
    [MemberData(nameof(Parameters))]
    public void Test_SelectNextToken(string input, KeyValuePair<TokenType, string>[] expected)
    {
        var lexer = new QueryLexer(input);

        foreach (var test in expected)
        {
            var token = lexer.NextToken();

            Assert.Equal(test.Key, token.Type);
            Assert.Equal(test.Value, token.Literal);
        }
    }
}