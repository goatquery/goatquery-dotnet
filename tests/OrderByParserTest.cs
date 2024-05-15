using Xunit;

public sealed class OrderByParserTest
{
    [Theory]
    [InlineData("id asc", "id", "asc")]
    [InlineData("ID desc", "ID", "desc")]
    // [InlineData("Name", "")]
    public void Test_ParsingOrderByStatement(string input, string expectedIdentifier, string expectedOrder)
    {
        var lexer = new QueryLexer(input);
        var parser = new QueryParser(lexer);

        var program = parser.ParseOrderBy();
        Assert.Single(program);

        var statement = program.FirstOrDefault();
        Assert.NotNull(statement);

        
    }
}