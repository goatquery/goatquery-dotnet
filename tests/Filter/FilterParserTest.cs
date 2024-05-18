using Xunit;

public sealed class FilterParserTest
{
    public static IEnumerable<object[]> Parameters()
    {
        yield return new object[]
        {
            "Name eq 'John'",
            new OrderByStatement[]
            {
                new OrderByStatement(new Token(TokenType.IDENT, "ID"), OrderByDirection.Descending),
            }
        };
    }

    [Theory]
    [InlineData("Name eq 'John'", "Name", "eq", "John")]
    [InlineData("Firstname eq 'Jane'", "Firstname", "eq", "Jane")]
    [InlineData("Age eq 21", "Age", "eq", "21")]
    public void Test_ParsingFilterStatement(string input, string expectedLeft, string expectedOperator, string expectedRight)
    {
        var lexer = new QueryLexer(input);
        var parser = new QueryParser(lexer);

        var program = parser.ParseFilter();
        Assert.Single(program);

        var statement = program.FirstOrDefault();
        Assert.NotNull(statement);

        Assert.Equal(expectedLeft, statement.Left.TokenLiteral());
        Assert.Equal(expectedOperator, statement.Operator);
        Assert.Equal(expectedRight, statement.Right.TokenLiteral());
    }
}