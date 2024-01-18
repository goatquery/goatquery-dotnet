public record Query
{
    public int? Top { get; set; } = 0;
    public int? Skip { get; set; } = 0;
    public bool? Count { get; set; } = false;
    public string? OrderBy { get; set; } = string.Empty;
    public string? Select { get; set; } = string.Empty;
    public string? Search { get; set; } = string.Empty;
    public string? Filter { get; set; } = string.Empty;
}