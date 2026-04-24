using Xunit;

public sealed class FilterParserTest
{
    [Theory]
    [InlineData("Name eq 'John'", "Name", "eq", "John")]
    [InlineData("Firstname eq 'Jane'", "Firstname", "eq", "Jane")]
    [InlineData("Age eq 21", "Age", "eq", "21")]
    [InlineData("Age ne 10", "Age", "ne", "10")]
    [InlineData("Name contains 'John'", "Name", "contains", "John")]
    [InlineData("Id eq e4c7772b-8947-4e46-98ed-644b417d2a08", "Id", "eq", "e4c7772b-8947-4e46-98ed-644b417d2a08")]
    [InlineData("Id eq 3.14159265359f", "Id", "eq", "3.14159265359f")]
    [InlineData("Id eq 3.14159265359m", "Id", "eq", "3.14159265359m")]
    [InlineData("Id eq 3.14159265359d", "Id", "eq", "3.14159265359d")]
    [InlineData("Age lt 99", "Age", "lt", "99")]
    [InlineData("Age lte 99", "Age", "lte", "99")]
    [InlineData("Age gt 99", "Age", "gt", "99")]
    [InlineData("Age gte 99", "Age", "gte", "99")]
    [InlineData("dateOfBirth eq 2000-01-01", "dateOfBirth", "eq", "2000-01-01")]
    [InlineData("dateOfBirth lt 2000-01-01", "dateOfBirth", "lt", "2000-01-01")]
    [InlineData("dateOfBirth lte 2000-01-01", "dateOfBirth", "lte", "2000-01-01")]
    [InlineData("dateOfBirth gt 2000-01-01", "dateOfBirth", "gt", "2000-01-01")]
    [InlineData("dateOfBirth gte 2000-01-01", "dateOfBirth", "gte", "2000-01-01")]
    [InlineData("dateOfBirth eq 2023-01-30T09:29:55.1750906Z", "dateOfBirth", "eq", "2023-01-30T09:29:55.1750906Z")]
    [InlineData("balance eq null", "balance", "eq", "null")]
    [InlineData("balance ne null", "balance", "ne", "null")]
    [InlineData("name eq NULL", "name", "eq", "NULL")]
    [InlineData(@"Name eq 'O\'Brien'", "Name", "eq", "O'Brien")]
    [InlineData(@"Name eq 'back\\slash'", "Name", "eq", @"back\slash")]
    public void Test_ParsingFilterStatement(string input, string expectedLeft, string expectedOperator, string expectedRight)
    {
        var lexer = new QueryLexer(input);
        var parser = new QueryParser(lexer);

        var program = parser.ParseFilter();

        var expression = program.Value.Expression;
        Assert.NotNull(expression);

        Assert.Equal(expectedLeft, expression.Left.TokenLiteral());
        Assert.Equal(expectedOperator, expression.Operator);
        Assert.Equal(expectedRight, expression.Right.TokenLiteral());
    }

    [Theory]
    [InlineData("Name")]
    [InlineData("")]
    [InlineData("eq nee")]
    [InlineData("name nee 10")]
    [InlineData("id contains 10")]
    [InlineData("id contaiins '10'")]
    [InlineData("id eq       John'")]
    [InlineData("name contains null")]
    [InlineData("age lt null")]
    [InlineData("age gt null")]
    [InlineData("age lte null")]
    [InlineData("age gte null")]
    public void Test_ParsingInvalidFilterReturnsError(string input)
    {
        var lexer = new QueryLexer(input);
        var parser = new QueryParser(lexer);

        var result = parser.ParseFilter();

        Assert.True(result.IsFailed);
    }

