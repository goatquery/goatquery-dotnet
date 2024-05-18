public sealed class Identifier : Node
{
    public string Value { get; set; }

    public Identifier(Token token, string value) : base(token)
    {
        Value = value;
    }
}