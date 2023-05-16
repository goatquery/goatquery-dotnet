public class GoatQueryException : Exception
{
    public GoatQueryException()
    {
    }

    public GoatQueryException(string message)
        : base(message)
    {
    }

    public GoatQueryException(string message, Exception inner)
        : base(message, inner)
    {
    }
}