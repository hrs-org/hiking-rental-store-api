using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HRS.Domain.Entities;

[Table("Stores")]
[Index(nameof(UserId), IsUnique = true)]
public class Store
{
    [Key] public int Id { get; set; }

    [Required][MaxLength(200)] public string Name { get; set; } = null!;

    [MaxLength(1000)] public string Description { get; set; } = string.Empty;

    [MaxLength(500)] public string Address { get; set; } = string.Empty;

    [MaxLength(30)] public string PhoneNumber { get; set; } = string.Empty;

    [Required] public int UserId { get; set; }

    [ForeignKey(nameof(UserId))] public virtual User User { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
