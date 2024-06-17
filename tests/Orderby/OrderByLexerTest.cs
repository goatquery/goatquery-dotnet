using Xunit;

public sealed class OrderByLexerTest
{
    public static IEnumerable<object[]> Parameters()
    {
        yield return new object[]
        {
            "id asc",
            new KeyValuePair<TokenType, string>[]
            {
                new (TokenType.IDENT, "id"),
                new (TokenType.IDENT, "asc"),
            }
        };

        yield return new object[]
        {
            "iD desc",
            new KeyValuePair<TokenType, string>[]
            {
                new (TokenType.IDENT, "iD"),
                new (TokenType.IDENT, "desc"),
            }
        };

        yield return new object[]
        {
            "id aSc",
            new KeyValuePair<TokenType, string>[]
            {
                new (TokenType.IDENT, "id"),
                new (TokenType.IDENT, "aSc"),
            }
        };

        yield return new object[]
        {
            "id DeSc",
            new KeyValuePair<TokenType, string>[]
            {
                new (TokenType.IDENT, "id"),
                new (TokenType.IDENT, "DeSc"),
            }
        };

        yield return new object[]
        {
            "id AsC",
            new KeyValuePair<TokenType, string>[]
            {
                new (TokenType.IDENT, "id"),
                new (TokenType.IDENT, "AsC"),
            }
        };

        yield return new object[]
        {
            "asc asc",
            new KeyValuePair<TokenType, string>[]
            {
                new (TokenType.IDENT, "asc"),
                new (TokenType.IDENT, "asc"),
            }
        };

        yield return new object[]
        {
            "asc asc",
            new KeyValuePair<TokenType, string>[]
            {
                new (TokenType.IDENT, "asc"),
                new (TokenType.IDENT, "asc"),
            }
        };

        yield return new object[]
        {
            "id asc, firstname desc",
            new KeyValuePair<TokenType, string>[]
            {
                new (TokenType.IDENT, "id"),
                new (TokenType.IDENT, "asc"),

                new (TokenType.COMMA, ","),

                new (TokenType.IDENT, "firstname"),
                new (TokenType.IDENT, "desc"),
            }
        };
    }

    [Theory]
    [MemberData(nameof(Parameters))]
    public void Test_OrderByNextToken(string input, KeyValuePair<TokenType, string>[] expected)
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