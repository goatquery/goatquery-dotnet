namespace GoatQuery;

using System.Collections.Generic;

internal sealed class PropertyPath : QueryExpression
{
    public List<string> Segments { get; }

    public PropertyPath(Token token, List<string> segments)
        : base(token)
    {
        Segments = segments;
    }

    public override string TokenLiteral()
    {
        return string.Join("/", Segments);
    }
}
