using Microsoft.EntityFrameworkCore;

public record User
{
    public Guid Id { get; set; }
    public string Firstname { get; set; } = string.Empty;
    public string Lastname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}


public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    public DbSet<User> Users => Set<User>();
}