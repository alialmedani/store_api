namespace Store.Dtos;

public class ProductDetailsDto : ProductDto
{
	public List<ProductVariantSummaryDto> Variants { get; set; } = new();
}