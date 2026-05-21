using System.ComponentModel.DataAnnotations;

namespace Store.Models;

public class StockMovement : BaseEntity
{
	public int ProductVariantId { get; set; }

	public ProductVariant ProductVariant { get; set; } = null!;

	public StockMovementType MovementType { get; set; }

	public int QuantityChange { get; set; }

	public int OldQuantity { get; set; }

	public int NewQuantity { get; set; }

	[MaxLength(500)]
	public string? Note { get; set; }
}