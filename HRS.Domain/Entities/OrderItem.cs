using System.ComponentModel.DataAnnotations.Schema;

namespace HRS.Domain.Entities;

[Table("OrderItems")]
public class OrderItem
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public decimal TotalPrice { get; set; }

    public int ItemId { get; set; }

    [ForeignKey("ItemId")] public required Item Item { get; set; }
}
