namespace HRS.API.Contracts.DTOs.Item;

public class UpdateItemRequestDto
{
    public int? Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required int Quantity { get; set; }
    public required decimal Price { get; set; }
    public ICollection<UpdateItemRequestDto>? Children { get; set; }
}
