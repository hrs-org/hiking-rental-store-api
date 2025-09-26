using System.ComponentModel.DataAnnotations.Schema;

namespace HRS.Domain.Entities;

[Table("Items")]
public class Item
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public required string Description { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public int? ParentId { get; set; }

    public Item? Parent { get; set; }

    public ICollection<Item> Children { get; set; } = [];

    public int CreatedById { get; set; }

    [ForeignKey("CreatedById")] public required User CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? UpdatedById { get; set; }

    [ForeignKey("UpdatedById")] public User? UpdatedBy { get; set; }

    public DateTime UpdatedAt { get; set; }
}
