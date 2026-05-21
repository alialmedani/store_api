namespace Store.Dtos;

public class ProductVariantSummaryDto
{
	public int Id { get; set; }

	public string Color { get; set; } = string.Empty;

	public string Size { get; set; } = string.Empty;

	public int StockQuantity { get; set; }
}