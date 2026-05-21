 
namespace Store.Dtos;

public class StockMovementDto
{
	public int Id { get; set; }

	public int ProductVariantId { get; set; }

	public string ProductName { get; set; } = string.Empty;

	public string Color { get; set; } = string.Empty;

	public string Size { get; set; } = string.Empty;

	public LookupDto MovementType { get; set; } = new();
	public int QuantityChange { get; set; }

	public int OldQuantity { get; set; }

	public int NewQuantity { get; set; }

	public string? Note { get; set; }

	public DateTime CreatedAt { get; set; }
}