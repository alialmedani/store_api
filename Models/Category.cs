using System.ComponentModel.DataAnnotations;

namespace Store.Models;

public class Category : BaseEntity
{
	[Required]
	[MaxLength(100)]
	public string Name { get; set; } = string.Empty;

	[MaxLength(500)]
	public string? Description { get; set; }

	public bool IsActive { get; set; } = true;
	public CategorySizeType SizeType { get; set; } = CategorySizeType.None;
	public ICollection<Product> Products { get; set; } = new List<Product>();
}