using System.ComponentModel.DataAnnotations;
using Store.Models;
namespace Store.Dtos;

public class CreateCategoryDto
{
	[Required]
	[MaxLength(100)]
	public string Name { get; set; } = string.Empty;

	[MaxLength(500)]
	public string? Description { get; set; }
	public CategorySizeType SizeType { get; set; } = CategorySizeType.None;
	public bool IsActive { get; set; } = true;
}