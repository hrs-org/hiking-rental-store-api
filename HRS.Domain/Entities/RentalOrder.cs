using System.ComponentModel.DataAnnotations.Schema;
using HRS.Domain.Enums;

namespace HRS.Domain.Entities;

[Table("RentalOrders")]
public class RentalOrder
{
    public int Id { get; set; }
    public ICollection<Item> Items { get; set; } = new List<Item>();
    public OrderStatus Status { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int UpdatedById { get; set; }
    [ForeignKey("UpdatedById")] public required User UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CreatedById { get; set; }
    [ForeignKey("CreatedById")] public required User CreatedBy { get; set; }
}
