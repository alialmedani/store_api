using System.ComponentModel.DataAnnotations;

namespace Store.Models;

public class Product : BaseEntity
{
	[Required]
	[MaxLength(150)]
	public string Name { get; set; } = string.Empty;

	[MaxLength(1000)]
	public string? Description { get; set; }

	public decimal Price { get; set; }

	public bool IsActive { get; set; } = true;

	public ProductTargetAudience TargetAudience { get; set; } = ProductTargetAudience.Unisex;

	public int CategoryId { get; set; }

	public Category Category { get; set; } = null!;

	public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
}