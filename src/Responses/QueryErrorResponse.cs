public class QueryErrorResponse
{
    public QueryErrorResponse(int status, string message)
    {
        Status = status;
        Message = message;
    }

    public int Status { get; set; }
    public string Message { get; set; } = string.Empty;
}