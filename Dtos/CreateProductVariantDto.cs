using System.ComponentModel.DataAnnotations;

namespace Store.Dtos;

public class CreateProductVariantDto
{
	[Range(1, int.MaxValue)]
	public int ProductId { get; set; }

	[MaxLength(50)]
	public string? Color { get; set; }

	[MaxLength(20)]
	public string? Size { get; set; }

	[Range(0, int.MaxValue)]
	public int StockQuantity { get; set; }
}