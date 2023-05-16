public class PagedResponse<T>
{
    public PagedResponse(IEnumerable<T> data)
    {
        Value = data;
    }

    public IEnumerable<T> Value { get; set; }
}