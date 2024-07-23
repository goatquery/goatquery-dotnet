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

        yield return new object[]
        {
            "Name contains 'John'",
            new KeyValuePair<TokenType, string>[]
            {
                new (TokenType.IDENT, "Name"),
                new (TokenType.IDENT, "contains"),
                new (TokenType.STRING, "John"),
            }
        };

        yield return new object[]
        {
            "(Id eq 1 or Id eq 2) and Name eq 'John'",
            new KeyValuePair<TokenType, string>[]
            {
                new (TokenType.LPAREN, "("),
                new (TokenType.IDENT, "Id"),
                new (TokenType.IDENT, "eq"),
                new (TokenType.INT, "1"),
                new (TokenType.IDENT, "or"),
                new (TokenType.IDENT, "Id"),
                new (TokenType.IDENT, "eq"),
                new (TokenType.INT, "2"),
                new (TokenType.RPAREN, ")"),
                new (TokenType.IDENT, "and"),
                new (TokenType.IDENT, "Name"),
                new (TokenType.IDENT, "eq"),
                new (TokenType.STRING, "John")
            }
        };

        yield return new object[]
        {
            "address1Line eq '1 Main Street'",
            new KeyValuePair<TokenType, string>[]
            {
                new (TokenType.IDENT, "address1Line"),
                new (TokenType.IDENT, "eq"),
                new (TokenType.STRING, "1 Main Street"),
            }
        };

        yield return new object[]
        {
            "addASCress1Line contains '10 Test Av'",
            new KeyValuePair<TokenType, string>[]
            {
                new (TokenType.IDENT, "addASCress1Line"),
                new (TokenType.IDENT, "contains"),
                new (TokenType.STRING, "10 Test Av"),
            }
        };

        yield return new object[]
        {
            "id eq e4c7772b-8947-4e46-98ed-644b417d2a08",
            new KeyValuePair<TokenType, string>[]
            {
                new (TokenType.IDENT, "id"),
                new (TokenType.IDENT, "eq"),
                new (TokenType.GUID, "e4c7772b-8947-4e46-98ed-644b417d2a08"),
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