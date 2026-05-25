using System.ComponentModel.DataAnnotations;

namespace Store.Dtos;

public class GenerateProductVariantsDto
{
	[Range(1, int.MaxValue)]
	public int ProductId { get; set; }

	[Required]
	[MinLength(1)]
	public List<string> Colors { get; set; } = new();

	public List<string>? Sizes { get; set; }

	[Range(0, int.MaxValue)]
	public int DefaultStockQuantity { get; set; }

	public bool SkipExisting { get; set; } = false;
}