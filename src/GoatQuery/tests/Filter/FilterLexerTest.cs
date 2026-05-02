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
                new(TokenType.IDENT, "Name"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.STRING, "john"),
            },
        };

        yield return new object[]
        {
            "Id eq 1",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "Id"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.INT, "1"),
            },
        };

        yield return new object[]
        {
            "Name eq 'john' and Id eq 1",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "Name"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.STRING, "john"),
                new(TokenType.IDENT, "and"),
                new(TokenType.IDENT, "Id"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.INT, "1"),
            },
        };

        yield return new object[]
        {
            "eq eq 1",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "eq"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.INT, "1"),
            },
        };

        yield return new object[]
        {
            "Name eq 'john' or Id eq 1",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "Name"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.STRING, "john"),
                new(TokenType.IDENT, "or"),
                new(TokenType.IDENT, "Id"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.INT, "1"),
            },
        };

        yield return new object[]
        {
            "Id eq 1 and Name eq 'John' or Id eq 2",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "Id"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.INT, "1"),
                new(TokenType.IDENT, "and"),
                new(TokenType.IDENT, "Name"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.STRING, "John"),
                new(TokenType.IDENT, "or"),
                new(TokenType.IDENT, "Id"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.INT, "2"),
            },
        };

        yield return new object[]
        {
            "Id eq 1 or Name eq 'John' or Id eq 2",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "Id"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.INT, "1"),
                new(TokenType.IDENT, "or"),
                new(TokenType.IDENT, "Name"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.STRING, "John"),
                new(TokenType.IDENT, "or"),
                new(TokenType.IDENT, "Id"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.INT, "2"),
            },
        };

        yield return new object[]
        {
            "Id ne 1",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "Id"),
                new(TokenType.IDENT, "ne"),
                new(TokenType.INT, "1"),
            },
        };

        yield return new object[]
        {
            "Name contains 'John'",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "Name"),
                new(TokenType.IDENT, "contains"),
                new(TokenType.STRING, "John"),
            },
        };

        yield return new object[]
        {
            "(Id eq 1 or Id eq 2) and Name eq 'John'",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.LPAREN, "("),
                new(TokenType.IDENT, "Id"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.INT, "1"),
                new(TokenType.IDENT, "or"),
                new(TokenType.IDENT, "Id"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.INT, "2"),
                new(TokenType.RPAREN, ")"),
                new(TokenType.IDENT, "and"),
                new(TokenType.IDENT, "Name"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.STRING, "John"),
            },
        };

        yield return new object[]
        {
            "address1Line eq '1 Main Street'",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "address1Line"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.STRING, "1 Main Street"),
            },
        };

        yield return new object[]
        {
            "addASCress1Line contains '10 Test Av'",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "addASCress1Line"),
                new(TokenType.IDENT, "contains"),
                new(TokenType.STRING, "10 Test Av"),
            },
        };

        yield return new object[]
        {
            "id eq e4c7772b-8947-4e46-98ed-644b417d2a08",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "id"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.GUID, "e4c7772b-8947-4e46-98ed-644b417d2a08"),
            },
        };

        yield return new object[]
        {
            "id eq 10m",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "id"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.DECIMAL, "10m"),
            },
        };

        yield return new object[]
        {
            "id eq 10.50m",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "id"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.DECIMAL, "10.50m"),
            },
        };

        yield return new object[]
        {
            "id eq 10.50M",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "id"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.DECIMAL, "10.50M"),
            },
        };

        yield return new object[]
        {
            "id eq 10f",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "id"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.FLOAT, "10f"),
            },
        };

        yield return new object[]
        {
            "id ne 0.1121563052701180f",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "id"),
                new(TokenType.IDENT, "ne"),
                new(TokenType.FLOAT, "0.1121563052701180f"),
            },
        };

        yield return new object[]
        {
            "id ne 0.1121563052701180F",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "id"),
                new(TokenType.IDENT, "ne"),
                new(TokenType.FLOAT, "0.1121563052701180F"),
            },
        };

        yield return new object[]
        {
            "id eq 10d",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "id"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.DOUBLE, "10d"),
            },
        };

        yield return new object[]
        {
            "id eq 3.14159265359d",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "id"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.DOUBLE, "3.14159265359d"),
            },
        };

        yield return new object[]
        {
            "id eq 3.14159265359D",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "id"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.DOUBLE, "3.14159265359D"),
            },
        };

        yield return new object[]
        {
            "age lt 50",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "age"),
                new(TokenType.IDENT, "lt"),
                new(TokenType.INT, "50"),
            },
        };

        yield return new object[]
        {
            "age lte 50",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "age"),
                new(TokenType.IDENT, "lte"),
                new(TokenType.INT, "50"),
            },
        };

        yield return new object[]
        {
            "age gt 50",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "age"),
                new(TokenType.IDENT, "gt"),
                new(TokenType.INT, "50"),
            },
        };

        yield return new object[]
        {
            "age gte 50",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "age"),
                new(TokenType.IDENT, "gte"),
                new(TokenType.INT, "50"),
            },
        };

        yield return new object[]
        {
            "dateOfBirth eq 2000-01-01",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "dateOfBirth"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.DATE, "2000-01-01"),
            },
        };

        yield return new object[]
        {
            "dateOfBirth lt 2000-01-01",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "dateOfBirth"),
                new(TokenType.IDENT, "lt"),
                new(TokenType.DATE, "2000-01-01"),
            },
        };

        yield return new object[]
        {
            "dateOfBirth lte 2000-01-01",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "dateOfBirth"),
                new(TokenType.IDENT, "lte"),
                new(TokenType.DATE, "2000-01-01"),
            },
        };

        yield return new object[]
        {
            "dateOfBirth gt 2000-01-01",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "dateOfBirth"),
                new(TokenType.IDENT, "gt"),
                new(TokenType.DATE, "2000-01-01"),
            },
        };

        yield return new object[]
        {
            "dateOfBirth gte 2000-01-01",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "dateOfBirth"),
                new(TokenType.IDENT, "gte"),
                new(TokenType.DATE, "2000-01-01"),
            },
        };

        yield return new object[]
        {
            "dateOfBirth eq 2023-01-01T15:30:00Z",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "dateOfBirth"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.DATETIME, "2023-01-01T15:30:00Z"),
            },
        };

        yield return new object[]
        {
            "dateOfBirth eq 2023-01-30T09:29:55.1750906Z",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "dateOfBirth"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.DATETIME, "2023-01-30T09:29:55.1750906Z"),
            },
        };

        yield return new object[]
        {
            "balance eq null",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "balance"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.NULL, "null"),
            },
        };

        yield return new object[]
        {
            "balance ne NULL",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "balance"),
                new(TokenType.IDENT, "ne"),
                new(TokenType.NULL, "NULL"),
            },
        };

        yield return new object[]
        {
            "name eq 'test' and balance eq null",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "name"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.STRING, "test"),
                new(TokenType.IDENT, "and"),
                new(TokenType.IDENT, "balance"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.NULL, "null"),
            },
        };

        yield return new object[]
        {
            "manager/firstName eq 'John'",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "manager"),
                new(TokenType.SLASH, "/"),
                new(TokenType.IDENT, "firstName"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.STRING, "John"),
            },
        };

        yield return new object[]
        {
            "manager/manager/firstName eq 'John'",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "manager"),
                new(TokenType.SLASH, "/"),
                new(TokenType.IDENT, "manager"),
                new(TokenType.SLASH, "/"),
                new(TokenType.IDENT, "firstName"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.STRING, "John"),
            },
        };

        yield return new object[]
        {
            "tags/any(t: t eq 'tag 2')",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "tags"),
                new(TokenType.SLASH, "/"),
                new(TokenType.IDENT, "any"),
                new(TokenType.LPAREN, "("),
                new(TokenType.IDENT, "t"),
                new(TokenType.COLON, ":"),
                new(TokenType.IDENT, "t"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.STRING, "tag 2"),
                new(TokenType.RPAREN, ")"),
            },
        };

        // Basic all() syntax
        yield return new object[]
        {
            "tags/all(item: item contains 'test')",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "tags"),
                new(TokenType.SLASH, "/"),
                new(TokenType.IDENT, "all"),
                new(TokenType.LPAREN, "("),
                new(TokenType.IDENT, "item"),
                new(TokenType.COLON, ":"),
                new(TokenType.IDENT, "item"),
                new(TokenType.IDENT, "contains"),
                new(TokenType.STRING, "test"),
                new(TokenType.RPAREN, ")"),
            },
        };

        // Nested object property access in lambda
        yield return new object[]
        {
            "addresses/any(address: address/city eq 'New York')",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "addresses"),
                new(TokenType.SLASH, "/"),
                new(TokenType.IDENT, "any"),
                new(TokenType.LPAREN, "("),
                new(TokenType.IDENT, "address"),
                new(TokenType.COLON, ":"),
                new(TokenType.IDENT, "address"),
                new(TokenType.SLASH, "/"),
                new(TokenType.IDENT, "city"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.STRING, "New York"),
                new(TokenType.RPAREN, ")"),
            },
        };

        yield return new object[]
        {
            "status eq 0",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "status"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.INT, "0"),
            },
        };

        yield return new object[]
        {
            "status eq 1",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "status"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.INT, "1"),
            },
        };

        yield return new object[]
        {
            "status ne 0",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "status"),
                new(TokenType.IDENT, "ne"),
                new(TokenType.INT, "0"),
            },
        };

        yield return new object[]
        {
            "gender eq 'Male'",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "gender"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.STRING, "Male"),
            },
        };

        yield return new object[]
        {
            "gender eq 'Female'",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "gender"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.STRING, "Female"),
            },
        };

        yield return new object[]
        {
            "gender eq 'Alternative'",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "gender"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.STRING, "Alternative"),
            },
        };

        yield return new object[]
        {
            "gender ne 'Male'",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "gender"),
                new(TokenType.IDENT, "ne"),
                new(TokenType.STRING, "Male"),
            },
        };

        yield return new object[]
        {
            "gender eq null",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "gender"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.NULL, "null"),
            },
        };

        yield return new object[]
        {
            "gender eq 'Male' and status eq 0",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "gender"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.STRING, "Male"),
                new(TokenType.IDENT, "and"),
                new(TokenType.IDENT, "status"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.INT, "0"),
            },
        };

        yield return new object[]
        {
            @"Name eq 'O\'Brien'",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "Name"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.STRING, "O'Brien"),
            },
        };

        yield return new object[]
        {
            @"Name eq 'back\\slash'",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "Name"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.STRING, @"back\slash"),
            },
        };

        yield return new object[]
        {
            "id eq 100L",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "id"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.LONG, "100L"),
            },
        };

        yield return new object[]
        {
            "id eq 100l",
            new KeyValuePair<TokenType, string>[]
            {
                new(TokenType.IDENT, "id"),
                new(TokenType.IDENT, "eq"),
                new(TokenType.LONG, "100l"),
            },
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
