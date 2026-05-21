using System.ComponentModel.DataAnnotations;
using Store.Models;

namespace Store.Dtos;

public class CreateProductDto
{
	[Required]
	[MaxLength(150)]
	public string Name { get; set; } = string.Empty;

	[MaxLength(1000)]
	public string? Description { get; set; }

	[Range(0.01, double.MaxValue)]
	public decimal Price { get; set; }

	public bool IsActive { get; set; } = true;

	public ProductTargetAudience TargetAudience { get; set; } = ProductTargetAudience.Unisex;

	[Required]
	public int CategoryId { get; set; }
}