using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserManagement.Models;

public class UserLog
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
    public long UserId { get; set; }

    [Required]
    [StringLength(100)]
    public required string Action { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(2000)]
    public string? Details { get; set; }

    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Navigation property
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }
}
