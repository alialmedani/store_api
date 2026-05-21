using System.ComponentModel.DataAnnotations;

namespace Store.Dtos;

public class CreateBulkProductVariantItemDto
{
	[Required]
	[MaxLength(50)]
	public string Color { get; set; } = string.Empty;

	[MaxLength(20)]
	public string? Size { get; set; }

	[Range(0, int.MaxValue)]
	public int StockQuantity { get; set; }
}