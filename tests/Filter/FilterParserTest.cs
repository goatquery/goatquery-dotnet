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

        var expression = program.Expression;
        Assert.NotNull(expression);

        Assert.Equal(expectedLeft, expression.Left.TokenLiteral());
        Assert.Equal(expectedOperator, expression.Operator);
        Assert.Equal(expectedRight, expression.Right.TokenLiteral());
    }

    [Fact]
    public void Test_ParsingFilterStatementWithAnd()
    {
        var input = "Name eq 'John' and Age eq 10";

        var lexer = new QueryLexer(input);
        var parser = new QueryParser(lexer);

        var program = parser.ParseFilter();

        var expression = program.Expression;
        Assert.NotNull(expression);

        var left = expression.Left as InfixExpression;
        Assert.NotNull(left);

        Assert.Equal("Name", left.Left.TokenLiteral());
        Assert.Equal("eq", left.Operator);
        Assert.Equal("John", left.Right.TokenLiteral());

        Assert.Equal("and", expression.Operator);

        var right = expression.Right as InfixExpression;
        Assert.NotNull(right);

        Assert.Equal("Age", right.Left.TokenLiteral());
        Assert.Equal("eq", right.Operator);
        Assert.Equal("10", right.Right.TokenLiteral());
    }

    [Fact]
    public void Test_ParsingFilterStatementWithOr()
    {
        var input = "Name eq 'John' or Age eq 10";

        var lexer = new QueryLexer(input);
        var parser = new QueryParser(lexer);

        var program = parser.ParseFilter();

        var expression = program.Expression;
        Assert.NotNull(expression);

        var left = expression.Left as InfixExpression;
        Assert.NotNull(left);

        Assert.Equal("Name", left.Left.TokenLiteral());
        Assert.Equal("eq", left.Operator);
        Assert.Equal("John", left.Right.TokenLiteral());

        Assert.Equal("or", expression.Operator);

        var right = expression.Right as InfixExpression;
        Assert.NotNull(right);

        Assert.Equal("Age", right.Left.TokenLiteral());
        Assert.Equal("eq", right.Operator);
        Assert.Equal("10", right.Right.TokenLiteral());
    }

    [Fact]
    public void Test_ParsingFilterStatementWithAndAndOr()
    {
        var input = "Name eq 'John' and Age eq 10 or Id eq 10";

        var lexer = new QueryLexer(input);
        var parser = new QueryParser(lexer);

        var program = parser.ParseFilter();

        var expression = program.Expression;
        Assert.NotNull(expression);

        var left = expression.Left as InfixExpression;
        Assert.NotNull(left);

        var innerLeft = left.Left as InfixExpression;
        Assert.NotNull(innerLeft);

        Assert.Equal("Name", innerLeft.Left.TokenLiteral());
        Assert.Equal("eq", innerLeft.Operator);
        Assert.Equal("John", innerLeft.Right.TokenLiteral());

        Assert.Equal("and", left.Operator);

        var innerRight = left.Right as InfixExpression;
        Assert.NotNull(innerRight);

        Assert.Equal("Age", innerRight.Left.TokenLiteral());
        Assert.Equal("eq", innerRight.Operator);
        Assert.Equal("10", innerRight.Right.TokenLiteral());

        Assert.Equal("or", expression.Operator);

        var right = expression.Right as InfixExpression;
        Assert.NotNull(right);

        Assert.Equal("Id", right.Left.TokenLiteral());
        Assert.Equal("eq", right.Operator);
        Assert.Equal("10", right.Right.TokenLiteral());
    }
}