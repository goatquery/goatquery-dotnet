public record User
{
    public Guid Id { get; set; }
    public string Firstname { get; set; } = string.Empty;
    public string Lastname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
}