using System.ComponentModel.DataAnnotations;

namespace Store.Dtos;

public class GenerateProductVariantsDto
{
	[Range(1, int.MaxValue)]
	public int ProductId { get; set; }

	public List<string>? Colors { get; set; }

	public List<string>? Sizes { get; set; }

	[Range(0, int.MaxValue)]
	public int DefaultStockQuantity { get; set; }

	public bool SkipExisting { get; set; } = false;
}