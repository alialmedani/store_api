using System.ComponentModel.DataAnnotations;

namespace Store.Dtos;

public class AdjustProductVariantStockDto
{
	public int QuantityChange { get; set; }

	[MaxLength(500)]
	public string? Note { get; set; }
}