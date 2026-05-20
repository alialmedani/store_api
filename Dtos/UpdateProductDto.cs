using System.ComponentModel.DataAnnotations;
using Store.Models;

namespace Store.Dtos;

public class UpdateProductDto
{
	[Required]
	[MaxLength(150)]
	public string Name { get; set; } = string.Empty;

	[MaxLength(1000)]
	public string? Description { get; set; }

	[Range(0.01, double.MaxValue)]
	public decimal Price { get; set; }

	[Range(0, int.MaxValue)]
	public int StockQuantity { get; set; }

	public bool IsActive { get; set; }

	public ProductTargetAudience TargetAudience { get; set; }

	[Required]
	public int CategoryId { get; set; }
}