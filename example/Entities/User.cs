using System.ComponentModel.DataAnnotations.Schema;

public record User
{
    public Guid Id { get; set; }
    public string Firstname { get; set; } = string.Empty;
    public string Lastname { get; set; } = string.Empty;
    public int Age { get; set; }
    public bool IsDeleted { get; set; }
    public double Test { get; set; }
    public int? NullableInt { get; set; }

    [Column(TypeName = "timestamp with time zone")]
    public DateTime DateOfBirthUtc { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime DateOfBirthTz { get; set; }
}