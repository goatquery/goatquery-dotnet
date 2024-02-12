using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

public record User
{
    public Guid Id { get; set; }
    public string Firstname { get; set; } = string.Empty;
    public string Lastname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string UserName { get; set; } = string.Empty;

    [Column("PersonSex", TypeName = "varchar(32)")]
    public string Gender { get; set; } = string.Empty;
    public int Age { get; set; }
}


public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    public DbSet<User> Users => Set<User>();
}