    [Fact]
    public void Test_ParsingFilterStatementWithAnd()
    {
        var input = "Name eq 'John' and Age eq 10";

        var lexer = new QueryLexer(input);
        var parser = new QueryParser(lexer);

        var program = parser.ParseFilter();

        var expression = program.Value.Expression;
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

        var expression = program.Value.Expression;
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

        var expression = program.Value.Expression;
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

    [Theory]
    [InlineData("manager/firstName eq 'John'", new string[] { "manager", "firstName" }, "eq", "John")]
    [InlineData("manager/manager/firstName eq 'John'", new string[] { "manager", "manager", "firstName" }, "eq", "John")]
    public void Test_ParsingFilterStatementWithNestedProperty(string input, string[] expectedLeft, string expectedOperator, string expectedRight)
    {
        var lexer = new QueryLexer(input);
        var parser = new QueryParser(lexer);

        var program = parser.ParseFilter();

        var expression = program.Value.Expression;
        Assert.NotNull(expression);

        var left = expression.Left as PropertyPath;
        Assert.NotNull(left);

        Assert.Equal(expectedLeft, left.Segments);
        Assert.Equal(expectedOperator, expression.Operator);
        Assert.Equal(expectedRight, expression.Right.TokenLiteral());
    }

    [Theory]
    [InlineData("tags/any(t: t eq 'tag 2')", "tags", "any", "t", "t", "eq", "tag 2")]
    [InlineData("tags/all(item: item contains 'test')", "tags", "all", "item", "item", "contains", "test")]
    [InlineData("categories/any(c: c eq 'electronics')", "categories", "any", "c", "c", "eq", "electronics")]
    [InlineData("items/all(i: i ne null)", "items", "all", "i", "i", "ne", "null")]
    public void Test_ParsingQueryLambdaExpression(string input, string expectedProperty, string expectedFunction,
        string expectedParameter, string expectedLambdaLeft, string expectedLambdaOperator, string expectedLambdaRight)
    {
        var lexer = new QueryLexer(input);
        var parser = new QueryParser(lexer);

        var program = parser.ParseFilter();

        Assert.True(program.IsSuccess);
        var expression = program.Value.Expression;
        Assert.NotNull(expression);

        // Lambda expressions are wrapped in InfixExpression with empty operator
        var lambda = expression.Left as QueryLambdaExpression;
        Assert.NotNull(lambda);
        Assert.Equal(string.Empty, expression.Operator);

        // Verify lambda structure
        Assert.Equal(expectedProperty, lambda.Property.TokenLiteral());
        Assert.Equal(expectedFunction, lambda.Function);
        Assert.Equal(expectedParameter, lambda.Parameter);

        // Verify lambda body (inner expression)
        var bodyExpression = lambda.Body as InfixExpression;
        Assert.NotNull(bodyExpression);
        Assert.Equal(expectedLambdaLeft, bodyExpression.Left.TokenLiteral());
        Assert.Equal(expectedLambdaOperator, bodyExpression.Operator);
        Assert.Equal(expectedLambdaRight, bodyExpression.Right.TokenLiteral());
    }

    [Theory]
    [InlineData("addresses/any(address: address/city eq 'New York')", "addresses", "any", "address", new string[] { "address", "city" }, "eq", "New York")]
    [InlineData("orders/all(order: order/status eq 'completed')", "orders", "all", "order", new string[] { "order", "status" }, "eq", "completed")]
    public void Test_ParsingQueryLambdaExpressionWithNestedProperty(string input, string expectedProperty, string expectedFunction,
        string expectedParameter, string[] expectedNestedProperty, string expectedOperator, string expectedValue)
    {
        var lexer = new QueryLexer(input);
        var parser = new QueryParser(lexer);

        var program = parser.ParseFilter();

        Assert.True(program.IsSuccess);
        var expression = program.Value.Expression;
        Assert.NotNull(expression);

        // Lambda expressions are wrapped in InfixExpression with empty operator
        var lambda = expression.Left as QueryLambdaExpression;
        Assert.NotNull(lambda);
        Assert.Equal(string.Empty, expression.Operator);

        // Verify lambda structure
        Assert.Equal(expectedProperty, lambda.Property.TokenLiteral());
        Assert.Equal(expectedFunction, lambda.Function);
        Assert.Equal(expectedParameter, lambda.Parameter);

        // Verify lambda body contains nested property access
        var bodyExpression = lambda.Body as InfixExpression;
        Assert.NotNull(bodyExpression);

        var propertyPath = bodyExpression.Left as PropertyPath;
        Assert.NotNull(propertyPath);
        Assert.Equal(expectedNestedProperty, propertyPath.Segments);
        Assert.Equal(expectedOperator, bodyExpression.Operator);
        Assert.Equal(expectedValue, bodyExpression.Right.TokenLiteral());
    }

    [Theory]
    [InlineData("name eq 'John' and tags/any(t: t eq 'important')", "and")]
    [InlineData("age gt 18 or categories/all(c: c ne null)", "or")]
    [InlineData("tags/any(t: t contains 'work') and status eq 'active'", "and")]
    public void Test_ParsingQueryLambdaExpressionWithLogicalOperators(string input, string expectedLogicalOperator)
    {
        var lexer = new QueryLexer(input);
        var parser = new QueryParser(lexer);

        var program = parser.ParseFilter();

        Assert.True(program.IsSuccess);
        var expression = program.Value.Expression;
        Assert.NotNull(expression);

        // Verify the logical operator between expressions
        Assert.Equal(expectedLogicalOperator, expression.Operator);

        // One side should be a regular expression, the other should contain a lambda
        // The exact structure depends on precedence, but we can verify both sides exist
        Assert.NotNull(expression.Left);
        Assert.NotNull(expression.Right);
    }

    [Theory]
    [InlineData("tags/any(t: t eq 'tag1' and t ne 'tag2')", "tags", "any", "t")]
    [InlineData("items/all(i: i/price gt 100 or i/discount lt 0.1)", "items", "all", "i")]
    public void Test_ParsingComplexQueryLambdaExpression(string input, string expectedProperty, string expectedFunction, string expectedParameter)
    {
        var lexer = new QueryLexer(input);
        var parser = new QueryParser(lexer);

        var program = parser.ParseFilter();

        Assert.True(program.IsSuccess);
        var expression = program.Value.Expression;
        Assert.NotNull(expression);

        // Lambda expressions are wrapped in InfixExpression with empty operator
        var lambda = expression.Left as QueryLambdaExpression;
        Assert.NotNull(lambda);
        Assert.Equal(string.Empty, expression.Operator);

        // Verify basic lambda structure
        Assert.Equal(expectedProperty, lambda.Property.TokenLiteral());
        Assert.Equal(expectedFunction, lambda.Function);
        Assert.Equal(expectedParameter, lambda.Parameter);

        // Verify lambda body contains complex expressions with logical operators
        var bodyExpression = lambda.Body as InfixExpression;
        Assert.NotNull(bodyExpression);

        // The body should have logical operators (and/or)
        Assert.True(bodyExpression.Operator.Equals("and", StringComparison.OrdinalIgnoreCase) ||
                   bodyExpression.Operator.Equals("or", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("tags/any(t: t eq)")]                    // Missing right operand
    [InlineData("tags/any(t t eq 'test')")]              // Missing colon
    [InlineData("tags/any( : t eq 'test')")]             // Missing parameter name
    [InlineData("tags/any(t:)")]                         // Missing lambda body
    [InlineData("tags/any")]                             // Missing parentheses
    [InlineData("tags/any()")]                           // Empty lambda
    [InlineData("tags/invalid(t: t eq 'test')")]         // Invalid function name
    public void Test_ParsingInvalidQueryLambdaExpression(string input)
    {
        var lexer = new QueryLexer(input);
        var parser = new QueryParser(lexer);

        var result = parser.ParseFilter();

        Assert.True(result.IsFailed);
    }

    [Theory]
    [InlineData("status eq 0", "status", "eq", "0")]
    [InlineData("status eq 1", "status", "eq", "1")]
    [InlineData("status ne 0", "status", "ne", "0")]
    [InlineData("status ne 1", "status", "ne", "1")]
    public void Test_ParsingEnumWithIntegerValue(string input, string expectedLeft, string expectedOperator, string expectedRight)
    {
        var lexer = new QueryLexer(input);
        var parser = new QueryParser(lexer);

        var program = parser.ParseFilter();

        var expression = program.Value.Expression;
        Assert.NotNull(expression);

        Assert.Equal(expectedLeft, expression.Left.TokenLiteral());
        Assert.Equal(expectedOperator, expression.Operator);
        Assert.Equal(expectedRight, expression.Right.TokenLiteral());
    }

    [Theory]
    [InlineData("gender eq 'Male'", "gender", "eq", "Male")]
    [InlineData("gender eq 'Female'", "gender", "eq", "Female")]
    [InlineData("gender eq 'Alternative'", "gender", "eq", "Alternative")]
    [InlineData("gender ne 'Male'", "gender", "ne", "Male")]
    [InlineData("gender ne 'Female'", "gender", "ne", "Female")]
    public void Test_ParsingEnumWithStringValue(string input, string expectedLeft, string expectedOperator, string expectedRight)
    {
        var lexer = new QueryLexer(input);
        var parser = new QueryParser(lexer);

        var program = parser.ParseFilter();

        var expression = program.Value.Expression;
        Assert.NotNull(expression);

        Assert.Equal(expectedLeft, expression.Left.TokenLiteral());
        Assert.Equal(expectedOperator, expression.Operator);
        Assert.Equal(expectedRight, expression.Right.TokenLiteral());
    }

    [Theory]
    [InlineData("gender eq null", "gender", "eq", "null")]
    [InlineData("gender ne null", "gender", "ne", "null")]
    public void Test_ParsingNullableEnum(string input, string expectedLeft, string expectedOperator, string expectedRight)
    {
        var lexer = new QueryLexer(input);
        var parser = new QueryParser(lexer);

        var program = parser.ParseFilter();

        var expression = program.Value.Expression;
        Assert.NotNull(expression);

        Assert.Equal(expectedLeft, expression.Left.TokenLiteral());
        Assert.Equal(expectedOperator, expression.Operator);
        Assert.Equal(expectedRight, expression.Right.TokenLiteral());
    }

    [Fact]
    public void Test_ParsingEnumWithLogicalOperators()
    {
        var input = "gender eq 'Male' and status eq 0";

        var lexer = new QueryLexer(input);
        var parser = new QueryParser(lexer);

        var program = parser.ParseFilter();

        var expression = program.Value.Expression;
        Assert.NotNull(expression);

        var left = expression.Left as InfixExpression;
        Assert.NotNull(left);

        Assert.Equal("gender", left.Left.TokenLiteral());
        Assert.Equal("eq", left.Operator);
        Assert.Equal("Male", left.Right.TokenLiteral());

        Assert.Equal("and", expression.Operator);

        var right = expression.Right as InfixExpression;
        Assert.NotNull(right);

        Assert.Equal("status", right.Left.TokenLiteral());
        Assert.Equal("eq", right.Operator);
        Assert.Equal("0", right.Right.TokenLiteral());
    }

}
