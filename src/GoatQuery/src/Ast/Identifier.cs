public sealed class Identifier : QueryExpression
{
    public string Value { get; set; }

    public Identifier(Token token, string value) : base(token)
    {
        Value = value;
    }
}