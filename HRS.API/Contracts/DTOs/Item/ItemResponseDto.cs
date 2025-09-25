namespace HRS.API.Contracts.DTOs.Item;

public class ItemResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int Price { get; set; }
    public ICollection<ItemResponseDto>? Children { get; set; }
}
