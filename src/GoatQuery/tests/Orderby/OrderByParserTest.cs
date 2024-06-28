using Xunit;

public sealed class OrderByParserTest
{
    public static IEnumerable<object[]> Parameters()
    {
        yield return new object[]
        {
            "ID desc",
            new OrderByStatement[]
            {
                new OrderByStatement(new Token(TokenType.IDENT, "ID"), OrderByDirection.Descending),
            }
        };

        yield return new object[]
        {
            "id asc",
            new OrderByStatement[]
            {
                new OrderByStatement(new Token(TokenType.IDENT, "id"), OrderByDirection.Ascending),
            }
        };

        yield return new object[]
        {
            "Name",
            new OrderByStatement[]
            {
                new OrderByStatement(new Token(TokenType.IDENT, "Name"), OrderByDirection.Ascending),
            }
        };

        yield return new object[]
        {
            "id asc, name desc",
            new OrderByStatement[]
            {
                new OrderByStatement(new Token(TokenType.IDENT, "id"), OrderByDirection.Ascending),
                new OrderByStatement(new Token(TokenType.IDENT, "name"), OrderByDirection.Descending)
            }
        };

        yield return new object[]
        {
            "id asc, name desc, age, address asc, postcode desc",
            new OrderByStatement[]
            {
                new OrderByStatement(new Token(TokenType.IDENT, "id"), OrderByDirection.Ascending),
                new OrderByStatement(new Token(TokenType.IDENT, "name"), OrderByDirection.Descending),
                new OrderByStatement(new Token(TokenType.IDENT, "age"), OrderByDirection.Ascending),
                new OrderByStatement(new Token(TokenType.IDENT, "address"), OrderByDirection.Ascending),
                new OrderByStatement(new Token(TokenType.IDENT, "postcode"), OrderByDirection.Descending)
            }
        };

        yield return new object[]
        {
            "",
            new OrderByStatement[] { }
        };
    }

    [Theory]
    [MemberData(nameof(Parameters))]
    public void Test_ParsingMultipleOrderByStatement(string input, IEnumerable<OrderByStatement> expected)
    {
        var lexer = new QueryLexer(input);
        var parser = new QueryParser(lexer);

        var program = parser.ParseOrderBy();
        Assert.Equal(expected.Count(), program.Count());

        for (var i = 0; i < expected.Count(); i++)
        {
            var expectedStatement = expected.ElementAt(i);
            var statement = program.ElementAt(i);

            Assert.Equal(expectedStatement.TokenLiteral(), statement.TokenLiteral());
            Assert.Equal(expectedStatement.Direction, statement.Direction);
        }
    }
}