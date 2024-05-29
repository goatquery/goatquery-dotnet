using Xunit;

public sealed class FilterLexerTest
{
    public static IEnumerable<object[]> Parameters()
    {
        yield return new object[]
        {
            "Name eq 'john'",
            new KeyValuePair<TokenType, string>[]
            {
                new (TokenType.IDENT, "Name"),
                new (TokenType.IDENT, "eq"),
                new (TokenType.STRING, "john"),
            }
        };

        yield return new object[]
        {
            "Id eq 1",
            new KeyValuePair<TokenType, string>[]
            {
                new (TokenType.IDENT, "Id"),
                new (TokenType.IDENT, "eq"),
                new (TokenType.INT, "1"),
            }
        };

        yield return new object[]
        {
            "Name eq 'john' and Id eq 1",
            new KeyValuePair<TokenType, string>[]
            {
                new (TokenType.IDENT, "Name"),
                new (TokenType.IDENT, "eq"),
                new (TokenType.STRING, "john"),
                new (TokenType.IDENT, "and"),
                new (TokenType.IDENT, "Id"),
                new (TokenType.IDENT, "eq"),
                new (TokenType.INT, "1"),
            }
        };

        yield return new object[]
        {
            "eq eq 1",
            new KeyValuePair<TokenType, string>[]
            {
                new (TokenType.IDENT, "eq"),
                new (TokenType.IDENT, "eq"),
                new (TokenType.INT, "1"),
            }
        };

        yield return new object[]
        {
            "Name eq 'john' or Id eq 1",
            new KeyValuePair<TokenType, string>[]
            {
                new (TokenType.IDENT, "Name"),
                new (TokenType.IDENT, "eq"),
                new (TokenType.STRING, "john"),
                new (TokenType.IDENT, "or"),
                new (TokenType.IDENT, "Id"),
                new (TokenType.IDENT, "eq"),
                new (TokenType.INT, "1"),
            }
        };

        yield return new object[]
        {
            "Id eq 1 and Name eq 'John' or Id eq 2",
            new KeyValuePair<TokenType, string>[]
            {
                new (TokenType.IDENT, "Id"),
                new (TokenType.IDENT, "eq"),
                new (TokenType.INT, "1"),
                new (TokenType.IDENT, "and"),
                new (TokenType.IDENT, "Name"),
                new (TokenType.IDENT, "eq"),
                new (TokenType.STRING, "John"),
                new (TokenType.IDENT, "or"),
                new (TokenType.IDENT, "Id"),
                new (TokenType.IDENT, "eq"),
                new (TokenType.INT, "2"),
            }
        };

        yield return new object[]
        {
            "Id eq 1 or Name eq 'John' or Id eq 2",
            new KeyValuePair<TokenType, string>[]
            {
                new (TokenType.IDENT, "Id"),
                new (TokenType.IDENT, "eq"),
                new (TokenType.INT, "1"),
                new (TokenType.IDENT, "or"),
                new (TokenType.IDENT, "Name"),
                new (TokenType.IDENT, "eq"),
                new (TokenType.STRING, "John"),
                new (TokenType.IDENT, "or"),
                new (TokenType.IDENT, "Id"),
                new (TokenType.IDENT, "eq"),
                new (TokenType.INT, "2"),
            }
        };

        yield return new object[]
        {
            "Id ne 1",
            new KeyValuePair<TokenType, string>[]
            {
                new (TokenType.IDENT, "Id"),
                new (TokenType.IDENT, "ne"),
                new (TokenType.INT, "1"),
            }
        };
    }

    [Theory]
    [MemberData(nameof(Parameters))]
    public void Test_FilterNextToken(string input, KeyValuePair<TokenType, string>[] expected)
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