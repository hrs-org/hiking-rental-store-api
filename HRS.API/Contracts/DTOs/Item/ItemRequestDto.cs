namespace HRS.API.Contracts.DTOs.Item;

public class ItemRequestDto
{
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public required int Quantity { get; set; }
    public required decimal Price { get; set; }
}

public class AddItemRequestDto : ItemRequestDto
{
    public ICollection<AddItemRequestDto>? Children { get; set; }
}

public class UpdateItemRequestDto : ItemRequestDto
{
    public int? Id { get; set; }
    public ICollection<UpdateItemRequestDto>? Children { get; set; }
}
