namespace HRS.API.Contracts.DTOs.Item;

public class AddItemRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int Price { get; set; }
    public ICollection<AddItemRequestDto>? Children { get; set; }
}
