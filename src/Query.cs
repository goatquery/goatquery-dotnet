public record Query
{
    public int? Top { get; set; }
    public int? Skip { get; set; }
    public bool? Count { get; set; }
    public string? OrderBy { get; set; } = string.Empty;
    public string? Select { get; set; } = string.Empty;
    public string? Search { get; set; } = string.Empty;
    public string? Filter { get; set; } = string.Empty;
}