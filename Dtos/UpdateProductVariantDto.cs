using System.ComponentModel.DataAnnotations;

namespace Store.Dtos;

public class UpdateProductVariantDto
{
	[Required]
	[MaxLength(50)]
	public string Color { get; set; } = string.Empty;

	[Required]
	[MaxLength(20)]
	public string Size { get; set; } = string.Empty;

	[Range(0, int.MaxValue)]
	public int StockQuantity { get; set; }
}