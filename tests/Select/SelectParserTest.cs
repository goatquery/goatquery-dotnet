using Xunit;

public sealed class SelectParserTest
{
    public static IEnumerable<object[]> Parameters()
    {
        yield return new object[]
        {
            "id",
            new SelectStatement[]
            {
                new SelectStatement(new Token(TokenType.IDENT, "id")),
            }
        };

        yield return new object[]
        {
            "id ,asc",
            new SelectStatement[]
            {
                new SelectStatement(new Token(TokenType.IDENT, "id")),
                new SelectStatement(new Token(TokenType.IDENT, "asc")),
            }
        };

        yield return new object[]
        {
            "Id, Name",
            new SelectStatement[]
            {
                new SelectStatement(new Token(TokenType.IDENT, "Id")),
                new SelectStatement(new Token(TokenType.IDENT, "Name")),
            }
        };

        yield return new object[]
        {
            "id, name, age ,address , postcode",
            new SelectStatement[]
            {
                new SelectStatement(new Token(TokenType.IDENT, "id")),
                new SelectStatement(new Token(TokenType.IDENT, "name")),
                new SelectStatement(new Token(TokenType.IDENT, "age")),
                new SelectStatement(new Token(TokenType.IDENT, "address")),
                new SelectStatement(new Token(TokenType.IDENT, "postcode")),
            }
        };

        yield return new object[]
        {
            "",
            new SelectStatement[] { }
        };
    }

    [Theory]
    [MemberData(nameof(Parameters))]
    public void Test_ParsingSelectStatement(string input, IEnumerable<SelectStatement> expected)
    {
        var lexer = new QueryLexer(input);
        var parser = new QueryParser(lexer);

        var program = parser.ParseSelect();
        Assert.Equal(expected.Count(), program.Count());

        for (var i = 0; i < expected.Count(); i++)
        {
            var expectedStatement = expected.ElementAt(i);
            var statement = program.ElementAt(i);

            Assert.Equal(expectedStatement.TokenLiteral(), statement.TokenLiteral());
        }
    }

    [Theory]
    [InlineData("Id Name")]
    [InlineData("Id ,1")]
    public void Test_InvaliSelectThrowsException(string input)
    {
        var lexer = new QueryLexer(input);
        var parser = new QueryParser(lexer);

        Assert.Throws<GoatQueryException>(parser.ParseSelect);
    }
}