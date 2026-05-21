using System.ComponentModel.DataAnnotations;

namespace Store.Dtos;

public class CreateBulkProductVariantsDto
{
	[Range(1, int.MaxValue)]
	public int ProductId { get; set; }

	[Required]
	[MinLength(1)]
	public List<CreateBulkProductVariantItemDto> Variants { get; set; } = new();
}