using Xunit;

public sealed class FilterParserTest
{
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

    [Fact]
    public void Test_ParsingFilterStatementWithAnd()
    {
        var input = "Name eq 'John' and Age eq 10";

        var lexer = new QueryLexer(input);
        var parser = new QueryParser(lexer);

        var program = parser.ParseFilter();
        Assert.Equal(2, program.Count());

        var firstStatement = program.ElementAt(0);
        Assert.NotNull(firstStatement);

        Assert.Equal("Name", firstStatement.Left.TokenLiteral());
        Assert.Equal("eq", firstStatement.Operator);
        Assert.Equal("John", firstStatement.Right.TokenLiteral());

        var secondStatement = program.ElementAt(1);
        Assert.NotNull(firstStatement);

        Assert.Equal("Age", secondStatement.Left.TokenLiteral());
        Assert.Equal("eq", secondStatement.Operator);
        Assert.Equal("10", secondStatement.Right.TokenLiteral());
    }
}