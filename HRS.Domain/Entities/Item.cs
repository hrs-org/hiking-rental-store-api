using System.ComponentModel.DataAnnotations.Schema;

namespace HRS.Domain.Entities;

[Table("Items")]
public class Item
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int Quantity { get; set; }
    public int Price { get; set; }
    public int? ParentId { get; set; }
    public Item? Parent { get; set; }
    public ICollection<Item> Children { get; set; } = new List<Item>();
}
