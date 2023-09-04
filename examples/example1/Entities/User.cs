using System.ComponentModel.DataAnnotations.Schema;
using Bogus.DataSets;

public record User
{
    public Guid Id { get; set; }

    [Column(TypeName = "varchar(128)")]
    public string Firstname { get; set; } = string.Empty;

    [Column(TypeName = "varchar(128)")]
    public string Lastname { get; set; } = string.Empty;

    [Column(TypeName = "varchar(320)")]
    public string Email { get; set; } = string.Empty;

    [Column(TypeName = "varchar(1024)")]
    public string AvatarUrl { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }

    [Column(TypeName = "varchar(64)")]
    public string UserName { get; set; } = string.Empty;

    [Column("PersonSex", TypeName = "varchar(32)")]
    public string Gender { get; set; } = string.Empty;
}