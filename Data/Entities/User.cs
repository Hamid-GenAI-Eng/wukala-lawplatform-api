using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserAuthAPI.Data.Entities;

public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? PasswordHash { get; set; }

    [MaxLength(50)]
    public string Provider { get; set; } = "Local";

    [MaxLength(255)]
    public string? GoogleId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [NotMapped]
    public bool IsGoogleUser => Provider == "Google";

    [NotMapped]
    public bool IsLocalUser => Provider == "Local";
}