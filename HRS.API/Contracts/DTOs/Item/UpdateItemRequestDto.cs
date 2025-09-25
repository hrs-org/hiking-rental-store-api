namespace HRS.API.Contracts.DTOs.Item;

public class UpdateItemRequestDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int Price { get; set; }
    public ICollection<UpdateItemChildDto>? Children { get; set; }
}

public class UpdateItemChildDto
{
    public int? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int Price { get; set; }
}
